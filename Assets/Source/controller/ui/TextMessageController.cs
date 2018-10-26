using UnityEngine;
using System.Collections;
using System;

namespace com.perroelectrico.flip.controller.ui {

    public class TextMessageController : MonoBehaviour {

        public string message;
        public float duration = 3.5f;
        public TextMesh textMesh;

        public event Action OnEnd;

        public float TimeLeft {
            get {
                return Time.time - startTime;
            }
        }

        private float startTime;
        void Start() {
            Debug.Log("Showing message " + message);
            textMesh.text = message;
            startTime = Time.time;
            GetComponent<Animation>().Play();
            Invoke("Remove", duration);
        }

        void Remove() {
            Debug.Log("Removing message " + message);
            Destroy(gameObject);
            OnEnd();
        }

    }
}