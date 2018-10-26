using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SimpleJSON;
using com.perroelectrico.flip.db;

namespace com.perroelectrico.flip.core {

    public struct AfterSolvedState {

        public const int PUZZLE_SOLVED       = 0x0001;
        public const int LEVEL_UNLOCKED      = 0x0002;
        public const int LEVEL_FINISHED      = 0x0004;
        public const int LEVEL_MASTERED      = 0x0008;
        public const int TYPE_FINISHED       = 0x0010;
        public const int TYPE_MASTERED       = 0x0020;
        public const int ALL_LEVELS_FINISHED = 0x0040;
        public const int ALL_LEVELS_MASTERED = 0x0080;

        public static readonly AfterSolvedState Default = new AfterSolvedState(0);

        public AfterSolvedState(int state) {
            this.state = state;
        }

        private int state;
        public bool NoChanges {
            get { return state == 0; }
        }
        public bool PuzzleSolved {
            get { return (state & PUZZLE_SOLVED) != 0; }
            set { if (value) { state = state | PUZZLE_SOLVED; } else { state = state & (~PUZZLE_SOLVED); } }
        }
        public bool LevelUnlocked {
            get { return (state & LEVEL_UNLOCKED) != 0; }
            set { if (value) { state = state | LEVEL_UNLOCKED; } else { state = state & (~LEVEL_UNLOCKED); } }
        }
        public bool LevelFinished {
            get { return (state & LEVEL_FINISHED) != 0; }
            set { if (value) { state = state | LEVEL_FINISHED; } else { state = state & (~LEVEL_FINISHED); } }
        }
        public bool LevelMastered {
            get { return (state & LEVEL_MASTERED) != 0; }
            set { if (value) { state = state | LEVEL_MASTERED; } else { state = state & (~LEVEL_MASTERED); } }
        }
        public bool TypeFinished {
            get { return (state & TYPE_FINISHED) != 0; }
            set { if (value) { state = state | TYPE_FINISHED; } else { state = state & (~TYPE_FINISHED); } }
        }
        public bool TypeMastered {
            get { return (state & TYPE_MASTERED) != 0; }
            set { if (value) { state = state | TYPE_MASTERED; } else { state = state & (~TYPE_MASTERED); } }
        }
        public bool AllLevelsFinished {
            get { return (state & ALL_LEVELS_FINISHED) != 0; }
            set { if (value) { state = state | ALL_LEVELS_FINISHED; } else { state = state & (~ALL_LEVELS_FINISHED); } }
        }
        public bool AllLevelsMastered {
            get { return (state & ALL_LEVELS_MASTERED) != 0; }
            set { if (value) { state = state | ALL_LEVELS_MASTERED; } else { state = state & (~ALL_LEVELS_MASTERED); } }
        }

        public override string ToString() {
            return string.Format("state: {0}, PuzzleSolved:{1}, LevelUnlocked:{2}, LevelFinished:{3}, LevelMastered:{4}, TypeFinished:{5}, TypeMastered:{6}, AllLevelsFinished:{7}, AllLevelsMastered:{8}",
                state.ToString("x4"), PuzzleSolved, LevelUnlocked, LevelFinished, LevelMastered, TypeFinished, TypeMastered, AllLevelsFinished, AllLevelsMastered);
        }
    }

    public enum PuzzleSolvedState {
        Unsolved = 0, // not solved yet
        Ok = 1, // in any number of moves
        Good = 2, // in a 'reasonable' number of moves
        Perfect = 3 // in the minimum number of moves
    }

    public struct SolvedStats {
        public readonly int NumUnsolved;
        public readonly int NumOk;
        public readonly int NumGood;
        public readonly int NumPerfect;
        public readonly int Total;

        public SolvedStats(int unsolved, int ok, int good, int perfect) {
            NumUnsolved = unsolved;
            NumOk = ok;
            NumGood = good;
            NumPerfect = perfect;
            Total = unsolved + ok + good + perfect;
        }
        public override string ToString() {
            return string.Format("[unsolved: {0}, ok: {1}, good: {2}, perfect: {3}", NumUnsolved, NumOk, NumGood, NumPerfect);
        }

        public SolvedState State {
            get {
                if (NumPerfect == Total) {
                    return SolvedState.Mastered;
                } else if (NumUnsolved == 0) {
                    return SolvedState.Finished;
                } else {
                    return SolvedState.Unsolved;
                }
            }
        }
    }

    public enum SolvedState {
        Locked,
        Unsolved,   // Some levels are still unsolved
        Finished,   // All levels solved, some not perfect
        Mastered    // All levels perfect
    }

    public struct AfterSolvedInfo {
        public readonly AfterSolvedState state;
        public readonly int newStars;

        public static readonly AfterSolvedInfo NoChanges = new AfterSolvedInfo(0, AfterSolvedState.Default);

        public AfterSolvedInfo(int newStars, AfterSolvedState state) {
            this.state = state;
            this.newStars = newStars;
        }

        public override string ToString() {
            return "new stars: " + newStars + ", new state: " + state;
        }
    }

    public class LevelManager {

        public const int NUM_LEVELS = 24;

        private readonly PuzzleSolvedState[] NotPerfectStates = { PuzzleSolvedState.Unsolved, PuzzleSolvedState.Ok, PuzzleSolvedState.Good };
        private readonly PuzzleSolvedState[] SolvedStates = { PuzzleSolvedState.Ok, PuzzleSolvedState.Good, PuzzleSolvedState.Perfect };

        public const int STARS_PERFECT = 3;
        public const int STARS_GOOD = 2;
        public const int STARS_OK = 1;

        private static readonly string LEVELS_BASE_DIR = "levels/";
        private static readonly string LEVELS_FULL_FILE = LEVELS_BASE_DIR + "full";

        private Dictionary<Level.LevelType, List<Level>> levels = new Dictionary<Level.LevelType, List<Level>>();

        private int stars = 0;
        public int Stars {
            get {
                return stars;
            }
            set {
                stars = value;
                Statistics.Instance[Statistics.Stats.Stars] = value;
            }
        }

        private DBManager db;
        private bool dbInitialized = false;

        private LevelManager() {
            LoadLevelDefs(Resources.Load<TextAsset>(LEVELS_FULL_FILE).text);

            db = DBManager.Instance;
            InitializeDB();
            CheckPuzzleCoherence();
            stars = Statistics.Instance[Statistics.Stats.Stars];
        }

        /// <summary>
        /// Initializes the DB for first-time use. This means defining the random puzzles
        /// that the user will play.
        /// </summary>
        private void InitializeDB() {
            if (dbInitialized || db.IsInitialized()) {
                Debug.Log("Db is already initialized");
                return;
            }

            dbInitialized = true;
            Debug.Log("Initializing DB...");

            List<Puzzle> initPuzzles = new List<Puzzle>();
            foreach (var levelType in levels) {
                foreach (var level in levelType.Value) {
                    level.Init();

                    var puzzles = level.GetRandomPuzzles(level.NumPuzzles);
                    if (puzzles.Count == 0) {
                        Debug.LogErrorFormat("No puzzles could be found for {0}", level.solved);
                    }
                    initPuzzles.AddRange(puzzles);
                }
                if (!db.AddPuzzles(initPuzzles)) {
                    Debug.LogWarningFormat("could not insert puzzle of type {0}", levelType.Key);
                }
                initPuzzles.Clear();
            }
        }

        private static object syncLock = new System.Object();
        private static LevelManager levelManager;
        public static LevelManager Instance {
            get {
                if (levelManager == null) {
                    lock (syncLock) {
                        if (levelManager == null) {
                            levelManager = new LevelManager();
                        }
                    }
                }
                return levelManager;
            }
        }

        private Level.LevelRef tutorialLevelRef;
        public Level.LevelRef TutorialLevelRef {
            get {
                return tutorialLevelRef;
            }
        }

        public int NumLevelsInType(Level.LevelType type) {
            return levels[type].Count;
        }

        private bool StoreStats {
            get {
                return Application.platform != RuntimePlatform.WebGLPlayer;
            }
        }

        private static PuzzleSolvedState EstimateSolvedState(Puzzle puzzle, int moves) {
            return (moves == puzzle.level.MinMoves)    ? PuzzleSolvedState.Perfect :
                   (moves < puzzle.level.MinMoves * 2) ? PuzzleSolvedState.Good :
                                                         PuzzleSolvedState.Ok;
        }

        public static int GetNewStars(PuzzleSolvedState oldState, PuzzleSolvedState newState) {
            return newState - oldState;
        }

        /// <summary>
        /// Updates also level status (finished, unlocked, etc.)
        /// </summary>
        /// <param name="puzzle"></param>
        public AfterSolvedInfo UpdateSolved(Puzzle puzzle, int moves) {

            var lastPuzzleState = GetSolvedState(puzzle);
            var lastLevelState = GetLevelState(puzzle.LevelRef);
            var lastTypeStats = GetTypeStats(puzzle.level.type);

            var newState = EstimateSolvedState(puzzle, moves);
            if (newState <= lastPuzzleState) {
                return AfterSolvedInfo.NoChanges;
            }

            var newStars = GetNewStars(lastPuzzleState, newState);
            db.SetPuzzleState(puzzle, newState);

            if (puzzle.level.IsTutorial) {
                Settings.Instance.TutorialDone = true;
            }
            var ass = new AfterSolvedState(AfterSolvedState.PUZZLE_SOLVED);
            var levelState = GetLevelState(puzzle.LevelRef);
            if (levelState != lastLevelState) {
                ass.LevelMastered = (levelState == SolvedState.Mastered);
                ass.LevelFinished = (levelState == SolvedState.Finished) || ((levelState == SolvedState.Mastered) && lastLevelState == SolvedState.Unsolved);
            }
            ass.LevelUnlocked = AnyLevelUnlocked(stars, stars + newStars);

            var typeStats = GetTypeStats(puzzle.level.type);
            if (typeStats.State != lastTypeStats.State) {
                ass.TypeMastered = (typeStats.State == SolvedState.Mastered);
                ass.TypeFinished = (typeStats.State == SolvedState.Finished) || ((typeStats.State == SolvedState.Mastered) && (lastTypeStats.State == SolvedState.Unsolved));
            }

            var levelStats = db.GetLevelStats(AllLevels.Select(l => l.Ref));
            var numMastered = levelStats.Values.Where(s => s.State == SolvedState.Mastered).Count();
            var numFinished = levelStats.Values.Where(s => s.State == SolvedState.Finished).Count();
            ass.AllLevelsMastered = (numMastered == NUM_LEVELS);
            ass.AllLevelsFinished = (numFinished == NUM_LEVELS);

            return new AfterSolvedInfo(newStars, ass);
        }

        private bool AnyLevelUnlocked(int oldStars, int newStars) {
            return levels.Values.Aggregate((all, l) => all.Concat(l).ToList())
                    .Find((l) => l.starsRequired > oldStars && l.starsRequired <= newStars) != null;
        }

        public int LevelIndex(Level level) {
            return levels[level.type].IndexOf(level);
        }

        public int NumPuzzlesToMaster(Level.LevelRef lref) {
            return db.PuzzlesWithState(lref, NotPerfectStates);
        }

        public int NumPuzzlesToFinish(Level.LevelRef lref) {
            return db.PuzzlesWithState(lref, PuzzleSolvedState.Unsolved);
        }

        public int NumPuzzlesSolved(Level.LevelRef lref) {
            return db.PuzzlesWithState(lref, SolvedStates);
        }

        private void CheckPuzzleCoherence() {
            int numIssues = 0;
            foreach (var levelList in levels.Values) {
                foreach (var level in levelList) {
                    int unsolved = db.PuzzlesWithState(level.Ref, PuzzleSolvedState.Unsolved);
                    int ok = db.PuzzlesWithState(level.Ref, PuzzleSolvedState.Ok);
                    int good = db.PuzzlesWithState(level.Ref, PuzzleSolvedState.Good);
                    int perfect = db.PuzzlesWithState(level.Ref, PuzzleSolvedState.Perfect);
                    int sum = unsolved + ok + good + perfect;

                    int total = level.NumPuzzles;
                    if (sum != total) {
                        numIssues++;
                        Debug.LogWarningFormat("Incoherent data found for level {6}: Unsolved = {0}, Ok = {1}, Good = {2}, Perfect = {3}, sum = {4}, Total = {5}",
                            unsolved, ok, good, perfect, sum, total, level.Ref);
                    }

                }
            }
            Debug.Log("DB coherence checked. Issues found: " + numIssues);
        }


        public Level GetLevel(Level.LevelRef lr) {
            foreach (var level in GetLevels(lr.type)) {
                if (level.solved.Equals(lr.cc)) {
                    return level;
                }
            }
            return null;
        }

        private List<Level> availableLevels;
        public bool IsLastLevel(Level level) {
            return availableLevels.IndexOf(level) == availableLevels.Count - 1;
        }


        public Level GetNextLevel(Level level) {
            if (IsLastLevel(level)) {
                return null;
            }

            int index = availableLevels.IndexOf(level);
            return availableLevels[index + 1];
        }

        private List<Level> allLevels = null;
        /// <summary>
        /// List of all levels available. Will initialize lazily on first request.
        /// </summary>
        public List<Level> AllLevels {
            get {
                if (allLevels == null) {
                    allLevels = levels.Values.Aggregate((all, l) => all.Concat(l).ToList()).ToList();
                }
                return allLevels;
            }
        }

        public List<Level> GetLevels(Level.LevelType type) {
            return levels[type];
        }

        public Level GetLevel(Level.LevelType type, int index) {
            return levels[type][index];
        }

        /// <summary>
        /// Loads all level classes from metadata file
        /// format
        /// type, image file, solved config, configs move level 1, configs move level 2, ... configs move level N
        /// </summary>
        /// <param name="metaText"></param>
        private void LoadLevelDefs(string metaText) {
            levels.Clear();

            var json = JSON.Parse(metaText);

            foreach (JSONClass typeData in json["types"].AsArray) {
                var type = (Level.LevelType)Enum.Parse(typeof(Level.LevelType), typeData["type"]);
                var typeTexture = typeData["texture"];

                foreach (JSONClass levelData in typeData["levels"].AsArray) {
                    var textureFile = levelData["texture"] ?? typeTexture;
                    var pieces = levelData["pieces"];
                    var indexesStr = levelData["indexes"].AsArray;
                    var indexes = new int[indexesStr.Count];
                    for (int cs = 0 ; cs < indexesStr.Count ; cs++) {
                        indexes[cs] = int.Parse(indexesStr[cs]);
                    }
                    var playIndex = levelData["moves"].AsInt;
                    var starsRequired = levelData["starsRequired"].AsInt;

                    bool forceLoadTree = levelData["forceLoadTree"] != null;
                    bool tutorial = levelData["tutorial"] != null;
                    Level level = new Level(type, PuzzleState.FromString(pieces), textureFile, LEVELS_BASE_DIR + pieces, indexes, playIndex, starsRequired, tutorial, forceLoadTree);
                    if (tutorial) {
                        tutorialLevelRef = level.Ref;
                    }
                    AddLevel(level);
                }
            }

            availableLevels = new List<Level>();
            foreach (var ll in levels.Values) {
                availableLevels.AddRange(ll);
            }
            availableLevels.Sort();
        }

        private void AddLevel(Level level) {
            List<Level> llist = null;
            if (!levels.TryGetValue(level.type, out llist)) {
                llist = new List<Level>();
                levels.Add(level.type, llist);
            }
            llist.Add(level);
        }

        public PuzzleSolvedState GetSolvedState(Puzzle puzzle) {
            return db.GetPuzzleState(puzzle);
        }

        public bool LevelFinished(Level.LevelRef lref) {
            return NumPuzzlesToFinish(lref) == 0;
        }

        public bool LevelMastered(Level.LevelRef lref) {
            return NumPuzzlesToMaster(lref) == 0;
        }

        public bool LevelUnlocked(Level.LevelRef lref) {
            return GetLevel(lref).starsRequired <= stars;
        }

        public void SetLevelPlayed(Level level) {
            if (!level.IsTutorial) {
                db.SetLevelPlayed(level.Ref);
            }
        }

        internal List<Puzzle> GetPuzzles(Level level) {
            return db.GetPuzzlesWithState(level, NotPerfectStates);
        }

        public SolvedStats GetTypeStats(Level.LevelType type) {
            return db.GetTypeStats(type);
        }

        public SolvedStats GetLevelStats(Level.LevelRef lref) {
            return db.GetLevelStats(lref);
        }

        public SolvedState GetLevelState(Level.LevelRef lref) {
            if (!LevelUnlocked(lref)) {
                return SolvedState.Locked;
            }
            var stats = GetLevelStats(lref);
            if (stats.NumPerfect == stats.Total) {
                return SolvedState.Mastered;
            } else if (stats.NumUnsolved == 0) {
                return SolvedState.Finished;
            }
            return SolvedState.Unsolved;
        }

        internal void ResetPlayData() {
            db.Clear();
            Stars = 0;
            dbInitialized = false;
            InitializeDB();
        }
    }
}