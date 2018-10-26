using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using com.perroelectrico.flip.util;
using com.perroelectrico.flip.audio;


namespace com.perroelectrico.flip.core {

    class Settings {
        private static readonly string LANGUAGE_KEY = "language";
        private static readonly string MUSIC_KEY = "music";
        private static readonly string SOUND_KEY = "sound";
        private static readonly string TUTORIAL_DONE = "tutorialDone";
        private static readonly string HINTS = "hints";
        private static readonly string LAST_TYPE_VIEWED = "lastTypeViewed";
        private static readonly string LAST_LEVEL_PLAYED = "lastLevelPlayed";
        private static readonly string GAME_STATE = "gameState";

        private static Settings settings;
        private Settings() { }

        public static Settings Instance {
            get {
                return settings = settings ?? new Settings();
            }
        }

        public float MusicVolume {
            get { return PlayerPrefs.GetFloat(MUSIC_KEY, 0.75f); }
            set { PlayerPrefs.SetFloat(MUSIC_KEY, value); }
        }

        public float EffectsVolume {
            get { return PlayerPrefs.GetFloat(SOUND_KEY, 0.75f); }
            set { PlayerPrefs.SetFloat(SOUND_KEY, value); }
        }

        public bool Hints {
            get {
                return PlayerPrefs.GetInt(HINTS, 1) == 1;
            }
            set {
                PlayerPrefs.SetInt(HINTS, value ? 1 : 0);
            }
        }

        public bool TutorialDone {
            get {
                return PlayerPrefs.GetInt(TUTORIAL_DONE, 0) == 1;
            }
            set {
                PlayerPrefs.SetInt(TUTORIAL_DONE, value ? 1 : 0);
            }
        }

        public Level.LevelType LastTypeViewed {
            get {
                return (Level.LevelType)PlayerPrefs.GetInt(LAST_TYPE_VIEWED, (int)Level.LevelType.simple);
            }
            set {
                PlayerPrefs.SetInt(LAST_TYPE_VIEWED, (int)value);
            }
        }

        public Level.LevelRef LastLevelPlayed {
            get {
                var str = PlayerPrefs.GetString(LAST_LEVEL_PLAYED, null);
                if (str == null) {
                    return LevelManager.Instance.TutorialLevelRef;
                }
                return Level.LevelRef.FromCompactString(str);
            }
            set {
                PlayerPrefs.SetString(LAST_LEVEL_PLAYED, value.ToCompactString());
            }
        }

        public bool LanguageSet {
            get {
                return PlayerPrefs.HasKey(LANGUAGE_KEY);
            }
        }

        public string Language {
            get {
                return PlayerPrefs.GetString(LANGUAGE_KEY, Localization.DEFAULT_LANG);
            }
            set {
                PlayerPrefs.SetString(LANGUAGE_KEY, value);
            }
        }

        public GameState LastGameState {
            get {
                var str = PlayerPrefs.GetString(GAME_STATE);
                if (str == null || str.Equals("")) {
                    return new GameState();
                }
                return GameState.FromString(str);
            }
            set {
                PlayerPrefs.SetString(GAME_STATE, value.ToString());
            }
        }

        public void SwitchHints(bool newValue) {
            Hints = newValue;
        }

        internal void SetLanguage(string val) {
            Language = val;
            TextResource.ChangeLanguage(val);
        }

    }
}