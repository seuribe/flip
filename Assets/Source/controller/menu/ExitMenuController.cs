using UnityEngine;
using System.Collections;

namespace com.perroelectrico.flip.controller {

    public class ExitMenuController : MonoBehaviour, IMenuController {

        public const string ID = "Exit";

        public string Id() {
            return ID;
        }

        public void DoBeforeArrival() {}
        public void DoAfterArrival() {}
        public void DoOnLeaving() {}

        private static IEnumerator Exit() {
            yield return new WaitForSeconds(1f);
            Application.Quit();
        }

        public void Back() {
            var mm = GameObject.FindObjectOfType<MenuManager>() as MenuManager;
            mm.ShowMenu(MainMenuController.ID);
        }

        public void Execute(string cmd) {
            switch (cmd) {
                case "Exit":
                    Application.Quit();
                    break;
            }
        }
    }
}