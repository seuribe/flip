using UnityEngine;
using System.Collections;
using System.Linq;
using System;
using System.Collections.Generic;
using com.perroelectrico.flip.core;
using com.perroelectrico.flip.audio;

namespace com.perroelectrico.flip.controller.ui {

    public class AudioOptionsController : MonoBehaviour {
        public ProgressBar musicVolumeBar;
        public ProgressBar effectsVolumeBar;

        void Awake() {

            musicVolumeBar.ProgressSet += (vol) => {
                Settings.Instance.MusicVolume = vol;
                EffectsManager.Instance.MusicVolume = vol;
            };
            effectsVolumeBar.ProgressSet += (vol) => {
                Settings.Instance.EffectsVolume = vol;
                EffectsManager.Instance.EffectsVolume = vol;
            };
        }

        void Start() {
            musicVolumeBar.Progress = Settings.Instance.MusicVolume;
            effectsVolumeBar.Progress = Settings.Instance.EffectsVolume;
        }
    }

}