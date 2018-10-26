using UnityEngine;
using System.Collections.Generic;
using com.perroelectrico.flip.audio;
using com.perroelectrico.flip.util;

namespace com.perroelectrico.flip.controller {

    public class FinishedMessagesController : MonoBehaviour {

        public GameObject messagePrefab;
        [Range(0, 1)]
        public float interDelay = 0.75f;
        public float soundDelay = 0.4f;
        public Vector3 topPosition = new Vector3(0, 6, 0);
        public Vector3 interLineSpace = new Vector3(0, -1, 0);

        private TextResource tr;
        private int currentLine = 0;
        private List<string> keys;

        void Start() {
            tr = TextResource.Get("Finished").Filter("finished");
            keys = new List<string>(tr.Keys);
            InvokeRepeating("ShowMessage", interDelay, interDelay);
        }

        private void ShowMessage() {
            var key = keys[currentLine];
            var str = tr[key];
            var container = new GameObject();
            container.transform.localPosition = topPosition + (interLineSpace * currentLine);
            var m = Instantiate(messagePrefab) as GameObject;
            m.GetComponent<TextMesh>().text = str;
            m.GetComponent<Animation>().Play("MessageIn");
            m.transform.parent = container.transform;

            Invoke("PlaySound", soundDelay);

            currentLine++;
            if (currentLine >= keys.Count) {
                CancelInvoke("ShowMessage");
            }
        }

        private void PlaySound() {
            EffectsManager.Instance.PlayEvent(EffectsManager.AudioEvent.GameFlip);
        }
    }
}