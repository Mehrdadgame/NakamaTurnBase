// ─────────────────────────────────────────────────────────────────────────────
//  UserIdLogger.cs   —   TEMPORARY DEBUG TOOL   (v2 - self-diagnosing)
//
//  خودش موقع Play بالا می‌آید. نیازی به قرار دادن روی هیچ صحنه‌ای نیست.
//  اگر لاگ نمی‌بینید، این نسخه خودش می‌گوید کجا گیر کرده.
//
//  منوی دستی:  Tools ▸ Nakama ▸ Print My User ID
//  کلید میان‌بر در حین بازی:  F9
//
//  حذف:  کل پوشه‌ی Assets/_TempDebug را پاک کنید.
// ─────────────────────────────────────────────────────────────────────────────

using System.Collections;
using Nakama.Helpers;
using UnityEngine;

namespace TempDebug
{
    public class UserIdLogger : MonoBehaviour
    {
        const float LoginTimeout = 120f;   // چقدر منتظر لاگین بماند
        const float AccountTimeout = 20f;  // چقدر منتظر لود اکانت بماند
        const float HeartbeatEvery = 5f;   // هر چند ثانیه گزارش وضعیت بدهد
        const KeyCode ReprintKey = KeyCode.F9;

        static UserIdLogger _instance;

        // ─── راه‌اندازی خودکار ───
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoSpawn()
        {
            if (_instance != null) return;

            var go = new GameObject("~UserIdLogger [TEMP]");
            _instance = go.AddComponent<UserIdLogger>();
            DontDestroyOnLoad(go);

            // اولین سیگنال حیات — این باید فوراً در Console ظاهر شود
            Debug.Log("[UserIdLogger] ALIVE. Waiting for Nakama login... " +
                      "(press F9 any time, or use Tools > Nakama > Print My User ID)");
        }

        IEnumerator Start()
        {
            float t = 0f;
            float nextBeat = HeartbeatEvery;

            while (true)
            {
                var mgr = NakamaManager.Instance;

                if (mgr != null && mgr.Session != null)
                    break;

                t += Time.unscaledDeltaTime;

                if (t >= nextBeat)
                {
                    nextBeat += HeartbeatEvery;
                    if (mgr == null)
                    {
                        Debug.Log($"[UserIdLogger] {t:F0}s - NakamaManager.Instance is NULL. " +
                                  "This scene has no NakamaManager. Start from the 0-Initializer scene.");
                    }
                    else
                    {
                        Debug.Log($"[UserIdLogger] {t:F0}s - NakamaManager found, but Session is still null " +
                                  "(login in progress or failed).");
                    }
                }

                if (t > LoginTimeout)
                {
                    Debug.LogWarning($"[UserIdLogger] Gave up after {LoginTimeout}s. No Nakama session.");
                    yield break;
                }

                yield return null;
            }

            var session = NakamaManager.Instance.Session;
            Print(session.UserId, session.Username, null, null);

            // اطلاعات کامل اکانت (نام نمایشی و کیف پول) بعداً می‌رسد
            float t2 = 0f;
            while ((NakamaUserManager.Instance == null || !NakamaUserManager.Instance.LoadingFinished)
                   && t2 < AccountTimeout)
            {
                t2 += Time.unscaledDeltaTime;
                yield return null;
            }

            if (NakamaUserManager.Instance != null && NakamaUserManager.Instance.LoadingFinished)
                Print(session.UserId, session.Username, SafeDisplayName(), SafeWallet());
            else
                Debug.Log("[UserIdLogger] Account details did not load, but the USER ID above is valid.");
        }

        void Update()
        {
            if (Input.GetKeyDown(ReprintKey))
                PrintNow();
        }

        [ContextMenu("Print User ID Now")]
        public void PrintNow() => Report();

        /// <summary>قابل صدا زدن از منوی Tools هم هست</summary>
        public static void Report()
        {
            var mgr = NakamaManager.Instance;

            if (mgr == null)
            {
                Debug.LogWarning("[UserIdLogger] NakamaManager.Instance is null. " +
                                 "Are you in Play mode, and does this scene create it?");
                return;
            }

            if (mgr.Session == null)
            {
                Debug.LogWarning("[UserIdLogger] Logged out - no session yet.");
                return;
            }

            Print(mgr.Session.UserId, mgr.Session.Username, SafeDisplayName(), SafeWallet());
        }

        static string SafeDisplayName()
        {
            try
            {
                var um = NakamaUserManager.Instance;
                if (um == null || !um.LoadingFinished) return null;
                return string.IsNullOrEmpty(um.DisplayName) ? "(not set)" : um.DisplayName;
            }
            catch { return null; }
        }

        static string SafeWallet()
        {
            try
            {
                var um = NakamaUserManager.Instance;
                if (um == null || !um.LoadingFinished) return null;
                return string.IsNullOrEmpty(um.Wallet) ? "(empty)" : um.Wallet;
            }
            catch { return null; }
        }

        static void Print(string userId, string username, string displayName, string wallet)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine();
            sb.AppendLine("==================== NAKAMA ACCOUNT (debug) ====================");
            sb.AppendLine("  USER ID      : " + userId);
            sb.AppendLine("  Username     : " + (username ?? "-"));
            if (displayName != null) sb.AppendLine("  Display Name : " + displayName);
            if (wallet != null) sb.AppendLine("  Wallet       : " + wallet);
            sb.AppendLine("----------------------------------------------------------------");
            sb.AppendLine("  User ID copied to clipboard.  F9 = print again.");
            sb.AppendLine("  Nakama Console > Accounts > paste ID > Wallet");
            sb.AppendLine("================================================================");

            Debug.Log(sb.ToString());

            try { GUIUtility.systemCopyBuffer = userId; }
            catch { /* روی بعضی پلتفرم‌ها در دسترس نیست */ }
        }
    }
}

#if UNITY_EDITOR
namespace TempDebug
{
    public static class UserIdLoggerMenu
    {
        [UnityEditor.MenuItem("Tools/Nakama/Print My User ID")]
        static void PrintUserId()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[UserIdLogger] Enter Play mode first - the session only exists at runtime.");
                return;
            }
            UserIdLogger.Report();
        }
    }
}
#endif
