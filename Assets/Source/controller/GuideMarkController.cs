using UnityEngine;

namespace com.perroelectrico.flip.controller {
    public class GuideMarkController : MonoBehaviour {

        private int index = 0;
        private Material mat;
        private MeshRenderer mr;
        private float startTime = 0;

        public float initialSpeed = 8;

        public float Speed { get; set; } = 8;

        public void Reset() {
            Speed = initialSpeed;
        }

        void Awake() {
            for (int i = 0 ; i < transform.parent.childCount ; i++) {
                if (transform.parent.GetChild(i) == transform) {
                    index = i;
                    break;
                }
            }
            mr = GetComponent<MeshRenderer>();
            mat = mr.material;
        }

        public void Show() {
            startTime = Time.time;
        }

        // Update is called once per frame
        void Update() {
            float timeStep = (Time.time - startTime) * Speed - (index / 2);
            float alpha = Mathf.Clamp01(1.8f * (Mathf.Cos(timeStep*2f/3f) - 0.5f)) * 0.85f;

            Color c = mat.color;
            c.a = alpha;
            mat.color = c;
        }
    }

}
