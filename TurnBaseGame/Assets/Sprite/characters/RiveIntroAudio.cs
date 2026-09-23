using UnityEngine;

/// <summary>
/// این اسکریپت صدای شیپور شاهانه را در لحظه شلیک شیپورها (فریم 142 معادل ثانیه 2.36) در یونیتی پخش می‌کند.
/// کافیست این اسکریپت را به همان GameObject دارای RiveWidget یا یک شیء در صحنه اضافه کنید.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class RiveIntroAudio : MonoBehaviour
{
    [Header("فایل صوتی شیپور (fanfare_trumpet.wav)")]
    public AudioClip fanfareClip;

    [Header("تاخیر شلیک شیپورها به ثانیه (فریم 142 در 60fps = 2.36s)")]
    public float blastDelaySeconds = 2.36f;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        if (fanfareClip != null)
        {
            // پخش صدا دقیقاً در لحظه فوران شیپورها و پارتیکل‌ها
            audioSource.clip = fanfareClip;
            audioSource.PlayDelayed(blastDelaySeconds);
        }
        else
        {
            Debug.LogWarning("لطفاً فایل صوتی fanfare_trumpet را در فیلد Fanfare Clip در اینسپکتور قرار دهید.");
        }
    }
}
