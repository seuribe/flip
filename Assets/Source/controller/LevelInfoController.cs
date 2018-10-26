using UnityEngine;
using com.perroelectrico.flip.controller.ui;
using com.perroelectrico.flip.core;
using com.perroelectrico.flip.util;
using com.perroelectrico.flip.audio;
using UnityEngine.SceneManagement;

namespace com.perroelectrico.flip.controller {

    public class LevelInfoController : MonoBehaviour {

        public const float DIALOG_Z_POS = -2f;

        public TextMesh numPuzzles;
        public TextMesh numMoves;

        public LevelStateBar levelStateBar;
        public GameObject back;
        public CoinGenerator generator;
        public GameObject locked;
        public UIButtonsController clickButton;

        public GameObject dialogPrefab;

        public Material finishedMat;
        public Material masteredMat;
        public Material normalMat;

        private TextResource trDialog;

        public void SetLevel(Level level, Vector3 from, Vector3 coord, float animTime) {
            var tr = TextResource.Get("menu").Filter("menu.play");
            trDialog = TextResource.Get("dialog").Filter("dialog");

            var manager = LevelManager.Instance;
            generator.GeneratePuzzle(level.solved, level);
            var levelState = manager.GetLevelState(level.Ref);

            clickButton.OnClick += (id) => {
                switch (levelState) {
                    case SolvedState.Unsolved: Play(level); break;
                    case SolvedState.Finished: ShowDialogId(level, "Finished"); break;
                    case SolvedState.Mastered:
                        goto case SolvedState.Unsolved;
                }
            };
            VisibilitySwitch.SetVisibility(locked, level.Locked);
            if (level.Locked) {
                var tm = locked.GetComponentInChildren<TextMesh>();
                tm.text = string.Format(tr["StarsToUnlock"], level.starsRequired - manager.Stars);
            }

            var stats = LevelManager.Instance.GetLevelStats(level.Ref);

            if (stats.NumUnsolved == stats.Total) {
                levelStateBar.gameObject.SetActive(false);
            } else {
                levelStateBar.SetStats(stats);
            }
            numPuzzles.text = levelState == SolvedState.Mastered ? tr["Random"] : 
                string.Format("{0} {1}", level.NumPuzzles, tr["Puzzles"]);
            numMoves.text = 
                string.Format("{0} {1}", level.MinMoves, tr[level.MinMoves == 1 ? "Move" : "Moves"]);

            switch (levelState) {
                case SolvedState.Finished:
                    back.GetComponent<Renderer>().material = finishedMat;
                    break;
                case SolvedState.Mastered:
                    back.GetComponent<Renderer>().material = masteredMat;
                    break;
            }

            if (animTime > 0) {
                GetComponent<Animation>().AddClip(
                    new ClipBuilder()
                        .LocalPosition(from, coord, 0, animTime)
                        .LocalScale(new Vector3(0.1f, 0.1f, 0.1f), new Vector3(1, 1, 1), 0, animTime).Clip,
                        "anim");

                GetComponent<Animation>().Play("anim");
            } else {
                transform.localPosition = coord;
                transform.localScale = new Vector3(1, 1, 1);
            }
        }

        internal void ShowDialogId(Level level, string tid) {
            Debug.LogFormat("ShowDialogId {0}", tid);
            var go = Instantiate(dialogPrefab) as GameObject;
            go.transform.position = new Vector3(0, 0, DIALOG_Z_POS);
            var dlg = go.GetComponentInChildren<Dialog>();
            dlg.Message = trDialog[tid];
            dlg.AddButton(tid + "_Dialog_Yes", trDialog[tid + ".Continue"], (id) => {
                dlg.Close();
                Play(level);
            });
            dlg.AddButton(tid + "_Dialog_Back", trDialog[tid + ".Back"], (id) => {
                dlg.Close();
            });
            dlg.Show();
        }

        internal void Play(Level level) {
            GameController.startLevel = level;
            VisibilitySwitch.SetVisibility(GameObject.Find("Loading"), true);
            EffectsManager.Instance.FadeOutMusic(() => { SceneManager.LoadSceneAsync("GameScene"); });
        }

    }
}
