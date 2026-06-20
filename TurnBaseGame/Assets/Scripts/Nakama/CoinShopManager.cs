using System;
using System.Threading.Tasks;
using RTLTMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_ANDROID
using System.Collections.Generic;
using MyketPlugin;
#endif

namespace Nakama.Helpers
{
    [Serializable]
    public class CoinPackProduct
    {
        public string productId;
        public int coinsAmount;      // display only — the server (COIN_PACKS) is authoritative
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

        [Header("Myket")]
        [Tooltip("کلید عمومی RSA از پنل توسعه‌دهنده مایکت (بخش پرداخت درون‌برنامه‌ای). باید پر شود.")]
        [SerializeField] private string myketRsaPublicKey = "";

        [Header("Products (assign in Inspector)")]
        [SerializeField] private CoinPackProduct[] products;

        [Header("Feedback")]
        [SerializeField] private RTLTextMeshPro statusText;
        [SerializeField] private GameObject loadingOverlay;

        private bool _connected;
        private bool _busy;

        // ── Lifecycle ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            for (int i = 0; i < products.Length; i++)
            {
                int idx = i;
                if (products[idx].buyButton != null)
                    products[idx].buyButton.onClick.AddListener(() => OnBuyClicked(idx));

                if (products[idx].coinsLabel != null)
                    products[idx].coinsLabel.text = FormatCoins(products[idx].coinsAmount);

                if (products[idx].priceText != null && !string.IsNullOrEmpty(products[idx].priceLabel))
                    products[idx].priceText.text = PersianTextUtils.FixRTLPriceLabel(products[idx].priceLabel);
            }
        }

        private void OnEnable()
        {
            SetStatus("", Color.white);
#if UNITY_ANDROID
            IABEventManager.billingSupportedEvent        += OnBillingSupported;
            IABEventManager.billingNotSupportedEvent     += OnBillingNotSupported;
            IABEventManager.purchaseSucceededEvent       += OnPurchaseSucceeded;
            IABEventManager.purchaseFailedEvent          += OnPurchaseFailed;
            IABEventManager.consumePurchaseSucceededEvent += OnConsumeSucceeded;
            IABEventManager.consumePurchaseFailedEvent    += OnConsumeFailed;
            IABEventManager.queryInventorySucceededEvent  += OnQueryInventorySucceeded;
            IABEventManager.queryInventoryFailedEvent     += OnQueryInventoryFailed;
#endif
        }

        private void OnDisable()
        {
#if UNITY_ANDROID
            IABEventManager.billingSupportedEvent        -= OnBillingSupported;
            IABEventManager.billingNotSupportedEvent     -= OnBillingNotSupported;
            IABEventManager.purchaseSucceededEvent       -= OnPurchaseSucceeded;
            IABEventManager.purchaseFailedEvent          -= OnPurchaseFailed;
            IABEventManager.consumePurchaseSucceededEvent -= OnConsumeSucceeded;
            IABEventManager.consumePurchaseFailedEvent    -= OnConsumeFailed;
            IABEventManager.queryInventorySucceededEvent  -= OnQueryInventorySucceeded;
            IABEventManager.queryInventoryFailedEvent     -= OnQueryInventoryFailed;
#endif
        }

        private void Start()
        {
            SetButtonsInteractable(false);
#if UNITY_ANDROID
            if (string.IsNullOrEmpty(myketRsaPublicKey))
                Debug.LogError("[CoinShop] کلید عمومی RSA مایکت تنظیم نشده — آن را از پنل مایکت در اینسپکتور وارد کنید.");

            SetStatus("در حال اتصال...", Color.white);
            MyketIAB.init(myketRsaPublicKey);
#else
            SetStatus("خرید فقط روی اندروید در دسترس است.", Color.yellow);
#endif
        }

        private void OnDestroy()
        {
#if UNITY_ANDROID
            MyketIAB.unbindService();
#endif
        }

#if UNITY_ANDROID

        // ── Connect ───────────────────────────────────────────────────────────────

        private void OnBillingSupported()
        {
            _connected = true;
            SetStatus("", Color.white);
            SetButtonsInteractable(true);

            // Refresh store prices and reconcile any purchase left un-consumed by an
            // interrupted flow (server verification is idempotent, so re-processing is safe).
            MyketIAB.queryInventory(GetProductIds());
        }

        private void OnBillingNotSupported(string error)
        {
            _connected = false;
            SetStatus("اتصال به مایکت ناموفق بود.", Color.red);
            Debug.LogWarning("[CoinShop] Billing not supported: " + error);
        }

        // ── Buy ───────────────────────────────────────────────────────────────────

        private void OnBuyClicked(int index)
        {
            if (!_connected)
            {
                SetStatus("اتصال به مایکت برقرار نیست.", Color.red);
                return;
            }
            if (_busy) return;

            _busy = true;
            SetButtonsInteractable(false);
            SetStatus("در حال خرید...", Color.white);
            SetLoading(true);

            MyketIAB.purchaseProduct(products[index].productId);
        }

        private void OnPurchaseSucceeded(MyketPurchase purchase)
        {
            SetStatus("در حال تایید خرید...", Color.white);
            _ = VerifyAndConsume(purchase, isRecovery: false);
        }

        private void OnPurchaseFailed(string error)
        {
            bool canceled = !string.IsNullOrEmpty(error) &&
                            error.ToLowerInvariant().Contains("cancel");
            SetStatus(canceled ? "خرید لغو شد." : "خرید ناموفق بود.",
                      canceled ? Color.yellow : Color.red);
            Debug.LogWarning("[CoinShop] Purchase failed: " + error);
            EndBusy();
        }

        private void OnConsumeSucceeded(MyketPurchase purchase)
        {
            Debug.Log("[CoinShop] Consumed: " + purchase.ProductId);
        }

        private void OnConsumeFailed(string error)
        {
            Debug.LogWarning("[CoinShop] Consume failed (non-critical): " + error);
        }

        // ── Inventory: live prices + recovery of stuck consumables ──────────────────

        private void OnQueryInventorySucceeded(List<MyketPurchase> purchases, List<MyketSkuInfo> skus)
        {
            if (skus != null)
            {
                foreach (var sku in skus)
                {
                    int idx = IndexOfProduct(sku.ProductId);
                    if (idx >= 0 && products[idx].priceText != null && !string.IsNullOrEmpty(sku.Price))
                        products[idx].priceText.text = PersianTextUtils.FixRTLPriceLabel(sku.Price);
                }
            }

            if (purchases != null)
            {
                foreach (var p in purchases)
                {
                    if (p.PurchaseState == MyketPurchase.MyketPurchaseState.Purchased &&
                        IndexOfProduct(p.ProductId) >= 0)
                        _ = VerifyAndConsume(p, isRecovery: true);
                }
            }
        }

        private void OnQueryInventoryFailed(string error)
        {
            Debug.LogWarning("[CoinShop] Query inventory failed: " + error);
        }

        // ── Server verify → consume ─────────────────────────────────────────────────

        private async Task VerifyAndConsume(MyketPurchase purchase, bool isRecovery)
        {
            try
            {
                var payload = JsonUtility.ToJson(new VerifyPayload
                {
                    productId     = purchase.ProductId,
                    purchaseToken = purchase.PurchaseToken,
                    orderId       = purchase.OrderId,
                    dataSignature = purchase.Signature,
                    originalJson  = purchase.OriginalJson,
                });

                var rpc = await NakamaManager.Instance.SendRPC(VerifyCoinPurchaseRpcId, payload);
                var res = (rpc != null && !string.IsNullOrEmpty(rpc.Payload))
                    ? rpc.Payload.Deserialize<CoinPurchaseResult>()
                    : null;

                if (res != null && res.success)
                {
                    MyketIAB.consumeProduct(purchase.ProductId);

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
                    if (err.Contains("Already processed"))
                        MyketIAB.consumeProduct(purchase.ProductId);

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

        [Serializable]
        private class VerifyPayload
        {
            public string productId;
            public string purchaseToken;
            public string orderId;
            public string dataSignature;
            public string originalJson;
        }

        private string[] GetProductIds()
        {
            var ids = new string[products.Length];
            for (int i = 0; i < products.Length; i++)
                ids[i] = products[i].productId;
            return ids;
        }

        private int IndexOfProduct(string productId)
        {
            for (int i = 0; i < products.Length; i++)
                if (products[i].productId == productId)
                    return i;
            return -1;
        }

#endif

        // ── Helpers ─────────────────────────────────────────────────────────────────

        private void EndBusy()
        {
            _busy = false;
            SetButtonsInteractable(true);
            SetLoading(false);
        }

        private void SetStatus(string msg, Color color)
        {
            if (statusText == null) return;
            statusText.text = msg;
            statusText.color = color;
        }

        private void SetButtonsInteractable(bool on)
        {
            foreach (var p in products)
                if (p.buyButton != null) p.buyButton.interactable = on;
        }

        private void SetLoading(bool on)
        {
            if (loadingOverlay != null) loadingOverlay.SetActive(on);
        }

        private static string FormatCoins(int amount) =>
            PersianTextUtils.FormatNumber(amount);
    }
}
