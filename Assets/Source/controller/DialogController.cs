using UnityEngine;
using com.perroelectrico.flip.util;
using com.perroelectrico.flip.controller.ui;

namespace com.perroelectrico.flip.controller {

    // TODO: remove, not used anymore -- Seu
    public class DialogManager : MonoBehaviour {
        public TextMesh textMesh;
        public UIButtonsController closeButton;

        public string text;
        public string id;
        public float displayTime = 4f;
        public float delayTime = 0f;

        private bool closed = false;

        private static DialogManager current;

        public string Text {
            set {
                textMesh.text = value;
            }
        }

        void Awake() {
            gameObject.name = id;
            if (textMesh != null) {
                textMesh.text = text;
            }
            if (closeButton != null) {
                closeButton.OnClick += OnClose;
            }
        }

        private void OnClose(string id) {
            CloseNow();
        }

        public static bool IsDialogOpen() {
            return current != null;
        }

        public static void Close() {
            if (current != null) {
                current.CloseNow();
            }
        }

        public static void ShowPrefab(GameObject dialogPrefab) {
            var go = Instantiate(dialogPrefab) as GameObject;
            var dc = go.GetComponent<DialogManager>();
            if (dc != null) {
                dc.Show();
            } else {
                Debug.LogWarning("Dialog Prefab " + dialogPrefab + " has no DialogController component");
            }
        }

        public static void Show(DialogManager dialog) {
            dialog.Show();
        }

        public static void ShowMessageId(string id, float displayTime = 0, float delayTime = 0) {
            var tr = TextResource.Get("dialog").Filter("dialog");
            ShowMessage(tr[id], displayTime, delayTime);
        }

        public static void ShowMessage(string text, float displayTime, float delayTime) {
            var go = Instantiate(Resources.Load<GameObject>("MessageDialog")) as GameObject;
            go.name = "MessageDialog";
            go.transform.localPosition = new Vector3(0, 0, -1);
            var dc = go.GetComponent<DialogManager>();
            dc.displayTime = displayTime;
            dc.delayTime = delayTime;
            dc.Show(text);
        }

        private void Show(string text = null) {
            if (text != null && textMesh != null) {
                textMesh.text = text;
            }
            if (delayTime > 0) {
                Invoke("ShowNow", delayTime);
            } else {
                ShowNow();
            }

            if (displayTime > 0) {
                Invoke("CloseNow", displayTime + delayTime);
            }
        }

        private void ShowNow() {
            Debug.Log("ShowNow, closed: " + closed);
            if (closed)
                return;

            if (current != null) {
                current.CloseNow();
            }
            current = this;

            VisibilitySwitch.SetVisibility(gameObject, true);
        }

        public void CloseNow() {
            Debug.LogFormat("CloseNow, was closed?: {0}", closed);
            if (closed)
                return;

            closed = true;
            if (current == this) {
                current = null;
            }
            CancelInvoke("CloseNow");

            VisibilitySwitch.SetVisibility(gameObject, false);
            Destroy(gameObject);
        }
    }
}