using UnityEngine;
using com.perroelectrico.flip.core;
using com.perroelectrico.flip.util;
using com.perroelectrico.flip.audio;
using com.perroelectrico.flip.controller.ui;
using UnityEngine.SceneManagement;

namespace com.perroelectrico.flip.controller {

    public class Tutorial : MonoBehaviour {

        public GameController controller;
        public SolutionDisplayController solution;
        public GUIController gui;
        public ButtonController movesArrow;

        private TextResource tr;

        public AudioClip music;

        enum Step {
            OneFlip,
            TwoFlip,
            TwoFlipAgain,
            FlipToMatch,
            MatchInMinMoves,
            LevelProgression,
            Ready
        }

        private Step step = Step.OneFlip;

        public void StartTutorial() {
            Debug.Log("Start Tutorial");
            solution.Hide();

            var em = EffectsManager.Instance;
            em.PlayMusic(music);

            tr = TextResource.Get("tutorial").Filter("tutorial");

            Tracking.Track(Tracking.LEVEL_TUTORIAL_START);

            movesArrow.Hide();
            gui.ClearGUI();
            gui.DisableListeners();
            gui.movesDisplay.Hide();

            controller.Execute(Command.Set, "0");

            Invoke("SetupStep", 1);
        }

        internal void SetupStep() {
            Debug.LogFormat("SetupStep: {0} ({1})", step.ToString(), (int)step);

            switch (step) {
                case Step.OneFlip:
                    gui.ShowText(tr["DragToFlip"]);
                    solution.Hide();
                    WaitAndHint(new Move(0, Side.Right), 1);
                    break;
                case Step.TwoFlip: {
                    gui.ShowText(tr["FlipAgain"]);
                    solution.Hide();
                    controller.SetPuzzle(Puzzle.TutorialTwoFlips());
                    WaitAndHint(new Move(1, Side.Left), 2f);
                    }break;
                case Step.TwoFlipAgain:
                    gui.ShowText(tr["InsideOut"]);
                    controller.SetPuzzle(Puzzle.TutorialTwoFlipsAgain());
                    WaitAndHint(new Move(1, Side.Right), 3f);
                    break;
                case Step.FlipToMatch: {
                    gui.ShowText(tr["FlipToMatch"]);
                    solution.Show();
                    var puzzle = Puzzle.TutorialPuzzleOne(controller.Current);
                    controller.SetPuzzle(puzzle);
                    controller.FlipFinished += AdvanceIfMatches;
                    } break;
                case Step.MatchInMinMoves: {
                    gui.ShowText(tr["OptimalSolutions"]);
                    var puzzle = Puzzle.TutorialPuzzleTwo();
                    controller.SetPuzzle(puzzle);
                    gui.movesDisplay.Init(0, 2, false);
                    gui.movesDisplay.Show();
                    movesArrow.Blink();
                    controller.NumMovesChanged += gui.OnNumMovesChanged;
                    controller.FlipFinished += AdvanceIfSolved;
                    } break;
                case Step.LevelProgression: {
                    gui.ShowText(tr["LevelDetails"]);
                    gui.levelDetail.gameObject.SetActive(true);
                    gui.levelDetail.OnHoverEnter += OnLevelHoverEnter;
                    } break;
                case Step.Ready: {
                    gui.ShowText(tr["StartPlaying"]);
                    Settings.Instance.TutorialDone = true;
                    Tracking.Track(Tracking.LEVEL_TUTORIAL_DONE);
                    Invoke("StartGame", 2);
                } break;
            }
        }

        void OnLevelHoverEnter() {
            gui.HideText();
            gui.levelDetail.OnHoverEnter -= OnLevelHoverEnter;
            gui.levelDetail.OnHoverExit += OnLevelHoverExit;
        }

        void OnLevelHoverExit() {
            gui.levelDetail.OnHoverExit -= OnLevelHoverExit;
            Advance(1);
        }

        internal void StartGame() {
            Debug.Log("StartGame");
            SceneManager.LoadSceneAsync("Main");
        }

        internal void WaitAndHint(Move move, float time) {
            controller.CanProcessInput = false;
            controller.Hint(move);
            controller.ResetFinished += OnResetFinished;
        }

        internal void AdvanceIfMatches(FlipEventArgs args) {
            movesArrow.Hide();

            if (args.isSolved) {
                controller.FlipFinished -= AdvanceIfMatches;
                Advance(1);
                gui.HideText();
            }
        }

        internal void AdvanceIfSolved(FlipEventArgs args) {
            movesArrow.Hide();

            int advanceTime = 1;
            if (args.isSolved) {
                controller.FlipFinished -= AdvanceIfSolved;

                bool solvedOptimal = gui.movesDisplay.Moves == 2;

                if (!solvedOptimal) {
                    gui.ShowText(tr["NotInTwo"]);
                    advanceTime = 2;
                }

                gui.levelDetail.SetLevelStats(new SolvedStats(0, 0, solvedOptimal ? 0 : 1, solvedOptimal ? 1 : 0));

                Advance(advanceTime);
                gui.HideText();
            }
        }

        // Hints finish with an OnReset event
        internal void OnResetFinished(FlipEventArgs args) {
            controller.ResetFinished -= OnResetFinished;
            controller.CanProcessInput = true;
            controller.FlipFinished += OnFlipFinished;
        }

        internal void OnFlipFinished(FlipEventArgs args) {
            EffectsManager.Instance.PlayEvent(EffectsManager.AudioEvent.GameSolved);
            controller.FlipFinished -= OnFlipFinished;
            Advance(1);
            gui.HideText();
        }

        void Advance(float time = 0) {
            step++;
            Invoke("SetupStep", time);
        }
    }
}