using com.perroelectrico.flip.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.perroelectrico.flip.util {

    public interface MultiLangComponent {
        void SetTexts(TextResource tr);
    }

    public class TextResource {
        private static readonly string BASE_PATH = "Texts/";
        private static string currentLanguage = null;

        private static Dictionary<string, TextResource> cache = new Dictionary<string, TextResource>();

        public delegate void LanguageChanged(string newLanguage);
        public static event LanguageChanged OnLanguageChanged;
        public static void ChangeLanguage(string newLanguage) {
            Debug.LogFormat("ChangeLanguage {0}", newLanguage);
            foreach (var tr in cache.Values) {
                tr.Load(newLanguage);
                if (tr.OnResourceChanged != null) {
                    tr.OnResourceChanged(newLanguage);
                }
            }
            currentLanguage = newLanguage;
        }

        public event LanguageChanged OnResourceChanged;

        private static TextResource GetBase(string resName) {
            lock (cache) {
                resName = "res";
                TextResource tr;
                if (!cache.TryGetValue(resName, out tr)) {
                    tr = new TextResource(resName);
                    cache.Add(resName, tr);
                }
                return tr;
            }
        }

        public static TextResource Get(string resName, string filter = null) {
            lock (cache) {
                if (currentLanguage == null) {
                    currentLanguage = Settings.Instance.Language;
                }
                var tr = GetBase(resName);
                if (filter != null && !filter.Equals("")) {
                    tr = tr.Filter(filter);
                }
                return tr;
            }
        }

        private string filter;
        private string filename;
        private string language;
        private Dictionary<string, string> texts;

        public string this[string key] {
            get {
                var text = "";
                texts.TryGetValue(key, out text);
                return text;
            }
        }

        public IEnumerable<string> Keys {
            get {
                return texts.Keys;
            }
        }

        private TextResource(string name) {
            this.filename = name;
            this.filter = null;
            Load(currentLanguage ?? Settings.Instance.Language);
            OnLanguageChanged += Load;
        }

        private void Load(string lang) {
            if (lang != null && lang.Equals(language)) {
                return;
            }
            Debug.LogFormat("Load language {0}", lang);
            var assetFile = string.Format("{0}{1}.{2}", BASE_PATH, filename, lang);
            var asset = Resources.Load<TextAsset>(assetFile);

            Debug.LogFormat("Loading {0}", assetFile);
            texts = new Dictionary<string, string>();
            var lines = asset.text.Split('\n');

            foreach (var line in lines) {
                if (line.Length == 0) {
                    continue;
                }
                var splitPoint = line.IndexOf(':');
                if (splitPoint == -1) {
                    Debug.LogWarningFormat("Incomplete or invalid text resource '{0}'", line);
                    break;
                }
                var key = line.Substring(0, splitPoint);
                var value = line.Substring(splitPoint + 1).Replace("||", "\n");
                texts.Add(key, value);
            }

            if (OnResourceChanged != null) {
                OnResourceChanged(language);
            }
            language = lang;
        }

        private TextResource(TextResource parent, string keyFilter) {
            this.filename = parent.filename;
            this.language = parent.language;
            this.filter = (parent.filter == null ? "" : (parent.filter + ".")) + keyFilter;
            texts = new Dictionary<string, string>();
            keyFilter = keyFilter + ".";
            foreach (var key in parent.texts.Keys) {
                if (key.StartsWith(keyFilter)) {
                    texts.Add(key.Substring(keyFilter.Length), parent.texts[key]);
                }
            }
        }

        public TextResource Filter(string key) {
            return new TextResource(this, key);
        }

        public override string ToString() {
            return filename + "." + language + ":" + filter;
        }
    }
}