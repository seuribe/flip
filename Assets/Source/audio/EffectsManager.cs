using com.perroelectrico.flip.controller;
using com.perroelectrico.flip.core;
using com.perroelectrico.flip.util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;

namespace com.perroelectrico.flip.audio {

    public class EffectsManager : MonoBehaviour {
        public enum AudioEvent {
            None,
            MenuOptionClick,
            MenuSwitch,
            GameFlip,
            GameResetMove,
            GameResetLevel,
            GameUndo,
            GameSolved,
            GameNoMove,
            AppStarted,
            GameStarted,
            Error,
            Stars,
            Hint,
            UndoButton,
            OptimallySolved
        }

        private const float TOTAL_SILENCE = 0;

        public AudioMixerGroup output;
        public AudioSource musicSource;
        public AudioSource bgLayerSource;

        private AudioClip[] bgLayers;
        public AudioSource effectsSource;
        public AudioBinding[] bindings;

        [Serializable]
        public struct TypeLayers {
            public Level.LevelType type;
            public AudioClip[] bgLayers;
        }

        public TypeLayers[] bgMusicLayers;

        public AudioMixerGroup swishMixer;
        public AudioSource swishSource;
        public float swishVolumeMult = 2f;
        public float swishMaxPan = 0.5f;
        public float swishMinPitch = 0.45f;
        public float swishMaxPitch = 0.85f;

        public float swishInertia = 0.75f;

        private List<AudioSource> soundsSources = new List<AudioSource>();

        private int currentBgLayer;
        private GameController controller;

        public float MusicVolume {
            set { SetChannelVolume("musicVolume", value); }
        }
        public float EffectsVolume {
            set { SetChannelVolume("effectsVolume", value); }
        }

        [Serializable]
        public class AudioBinding {
            public AudioEvent ev;
            public AudioClip clip;
        }
        public static EffectsManager Instance { get; private set; }

        private AudioClip GetClip(AudioEvent evt) {
            var potentialClips = bindings.Where(binding => binding.ev == evt)
                                         .Select(binding => binding.clip)
                                         .ToList();

            if (potentialClips.Count == 0)
                return null;

            // if more than one event matches, return a random one
            return potentialClips[UnityEngine.Random.Range(0, potentialClips.Count)];
        }

        public void Awake() {
            Instance = this;
            soundsSources.Add(effectsSource);
        }

        public void Start() {
            controller = FindObjectOfType<GameController>();

            if (controller != null) {

                if (controller != null) { // this will also get called in other scenes, so the GameController might not be there
                    controller.AngularVelocity += OnAngularVelocity;
                }

                foreach (var layer in bgMusicLayers) {
                    if (layer.type == controller.CurrentLevel.type) {
                        bgLayers = layer.bgLayers;
                        break;
                    }
                }
            }
            if (bgLayers != null && bgLayers.Length > 0) {
                PlayBGLayer(0);
            }

            FadeInMusic(null);
        }

        private float lastAv = 0;
        private void OnAngularVelocity(float av) {
            float newAv = Math.Abs(lastAv * swishInertia + av * (1 - swishInertia));
            swishSource.volume = Mathf.Min(newAv * swishVolumeMult, 1);

            float pan = (Input.mousePosition.x - Screen.width / 2) * swishMaxPan;
            swishSource.panStereo = pan;

            lastAv = newAv;
        }

        private void PlayBGLayer(int index) {
            currentBgLayer = index;
            var clip = bgLayers[index];
            bgLayerSource.clip = clip;
            bgLayerSource.Play();
            Invoke("NextBgLayer", clip.length);
        }

        void NextBgLayer() {
            PlayBGLayer(currentBgLayer == bgLayers.Length - 1 ? 0 : currentBgLayer + 1);
        }

        private AudioSource GetAvailableAudioSource() {
            foreach (var ss in soundsSources) {
                if (!ss.isPlaying) {
                    return ss;
                }
            }
            var newSource = effectsSource.gameObject.AddComponent<AudioSource>();
            newSource.outputAudioMixerGroup = output;
            newSource.rolloffMode = AudioRolloffMode.Linear;
            soundsSources.Add(newSource);
            return newSource;
        }

        public void PlayMusic(AudioClip clip) {
            musicSource.clip = clip;
            musicSource.Play();
        }

        public void PlayEvent(AudioEvent evt, float vol = 1f) {
            AudioClip clip = GetClip(evt);
            if (clip == null)
                return;

            var source = GetAvailableAudioSource();
            source.clip = clip;
            source.volume = vol;
            source.Play();
        }

        public void FadeOutMusic(Action onFadeOut) {
            StartCoroutine(Fade(false, onFadeOut));
        }
        public void FadeInMusic(Action onFadeOut) {
            StartCoroutine(Fade(true, onFadeOut));
        }

        IEnumerator Fade(bool fadeIn, Action onFadeEnd) {
            float volume = TOTAL_SILENCE;
            float targetVolume = 0;
            float mult = fadeIn ? 1 : -1;

            if (fadeIn) {
                targetVolume = Settings.Instance.MusicVolume;
                MusicVolume = 0;
            } else {
                volume = Settings.Instance.MusicVolume;
            }
            while ((fadeIn && volume < targetVolume) || (!fadeIn && volume > TOTAL_SILENCE)) {
                volume += Time.deltaTime * mult;
                MusicVolume = volume;
                yield return 0;
            }
            if (onFadeEnd != null) {
                onFadeEnd();
            }
        }

        public void SetChannelVolume(string channel, float value) {
            output.audioMixer.SetFloat(channel, (value * 100) - 80);
        }
    }
}