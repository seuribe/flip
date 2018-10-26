using com.perroelectrico.flip.controller.ui;
using com.perroelectrico.flip.util;
using UnityEngine;

namespace com.perroelectrico.flip.controller {

    public class MainMenuController : MonoBehaviour, IMenuController {

        public const string ID = "Main";

        public GameObject dialogPrefab;

        public string Id() {
            return ID;
        }

        public void DoBeforeArrival() { }

        public void DoAfterArrival() { }

        public void DoOnLeaving() { }

        public void Back() {
            ShowExitDialog();
        }

        public void ShowExitDialog() {
            var tr = TextResource.Get("dialog");
            Dialog.ShowDialog(tr["dialog.Exit"],
                new Dialog.DialogButton { text = tr["general.Yes"], id = "Dialog_Exit", onClick = (id) => {
                    Application.Quit();
                } },
                new Dialog.DialogButton { text = tr["general.Back"], id = "Back" }
                );
        }

        public void Execute(string cmd) {
            // if true, was a valid real menu
            if (cmd.Equals("Exit")) {
                ShowExitDialog();
            } else if (MenuManager.Current.ShowMenu(cmd)) {
                return;
            }
        }
    }
}