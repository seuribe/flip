using UnityEngine;
using System.Collections;
using System.Linq;
using System;
using com.perroelectrico.flip.util;

namespace com.perroelectrico.flip.controller {

    public class MultiOptionSwitch : MonoBehaviour, MultiLangComponent {

        public delegate void ValueChanged(string newValue);
        public event ValueChanged OnValueChanged;

        [Serializable]
        public class Option {
            public string display;
            public string value;
        }

        public Option[] options;

        public string baseText;
        public TextMesh text;

        private IterableArray<Option> iterOptions;

        // Use this for initialization
        void Start() {
            iterOptions = new IterableArray<Option>(options, true);
            RegenText();
        }

        public void SetTexts(TextResource tr) {
            baseText = tr["Base"];
            RegenText();
        }

        private void RegenText() {
            text.text = string.Format("{0}: {1}", baseText, options[iterOptions.Index].display);
        }

        public void Set(string value) {
            var vals = options.Select((opt) => {return opt.value; }).ToArray();
            iterOptions.Index = Array.IndexOf(vals, value);
            RegenText();
        }

        void OnMouseDown() {
            var option = iterOptions.Next;
            RegenText();
            if (OnValueChanged != null) {
                OnValueChanged(option.value);
            }
        }
    }
}