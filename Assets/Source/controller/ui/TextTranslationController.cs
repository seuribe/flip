using UnityEngine;
using com.perroelectrico.flip.util;

namespace com.perroelectrico.flip.controller.ui {

    /// <summary>
    /// Will keep a textmesh text updated through language changes
    /// </summary>
    public class TextTranslationController : MonoBehaviour {

        public string resourceFileName;
        public string resourceFilter = null;
        public string resourceId;

        private TextMesh text;
        private TextResource tr;

        public string ResourceId {
            set {
                resourceId = value;
                ChangeText(null);
            }
        }

        void Awake() {
//            Debug.LogFormat("TextTranslationController {0}/{1}/{2}", resourceFileName, resourceFilter, resourceId);
            text = gameObject.GetComponent<TextMesh>();
            tr = TextResource.Get(resourceFileName, resourceFilter);
            tr.OnResourceChanged += ChangeText;
        }

        void Start() {
            ChangeText(null);
        }

        void ChangeText(string newLang) {
            var newText = tr[resourceId];
            if (newText == null || newText.Trim().Length == 0) {
                Debug.LogWarningFormat("Null or empty string: {0}/{1}/{2} ({3})", resourceFileName, resourceFilter, resourceId, newLang);
            } else {
                text.text = newText;
            }
        }

        void OnDestroy() {
            tr.OnResourceChanged -= ChangeText;
        }
    }
}