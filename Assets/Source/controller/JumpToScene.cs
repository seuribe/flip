using UnityEngine;
using System.Collections;
using System;
using com.perroelectrico.flip.controller.ui;
using UnityEngine.SceneManagement;

namespace com.perroelectrico.flip.controller {

    public class JumpToScene : MonoBehaviour {

        public AudioSource playSound;
        public float delay = 1;
        public string scene;
        public UIButtonsController skipButton;
        public GameObject loadingSign;

        public event Action JumpRequested;

        private bool called = false;
        private bool jumped = false;

        void Start() {
            skipButton.OnClick += (id) => {
                Jump();
            };
            if (loadingSign != null) {
                VisibilitySwitch.SetVisibility(loadingSign, false);
            }
        }

        public void Jump() {
            if (jumped)
                return;
            jumped = true;

            if (JumpRequested != null) {
                JumpRequested();
            }

            if (loadingSign != null) {
                VisibilitySwitch.SetVisibility(loadingSign, true);
            }
            Invoke("DoJump", 0.1f);
        }

        private void DoJump() {
            SceneManager.LoadSceneAsync(scene);
        }

        void Update() {
            if (!called) {
                if (playSound != null) {
                    playSound.Play();
                }
                if (delay > 0) {
                    Invoke("Jump", delay);
                }
            }
            called = true;
        }

    }
}