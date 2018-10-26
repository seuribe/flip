using UnityEngine;
using System.Collections;
using com.perroelectrico.flip.util;

namespace com.perroelectrico.flip.controller {

    public class CreditsMenuController : MonoBehaviour, IMenuController {
        public const string ID = "Credits";

        public GameObject leftSideModel;
        public GameObject rightSideModel;
        public GameObject textsParent;

        public float lineIncrement = 0.2f;

        void Start() {
            var asset = Resources.Load<TextAsset>("Texts/credits");
            var lines = asset.text.Split('\n');
            float y = 0;
            foreach (var key in lines) {
                if (key.Trim().Length == 0) {
                    continue;
                }
                var parts = key.Split('|');
                var ngo = Instantiate(leftSideModel) as GameObject;
                ngo.GetComponent<Renderer>().enabled = true;
                ngo.transform.parent = textsParent.transform;
                ngo.transform.rotation = textsParent.transform.rotation;
                ngo.transform.localPosition = new Vector3(0, y, 0);
                ngo.GetComponent<TextMesh>().text = parts[0];

                var content = Instantiate(rightSideModel) as GameObject;
                content.GetComponent<Renderer>().enabled = true;
                content.transform.parent = textsParent.transform;
                content.transform.rotation = textsParent.transform.rotation;
                content.transform.localPosition = new Vector3(0 + 0.25f, y, 0);
                content.GetComponent<TextMesh>().text = parts[1];

                y -= lineIncrement;
            }
        }

        public string Id() {
            return ID;
        }

        public void DoBeforeArrival() {
/*
            foreach (Transform ch in textsParent.transform) {
                DestroyImmediate(ch.gameObject);
            }
            textsParent.transform.DetachChildren();
*/

            var trm = TextResource.Get("menu").Filter("menu.credits");
            gameObject.SetChildText("Return", trm["Return"]);
            gameObject.SetChildText("Title", trm["Title"]);
        }

        public void DoAfterArrival() {
        }

        public void DoOnLeaving() {
        }

        public void Back() {
            var mm = GameObject.FindObjectOfType<MenuManager>() as MenuManager;
            mm.ShowMenu(MainMenuController.ID);
        }

        public void Execute(string cmd) {
            //
        }

    }
}