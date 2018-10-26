using UnityEngine;
using System.Collections.Generic;
using com.perroelectrico.flip.util;
using com.perroelectrico.flip.core;
using com.perroelectrico.flip.controller.ui;

namespace com.perroelectrico.flip.controller {

    public class MenuManager : MonoBehaviour {

        private static string lastId = MainMenuController.ID;

        [Range(0.1f, 1f)]
        public float menuChangeTime = 0.5f; // seconds

        private Dictionary<string, IMenuController> menus = new Dictionary<string, IMenuController>();
        public GameObject menuWheel;

        private IMenuController current;
        private IMenuController next;
        private readonly AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private float elapsed = 0;
        private Quaternion targetRotation;
        private Quaternion currentRotation;

        public GameObject backButton;

        public IMenuController this[string id] => menus[id];
        public static MenuManager Current => FindObjectOfType<MenuManager>() as MenuManager;
        public bool Transitioning => current != next;

        void Awake() {
            // Force initialization from here
            var lm = LevelManager.Instance;
            Debug.Log("Initialized Level Manager: " + lm);

            menus.Add(MainMenuController.ID, FindObjectOfType<MainMenuController>());
            menus.Add(OptionsMenuController.ID, FindObjectOfType<OptionsMenuController>());
            menus.Add(StatsMenuController.ID, FindObjectOfType<StatsMenuController>());
            menus.Add(ExitMenuController.ID, FindObjectOfType<ExitMenuController>());
            menus.Add(StartMenuController.ID, FindObjectOfType<StartMenuController>());
            menus.Add(CreditsMenuController.ID, FindObjectOfType<CreditsMenuController>());
            menus.Add(BadgesMenuController.ID, FindObjectOfType<BadgesMenuController>());
            next = menus[MainMenuController.ID];
        }


        public bool IsValidMenuId(string id) {
            return menus.ContainsKey(id);
        }

        void Start() {
            ShowMenu(lastId, true);
            Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// true if menu exists and will be shown, false if it is not a valid menu id name
        /// </summary>
        /// <param name="menuId"></param>
        /// <param name="immediate"></param>
        /// <returns></returns>
        public bool ShowMenu(string menuId, bool immediate = false) {
            Tracking.Track(Tracking.MENU_GOTO + ":" + menuId);
            immediate = immediate || (current != null && current.Id().Equals(menuId));
            if (current != null) {
                current.DoOnLeaving();
            }
            if (!menus.TryGetValue(menuId, out next)) {
                return false;
            }
            next.DoBeforeArrival();
            var nextTrans = ((MonoBehaviour)next).transform;
            targetRotation = Quaternion.Inverse(nextTrans.localRotation);

            if (immediate) {
                menuWheel.transform.localRotation = targetRotation;
                Arrived();
            } else {
                currentRotation = menuWheel.transform.localRotation;
                elapsed = 0f;
            }
            return true;
        }

        private void Arrived() {
            if (next != null) {
                current = next;
            }
            if (current != null) {
                lastId = current.Id();
                current.DoAfterArrival();
            }
        }
        
        public void Back() {
            if (Transitioning) {
                return;
            }

            var dialog = FindObjectOfType<Dialog>();
            if (dialog != null) {
                dialog.Close();
                return;
            }
            current.Back();
        }

        public void Update() {
            if ((Application.platform == RuntimePlatform.Android && (Input.GetKey(KeyCode.Escape) || Input.GetKey(KeyCode.Menu))) ||
                Input.GetKeyDown(KeyCode.Escape)) {
                Back();
            }

            if (Transitioning) {
                elapsed += Time.deltaTime / menuChangeTime;
                if (elapsed >= 1) {
                    elapsed = 1;
                    Arrived();
                }
                menuWheel.transform.localRotation = Quaternion.Lerp(currentRotation, targetRotation, rotationCurve.Evaluate(elapsed));
            }
        }
    }
}