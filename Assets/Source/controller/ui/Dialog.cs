using UnityEngine;
using System.Collections;
using System.Linq;
using System;
using System.Collections.Generic;

namespace com.perroelectrico.flip.controller.ui {

    public class Dialog : MonoBehaviour {

        public class DialogButton {
            public string id;
            public string text;
            public UIButtonsController.ButtonEvent onClick;
        }

        private AnimationCurve fadeCurve;
        private float currentFadeTime = 0;
        public float fadeTime = 1f;
        public float ButtonSeparation = 0.8f;
        public float ButtonPadding = 0.2f;
        public float backAlpha = 0.78f;

        private Dictionary<string, DialogButton> buttons = new Dictionary<string, DialogButton>();

        public TextMesh messageMesh;
        public GameObject mainArea;
        public GameObject buttonBase;
        public GameObject buttonModel;

        private Material backMaterial;
        private Color backColor;

        public event Action OnClose;
        public static Dialog Current { get; private set; } = null;
        public bool Cancellable { set; get; } = true;
        public bool Active { get; private set; } = false;

        private string message;
        public string Message {
            set {
                message = value;
                messageMesh.text = message;
            }
            get {
                return message;
            }
        }

        private static Bounds GetFullBounds(GameObject go) {
            var rnds = go.GetComponentsInChildren<MeshRenderer>();
            Vector3 min = new Vector3();
            Vector3 max = new Vector3();
            foreach (var rnd in rnds) {
                min.x = Math.Min(min.x, rnd.bounds.min.x);
                min.y = Math.Min(min.y, rnd.bounds.min.y);
                min.z = Math.Min(min.z, rnd.bounds.min.z);
                max.x = Math.Max(max.x, rnd.bounds.max.x);
                max.y = Math.Max(max.y, rnd.bounds.max.y);
                max.z = Math.Max(max.z, rnd.bounds.max.z);
            }
            var size = max - min;
            var center = min + (size / 2);
            return new Bounds(center, size);
        }

        public GameObject MainChild {
            set {
                foreach (Transform child in mainArea.transform) {
                    Destroy(child.gameObject);
                }
                var newChild = value;
                newChild.transform.parent = mainArea.transform;
                newChild.transform.localPosition = -GetFullBounds(newChild).extents + new Vector3(0, 0, -0.2f);
            }
            get {
                if (mainArea.transform.childCount > 0) {
                    return mainArea.transform.GetChild(0).gameObject;
                }
                return null;
            }
        }

        public static Dialog ShowDialog(string text, params DialogButton[] buttons) {
            var go = Instantiate(Resources.Load<GameObject>("Dialog")) as GameObject;
            go.name = "Dialog";
            go.transform.localPosition = new Vector3(0, 0, -2);
            var dc = go.GetComponent<Dialog>();
            dc.Message = text;
            foreach (var btn in buttons) {
                dc.AddButton(btn);
            }
            dc.Show();
            return dc;
        }

        public static Dialog ShowMessage(string text) {
            return ShowDialog(text, new DialogButton { id = "Dialog_Close", text = "Ok", onClick = null });
        }

        public void AddButton(DialogButton btn) {
            if (btn.onClick == null) {
                btn.onClick = (i) => { Close(); };
            }
            buttons.Add(btn.id, btn);
        }

        public void AddButton(string id, string text, UIButtonsController.ButtonEvent onClick) {
            AddButton(new DialogButton { id = id, text = text, onClick = onClick });
        }

        public void Show() {
            fadeCurve = AnimationCurve.EaseInOut(0, 0, fadeTime, backAlpha);

            float totalWidth = 0;
            Transform first = null;
            foreach (var button in buttons.Values) {
                var btnGO = Instantiate(buttonModel);
                btnGO.transform.parent = buttonBase.transform;

                var tm = btnGO.GetComponentInChildren<TextMesh>();
                tm.text = button.text;
                var bounds = tm.gameObject.GetComponent<MeshRenderer>().bounds;
                var width = Mathf.Max(1, bounds.size.x + ButtonPadding);

                var uib = btnGO.GetComponentInChildren<UIButtonsController>();
                uib.OnClick += button.onClick;
                uib.id = button.id;
                uib.transform.localScale = new Vector3(width, 1, 0.1f);

                var center = new Vector3(totalWidth + width/2, 0, -0.5f);
                // add the separation now, so the next button will be positioned correctly
                totalWidth += width + ButtonSeparation;
                btnGO.transform.localPosition = center;
                first = first ?? btnGO.transform;

            }
            // remove the last separation and center the whole button bar
            totalWidth -= ButtonSeparation;
            buttonBase.transform.localPosition += new Vector3(-totalWidth / 2, 0, 0);
            currentFadeTime = 0;
            backMaterial = gameObject.transform.Find("Back").GetComponent<MeshRenderer>().material;
            backColor = backMaterial.color;
            backColor.a = 0;
            SetBackColor();
            Active = true;
            Current = this;
        }

        public void Close() {
            if (!Active)
                return;

            Destroy(gameObject);
            Active = false;
            Current = null;

            OnClose?.Invoke();
        }

        void Update() {
            if (!Active)
                return;

            if (Input.GetKeyDown(KeyCode.Escape) && Cancellable) {
                Close();
                return;
            }
            if (currentFadeTime < fadeTime) {
                currentFadeTime += Time.deltaTime;
                backColor.a = fadeCurve.Evaluate(currentFadeTime);
                SetBackColor();
            }
        }

        private void SetBackColor() {
            foreach (var mr in gameObject.transform.GetComponentsInChildren<MeshRenderer>()) {
                if (mr.gameObject.name.Equals("Back")) {
                    mr.material.color = backColor;
                }
            }
        }
    }
}