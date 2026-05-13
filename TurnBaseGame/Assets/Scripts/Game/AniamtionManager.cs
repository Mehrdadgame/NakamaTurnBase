using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class AniamtionManager : MonoBehaviour
{
    public static AniamtionManager instance;

    public Image PageMatchMaking;
    public Animator AnimGoToUpMe;
    public Animator AnimGoToUpOpp;
    public RectTransform IconMe;
    public RectTransform IconOpp;

    [Header("Avatar Images (assign the Image on each icon GameObject)")]
    public Image AvatarImageMe;
    public Image AvatarImageOpp;

    [Header("Matchmaking — Sprite Roulette (lootbox style)")]
    [Tooltip("لیست sprite ها که مثل lootbox سریع cycle بشن (مثلاً همه آواتارها)")]
    [SerializeField] private List<Sprite> rouletteSprites = new List<Sprite>();
    [Tooltip("هر چند ثانیه یک sprite عوض بشه (کمتر = سریع‌تر)")]
    [SerializeField] private float spriteSwapInterval = 0.08f;

    [Header("Matchmaking — Pulse (DOTween)")]
    [SerializeField] private float pulseScale    = 1.12f;
    [SerializeField] private float pulseDuration = 0.4f;

    private Sequence  _pulseTween;
    private Coroutine _rouletteCo;

    void Awake()
    {
        instance = this;
        ApplyMyAvatarFromLocal();
        StartMatchmakingAnimation();
    }

    /// از همون اول صحنه‌ی گیم پلی، آواتار خودم رو از داده لوکال ست کن
    private void ApplyMyAvatarFromLocal()
    {
        if (AvatarImageMe == null) return;
        var ps = Nakama.Helpers.ProfileService.Instance;
        if (ps == null || ps.AvatarLibrary == null) return;

        string myAvatarId = !string.IsNullOrEmpty(ps.CurrentAvatarId) ? ps.CurrentAvatarId : "avatar_0";
        var sprite = ps.AvatarLibrary.GetSprite(myAvatarId);
        if (sprite != null)
        {
            AvatarImageMe.sprite = sprite;
            Debug.Log("[AniamtionManager] AvatarImageMe set on scene start: " + myAvatarId);
        }
    }

    public void StartMatchmakingAnimation()
    {
        var img = AvatarImageOpp;
        if (img == null)
        {
            Debug.LogWarning("[AniamtionManager] AvatarImageOpp در Inspector assign نشده!");
            return;
        }

        // Animator موجود رو غیرفعال کن — وگرنه sprite رو override میکنه
        var animOnImg = img.GetComponent<Animator>();
        if (animOnImg != null) animOnImg.enabled = false;
        // Animator روی parent (مثل AnimGoToUpOpp) هم اگه هست غیرفعال کن
        var animOnParent = img.GetComponentInParent<Animator>();
        if (animOnParent != null && animOnParent != animOnImg) animOnParent.enabled = false;

        Debug.Log("[AniamtionManager] Matchmaking started. roulette sprites=" +
                  (rouletteSprites != null ? rouletteSprites.Count : 0) +
                  " img=" + img.name);

        // Reset
        img.transform.localScale    = Vector3.one;
        img.transform.localRotation = Quaternion.identity;

        // Sprite roulette
        if (_rouletteCo != null) StopCoroutine(_rouletteCo);
        if (rouletteSprites != null && rouletteSprites.Count > 0)
        {
            _rouletteCo = StartCoroutine(SpriteRouletteLoop(img));
        }
        else
        {
            Debug.LogWarning("[AniamtionManager] Roulette Sprites خالیه — فقط pulse اجرا میشه. " +
                             "در Inspector لیست sprite ها رو پر کن.");
        }

        // Pulse (همیشه اجرا میشه حتی اگه روولت نباشه)
        _pulseTween?.Kill();
        _pulseTween = DOTween.Sequence()
            .Append(img.transform.DOScale(pulseScale, pulseDuration).SetEase(Ease.InOutSine))
            .Append(img.transform.DOScale(1f,         pulseDuration).SetEase(Ease.InOutSine))
            .SetLoops(-1);
    }

    public void StopMatchmakingAnimation()
    {
        if (_rouletteCo != null) { StopCoroutine(_rouletteCo); _rouletteCo = null; }

        _pulseTween?.Kill();
        _pulseTween = null;

        if (AvatarImageOpp != null)
        {
            AvatarImageOpp.transform.localScale    = Vector3.one;
            AvatarImageOpp.transform.localRotation = Quaternion.identity;
        }
    }

    private IEnumerator SpriteRouletteLoop(Image img)
    {
        int i = 0;
        var wait = new WaitForSeconds(spriteSwapInterval);
        while (true)
        {
            img.sprite = rouletteSprites[i % rouletteSprites.Count];
            i++;
            yield return wait;
        }
    }
}
