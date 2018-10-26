using UnityEngine;
using System.Collections;
using com.perroelectrico.flip.controller.ui;

namespace com.perroelectrico.flip.controller {

    public class VisibilitySwitch : MonoBehaviour {

        public delegate void VSEvent(bool newValue);
        public event VSEvent VisibilityChanged;

        public GameObject target;
        public bool visible = true;

        public bool Visible {
            get {
                return visible;
            }
            set {
                visible = value;
                SetVisibility(target, value);
                if (VisibilityChanged != null) {
                    VisibilityChanged(visible);
                }
            }
        }

        void Start() {
            Visible = false;
        }

        void OnMouseDown() {
            Visible = !Visible;
        }

        /// <summary>
        /// Sets visibility for all MeshRenderers in children of this object
        /// </summary>
        /// <param name="go"></param>
        /// <param name="visible"></param>
        public static void SetVisibility(GameObject go, bool visible) {
            foreach (var r in go.GetComponentsInChildren<MeshRenderer>()) {
                r.enabled = visible;
            }
            foreach (var b in go.GetComponentsInChildren<ButtonController>()) {
                if (visible) {
                    b.Show();
                } else {
                    b.Hide();
                }
            }
        }
    }
}