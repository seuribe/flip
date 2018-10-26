using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using com.perroelectrico.flip.util;
using com.perroelectrico.flip.controller.ui;
using com.perroelectrico.flip.core;
using Assets.Source.controller.ui;
using com.perroelectrico.flip.audio;
using UnityEngine.SceneManagement;

namespace com.perroelectrico.flip.controller {

    public class GUIController : MonoBehaviour {

        private const int MAX_STARS = 3;

        public GameController controller;
        public MovesDisplayController movesDisplay;
        public Transform messageLocation;
        public ButtonController oneLiner;
        public ButtonController nextPuzzleButton;
        public UIButtonsController backButton;
        public ButtonController undoButton;
        public ButtonController hintButton;
        public GameObject starsBase;
        public TextMesh starsCount;
        public GameObject messagePrefab;
        public GameObject starPrefab;
        public LevelProgressDetail levelDetail;
        public GameObject audioOptionsPrefab;
        public GameObject achievementPrefab;

        public GameObject loading;

        private GameObject[] stars;
        private TextResource oneLiners;
        private TextResource tr;
        private TextResource trDialog;

        public void DisableListeners() {
            controller.LevelSet -= OnLevelSet;
            controller.NumMovesChanged -= OnNumMovesChanged;
            controller.PuzzleSolved -= OnPuzzleSolved;
            controller.PuzzleSet -= OnPuzzleSet;
            BadgeManager.Instance.BadgeEarned -= OnBadgeEarned;
            badgeQueue.Clear();
            messageQueue.Clear();
        }

        public void EnableListeners() {
            controller.LevelSet += OnLevelSet;
            controller.NumMovesChanged += OnNumMovesChanged;
            controller.PuzzleSolved += OnPuzzleSolved;
            controller.PuzzleSet += OnPuzzleSet;
            BadgeManager.Instance.BadgeEarned += OnBadgeEarned;
        }

        private QueuedSerialEvents<string> messageQueue = new QueuedSerialEvents<string>();
        private QueuedSerialEvents<Badge> badgeQueue = new QueuedSerialEvents<Badge>();

        void Destroy() {
            messageQueue.FireEvent -= LaunchMessage;
            badgeQueue.FireEvent -= ShowBadge;
        }

        void Awake() {
            StartCoroutine(LoadStars());
        }

        IEnumerator<int> LoadStars() {
            stars = new GameObject[MAX_STARS];
            for (int i = 0 ; i < MAX_STARS ; i++) {
                stars[i] = Instantiate<GameObject>(starPrefab);
                stars[i].SetActive(false);
                stars[i].GetComponent<FloatingStar>().OnArrival += () => {
                    starsCount.text = "" + (int.Parse(starsCount.text) + 1);
                    EffectsManager.Instance.PlayEvent(EffectsManager.AudioEvent.Stars);
                };
            }
            yield return MAX_STARS;
        }

        void Start() {
            EnableListeners();
            messageQueue.FireEvent += LaunchMessage;
            badgeQueue.FireEvent += ShowBadge;

            controller.UndoStarted += (args) => { undoButton.Click(); };

            nextPuzzleButton.Hide();
            hintButton.Hide();

            backButton.OnClick += (id) => { ShowGameOptions(); };

            undoButton.OnClick += () => {
                EffectsManager.Instance.PlayEvent(EffectsManager.AudioEvent.UndoButton);
                Tracking.Track(Tracking.UNDO_BUTTON);
            };
            hintButton.OnClick += () => { Tracking.Track(Tracking.HINT_BUTTON); };
            nextPuzzleButton.OnClick += () => { Tracking.Track(Tracking.NEXT_PUZZLE_BUTTON); };

            oneLiners = TextResource.Get("oneliners").Filter("oneliner");
            tr = TextResource.Get("gui").Filter("game.gui");
            trDialog = TextResource.Get("dialog").Filter("dialog");

            if (!controller.PlayingTutorial) {
                levelDetail.SetLevelStats(LevelManager.Instance.GetLevelStats(controller.CurrentLevel.Ref));
                starsCount.text = "" + LevelManager.Instance.Stars;
            }

            starsBase.SetActive(controller.PlayingNormal);

            if (controller.PlayingRandom || controller.PlayingTutorial) {
                levelDetail.gameObject.SetActive(false);
            }

            if (!controller.LevelLoaded) {
                movesDisplay.Hide();
            }
        }

        void ShowGameOptions() {
            if (Dialog.Current != null) {
                return;
            }
            controller.CanProcessInput = false;

            Dialog dlg = Dialog.ShowDialog("",
                new Dialog.DialogButton { id = "Exit",
                    onClick = (id) => { GotoMain(); }, text = tr["LeaveGame"] },
                new Dialog.DialogButton { id = "Continue", text = tr["ContinuePlaying"] });
            dlg.MainChild = Instantiate(audioOptionsPrefab);
            dlg.Cancellable = true;
            dlg.OnClose += () => {
                controller.CanProcessInput = true;
            };
        }

        IEnumerator<int> ShowStars(int numStars) {
            for (int i = 0 ; i < numStars ; i++) {
                var star = stars[i];
                star.SetActive(true);
                var starController = star.GetComponent<FloatingStar>();
                starController.target = starsCount.transform;
            }

            yield return numStars;
        }

        public void OnPuzzleSolved(FlipEventArgs args) {
            undoButton.Hide();
            levelDetail.SetLevelStats(LevelManager.Instance.GetLevelStats(controller.CurrentLevel.Ref));

            if (controller.PlayingRandom) {
                return;
            }

            StartCoroutine(ShowStars(args.info.newStars));

            bool showSmallCongrats = true;
            var state = args.info.state;
            if (state.LevelUnlocked) {
                ShowMessageId("solved.LevelUnlocked");
                showSmallCongrats = false;
            }
            if (state.LevelMastered) {
                nextPuzzleButton.Hide();
                controller.CanProcessInput = false;
                Invoke("LevelMastered", 2);
                showSmallCongrats = false;
            } else if (state.LevelFinished) {   // else if, because both level finished and levelmastered might be triggered together
                nextPuzzleButton.Hide();
                controller.CanProcessInput = false;
                Invoke("LevelFinished", 2);
                showSmallCongrats = false;
            }
            if (state.AllLevelsFinished) {
                controller.CanProcessInput = false;
                showSmallCongrats = false;
            }

            if (showSmallCongrats && state.PuzzleSolved) {
                if (Random.Range(0, 2) == 0) {
                    if (args.info.newStars == 2) {
                        ShowMessageId("solved.Good");
                    } else if (args.info.newStars == 3) {
                        ShowMessageId("solved.Perfect");
                    }
                }
            }
        }

        private void LevelFinished() {
            Dialog.ShowDialog(trDialog["Finished"],
                new Dialog.DialogButton { id = "Finished_Continue", text = trDialog["Finished.Continue"], onClick = (id) => { Restart(); } },
                new Dialog.DialogButton { id = "Finished_Back", text = trDialog["Finished.Back"], onClick = (id) => { GotoMain(); } })
            .Cancellable = false;
        }

        private void LevelMastered() {
            Dialog.ShowDialog(trDialog["Mastered"],
                new Dialog.DialogButton { id = "Mastered_Continue", text = trDialog["Mastered.Continue"], onClick = (id) => { Restart(); } },
                new Dialog.DialogButton { id = "Mastered_Back", text = trDialog["Mastered.Back"], onClick = (id) => { GotoMain(); } })
            .Cancellable = false;
        }

        private void GotoMain() {
            loading.SetActive(true);
            Tracking.Track(Tracking.GAME_EXIT);
            SceneManager.LoadSceneAsync("Main");
        }
        private void Restart() {
            SceneManager.LoadSceneAsync("GameScene");
        }

        private void ShowMessageId(string id, params object[] pms) {
            ShowMessage(tr[id], pms);
        }
        private void ShowMessage(string text, params object[] pms) {
            messageQueue.NewEvent(string.Format(text, pms));
        }

        private void LaunchMessage(string text) {
            StartCoroutine(LaunchMessageCR(text));
        }

        IEnumerator LaunchMessageCR(string text) {
            var ms = Instantiate(messagePrefab);
            var msController = ms.GetComponentInChildren<TextMessageController>();
            msController.message = text;
            msController.OnEnd += messageQueue.EndEvent;
            yield return null;
        }

        public void OnNumMovesChanged(FlipEventArgs args) {
            if (args.Moves == 0 || args.isSolved) {
                undoButton.Hide();
            } else {
                undoButton.Show();
            }
            if (controller.CanHint) {
                hintButton.Show();
            } else {
                hintButton.Hide();
            }
            movesDisplay.SetValue(args.Moves, args.isSolved);
        }

        public void HideText() {
            oneLiner.Hide();
        }
        public void ShowText(string str) {
            oneLiner.Text = str;
            oneLiner.Show();
        }

        public void OnPuzzleSet(FlipEventArgs args) {
            ClearGUI();
            movesDisplay.Init(0, args.puzzle.level.MinMoves, false);

            if (!controller.PlayingTutorial) {
                nextPuzzleButton.Show();
                movesDisplay.Show();
            }

            starsBase.SetActive(controller.PlayingNormal);
        }

        public void OnLevelSet(FlipEventArgs args) {
            ClearGUI();
            levelDetail.SetLevelStats(LevelManager.Instance.GetLevelStats(controller.CurrentLevel.Ref));

            var level = args.puzzle.level;
            if (level != null) {
                if (controller.PlayingNormal) {
                    var oneliner = string.Format("{0}\n({1} {2})", oneLiners[level.type.ToString()], level.MinMoves, tr["Moves"]);
                    ShowText(oneliner);
                    Invoke("HideText", 3);
                }
                if (!controller.PlayingTutorial) {
                    movesDisplay.Init(0, args.puzzle.level.MinMoves, false);
                    movesDisplay.Show();
                }
            }
        }

        public void ClearGUI() {
            undoButton.Hide();
        }

        void ShowBadge(Badge badge) {
            var ms = Instantiate(achievementPrefab);
            var acController = ms.GetComponentInChildren<AchievementController>();
            acController.Show(badge, badgeQueue.EndEvent);
        }

        void OnBadgeEarned(Badge badge) {
            badgeQueue.NewEvent(badge);
        }

        void Update() {
            if ((Application.platform == RuntimePlatform.Android &&
                    (Input.GetKey(KeyCode.Escape) || Input.GetKey(KeyCode.Menu))) ||
                Input.GetKeyDown(KeyCode.Escape)) {

                if (Dialog.Current == null) {
                    ShowGameOptions();
                }
            }

            if (Input.touchCount > 1) {
                Invoke("HideText", 2);
            }
        }
    }
}