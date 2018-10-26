using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using com.perroelectrico.flip.core;
using com.perroelectrico.flip.util;
using com.perroelectrico.flip.controller.ui;
using System;

namespace Assets.Source.controller.ui {
    public class LevelProgressDetail : MonoBehaviour {

        private const int UI_LAYER = 5;

        public LevelStateBar levelProgressBar;

        public Collider hoverCollider;
        public GameObject showObject;

        public TextMesh perfect;
        public TextMesh veryGood;
        public TextMesh ok;
        public TextMesh unsolved;

        public Vector3 hiddenPos = new Vector3(-0.6f, 0.5f, 0f);
        public Vector3 shownPos = new Vector3(-0.6f, -1, 0f);

//        private TextResource tr;

        public event Action OnHoverEnter;
        public event Action OnHoverExit;

        public void SetLevelStats(SolvedStats stats) {
            levelProgressBar.SetStats(stats);

            perfect.text = "" + stats.NumPerfect;
            veryGood.text = "" + stats.NumGood;
            ok.text = "" + (stats.NumOk + stats.NumPerfect + stats.NumGood);
            unsolved.text = "" + stats.NumUnsolved;
        }

        void Awake() {
//            tr = TextResource.Get("game.gui");
            showObject.transform.localPosition = hiddenPos;
        }

        private bool hover = false;
        void Update() {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            bool oldHover = hover;
            hover = hoverCollider.bounds.IntersectRay(ray);

            if (hover && !oldHover) {
                if (OnHoverEnter != null) {
                    OnHoverEnter();
                }
            } else if (!hover && oldHover) {
                if (OnHoverExit != null) {
                    OnHoverExit();
                }
            }

            // TODO: make this time-based, and not dependant only on the lerp factor -- Seu
            if (hover && Vector3.Distance(showObject.transform.localPosition, shownPos) > 0.001f) {
                showObject.transform.localPosition = Vector3.Lerp(showObject.transform.localPosition, shownPos, 0.20f);
            } else if (!hover && Vector3.Distance(showObject.transform.localPosition, hiddenPos) > 0.001f) {
                showObject.transform.localPosition = Vector3.Lerp(showObject.transform.localPosition, hiddenPos, 0.20f);
            }
        }

    }

}
