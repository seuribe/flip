using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.perroelectrico.flip.core {

    /// <summary>
    /// A level is a Coin Configuration (solution) for a certain type, and a specified
    /// minimum moves to solve it.
    /// When initialized, it includes either the full solution tree, or the initial configurations
    /// from which puzzles are created.
    /// </summary>
    [Serializable]
    public class Level : IComparable<Level> {
        private System.Random random = new System.Random();

        public const int MAX_PUZZLES = 100;

        /// <summary>
        /// A Lever Reference allows pointing at a level without having it loaded into
        /// memory.
        /// </summary>
        [Serializable]
        public struct LevelRef {
            public readonly LevelType type;
            public readonly PuzzleState cc;

            public LevelRef(LevelType type, PuzzleState cc) {
                this.type = type;
                this.cc = cc;
            }

            public LevelRef(Level level) {
                this.type = level.type;
                this.cc = level.solved;
            }

            public static LevelRef FromString(string str) {
                LevelRef lr = new LevelRef();
                var ss = str.Trim();
                if (ss.Length > 0) {
                    var parts = str.Split('/');
                    var type = (Level.LevelType)Enum.Parse(typeof(Level.LevelType), parts[0]);
                    var cc = PuzzleState.FromString(parts[1]);
                    lr = new LevelRef(type, cc);
                }
                return lr;
            }

            public override string ToString() {
                return type.ToString() + "/" + cc.ToString();
            }

            public static LevelRef FromCompactString(string str) {
                LevelRef lr = new LevelRef();
                var ss = str.Trim();
                if (ss.Length > 0) {
                    var parts = str.Split('/');
                    var type = (Level.LevelType)Enum.Parse(typeof(Level.LevelType), parts[0]);
                    var cc = PuzzleState.FromCompactString(parts[1]);
                    lr = new LevelRef(type, cc);
                }
                return lr;
            }

            public string ToCompactString() {
                return type.ToString() + "/" + cc.ToCompactString();
            }
        }

        [Serializable]
        public enum LevelType {
            simple,
            sided,
            wired,
            image
        }

        private readonly int[] puzzlesPerLevel;
        private readonly int puzzleMoves;

        public PuzzleState[] InitialConfigs { get; private set; }

        public readonly PuzzleState solved;
        public readonly LevelType type;

        private static System.Random rnd = new System.Random();

        private readonly string textureFile;
        private Texture texture;

        private readonly string dataFile;
        private CCTrie tree;

        public readonly bool tutorialLevel;
        public readonly int starsRequired;

        private bool forceLoadTree;

        private bool initialized = false;
        public bool Initialized {
            get { return initialized; }
        }

        public LevelRef Ref {
            get { return new LevelRef(this); }
        }

        public bool Sided {
            get { return type == LevelType.sided || type == LevelType.image; }
        }

        public int IndexInType {
            get {
                // TODO: cache/pass this info on level creation -- Seu
                return LevelManager.Instance.GetLevels(type).IndexOf(this);
            }
        }

        public bool Finished {
            get {
                return LevelManager.Instance.LevelFinished(Ref);
            }
        }

        public bool Mastered {
            get {
                return LevelManager.Instance.LevelMastered(Ref);
            }
        }

        /// <summary>
        /// True if level does not requiere previous levels to be unlocked before playing it,
        /// or such levels have already been unlocked
        /// </summary>
        public bool Locked {
            get { return !LevelManager.Instance.LevelUnlocked(Ref); }
        }

        public bool CanFindSolution {
            get { return tree != null; }
        }

        public int NumPuzzles {
            get { return Math.Min(MAX_PUZZLES, puzzlesPerLevel[puzzleMoves - 1]); }
        }

        public bool IsImageLevel {
            get { return type == LevelType.image; }
        }

        public Texture Texture {
            get { return texture = texture ?? Resources.Load<Texture>(textureFile); }
        }

        public int MinMoves {
            get { return puzzleMoves; }
        }

        public int NumCoins {
            get { return solved.Count; }
        }

        public int NumColors {
            get { return solved.numColors; }
        }

        public bool HasSolutionTree {
            get { return tree != null; }
        }

        public bool IsTutorial {
            get { return tutorialLevel; }
        }

        public override string ToString() {
            return string.Format("{0},{1},{2},{3}", type, solved.ToString(), textureFile, dataFile);
        }

        public static Level TutorialLevel() {
            var initial = PuzzleState.FromString("0");
            return new Level(LevelType.simple, initial, null, null, new int[] { 1 }, 1, 0, true, false);
        }

        public static Level TutorialLevel(PuzzleState solved, int[] puzzlesPerLevel, int puzzleMoves) {
            return new Level(solved.sided ? LevelType.sided : LevelType.simple,
                solved, null, null, puzzlesPerLevel, puzzleMoves, 0, true, false);
        }

        public Level(LevelType type, PuzzleState solved, string textureFile, string dataFile, int[] puzzlesPerLevel, int puzzleMoves, int starsRequired, bool tutorialLevel, bool forceLoadTree) {
            this.type = type;
            this.solved = solved;
            this.textureFile = textureFile;
            this.dataFile = dataFile;
            this.puzzlesPerLevel = puzzlesPerLevel;
            this.starsRequired = starsRequired;
            this.puzzleMoves = puzzleMoves;
            this.tutorialLevel = tutorialLevel;
            this.forceLoadTree = forceLoadTree;
        }

        public Level(LevelType type, PuzzleState solved, string textureFile, string dataFile, int[] puzzlesPerLevel, int puzzleMoves, int starsRequired)
            : this(type, solved, textureFile, dataFile, puzzlesPerLevel, puzzleMoves, starsRequired, false, false) {
        }

        public void Init(bool forceLoadTree = false) {
            if (initialized)
                return;

            if (!Application.isMobilePlatform || forceLoadTree || this.forceLoadTree) {
                tree = LoadTree();
            }

            if (tree == null && !tutorialLevel) {
                var configs = ReadConfigs(this);
                if (configs == null) { // cannot initialize
                    return;
                }
                InitialConfigs = configs.ToArray();
            }

            initialized = true;
        }

        public Move NextMove(PuzzleState cc) {
            if (!initialized || tree == null) {
                Debug.LogWarning("Asked for next move on non-initialized Level");
                return Move.NoMove;
            }
            Move move = new Move();
            tree.GetExtra(cc, out move);
            return move;
        }

        /// <summary>
        /// This is not meant to be used during gameplay to get a new level, use LevelManager for that
        /// </summary>
        /// <param name="unsolved"></param>
        /// <returns></returns>
        public Puzzle GetRandomPuzzle() {
            if (!initialized) {
                Debug.LogWarning("Asked for puzzle on non initialized Level");
                return new Puzzle();
            }

            int index = random.Next(InitialConfigs.Count());
            return new Puzzle(this, InitialConfigs[index]);
        }

        public List<Puzzle> GetRandomPuzzles(int n) {
            if (!initialized) {
                Debug.LogWarning("Asked for puzzle on non initialized Level");
                return new List<Puzzle>();
            }

            return InitialConfigs.Select(cc => new Puzzle(this, cc))
                .OrderBy(item => rnd.Next())
                .Take(n)
                .ToList();
        }

        public Puzzle GetPuzzle(int i) {
            if (!initialized) {
                Debug.LogWarning("Asked for puzzle on non initialized Level");
                return new Puzzle();
            }
            return new Puzzle(this, InitialConfigs[i]);
        }

        internal bool IsSolution(PuzzleState current) {
            return current.Equals(solved);
        }

        public List<Move> FindSolutionPath(PuzzleState current) {
            if (!initialized || tree == null) {
                Debug.LogFormat("Cannot find solution path for level {0}. Init: {1}, Tree: {2}", this, initialized, (tree == null) ? "no" : "yes");
                return null;
            }
            return tree.ReCreatePath(current, solved);
        }

        private static List<PuzzleState> ReadConfigs(Level level) {
            var textAsset = Resources.Load<TextAsset>(level.dataFile + "." + level.puzzleMoves);

            if (textAsset == null)
                return null;

            var lines = textAsset.text.Split('\n');
            var configs = new List<PuzzleState>();
            foreach (var line in lines) {
                var str = line.Trim();
                if (str.Length == 0) {
                    continue;
                }
                if (level.Sided) {
                    str = str.Substring(1);
                }
                configs.Add(PuzzleState.FromCompactString(str, level.solved.numColors, level.Sided));
            }
            return configs;
        }

        /// <summary>
        /// Reads a solution tree from a text file.
        /// </summary>
        /// TODO: has side effects, should be reprogrammed -- Seu
        /// <returns></returns>
        private CCTrie LoadTree() {
            var file = Resources.Load<TextAsset>(dataFile);
            if (file == null)
                return null;

            var tree = new CCTrie(solved);

            int numBeforeHardest = puzzleMoves == 1 ? 0 : puzzlesPerLevel.Take(puzzleMoves - 1).Aggregate((ppl, sum) => ppl + sum);

            var initialStates = new List<PuzzleState>();

            int read = 0;
            int first = Sided ? 1 : 0;
            int numPieces = solved.Count;
            string[] leaves = file.text.Split('\n');
            foreach (var leaf in leaves) {
                string str = leaf.Trim();
                if (str.Length == 0) {
                    continue;
                }
                PuzzleState cc = PuzzleState.FromCompactString(str.Substring(first, numPieces), solved.numColors, Sided);
                Move move = Move.FromChar(str[first + numPieces]);
                tree.Add(cc, move);

                if (read >= numBeforeHardest && read < numBeforeHardest + puzzlesPerLevel[puzzleMoves - 1]) {
                    initialStates.Add(cc);
                }
                read++;
            }

            if (initialStates.Count == 0) {
                Debug.LogErrorFormat("No initial states from Tree on level {0}", this.Ref);
            }

            InitialConfigs = initialStates.ToArray();

            return tree;
        }

        public int CompareTo(Level other) {
            var ltComp = type.CompareTo(other.type);
            return (ltComp == 0) ? MinMoves.CompareTo(other.MinMoves) : ltComp;
        }
    }
}