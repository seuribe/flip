using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using com.perroelectrico.flip.core;
using com.perroelectrico.flip.audio;
using com.perroelectrico.flip.util;

namespace com.perroelectrico.flip.controller {

    public delegate void FloatEvent(float value);

    public class GameController : MonoBehaviour, FlipExecutor {

        private const float TargetAngle = 180f;
        private const float EndFlipAngle = 135f;
        private const float HintAngle = 75f;

        private const float HintFlipTimeMultiplier = 2.5f;
        private const int HintingMinDistance = 3;

        public static Level startLevel;

        private FlipState flipState = FlipState.Idle;

        private GameState state;
        private GameMode mode;

        public Stack<Move> History => state.history;
        public PuzzleState Current => state.cc;

        public SolutionDisplayController solutionController;
        public CoinGenerator generator;

        public GameObject fingerTrack;
        public GameObject wirePrefab;

        public GameObject guideLeft;
        public GameObject guideRight;
        public Vector3 guideMarkScale = new Vector3(0.02f, 0.05f, 1f);

        [Range(0, 2)]
        public float FlippingTime = 1;
        [Range(0, 2)]
        public float hideCoinsTime = 0.5f;
        [Range(0, 2)]
        public float showCoinsTime = 0.5f;
        [Range(0, 2)]
        public float animImageTime = 1f;
        [Range(0, 2)]
        public float puzzleSwitchTime = 1f;
        [Range(0, 2)]
        public float imageSwitchTime = 2f;

        public bool enableMouse;
        public bool enableTouch;

        private List<Puzzle> puzzles;
        private Puzzle puzzle;

        public bool PlayingTutorial => mode == GameMode.Tutorial;
        public bool PlayingRandom => mode == GameMode.RandomPuzzles;
        public bool PlayingNormal => mode == GameMode.Normal;

        private bool Solved => CurrentLevel.IsSolution(state.cc);

        private CoinController heldCoin = null;
        private Vector2 startTouchPosition;
        private Vector2 rotationScreenCenter;

        public event FlipEvent FlipStarted;
        public event FlipEvent FlipFinished;
        public event FlipEvent NumMovesChanged;
        public event FlipEvent UndoStarted;
        public event FlipEvent UndoFinished;
        public event FlipEvent ResetFinished;
        public event FlipEvent HoldStarted;
        public event FlipEvent PuzzleSet;
        public event FlipEvent PuzzleSolved;
        public event FlipEvent LevelSet;

        public event FloatEvent RotationSet;
        public event FloatEvent AngularVelocity;

        private WireController[] wires;
        private GameObject[] solutionCoins;
        private Vector2 coinScreenSize;
        private RotationController rotationController;

        private GameObject[] coins;
        private float currentRotationAngle;
        private EffectsManager effectsManager;

        private float originalFlippingTime;

        protected bool rotating = false;

        public bool LevelLoaded => CurrentLevel != null;
        public bool Flipping => flipState != FlipState.Idle;

        private bool inputAllowed = true;
        public bool CanProcessInput {
            get {
                return inputAllowed && (!Solved || PlayingTutorial) &&
                        (flipState == FlipState.Idle || flipState == FlipState.MovingCoin || flipState == FlipState.Holding);
            }
            set { inputAllowed = value; }
        }

        public bool CoinsRotating => flipState == FlipState.Resetting || flipState == FlipState.FinishingFlip ||
                                     flipState == FlipState.Undoing || flipState == FlipState.Hinting;

        public bool CanReset => CurrentLevel != null && !Current.Equals(puzzle.initial);

        public bool CanHint {
            get {
                return !PlayingTutorial &&
                        flipState != FlipState.Holding && flipState != FlipState.MovingCoin && flipState != FlipState.Hinting && flipState != FlipState.Undoing &&
                        !Solved &&
                        CurrentLevel.HasSolutionTree &&
                        CurrentLevel.FindSolutionPath(Current).Count > HintingMinDistance;
            }
        }

        public Level CurrentLevel { get; private set; }
        public Move CurrentMove { get; private set; }

        private bool ShowWires => CurrentLevel.type == Level.LevelType.wired;

        protected float RotateDirection=> flipState == FlipState.Undoing ? -1 : 1;

        void Awake() {
            EnsureLevelPresentInEditor();

            BadgeManager.Instance.Controller = this;
            effectsManager = EffectsManager.Instance;

            InitializeGameMode();

            CurrentLevel = startLevel ?? LevelManager.Instance.GetLevel(Level.LevelType.simple, 0);

            originalFlippingTime = FlippingTime;
        }

        private void InitializeGameMode() {
            if (startLevel.IsTutorial) {
                mode = GameMode.Tutorial;
            } else if (startLevel != null && LevelManager.Instance.LevelMastered(startLevel.Ref)) {
                mode = GameMode.RandomPuzzles;
            } else {
                mode = GameMode.Normal;
            }
        }

        private static void EnsureLevelPresentInEditor() {
            if (Application.isEditor && startLevel == null)
                startLevel = LevelManager.Instance.GetLevel(Level.LevelType.simple, 3);
        }

        void Start() {
            VisibilitySwitch.SetVisibility(fingerTrack, false);
            state = new GameState();

            SetLevel(CurrentLevel);
            if (PlayingTutorial) {
                StartTutorial();
            }
            Resources.UnloadUnusedAssets();
        }

        private void CleanupLevel() {
            if (CurrentLevel.type == Level.LevelType.wired) {
                DestroyWires();
            }
        }

        public void SetLevel(Level newLevel, bool onlySet = false) {
            Debug.LogFormat("SetLevel {0}, onlySet: {1}", newLevel, onlySet);
            CleanupLevel();

            CurrentLevel = newLevel;
            CurrentLevel.Init();

            if (onlySet)
                return;

            puzzles = GetPuzzles();
            NextPuzzle();

            CanProcessInput = true;

            LevelSet?.Invoke(CurrentArgs);

            LevelManager.Instance.SetLevelPlayed(newLevel);
        }

        public void Flip(Move move) {
            if (rotating)
                return;
            rotating = true;

            FlipStarted?.Invoke(CurrentArgs);

            CurrentMove = move;
            StartRotation();
            currentRotationAngle = 1;
            flipState = FlipState.FinishingFlip;
        }

        void StartRotation() {
            rotationController = new RotationController(Current, CurrentMove, coins, ref rotationScreenCenter);
        }

        void EndRotation() {
            rotationController.ReleasePieces(gameObject);
        }

        private void OnSolved(AfterSolvedInfo info) {
            LevelManager.Instance.Stars += info.newStars;

            effectsManager.PlayEvent((History.Count == puzzle.level.MinMoves) ? EffectsManager.AudioEvent.OptimallySolved :
                EffectsManager.AudioEvent.GameSolved);

            Statistics.Instance[Statistics.Stats.PuzzlesSolved]++;

            if (!PlayingTutorial) {
                HideCoins();
            }

            if (info.state.PuzzleSolved || info.state.NoChanges || PlayingRandom) {
                Invoke("NextPuzzle", (CurrentLevel.type != Level.LevelType.image) ? puzzleSwitchTime : imageSwitchTime);
            }

            if (PlayingNormal) {
                Tracking.Track(Tracking.GAME_PUZZLE_SOLVED, 0);
                CallFlipEvent(PuzzleSolved, FlipEventArgs.PuzzleSolvedArgs(puzzle, state, mode, info));
            }
        }

        private IEnumerator PlayFlipSound(int n, float delay, float falloff) {
            float vol = 1f;
            for (int i = 0 ; i < n ; i++) {
                effectsManager.PlayEvent(EffectsManager.AudioEvent.GameFlip, vol);
                yield return new WaitForSeconds(delay);
                vol *= falloff;
            }
        }

        /// <summary>
        /// Finalizes a move, making the required changes to the coins and current configuration information
        /// </summary>
        /// <param name="move"></param>
        /// <returns>true if configuration changed, false if not</returns>
        public void FinalizeMove() {
            var prev = Current;

            state.Apply(CurrentMove);
            int first = Current.FirstCoinAffectedIndex(CurrentMove);
            int n = Current.NumCoinsAffected(CurrentMove);
            Array.Reverse(coins, first, n);

            // If after moving we are in the same situation, nothing has changed
            if (Current.Equals(prev)) {
                effectsManager.PlayEvent(EffectsManager.AudioEvent.GameNoMove);
                return;
            }

            // If by doing this move we reach the same configuration as doing the last move, it's
            // considered an undo
            if (History.Count > 0 && Current.Flip(History.Peek()).Equals(Current.Flip(CurrentMove))) {
                effectsManager.PlayEvent(EffectsManager.AudioEvent.GameUndo);
                History.Pop();
                Statistics.Instance[Statistics.Stats.FlipsUndone]++;
                if (flipState != FlipState.Undoing) {   // undo provoked by flipping and not by clicking the button
                    Tracking.Track(Tracking.GAME_UNDO_FLIP);
                }
            } else {
                StartCoroutine(PlayFlipSound(Mathf.Min(n/2, 5), 0.015f, 0.45f));
                History.Push(CurrentMove);
                Statistics.Instance[Statistics.Stats.FlipsDone]++;
                Tracking.Track(Tracking.GAME_FLIP);
            }
            CallFlipEvent(NumMovesChanged);

            currentRotationAngle = 0;

            if (Solved)  {
                OnSolvedRun();
            }
            CurrentMove = Move.NoMove;

            Settings.Instance.LastGameState = state;
        }

        private AfterSolvedInfo? afterSolvedState = null;
        private void OnSolvedRun() {
            afterSolvedState = CurrentLevel.IsTutorial ?
                AfterSolvedInfo.NoChanges :
                LevelManager.Instance.UpdateSolved(puzzle, state.Moves);
        }

        private CoinController GetTouchedCoin(Vector2 touchPosition) {
            if (coins == null)
                return null;

            Ray ray = Camera.main.ScreenPointToRay(touchPosition);

            var intersected = coins.Where((coin) => coin.GetComponentInChildren<MeshRenderer>().bounds.IntersectRay(ray));
            if (intersected.Count() == 1)
                return intersected.First().GetComponent<CoinController>();

            return null;
        }

        private void ReleaseAndSet(FlipState newState) {
            heldCoin.Release();
            heldCoin = null;
            flipState = newState;

            VisibilitySwitch.SetVisibility(fingerTrack, false);

            ResetGuide(guideLeft);
            ResetGuide(guideRight);
        }

        private void ResetGuide(GameObject guide) {
            VisibilitySwitch.SetVisibility(guide, false);

            foreach (Transform child in guide.transform) {
                var gmc = child.GetComponent<GuideMarkController>();
                gmc.Reset();
            }
        }

        /// <summary>
        /// Processes the start of a touch at position
        /// </summary>
        /// <param name="position"></param>
        private void ProcessStartHold(Vector2 position) {
            lock (this) {

                if (flipState != FlipState.Idle || heldCoin != null ||
                    (heldCoin = GetTouchedCoin(position)) == null) {
                    return;
                }

                flipState = FlipState.Holding;
                HoldStarted?.Invoke(CurrentArgs);
            }

            startTouchPosition = position;
            heldCoin.Hold();

            /// create possible moves circles
            int coinNumber = Array.IndexOf(coins, heldCoin.gameObject);

            SetupGuide(new Move((byte)coinNumber, Side.Left), guideLeft);
            SetupGuide(new Move((byte)coinNumber, Side.Right), guideRight);
        }

        void SetupGuide(Move move, GameObject guide) {
            int first = Current.FirstCoinAffectedIndex(move);
            int n = Current.NumCoinsAffected(move);
            var firstPos = coins[first].transform.position;
            var lastPos = coins[first + n - 1].transform.position;
            Vector3 centerPos = (firstPos + lastPos) / 2;

            var coinBound = coins[move.pos].GetComponent<Renderer>().bounds;
            var otherCoinBound = coins[(move.pos == first) ? (first + n - 1) : first].GetComponent<Renderer>().bounds;
            var arcStart = coinBound.center + (new Vector3(0, coinBound.extents.y, 0));
//            var arcEnd = otherCoinBound.center + (new Vector3(0, -otherCoinBound.extents.y, 0));

            var diffW = (arcStart - centerPos);
            diffW.z = 0;
            float scale = 2 * diffW.magnitude / (coinBound.extents.y);
            float angle = Vector3.Angle(diffW, new Vector3(1, 0, 0)) - 90;

            centerPos.z = -2;

            guide.transform.position = centerPos;
            guide.transform.localScale = new Vector3(scale * (int)move.side, scale, scale);
            guide.transform.rotation = Quaternion.Euler(0, 0, angle);

            float speedFactor = 1 - (n / Current.Count) * 0.25f;

            foreach (Transform child in guide.transform) {
                child.localScale = guideMarkScale / scale;
                var gmc = child.GetComponent<GuideMarkController>();
                gmc.Show();
                gmc.Speed *= speedFactor;
            }

            VisibilitySwitch.SetVisibility(guide, true);
        }

        /// <summary>
        /// Processes the end of a touch/hold at position
        /// </summary>
        /// <param name="position"></param>
        private void ProcessEndHold(Vector2 position) {
            if (heldCoin == null)
                return;

            if (flipState == FlipState.Idle)
                ResetFinished?.Invoke(CurrentArgs);

            ReleaseAndSet(
                (flipState == FlipState.Holding) ? FlipState.Idle :
                (currentRotationAngle > TargetAngle / 2) ? FlipState.FinishingFlip :
                                                            FlipState.Resetting);
        }

        /// <summary>
        /// Processes an in-between move in a touch/hold operation
        /// </summary>
        /// <param name="position"></param>
        private void ProcessInputMove(Vector2 position) {
            if ((flipState != FlipState.Holding && flipState != FlipState.MovingCoin) || heldCoin == null)
                return;

            var coinBound = heldCoin.InitialBound;

            // TODO: check if it's really needed to use the None Side here, or it could be prevented -- Seu
            Side direction = coinBound.xMin > position.x ? Side.Left : ((coinBound.xMax < position.x) ? Side.Right : Side.None);

            switch (flipState) {
                case FlipState.Holding:
                    if (direction == Side.None) {
                        break;
                    }
                    int coinNumber = Array.IndexOf(coins, heldCoin.gameObject);
                    CurrentMove = new Move((byte)coinNumber, direction);
                    StartRotation();

                    int first = Current.FirstCoinAffectedIndex(CurrentMove);
                    int num = Current.NumCoinsAffected(CurrentMove);
                    var endCoinIndex = first + (CurrentMove.side == Side.Left ? 0 : num - 1);

                    var endCoin = coins[endCoinIndex];
                    var endBounds = endCoin.GetComponent<Renderer>().bounds;
                    var v = new Vector2(endBounds.center.x, endBounds.center.y - endBounds.extents.y);

                    fingerTrack.transform.position = new Vector3(v.x, v.y, -4);
                    VisibilitySwitch.SetVisibility(fingerTrack, true);
                    VisibilitySwitch.SetVisibility(guideLeft, CurrentMove.side == Side.Left);
                    VisibilitySwitch.SetVisibility(guideRight, CurrentMove.side == Side.Right);

                    flipState = FlipState.MovingCoin;

                    // if reached this point, we are in moving already so fall through
                    goto case FlipState.MovingCoin;
                case FlipState.MovingCoin:

                    if (direction == CurrentMove.side) {
                        currentRotationAngle = Vector3.Angle(rotationScreenCenter - startTouchPosition, rotationScreenCenter - position);
                    } else {
                        if (position.y > rotationScreenCenter.y && currentRotationAngle < 30) {
                            currentRotationAngle = 0;
                        } else if (position.y < rotationScreenCenter.y && currentRotationAngle > 90) {
                            ReleaseAndSet(FlipState.FinishingFlip);
                        } else {
                            ReleaseAndSet(FlipState.Resetting);
                        }
                    }
                    break;
            }
        }

        private void ProcessInput() {
            if (enableTouch && Input.touchCount > 0) {
                if (Input.touchCount > 1) {
                    ProcessEndHold(Input.GetTouch(0).position);
                    return;
                }
                Touch touch = Input.GetTouch(0);

                switch (touch.phase) {
                    case TouchPhase.Began:
                        ProcessStartHold(touch.position);
                        break;
                    case TouchPhase.Moved:
                        ProcessInputMove(touch.position);
                        break;
                    case TouchPhase.Ended:
                        ProcessEndHold(touch.position);
                        break;
                }
            } else if (enableMouse) {
                if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0) {
                    ProcessInputMove(Input.mousePosition);
                }
                if (Input.GetMouseButtonUp(0)) {
                    ProcessEndHold(Input.mousePosition);
                } else if (Input.GetMouseButtonDown(0)) {
                    ProcessStartHold(Input.mousePosition);
                }
            }
        }

        void Update() {
            if (afterSolvedState.HasValue) {
                OnSolved(afterSolvedState.Value);
                afterSolvedState = null;
            }

            float lastAngle = currentRotationAngle;
            if (CanProcessInput) {
                ProcessInput();
            }

            ClampRotationAngle();
            UpdateAngularVelocity(lastAngle);

            // If angle changed
            if (lastAngle != currentRotationAngle)
                UpdateSelectedRotation();

            CheckReleaseOfPieces();
            CheckEndOfRotation();
        }

        private void CheckEndOfRotation() {
            if (!CoinsRotating || (currentRotationAngle != TargetAngle && currentRotationAngle != 0))
                return;

            switch (flipState) {
                case FlipState.FinishingFlip:
                    FinalizeMove();
                    CallFlipEvent(FlipFinished);
                    break;
                case FlipState.Resetting:
                    effectsManager.PlayEvent(EffectsManager.AudioEvent.GameResetMove);
                    CallFlipEvent(ResetFinished);
                    break;
                case FlipState.Undoing:
                    FinalizeMove();
                    CallFlipEvent(UndoFinished);
                    break;
            }
            EndRotation();
            fingerTrack.transform.parent = null;

            FlippingTime = originalFlippingTime;
            flipState = FlipState.Idle;
        }

        private void CheckReleaseOfPieces() {
            if (flipState == FlipState.Hinting && currentRotationAngle >= HintAngle) {
                ReleaseAndSet(FlipState.Resetting);
            } else if (flipState == FlipState.MovingCoin && currentRotationAngle >= EndFlipAngle) {
                ReleaseAndSet(FlipState.FinishingFlip);
            }
        }

        // Could happen that coin goes beyond limit and falls into resetting or finishing state
        private void ClampRotationAngle() {
            if (CoinsRotating) {
                float inc = (Time.deltaTime / FlippingTime) * TargetAngle * ((flipState == FlipState.Resetting) ? -1f : 1f);
                currentRotationAngle = Mathf.Clamp(currentRotationAngle + inc, 0, TargetAngle);
            }
        }

        private void UpdateAngularVelocity(float lastAngle) {
            float angularVelocity = (currentRotationAngle - lastAngle) * Time.deltaTime;
            if (AngularVelocity != null && flipState != FlipState.Hinting && flipState != FlipState.Undoing) {
                AngularVelocity(angularVelocity);
            }
        }

        // generate the current state arguments object for sending flip events
        private FlipEventArgs CurrentArgs => new FlipEventArgs(puzzle, state, mode, Solved);

        // calls the specified flip event with args, or generates the current ones if needed
        private void CallFlipEvent(FlipEvent evt, FlipEventArgs args = null) {
            evt?.Invoke(args ?? CurrentArgs);
        }

        // updates the rotation for the selected pieces and notifies listeners
        private void UpdateSelectedRotation() {

            if (rotationController != null) {
                float shownAngle = currentRotationAngle * (float)CurrentMove.side * -RotateDirection;
                rotationController.SetAngle(shownAngle);
                RotationSet?.Invoke(currentRotationAngle);
            }
        }

        // if possible, will lookup the solution tree and hint the user with the next move for solving the puzzle
        internal void Hint() {
            if (!CanProcessInput || !CanHint) {
                return;
            }

            Hint(CurrentLevel.NextMove(Current));
        }

        internal void Hint(Move move) {
            FlippingTime = FlippingTime * HintFlipTimeMultiplier;
            CurrentMove = move;
            StartRotation();

            effectsManager.PlayEvent(EffectsManager.AudioEvent.Hint);

            VisibilitySwitch.SetVisibility(fingerTrack, true);

            fingerTrack.transform.position = coins[CurrentMove.pos].transform.position + new Vector3(0, RotateDirection, 1);
            rotationController.AttachFingerTracking(fingerTrack);

            heldCoin = coins[CurrentMove.pos].GetComponentInChildren<CoinController>();
            heldCoin.Hold();
            currentRotationAngle = 1;

            flipState = FlipState.Hinting;
        }

        internal void Execute(string cmd) {
            if (cmd == null || cmd.Equals(""))
                return;

            try {
                string param = null;
                int index = cmd.IndexOf('_');
                if (index > 0) {
                    param = cmd.Substring(index + 1);
                    cmd = cmd.Substring(0, index);
                }
                Command c = (Command)Enum.Parse(typeof(Command), cmd);
                Execute(c, param);
            } catch (Exception e) {
                Debug.LogErrorFormat("Not a valid command {0}", e.Message);
            }
        }

        internal void Execute(Command cmd, string param = null) {
            switch (cmd) {
                case Command.Reset:
                    Tracking.Track(Tracking.GAME_TRY_AGAIN);
                    Reset();
                    break;
                case Command.Undo:
                    Undo();
                    break;
                case Command.NextPuzzle:
                    Tracking.Track(Tracking.GAME_NEXT_PUZZLE);
                    NextPuzzle();
                    break;
                case Command.Tutorial:
                    StartTutorial();
                    break;
                case Command.Flip: {
                    Flip(Move.FromString(param));
                } break;
                case Command.Hint: {
                    if (param == null) {
                        Hint();
                    } else {
                        Hint(Move.FromString(param));
                    }
                } break;
                case Command.NextLevel:
                    SetLevel(LevelManager.Instance.GetNextLevel(CurrentLevel));
                    break;
                case Command.Generate:
                    SetPuzzle(new Puzzle(CurrentLevel, PuzzleState.FromString(param)));
                    break;
                case Command.Set:
                    SetState(PuzzleState.FromString(param));
                    break;
            }
        }

        private void StartTutorial() {
            GameObject.Find("Tutorial").GetComponent<Tutorial>().StartTutorial();
        }

        internal void Undo() {
            if (flipState == FlipState.Undoing)
                FlippingTime /= 2;

            if (Flipping || History.Count == 0 || Solved) {
                return;
            }

            CurrentMove = History.Peek();
            StartRotation();
            currentRotationAngle = 1;
            flipState = FlipState.Undoing;
            CallFlipEvent(UndoStarted);
        }

        public void Reset() {
            if (!CanReset) // if level == null, cannot reset
                return;

            effectsManager.PlayEvent(EffectsManager.AudioEvent.GameResetLevel);

            if (coinsAreBig) {
                generator.AnimateRestoreImage(0.5f);
                coinsAreBig = false;
            }
            SetPuzzle(puzzle);
        }

        internal List<Puzzle> GetPuzzles() {
            return PlayingRandom ? CurrentLevel.GetRandomPuzzles(CurrentLevel.NumPuzzles) : LevelManager.Instance.GetPuzzles(CurrentLevel);
        }

        internal void NextPuzzle() {
            if (puzzles.Count == 0) {
                puzzles = GetPuzzles();
            }
            // If still empty, it's because things have really finished (playing random should always provide new puzzles)
            // the GUI should react by showing options
            if (puzzles.Count == 0) {
                Debug.LogWarning("NextPuzzle no results?");
                return;
            }
            // If not empty, then there are still some puzzles to show
            var p = puzzles.First();
            puzzles.Remove(p);
            SetPuzzle(p);
        }

        public void SetState(PuzzleState cc) {
            state = new GameState(cc);
            coins = generator.GeneratePuzzle(cc, CurrentLevel);
        }

        public void SetPuzzle(Puzzle puzzle) {
            this.puzzle = puzzle;

            SetState(puzzle.initial);
            if (coinsHidden) {
                ShowCoins();
            }

            if (solutionController != null) {
                solutionCoins = solutionController.SetPuzzle(puzzle);
            }

            RotationSet = null;

            if (ShowWires) {
                BuildWires();
            }

            if (PlayingTutorial) {
                SetLevel(puzzle.level, true);
            } else {
                Tracking.Track(Tracking.GAME_SET_PUZZLE, (int)puzzle.level.type * 10000 + LevelManager.Instance.LevelIndex(puzzle.level));
                Statistics.Instance[Statistics.Stats.TotalPuzzlesPlayed]++;
                Settings.Instance.LastLevelPlayed = CurrentLevel.Ref;
            }
            CallFlipEvent(PuzzleSet);
        }

        private void DestroyWires() {
            if (wires == null)
                return;

            foreach (var w in wires) {
                w.Dettach(this);
            }
            wires = null;
        }

        private void BuildWires() {
            DestroyWires();
            wires = WireController.Generate(this, coins, solutionCoins);
        }

        void OnDrawGizmos() {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(gameObject.transform.position, 1);
        }

        void OnDestroy() {
            BadgeManager.Instance.Controller = null;
        }

        private bool coinsHidden = false;
        private bool coinsAreBig = false;
        internal void HideCoins() {
            if (CurrentLevel.IsImageLevel) {
                generator.AnimateBigImage(animImageTime, 2);
                coinsAreBig = true;
            } else {
                generator.AnimateSeparation(hideCoinsTime);
            }
            coinsHidden = true;
        }

        internal void ShowCoins() {
            if (coinsAreBig) {
                generator.AnimateRestoreImage(showCoinsTime);
                coinsAreBig = false;
            } else {
                generator.AnimateRestoreSeparation(showCoinsTime);
            }
            coinsHidden = false;
        }
    }
}