using UnityEngine;
using System.Linq;
using com.perroelectrico.flip.core;
using com.perroelectrico.flip.audio;
using UnityEngine.SceneManagement;

namespace com.perroelectrico.flip.controller {

    public class StartMenuController : MonoBehaviour, IMenuController {

        public const string ID = "Start";

        public GameObject loading;

        void Start() {
            VisibilitySwitch.SetVisibility(loading, false);
        }

        public string Id() {
            return ID;
        }

        public void DoBeforeArrival() { }

        public void Back() {
            // if showing dialog...
            if (LevelTypeBubble.typeDialog != null) {
                LevelTypeBubble.CloseTypeDialog();
            } else {
                var mm = FindObjectOfType<MenuManager>() as MenuManager;
                mm.ShowMenu(MainMenuController.ID);
            }
        }

        public void DoAfterArrival() {
            if (GameController.startLevel != null) {
                var type = GameController.startLevel.type;
                var typeBubble = FindObjectsOfType<LevelTypeBubble>().ToList().Find((b) => (b.type == type));
                typeBubble.OpenTypeDialog(true);
                GameController.startLevel = null;
            }
        }

        public void DoOnLeaving() { }
        public void Execute(string cmd) {
            switch (cmd) {
                case "Tutorial":
                    Debug.Log("Start Tutorial");
                    GameController.startLevel = Level.TutorialLevel();
                    VisibilitySwitch.SetVisibility(GameObject.Find("Loading"), true);
                    EffectsManager.Instance.PlayEvent(EffectsManager.AudioEvent.GameStarted);
                    EffectsManager.Instance.FadeOutMusic(() => { SceneManager.LoadSceneAsync("GameScene"); });
                    break;
            }
        }
    }
}