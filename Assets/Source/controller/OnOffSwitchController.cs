using UnityEngine;
using com.perroelectrico.flip.util;

namespace com.perroelectrico.flip.controller {

    public class OnOffSwitchController : MonoBehaviour, MultiLangComponent {

        public delegate void ValueChanged(bool newValue);
        public event ValueChanged OnValueChanged;

        private static readonly float AngleOn = 0f;
        private static readonly float AngleOff = 180f;

        private GameObject onObj;
        private GameObject offObj;

        public bool state = false;
        public AnimationCurve curveOn = AnimationCurve.EaseInOut(0.0f, AngleOn, 1.0f, AngleOff);
        public AnimationCurve curveOff = AnimationCurve.EaseInOut(0.0f, AngleOff, 1.0f, AngleOn);
        private AnimationCurve activeCurve;
        private bool rotating = false;
        private bool halfSwitch;
        float elapsed = 0;

        public void SetTexts(TextResource tr) {
            onObj.GetComponent<TextMesh>().text = tr["On"];
            offObj.GetComponent<TextMesh>().text = tr["Off"];
        }

        void Start() {
            onObj = gameObject.Child("On");
            offObj = gameObject.Child("Off");
        }

        public void OnMouseDown() {
            elapsed = 0;
            activeCurve = state ? curveOn : curveOff;
            state = !state;
            rotating = true;
            halfSwitch = false;
        }

        public void Set(bool newValue) {
            if (state == newValue) {
                return;
            }
            elapsed = 1;
            activeCurve = state ? curveOn : curveOff;
            state = newValue;
            rotating = true;
            halfSwitch = false;
        }

        void Update() {
            if (rotating) {
                elapsed += Time.deltaTime;
                if (!halfSwitch && elapsed >= 0.5f) {
                    onObj.GetComponent<Renderer>().enabled = state;
                    offObj.GetComponent<Renderer>().enabled = !state;
                    halfSwitch = true;
                }
                if (elapsed >= 1) {
                    elapsed = 1;
                    rotating = false;
                    OnValueChanged?.Invoke(state);
                }
                float val = activeCurve.Evaluate(elapsed);
                gameObject.transform.localRotation = Quaternion.Euler(0, val, 0);
            }
        }
    }
}