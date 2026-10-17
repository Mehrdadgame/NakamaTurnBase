using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NinjaBattle.General
{
    public partial class AudioManager : MonoBehaviour
    {
        #region FIELDS

        private const float CooldownForSounds = 0.05f;
        public const float MusicVolume = 0.65f;
        public const float SoundVolume = 1f;

        private AudioSource musicChannel = null;
        private AudioSource musicChannelCrossFadeHelper = null;
        private AudioSource soundChannel = null;
        private List<AudioClip> currentSoundClips = new();
        private List<AudioClip> playingSoundClips = new();
        [SerializeField] private AudioClip defaultClickSound;

        #endregion

        #region PROPERTIES

        public static AudioManager Instance { get; private set; } = null;

        #endregion

        #region BEHAVIORS

        private void Awake()
        {

            Instance = this;
            musicChannel = gameObject.AddComponent<AudioSource>();
            musicChannelCrossFadeHelper = gameObject.AddComponent<AudioSource>();
            soundChannel = gameObject.AddComponent<AudioSource>();
            LoadVolume();
        }

        private void LoadVolume()
        {
            musicChannel.volume = MusicVolume;
            soundChannel.volume = SoundVolume;
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (clip == null) return;
            StopMusic();
            musicChannel.clip = clip;
            musicChannel.loop = loop;
            musicChannel.volume = 0.5f;
            musicChannel.Play();
        }

        public void StopMusic()
        {
            musicChannel.clip = null;
            musicChannel.Stop();
        }

        private AudioSource loopChannel = null;

        public void PlaySound(AudioClip clip)
        {
            PlaySound(clip, 1f, 1f);
        }

        public void PlaySound(AudioClip clip, float volumeScale, float pitch = 1f)
        {
            if (clip == null || currentSoundClips.Contains(clip))
                return;

            if (soundChannel != null)
            {
                soundChannel.pitch = pitch;
                soundChannel.PlayOneShot(clip, volumeScale);
            }
            currentSoundClips.Add(clip);
            StartCoroutine(SoundCooldown(clip, CooldownForSounds));
            StartCoroutine(Dispose(clip, clip.length));
        }

        public void PlaySoundRandomPitch(AudioClip clip, float minPitch = 0.94f, float maxPitch = 1.06f, float volumeScale = 1f)
        {
            PlaySound(clip, volumeScale, Random.Range(minPitch, maxPitch));
        }

        public void StartLoopSound(AudioClip clip, float volume = 0.7f, float pitch = 1f)
        {
            if (clip == null) return;
            if (loopChannel == null)
            {
                loopChannel = gameObject.AddComponent<AudioSource>();
                loopChannel.loop = true;
            }
            loopChannel.clip = clip;
            loopChannel.volume = volume;
            loopChannel.pitch = pitch;
            if (!loopChannel.isPlaying)
                loopChannel.Play();
        }

        public void StopLoopSound()
        {
            if (loopChannel != null && loopChannel.isPlaying)
            {
                loopChannel.Stop();
                loopChannel.clip = null;
            }
        }

        public bool IsMusicPlaying(AudioClip clip)
        {
            return clip != null && musicChannel != null &&
                   musicChannel.clip == clip && musicChannel.isPlaying;
        }

        public void PlayClickSound()
        {
            if (defaultClickSound == null)
            {
                // Legacy path first, then the casual clip set (the legacy Jump clip
                // was never inside a Resources folder, so clicks were silent).
                defaultClickSound = Resources.Load<AudioClip>("Audio/Sounds/Jump");
                if (defaultClickSound == null)
                    defaultClickSound = GameSfx.ClickClip;
            }
            if (defaultClickSound != null)
            {
                PlaySoundRandomPitch(defaultClickSound, 0.95f, 1.05f);
            }
        }

        public IEnumerator SoundCooldown(AudioClip clip, float cooldown)
        {
            yield return new WaitForSecondsRealtime(cooldown);
            currentSoundClips.Remove(clip);
        }

        public IEnumerator Dispose(AudioClip clip, float cooldown)
        {
            yield return new WaitForSecondsRealtime(cooldown);
            playingSoundClips.Remove(clip);
        }

        #endregion
    }
}
