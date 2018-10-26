using UnityEngine;
using System.Collections;
using com.perroelectrico.flip.core;

namespace com.perroelectrico.flip.controller.ui {

    public class LevelStatsIconController : MonoBehaviour {

        public Sprite lockedSprite;
        public Sprite normalSprite;
        public Sprite masteredSprite;
        public Sprite finishedSprite;

        public string level;

        // Use this for initialization
        void Start() {
            if (level != null) {
                var lref = Level.LevelRef.FromString(level);

                LevelManager manager = LevelManager.Instance;
                GetComponent<SpriteRenderer>().sprite =
                    manager.LevelFinished(lref) ? finishedSprite :
                    manager.LevelMastered(lref) ? masteredSprite :
                    manager.LevelUnlocked(lref) ? normalSprite :
                                                  lockedSprite;
            } else {
                GetComponent<SpriteRenderer>().sprite = normalSprite;
            }
        }

        // Update is called once per frame
        void Update() {

        }
    }
}