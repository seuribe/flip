using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using com.perroelectrico.flip.core;

namespace com.perroelectrico.flip.util {
    public class SetupAudio : MonoBehaviour {

        public AudioMixer mixer;

        void Awake() {
            mixer.SetFloat("musicVolume", (Settings.Instance.MusicVolume * 100) - 80);
            mixer.SetFloat("effectsVolume", (Settings.Instance.EffectsVolume * 100) - 80);
        }
    }
}
