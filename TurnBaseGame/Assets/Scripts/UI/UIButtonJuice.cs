using DG.Tweening;
using NinjaBattle.General;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NinjaBattle.UI
{
    [DisallowMultipleComponent]
    public class UIButtonJuice : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [SerializeField] private float pressedScale = 0.93f;
        [SerializeField] private float pressDuration = 0.08f;
        [SerializeField] private float releaseDuration = 0.16f;
        [SerializeField] private bool playClickAudio = true;
        [SerializeField] private AudioClip customClickClip;

        private Vector3 _originalScale = Vector3.one;
        private Tween _scaleTween;
        private Selectable _selectable;

        private void Awake()
        {
            _originalScale = transform.localScale;
            _selectable = GetComponent<Selectable>();
        }

        private void OnEnable()
        {
            transform.localScale = _originalScale;
        }

        private void OnDisable()
        {
            _scaleTween?.Kill();
            transform.localScale = _originalScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_selectable != null && !_selectable.interactable)
                return;

            _scaleTween?.Kill();
            _scaleTween = transform.DOScale(_originalScale * pressedScale, pressDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _scaleTween?.Kill();
            _scaleTween = transform.DOScale(_originalScale, releaseDuration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!playClickAudio)
                return;

            if (customClickClip != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(customClickClip);
            }
            else if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayClickSound();
            }
        }

        public static UIButtonJuice Attach(GameObject go, float scale = 0.93f, AudioClip sound = null)
        {
            if (go == null) return null;
            UIButtonJuice juice = go.GetComponent<UIButtonJuice>();
            if (juice == null)
                juice = go.AddComponent<UIButtonJuice>();

            juice.pressedScale = scale;
            juice.customClickClip = sound;
            return juice;
        }
    }
}
