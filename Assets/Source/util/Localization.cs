using System;

namespace com.perroelectrico.flip.util {

    public class Localization {

        public struct Lang {
            public string id;
            public string name;
        }

        public const string ENGLISH = "english";
        public const string SPANISH = "spanish";
        public const string GERMAN = "german";
        public const string ITALIAN = "italian";
        public const string FRENCH = "french";
        public const string RUSSIAN = "russian";
        public const string BRASILIAN = "brazilian";

        public static readonly Lang[] LANG_DESC = new Lang[] {
            new Lang{ id = ENGLISH, name = "English"},
            new Lang{ id = SPANISH, name = "Español"},
            new Lang{ id = GERMAN, name = "Deutsch"},
            new Lang{ id = ITALIAN, name = "Italiano"},
            new Lang{ id = FRENCH, name = "Français"},
            new Lang{ id = RUSSIAN, name = "ру́сский язы́к"},
            new Lang{ id = BRASILIAN, name = "Português brasileiro"},
        };

        public static readonly string[] SUPPORTED_LANGS = new string[] { ENGLISH, SPANISH, GERMAN, ITALIAN, FRENCH, RUSSIAN, BRASILIAN };

        public const string DEFAULT_LANG = ENGLISH;

        public static bool IsSupportedLanguage(string currentLang) {
            return Array.IndexOf(SUPPORTED_LANGS, currentLang) >= 0;
        }
    }
}