// FigmaApi.cs
// لایه ارتباط با Figma REST API
// محل قرارگیری: Assets/Editor/Figma/

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace FigmaImport
{
    public class FigmaApi : IDisposable
    {
        const string Base = "https://api.figma.com/v1";

        readonly HttpClient _api;      // با هدر توکن
        readonly HttpClient _plain;    // بدون هدر، برای دانلود از CDN آمازون
        readonly string _fileKey;

        /// <summary>حداکثر تعداد id در هر درخواست تصویر</summary>
        public int ImageBatchSize = 25;

        /// <summary>فاصله بین درخواست‌ها به میلی‌ثانیه (محدودیت فیگما ~۳۰ درخواست در دقیقه)</summary>
        public int ThrottleMs = 2200;

        static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            // فیگما فیلدهای زیادی دارد که ما مدل نکرده‌ایم؛ خطاها را نادیده بگیر
            Error = (sender, args) => { args.ErrorContext.Handled = true; }
        };

        public FigmaApi(string token, string fileKey)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Figma token is empty.");
            if (string.IsNullOrWhiteSpace(fileKey))
                throw new ArgumentException("Figma file key is empty.");

            _fileKey = fileKey.Trim();

            _api = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
            _api.DefaultRequestHeaders.Add("X-Figma-Token", token.Trim());

            _plain = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
        }

        // ---------------- درخواست پایه ----------------

        async Task<string> GetStringAsync(string url)
        {
            Exception last = null;

            for (int attempt = 0; attempt < 4; attempt++)
            {
                try
                {
                    var res = await _api.GetAsync(url);
                    var body = await res.Content.ReadAsStringAsync();

                    if ((int)res.StatusCode == 429)
                    {
                        // Rate limit — صبر تصاعدی
                        int wait = 8000 * (attempt + 1);
                        Debug.LogWarning($"[Figma] Rate limited. Waiting {wait / 1000}s before retry...");
                        await Task.Delay(wait);
                        continue;
                    }

                    if (!res.IsSuccessStatusCode)
                        throw new Exception($"Figma API {(int)res.StatusCode} {res.StatusCode}\nURL: {url}\n{body}");

                    return body;
                }
                catch (TaskCanceledException e)
                {
                    last = e;
                    await Task.Delay(3000);
                }
            }

            throw new Exception("Figma request failed after several retries.", last);
        }

        // ---------------- نودها ----------------

        /// <summary>گرفتن یک نود خاص (یک صفحه) با تمام زیرمجموعه‌هایش</summary>
        public async Task<FigmaNode> GetNodeAsync(string nodeId)
        {
            string id = NormalizeId(nodeId);
            string url = $"{Base}/files/{_fileKey}/nodes?ids={Uri.EscapeDataString(id)}";

            var json = await GetStringAsync(url);
            var res = JsonConvert.DeserializeObject<FigmaNodesResponse>(json, JsonSettings);

            if (res?.nodes == null || res.nodes.Count == 0)
                throw new Exception($"No node found with id {nodeId}. Check the file key and node id.");

            foreach (var kv in res.nodes)
            {
                if (kv.Value?.document != null)
                    return kv.Value.document;
            }

            throw new Exception($"Node {nodeId} came back empty.");
        }

        /// <summary>گرفتن کل ساختار فایل — برای پیدا کردن Node ID صفحات</summary>
        public async Task<FigmaNode> GetFileAsync(int depth = 2)
        {
            string url = $"{Base}/files/{_fileKey}?depth={depth}";
            var json = await GetStringAsync(url);
            var res = JsonConvert.DeserializeObject<FigmaFileResponse>(json, JsonSettings);

            if (res?.document == null)
                throw new Exception("File structure came back empty. Check the token and file key.");

            return res.document;
        }

        // ---------------- تصاویر ----------------

        /// <summary>
        /// گرفتن URL رندر شده‌ی نودها. ورودی می‌تواند صدها id باشد؛
        /// خودش دسته‌بندی و throttle می‌کند.
        /// </summary>
        public async Task<Dictionary<string, string>> GetImageUrlsAsync(
            IReadOnlyList<string> nodeIds, int scale, string format,
            Action<string, float> progress = null)
        {
            var result = new Dictionary<string, string>();
            if (nodeIds == null || nodeIds.Count == 0) return result;

            for (int i = 0; i < nodeIds.Count; i += ImageBatchSize)
            {
                var batch = new List<string>();
                for (int j = i; j < Math.Min(i + ImageBatchSize, nodeIds.Count); j++)
                    batch.Add(NormalizeId(nodeIds[j]));

                string ids = Uri.EscapeDataString(string.Join(",", batch));
                string url = $"{Base}/images/{_fileKey}?ids={ids}&format={format}&scale={scale}&use_absolute_bounds=true";

                progress?.Invoke($"Resolving image URLs ({i + batch.Count}/{nodeIds.Count})",
                                 (float)(i + batch.Count) / nodeIds.Count);

                var json = await GetStringAsync(url);
                var res = JsonConvert.DeserializeObject<FigmaImagesResponse>(json, JsonSettings);

                if (!string.IsNullOrEmpty(res?.err))
                    Debug.LogWarning($"[Figma] Image render error: {res.err}");

                if (res?.images != null)
                {
                    foreach (var kv in res.images)
                        if (!string.IsNullOrEmpty(kv.Value))
                            result[kv.Key] = kv.Value;
                }

                if (i + ImageBatchSize < nodeIds.Count)
                    await Task.Delay(ThrottleMs);
            }

            return result;
        }

        /// <summary>دانلود بایت‌های یک تصویر از URL موقت فیگما</summary>
        public async Task<byte[]> DownloadAsync(string url)
        {
            var res = await _plain.GetAsync(url);
            if (!res.IsSuccessStatusCode)
            {
                Debug.LogWarning($"[Figma] Download failed ({(int)res.StatusCode}): {url}");
                return null;
            }
            return await res.Content.ReadAsByteArrayAsync();
        }

        // ---------------- کمکی ----------------

        /// <summary>فیگما هم "2005:802" و هم "2005-802" را قبول می‌کند. یکسان‌سازی می‌کنیم.</summary>
        public static string NormalizeId(string id)
        {
            if (string.IsNullOrEmpty(id)) return id;
            return id.Trim().Replace('-', ':');
        }

        /// <summary>استخراج File Key از لینک کامل فیگما</summary>
        public static string ExtractFileKey(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;
            input = input.Trim();

            if (!input.Contains("figma.com")) return input;   // خودش key است

            // https://www.figma.com/design/<KEY>/<name>?node-id=...
            var parts = input.Split('/');
            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (parts[i] == "file" || parts[i] == "design" || parts[i] == "proto")
                    return parts[i + 1];
            }
            return input;
        }

        public void Dispose()
        {
            _api?.Dispose();
            _plain?.Dispose();
        }
    }
}
