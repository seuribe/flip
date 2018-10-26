using UnityEngine;
using com.perroelectrico.flip.core;
using com.perroelectrico.flip.db;
using com.perroelectrico.flip.util;

namespace com.perroelectrico.flip {
    public class EntryPoint : MonoBehaviour {

        void Start() {
            Debug.LogFormat("DB has been initialized: {0}", DBManager.Instance.IsInitialized());
            var currentLang = Settings.Instance.Language;

            if (!Settings.Instance.LanguageSet || !Localization.IsSupportedLanguage(currentLang)) {
                Settings.Instance.Language = Localization.DEFAULT_LANG;
            }
            BadgeManager.Instance.CheckLegacyErrors();

            Debug.LogFormat("Language: {0}", Settings.Instance.Language);
        }
    }
}
