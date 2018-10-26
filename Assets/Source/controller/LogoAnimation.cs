using UnityEngine;
using System.Collections;
using System;
using com.perroelectrico.flip.core;
using com.perroelectrico.flip.audio;

namespace com.perroelectrico.flip.controller {

    public class LogoAnimation : MonoBehaviour, FlipExecutor {
        public event FlipEvent FlipStarted;
        public event FlipEvent FlipFinished;

        public CoinGenerator generator;
        public float rotateSpeed = 0.4f;
        public float initialDelay = 0.5f;
        public float beforeMainDelay = 2f;
        public float bigImageDelay = 0.3f;
        public float bigImageRatio = 3f;

        protected GameObject[] coins;
        protected PuzzleState current;
        protected GameObject rotatingBase;
        protected float currentRotationAngle;
        protected Move currentMove;

        public JumpToScene jump;

        protected bool rotating = false;
        protected bool rotatingFromDown = false;
        protected float RotateDirection {
            get { return rotatingFromDown ? -1 : 1; }
        }

        public Texture texture;

        private Level level;

        void Awake() {
            var lref = Level.LevelRef.FromString("image/$0 1 2 3");
            level = LevelManager.Instance.GetLevel(lref);
            level.Init(true);
        }

        void Start() {
            Debug.Log("Start Logo Animation");
            if (level == null) {
                OnFinished(null);
            }
            jump.JumpRequested += OnJump;
            Puzzle puzzle = level.GetRandomPuzzle();
            current = puzzle.initial;
            coins = generator.GeneratePuzzle(current, level, texture);
        }

        private bool stop = false;
        private void OnJump() {
            stop = true;
        }

        public void Solve() {
            var solution = level.FindSolutionPath(current);
            if (solution != null && solution.Count > 0) {
                Debug.Log("Solving logo...");
                FlipPlayer fp = new FlipPlayer(this);
                fp.Finished += OnFinished;
                fp.Play(solution);
            } else {
                Debug.LogError("Logo level solution not found");
            }
        }

        private void OnFinished(FlipEventArgs args) {
            EffectsManager.Instance.PlayEvent(EffectsManager.AudioEvent.GameSolved);
            generator.AnimateBigImage(bigImageDelay, bigImageRatio);
            Invoke("GotoMain", beforeMainDelay + bigImageDelay);
        }

        internal void GotoMain() {
            jump.Jump();
        }

        bool firstFrame = true;
        void Update() {
            if (stop) {
                return;
            }
            if (firstFrame) {
                Invoke("Solve", initialDelay);

                firstFrame = false;
            }

            if (rotating && rotatingBase != null && currentRotationAngle > 0 && currentRotationAngle < 180) {
                currentRotationAngle += Time.deltaTime * 180 / rotateSpeed;
                if (currentRotationAngle >= 180) {
                    currentRotationAngle = 180;
                }
                float shownAngle = currentRotationAngle * (float)currentMove.side * -RotateDirection;
                Quaternion newRot = Quaternion.Euler(new Vector3(0, 0, shownAngle));
                rotatingBase.transform.rotation = newRot;

                if (currentRotationAngle == 180) {
                    EffectsManager.Instance.PlayEvent(EffectsManager.AudioEvent.GameFlip);
                    rotating = false;
                    DestroyRotatingBase();

                    UpdateState();

                    if (FlipFinished != null) {
                        StartCoroutine(CallFinished());
                    }
                }
            }
        }

        IEnumerator CallFinished() {
            yield return new WaitForSeconds(0.2f);
            FlipFinished(new FlipEventArgs());
        }

        public void Flip(Move move) {
            if (rotating) {
                return;
            }
            CreateRotatingBase(move);
            currentMove = move;
            currentRotationAngle = 1;
            rotating = true;
        }

        protected void CreateRotatingBase(Move move) {
            int first = current.FirstCoinAffectedIndex(move);
            int n = current.NumCoinsAffected(move);

            GameObject firstCoin = coins[first];
            GameObject lastCoin = coins[first + n - 1];
            Vector3 originalPos = (firstCoin.transform.position + lastCoin.transform.position) / 2;

            rotatingBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rotatingBase.transform.position = originalPos;
            rotatingBase.GetComponent<Renderer>().enabled = false;

            for (int i = first ; i < first + n ; i++) {
                coins[i].transform.parent = rotatingBase.transform;
            }
        }

        protected void DestroyRotatingBase() {
            if (rotatingBase != null) {
                GameObject.Destroy(rotatingBase);
                foreach (GameObject cc in coins) {
                    cc.transform.parent = gameObject.transform;
                }
                rotatingBase = null;
            }
        }

        protected void UpdateState() {
            current = current.Flip(currentMove);
            int first = current.FirstCoinAffectedIndex(currentMove);
            int n = current.NumCoinsAffected(currentMove);
            Array.Reverse(coins, first, n);
        }
    }
}