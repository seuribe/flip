using UnityEngine;

namespace com.perroelectrico.flip.controller.ui {

    [RequireComponent(typeof(Animator))]
    public class ButtonController : MonoBehaviour {

        private Animator animator;

        public delegate void ButtonEvent();
        public event ButtonEvent OnClick;

        public string Id {
            set {
                foreach (var buttonController in GetComponentsInChildren<UIButtonsController>()) {
                    buttonController.id = value;
                }
            }
        }

        public string Text {
            get { return GetComponentInChildren<TextMesh>().text; }
            set { GetComponentInChildren<TextMesh>().text = value; }
        }

        void Start() {
            animator = gameObject.GetComponent<Animator>();
        }

        public void Click() {
            animator.SetTrigger("Click");
            OnClick?.Invoke();
        }

        public void Hide() {
            animator = gameObject.GetComponent<Animator>();
            if (animator != null)
                animator.SetTrigger("Hide");

            SetChildrenEnabled(false);
        }

        public void Blink() {
            if (animator != null)
                animator.SetTrigger("Blink");

            SetChildrenEnabled(true);
        }

        public void Show() {
            if (animator != null)
                animator.SetTrigger("Show");

            SetChildrenEnabled(true);
        }

        private void SetChildrenEnabled(bool state) {
            foreach (var uibc in GetComponentsInChildren<UIButtonsController>())
                uibc.enabled = state;
            foreach (var mc in GetComponentsInChildren<MeshCollider>())
                mc.enabled = state;
            foreach (var mc in GetComponentsInChildren<BoxCollider2D>())
                mc.enabled = state;
        }
    }
}