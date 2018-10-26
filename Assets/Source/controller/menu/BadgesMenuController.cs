using com.perroelectrico.flip.controller.ui;
using com.perroelectrico.flip.core;
using System;
using UnityEngine;

namespace com.perroelectrico.flip.controller {

    public class BadgesMenuController : MonoBehaviour, IMenuController {

        public const string ID = "Badges";

        public float StartX = -3.5f;
        public float LastX = 3.5f;
        public float StartY = 0.5f;

        public float SeparationX = 0.75f;
        public float SeparationY = 1f;

        public GameObject badgePrefab;
        public GameObject badgesParent;
        public TextMesh description;

        public string Id() {
            return ID;
        }

        void Start() {
            Vector3 pos = new Vector3(StartX, StartY, 0);
            foreach (Badge badge in Enum.GetValues(typeof(Badge))) {
                var go = Instantiate(badgePrefab, badgesParent.transform);
                go.transform.localPosition = pos;
                var bc = go.GetComponentInChildren<BadgeController>();
                bc.Badge = badge;
                bc.description = description;
                pos.x += SeparationX;
                if (pos.x > LastX) {
                    pos.x = StartX;
                    pos.y -= SeparationY;
                }
            }
        }

        public void DoBeforeArrival() { }

        public void DoAfterArrival() { }

        public void DoOnLeaving() { }

        public void Back() {
            var mm = FindObjectOfType<MenuManager>() as MenuManager;
            mm.ShowMenu(MainMenuController.ID);
        }

        public void Execute(string cmd) { }
    }
}