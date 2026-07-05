using System;
using System.Threading.Tasks;
using RTLTMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_ANDROID
using Bazaar.Data;
using Bazaar.Poolakey;
using Bazaar.Poolakey.Data;
#endif

namespace Nakama.Helpers
{
    [Serializable]
    public class CoinPackProduct
    {
        public string productId;     // Cafe Bazaar SKU
        public int coinsAmount;      // display only - the server (COIN_PACKS) is authoritative
        public string priceLabel;    // fallback shown until the store reports the real price
        public Button buyButton;
        public RTLTextMeshPro coinsLabel;
        public RTLTextMeshPro priceText;
    }

    [Serializable]
    internal class CoinPurchaseResult { public bool success; public int coinsAwarded; public string error; }

    public class CoinShopManager : MonoBehaviour
    {
        private const string VerifyCoinPurchaseRpcId = "VerifyCoinPurchaseRpc";
        private const string StoreName = "cafebazaar";

        [Header("Cafe Bazaar / Poolakey")]
        [Tooltip("کلید عمومی RSA از پنل توسعه‌دهنده بازار (بخش پرداخت درون‌برنامه‌ای).")]
        [SerializeField] private string cafeBazaarRsaPublicKey = "";

        [Header("Products (assign in Inspector)")]
        [SerializeField] private CoinPackProduct[] products;

        [Header("Feedback")]
        [SerializeField] private RTLTextMeshPro statusText;
        [SerializeField] private GameObject loadingOverlay;

        private bool _connected;
        private bool _busy;
#if UNITY_ANDROID
        private Payment _payment;
#endif

        // ── Lifecycle ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (products == null) return;

            for (int i = 0; i < products.Length; i++)
            {
                int idx = i;
                var product = products[idx];
                if (product == null) continue;

                if (product.buyButton != null)
                    product.buyButton.onClick.AddListener(() => OnBuyClicked(idx));

                if (product.coinsLabel != null)
                    product.coinsLabel.text = FormatCoins(product.coinsAmount);

                if (product.priceText != null && !string.IsNullOrEmpty(product.priceLabel))
                    product.priceText.text = PersianTextUtils.FixRTLPriceLabel(product.priceLabel);
            }
        }

        private void OnEnable()
        {
            SetStatus("", Color.white);
        }

        private void Start()
        {
            SetButtonsInteractable(false);
#if UNITY_ANDROID
            if (string.IsNullOrEmpty(cafeBazaarRsaPublicKey))
                Debug.LogError("[CoinShop] کلید عمومی RSA بازار تنظیم نشده است.");

            SetStatus("در حال اتصال به بازار...", Color.white);
            _ = InitializeBazaarAsync();
#else
            SetStatus("خرید بازار فقط روی اندروید در دسترس است.", Color.yellow);
#endif
        }

        private void OnDestroy()
        {
#if UNITY_ANDROID
            _payment?.Disconnect();
#endif
        }

#if UNITY_ANDROID

        // ── Connect ──────────────────────────────────────────────────────────────

        private async Task InitializeBazaarAsync()
        {
            try
            {
                var securityCheck = string.IsNullOrEmpty(cafeBazaarRsaPublicKey)
                    ? SecurityCheck.Disable()
                    : SecurityCheck.Enable(cafeBazaarRsaPublicKey);
                _payment = new Payment(new PaymentConfiguration(securityCheck));

                var result = await _payment.Connect();
                if (result.status == Status.Success && result.data)
                {
                    _connected = true;
                    SetStatus("", Color.white);
                    SetButtonsInteractable(true);

                    await RefreshStoreAndRecoverPurchasesAsync();
                    return;
                }

                _connected = false;
                SetStatus("اتصال به بازار ناموفق بود.", Color.red);
                Debug.LogWarning("[CoinShop] Bazaar connect failed: " + FormatResult(result));
            }
            catch (Exception e)
            {
                _connected = false;
                SetStatus("اتصال به بازار ناموفق بود.", Color.red);
                Debug.LogWarning("[CoinShop] Bazaar connect error: " + e.Message);
            }
        }

        // ── Buy ───────────────────────────────────────────────────────────────────

        private async Task PurchaseAsync(int index)
        {
            if (!_connected)
            {
                SetStatus("اتصال به بازار برقرار نیست.", Color.red);
                return;
            }
            if (_busy) return;

            var product = GetProduct(index);
            if (product == null || string.IsNullOrEmpty(product.productId))
            {
                SetStatus("شناسه محصول بازار تنظیم نشده است.", Color.red);
                return;
            }

            _busy = true;
            SetButtonsInteractable(false);
            SetStatus("در حال خرید...", Color.white);
            SetLoading(true);

            try
            {
                var payload = BuildDeveloperPayload(product.productId);
                var result = await _payment.Purchase(
                    product.productId,
                    SKUDetails.Type.inApp,
                    OnPurchaseStarted,
                    payload: payload);

                if (result.status == Status.Success && result.data != null)
                {
                    SetStatus("در حال تایید خرید...", Color.white);
                    await VerifyAndConsume(result.data, isRecovery: false);
                    return;
                }

                bool canceled = result.status == Status.Canceled;
                SetStatus(canceled ? "خرید لغو شد." : "خرید ناموفق بود.",
                          canceled ? Color.yellow : Color.red);
                Debug.LogWarning("[CoinShop] Bazaar purchase failed: " + FormatResult(result));
                EndBusy();
            }
            catch (Exception e)
            {
                SetStatus("خرید ناموفق بود.", Color.red);
                Debug.LogWarning("[CoinShop] Bazaar purchase error: " + e.Message);
                EndBusy();
            }
        }

        private void OnPurchaseStarted(Result<PurchaseInfo> result)
        {
            Debug.Log("[CoinShop] Bazaar purchase flow started: " + FormatResult(result));
        }

        // ── Store data: live prices + recovery of stuck consumables ──────────────

        private async Task RefreshStoreAndRecoverPurchasesAsync()
        {
            try
            {
                await RefreshSkuPricesAsync();
                await RecoverUnconsumedPurchasesAsync();
            }
            catch (Exception e)
            {
                Debug.LogWarning("[CoinShop] Bazaar refresh error: " + e.Message);
            }
        }

        private async Task RefreshSkuPricesAsync()
        {
            var ids = GetProductIdsCsv();
            if (string.IsNullOrEmpty(ids)) return;

            var result = await _payment.GetSkuDetails(ids, SKUDetails.Type.inApp);
            if (result.status != Status.Success || result.data == null)
            {
                Debug.LogWarning("[CoinShop] Bazaar SKU details failed: " + FormatResult(result));
                return;
            }

            foreach (var sku in result.data)
            {
                int idx = IndexOfProduct(sku.sku);
                var product = GetProduct(idx);
                if (product != null && product.priceText != null && !string.IsNullOrEmpty(sku.price))
                    product.priceText.text = PersianTextUtils.FixRTLPriceLabel(sku.price);
            }
        }

        private async Task RecoverUnconsumedPurchasesAsync()
        {
            var result = await _payment.GetPurchases(SKUDetails.Type.inApp);
            if (result.status != Status.Success || result.data == null)
            {
                Debug.LogWarning("[CoinShop] Bazaar purchases query failed: " + FormatResult(result));
                return;
            }

            foreach (var purchase in result.data)
            {
                if (purchase.purchaseState == PurchaseInfo.State.Purchased &&
                    IndexOfProduct(purchase.productId) >= 0)
                {
                    await VerifyAndConsume(purchase, isRecovery: true);
                }
            }
        }

        // ── Server verify → consume ─────────────────────────────────────────────────

        private async Task VerifyAndConsume(PurchaseInfo purchase, bool isRecovery)
        {
            try
            {
                var payload = JsonUtility.ToJson(new VerifyPayload
                {
                    store = StoreName,
                    productId = purchase.productId,
                    purchaseToken = purchase.purchaseToken,
                    orderId = purchase.orderId,
                    packageName = purchase.packageName,
                    purchaseState = (int)purchase.purchaseState,
                    purchaseTime = purchase.purchaseTime,
                    developerPayload = purchase.payload,
                    dataSignature = purchase.dataSignature,
                    originalJson = purchase.originalJson,
                });

                var rpc = await NakamaManager.Instance.SendRPC(VerifyCoinPurchaseRpcId, payload);
                var res = (rpc != null && !string.IsNullOrEmpty(rpc.Payload))
                    ? rpc.Payload.Deserialize<CoinPurchaseResult>()
                    : null;

                if (res != null && res.success)
                {
                    await ConsumePurchaseAsync(purchase);

                    if (WalletManager.Instance != null)
                        await WalletManager.Instance.RefreshAsync();

                    if (!isRecovery)
                        SetStatus("+" + FormatCoins(res.coinsAwarded) + " کوین دریافت شد!",
                                  new Color(0.25f, 1f, 0.25f));
                }
                else
                {
                    string err = res?.error ?? "unknown";

                    // Coins were already granted on a previous attempt but the item was never
                    // consumed — consume it now so it can be purchased again.
                    if (IsAlreadyProcessed(err))
                    {
                        await ConsumePurchaseAsync(purchase);

                        if (WalletManager.Instance != null)
                            await WalletManager.Instance.RefreshAsync();

                        if (!isRecovery)
                            SetStatus("این خرید قبلاً ثبت شده بود.", Color.yellow);
                        return;
                    }

                    if (!isRecovery)
                        SetStatus("تایید خرید ناموفق بود.", Color.red);
                    Debug.LogWarning("[CoinShop] Server verify failed: " + err);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[CoinShop] VerifyAndConsume error: " + e.Message);
                if (!isRecovery) SetStatus("خطا در ارتباط با سرور.", Color.red);
            }
            finally
            {
                if (!isRecovery) EndBusy();
            }
        }

        private async Task ConsumePurchaseAsync(PurchaseInfo purchase)
        {
            var consume = await _payment.Consume(purchase.purchaseToken);
            if (consume.status == Status.Success)
                Debug.Log("[CoinShop] Bazaar consumed: " + purchase.productId);
            else
                Debug.LogWarning("[CoinShop] Bazaar consume failed (non-critical): " + FormatResult(consume));
        }

        private string BuildDeveloperPayload(string productId)
        {
            return JsonUtility.ToJson(new PurchasePayload
            {
                userId = NakamaManager.Instance?.Session?.UserId ?? "",
                productId = productId,
                nonce = Guid.NewGuid().ToString("N"),
            });
        }

        private static bool IsAlreadyProcessed(string error) =>
            !string.IsNullOrEmpty(error) &&
            error.IndexOf("Already processed", StringComparison.OrdinalIgnoreCase) >= 0;

        private static string FormatResult<T>(Result<T> result) =>
            result == null ? "null" : result.ToString();

        [Serializable]
        private class PurchasePayload
        {
            public string userId;
            public string productId;
            public string nonce;
        }

        [Serializable]
        private class VerifyPayload
        {
            public string store;
            public string productId;
            public string purchaseToken;
            public string orderId;
            public string packageName;
            public int purchaseState;
            public long purchaseTime;
            public string developerPayload;
            public string dataSignature;
            public string originalJson;
        }

        private string GetProductIdsCsv()
        {
            if (products == null || products.Length == 0) return "";

            string ids = "";
            for (int i = 0; i < products.Length; i++)
            {
                if (products[i] == null || string.IsNullOrEmpty(products[i].productId)) continue;
                if (ids.Length > 0) ids += ",";
                ids += products[i].productId;
            }

            return ids;
        }

        private int IndexOfProduct(string productId)
        {
            if (products == null || string.IsNullOrEmpty(productId)) return -1;

            for (int i = 0; i < products.Length; i++)
                if (products[i] != null && products[i].productId == productId)
                    return i;
            return -1;
        }

#endif

        // ── Helpers ─────────────────────────────────────────────────────────────────

        private void OnBuyClicked(int index)
        {
#if UNITY_ANDROID
            _ = PurchaseAsync(index);
#else
            SetStatus("خرید بازار فقط روی اندروید در دسترس است.", Color.yellow);
#endif
        }

        private void EndBusy()
        {
            _busy = false;
            SetButtonsInteractable(_connected);
            SetLoading(false);
        }

        private CoinPackProduct GetProduct(int index)
        {
            if (products == null || index < 0 || index >= products.Length)
                return null;
            return products[index];
        }

        private void SetStatus(string msg, Color color)
        {
            if (statusText == null) return;
            statusText.text = msg;
            statusText.color = color;
        }

        private void SetButtonsInteractable(bool on)
        {
            if (products == null) return;

            foreach (var p in products)
                if (p != null && p.buyButton != null) p.buyButton.interactable = on;
        }

        private void SetLoading(bool on)
        {
            if (loadingOverlay != null) loadingOverlay.SetActive(on);
        }

        private static string FormatCoins(int amount) =>
            PersianTextUtils.FormatNumber(amount);
    }
}
