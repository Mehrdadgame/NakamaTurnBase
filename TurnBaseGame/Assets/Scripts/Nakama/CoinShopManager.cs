using System;
using System.Collections;
using Bazaar.Data;
using Bazaar.Poolakey;
using Bazaar.Poolakey.Data;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nakama.Helpers
{
    [Serializable]
    public class CoinPackProduct
    {
        public string productId;
        public int coinsAmount;
        public string priceLabel;   // e.g. "۱۰,۰۰۰ تومان" — set in Inspector
        public Button buyButton;
        public RTLTextMeshPro coinsLabel;
        public RTLTextMeshPro priceText;
    }

    [Serializable]
    internal class CoinPurchasePayload
    {
        public string productId;
        public string purchaseToken;
        public string orderId;
        public string dataSignature;
        public string originalJson;
    }

    [Serializable]
    internal class CoinPurchaseResult
    {
        public bool success;
        public int coinsAwarded;
        public string error;
    }

    public class CoinShopManager : MonoBehaviour
    {
        private const string RsaPublicKey = "MIHNMA0GCSqGSIb3DQEBAQUAA4G7ADCBtwKBrwDSc64jaD0G9XdtMtV7UbKAEL6IAdcCV4NhqwzcwlvuD8nx8+jbAQh0uYUsNBRq/wIKQHw4tm6mvxlw13FPTUZRs7V2PEOlcws6PBSjkvJtBHM532WqT4qRcAN18QXP1VFSen2dRhRUgriTmSba1RzgYAIbXibRaYw5eHa51v6Xu4DL4CM0PYrjVMaznGC6Taf8FSYol7WhKxwNOP/Icb5iMcXGYAq/bd650f0iwbECAwEAAQ==";
        private const string VerifyRpcId = "VerifyCoinPurchaseRpc";

        [Header("Products (assign in Inspector)")]
        [SerializeField] private CoinPackProduct[] products;

        [Header("Feedback")]
        [SerializeField] private RTLTextMeshPro statusText;
        [SerializeField] private GameObject loadingOverlay;

        private Payment _payment;
        private bool _connected;
        private bool _busy;

        // ── Lifecycle ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            var securityCheck = SecurityCheck.Enable(RsaPublicKey);
            _payment = new Payment(new PaymentConfiguration(securityCheck));

            for (int i = 0; i < products.Length; i++)
            {
                int idx = i;
                if (products[idx].buyButton != null)
                    products[idx].buyButton.onClick.AddListener(() => OnBuyClicked(idx));

                if (products[idx].coinsLabel != null)
                    products[idx].coinsLabel.text = FormatCoins(products[idx].coinsAmount);

                if (products[idx].priceText != null && !string.IsNullOrEmpty(products[idx].priceLabel))
                    products[idx].priceText.text = products[idx].priceLabel;
            }
        }

        private IEnumerator Start()
        {
            SetStatus("در حال اتصال...", Color.white);
            yield return _payment.Connect(OnConnectResult);
        }

        private void OnDestroy()
        {
            _payment?.Disconnect();
        }

        private void OnEnable()
        {
            SetStatus("", Color.white);

        }

        // ── Connect ───────────────────────────────────────────────────────────────

        private void OnConnectResult(Result<bool> result)
        {
            if (result.status == Status.Success)
            {
                _connected = true;
                SetStatus("", Color.white);
                SetButtonsInteractable(true);
            }
            else
            {
                SetStatus("اتصال به بازار ناموفق بود.", Color.red);
                Debug.LogWarning("[CoinShop] Connect failed: " + result.message);
            }
        }

        // ── Buy ───────────────────────────────────────────────────────────────────

        private async void OnBuyClicked(int index)
        {
            if (!_connected)
            {
                SetStatus("اتصال به بازار برقرار نیست.", Color.red);
                return;
            }
            if (_busy) return;

            var product = products[index];
            _busy = true;
            SetButtonsInteractable(false);
            SetStatus("در حال خرید...", Color.white);
            SetLoading(true);

            try
            {
                var result = await _payment.Purchase(product.productId, SKUDetails.Type.inApp);

                if (result.status != Status.Success)
                {
                    if (result.status == Status.Canceled)
                        SetStatus("خرید لغو شد.", Color.yellow);
                    else
                        SetStatus("خرید ناموفق: " + result.message, Color.red);
                    return;
                }

                var info = result.data;
                SetStatus("در حال تایید...", Color.white);

                bool serverOk = await VerifyWithServer(info, product.coinsAmount);
                if (!serverOk) return;

                // Consume so user can purchase this product again (consumable / type 7)
                var consumeResult = await _payment.Consume(info.purchaseToken);
                if (consumeResult.status != Status.Success)
                    Debug.LogWarning("[CoinShop] Consume failed (non-critical): " + consumeResult.message);

                SetStatus("+" + FormatCoins(product.coinsAmount) + " کوین دریافت شد!", new Color(0.25f, 1f, 0.25f));

                if (WalletManager.Instance != null)
                    await WalletManager.Instance.RefreshAsync();
            }
            catch (Exception e)
            {
                Debug.LogWarning("[CoinShop] Purchase error: " + e.Message);
                SetStatus("خطا: " + e.Message, Color.red);
            }
            finally
            {
                _busy = false;
                SetButtonsInteractable(true);
                SetLoading(false);
            }
        }

        private async System.Threading.Tasks.Task<bool> VerifyWithServer(PurchaseInfo info, int expectedCoins)
        {
            try
            {
                var payload = JsonUtility.ToJson(new CoinPurchasePayload
                {
                    productId = info.productId,
                    purchaseToken = info.purchaseToken,
                    orderId = info.orderId,
                    dataSignature = info.dataSignature,
                    originalJson = info.originalJson,
                });

                var rpc = await NakamaManager.Instance.SendRPC(VerifyRpcId, payload);
                if (rpc == null || string.IsNullOrEmpty(rpc.Payload))
                {
                    SetStatus("خطا: پاسخی از سرور دریافت نشد.", Color.red);
                    return false;
                }

                var res = rpc.Payload.Deserialize<CoinPurchaseResult>();
                if (res == null || !res.success)
                {
                    string err = res?.error ?? "unknown";
                    // "Already processed" means duplicate — treat as success (coins were granted before)
                    if (err == "Already processed")
                        return true;
                    SetStatus("خطا: " + err, Color.red);
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[CoinShop] Server verify error: " + e.Message);
                SetStatus("خطای سرور: " + e.Message, Color.red);
                return false;
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

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
            amount >= 1000 ? (amount / 1000) + "K" : amount.ToString();
    }
}
