using UnityEngine;
using System.Collections;

namespace com.perroelectrico.flip.controller.ui {

    public class ProgressBar : MonoBehaviour {

        public delegate void OnProgressSet(float newValue);

        public event OnProgressSet ProgressSet;

        public float Progress {
            set {
                GetComponent<MeshRenderer>().material.SetFloat("_Progress", value);
                if (ProgressSet != null) {
                    ProgressSet(value);
                }
            }
        }

        private bool allowInput = true;
        public bool AllowInput {
            get { return allowInput; }
            set {
                allowInput = value;
            }
        }

        void OnMouseOver() {
            if (allowInput && Input.GetButton("Fire1")) {
                var bounds = GetComponent<MeshRenderer>().bounds;
                var screenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.position.z - Camera.main.transform.position.z);
                var worldPos = Camera.main.ScreenToWorldPoint(screenPos);
                var firstX = bounds.center.x - bounds.extents.x;
                var lastX = bounds.center.x + bounds.extents.x;
                var percent = (worldPos.x - firstX) / (lastX - firstX);
                Progress = percent;
            }
        }
    }
}