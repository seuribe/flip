using com.perroelectrico.flip.controller;
using com.perroelectrico.flip.db;
using com.perroelectrico.flip.util;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.perroelectrico.flip.core {

    public enum Badge {
        Beginner, // finish the first level
        Initiated, // finish 10 levels
        Advanced, // finish 20 levels
        SeenItAll, // played at least one puzzle from each level
        DoneItAll, // finished at least one puzzle from each level
        StarGazer, // earned 100 stars
        StarCollector, // earned 500 stars
        Astronomer, // earned 1000 stars
        KeepItSimple, // finished all Simple levels
        MasterOfSimplicity, // mastered all Simple levels
        UpsideDown, // finished all Sided levels
        MasterFromAllSides, // mastered all Sided levels
        Untangler, // finished all Wired levels
        KnotMaster, // mastered all Wired levels
        ArtCollector, // finished all Image levels
        ImageMaster, // mastered all Image levels
        Flipper, // Did at 1000 flips
        SecondThoughts, // Undid 1000 flips
        SuperFlipper, // Did 10000 flips
        AllTheWayBack, // Undid 10000 flips
        Unpuzzled, // Played 1000 puzzles
        Binary, // Mastered the Simple/0 1 0 1 0 1 0
        RainbowWarrior, // Mastered the Simple/0 1 2 3 4 5 6 7 8
        BackForMore, // Played again a level in Random Puzzles Mode
        MasterOfTheUniverse, // mastered all puzzles, finished the game completely
    }

    public delegate void BadgeEvent(Badge badge);

    class BadgeManager {
        private const int FLIPS_FLIPPER = 1000;
        private const int FLIPS_SUPER_FLIPPER = 10000;
        private const int UNDOS_SECOND_THOUGHTS = 1000;
        private const int UNDOS_ALL_THE_WAY_BACK = 10000;

        private const int PUZZLES_UNPUZZLED = 1000;

        private const int NUM_LEVELS_FINISHED_INITIATED = 10;
        private const int NUM_LEVELS_FINISHED_ADVANCED = 20;

        private const int LEVEL_INDEX_BEGINNER = 0;
        private const int LEVEL_INDEX_BINARY = 2;
        private const int LEVEL_INDEX_RAINBOW_WARRIOR = 8;

        private const int STARS_STAR_GAZER = 100;
        private const int STARS_STAR_COLLECTOR = 500;
        private const int STARS_STAR_ASTRONOMER = 1000;

        public readonly int NUM_BADGES = System.Enum.GetValues(typeof(Badge)).Length;

        private readonly Dictionary<Level.LevelType, Badge> typeFinishedBadges = new Dictionary<Level.LevelType, Badge>();
        private readonly Dictionary<Level.LevelType, Badge> typeMasteredBadges = new Dictionary<Level.LevelType, Badge>();

        private HashSet<Badge> earned = new HashSet<Badge>();
        private DBManager dbm;
        private LevelManager manager;

        private GameController controller;
        private BadgeManager() {
            dbm = DBManager.Instance;
            manager = LevelManager.Instance;
            var dbBadges = dbm.GetEarnedBadges();
            earned.UnionWith(dbBadges);

            typeFinishedBadges[Level.LevelType.simple] = Badge.KeepItSimple;
            typeFinishedBadges[Level.LevelType.sided] = Badge.UpsideDown;
            typeFinishedBadges[Level.LevelType.wired] = Badge.Untangler;
            typeFinishedBadges[Level.LevelType.image] = Badge.ArtCollector;

            typeMasteredBadges[Level.LevelType.simple] = Badge.MasterOfSimplicity;
            typeMasteredBadges[Level.LevelType.sided] = Badge.MasterFromAllSides;
            typeMasteredBadges[Level.LevelType.wired] = Badge.KnotMaster;
            typeMasteredBadges[Level.LevelType.image] = Badge.ImageMaster;
        }

        // check for badges that were not fired in the past because of bugs
        public void CheckLegacyErrors() {
            Debug.Log("Verify Legacy Badge Fixes");
            // Master of the universe: was checking if all levels finished instead of all levels mastered
            var levelStats = dbm.GetLevelStats(manager.AllLevels.Select(l => l.Ref));
            var numMastered = levelStats.Values.Where(s => s.State == SolvedState.Mastered).Count();
            var AllLevelsMastered = (numMastered == LevelManager.NUM_LEVELS);

            if (AllLevelsMastered) {
                Debug.Log("Fixed assigned MOTU badge");
                NewBadge(Badge.MasterOfTheUniverse);
            }
        }

        /// <summary>
        /// Will erase all badges (locally) and clear the DB
        /// </summary>
        public void Reset() {
            Debug.Log("Reset all Badges");
            earned.Clear();
            dbm.ClearBadges();
        }

        public event BadgeEvent BadgeEarned;

        private static object syncLock = new System.Object();
        private static BadgeManager badgeManager;
        public static BadgeManager Instance {
            get {
                if (badgeManager == null) {
                    lock (syncLock) {
                        if (badgeManager == null) {
                            badgeManager = new BadgeManager();
                        }
                    }
                }
                return badgeManager;
            }
        }

        public bool HasEarned(Badge badge) {
            return earned.Contains(badge);
        }

        /// <summary>
        /// Used for setting from external sources, like Steam. This will not trigger other stuff
        /// </summary>
        /// <param name="badge"></param>
        public bool SetEarned(Badge badge) {
            if (earned.Contains(badge)) {
                return false;
            }
            earned.Add(badge);
            dbm.AddEarnedBadge(badge);
            return true;
        }

        private void NewBadge(Badge badge) {
            earned.Add(badge);
            dbm.AddEarnedBadge(badge);

            Tracking.Track(Tracking.BADGE_EARNED, (int)badge);

            if (BadgeEarned != null) {
                BadgeEarned(badge);
            }
        }

        private void OnFlipFinished(FlipEventArgs args) {
            var numFlips = Statistics.Instance[Statistics.Stats.FlipsDone];
            if (numFlips == FLIPS_FLIPPER) {
                NewBadge(Badge.Flipper);
            } else if (numFlips == FLIPS_SUPER_FLIPPER) {
                NewBadge(Badge.SuperFlipper);
            }
        }

        private void OnUndoFinished(FlipEventArgs args) {
            var numUndos = Statistics.Instance[Statistics.Stats.FlipsUndone];
            if (numUndos == UNDOS_SECOND_THOUGHTS) {
                NewBadge(Badge.SecondThoughts);
            } else if (numUndos == UNDOS_ALL_THE_WAY_BACK) {
                NewBadge(Badge.AllTheWayBack);
            }
        }

        private void OnPuzzleSet(FlipEventArgs args) {
            var numPuzzles = Statistics.Instance[Statistics.Stats.TotalPuzzlesPlayed];
            if (numPuzzles == PUZZLES_UNPUZZLED) {
                NewBadge(Badge.Unpuzzled);
            }
        }

        private static bool LevelMatches(Level level, Level.LevelType type, int index) {
            return level.type == type && level.IndexInType == index;
        }

        private static int LevelsInState(IEnumerable<SolvedStats> stats, Level.LevelType type, SolvedState state) {
            return stats.Aggregate(0, (s, stat) => s + ((stat.State == state) ? 1 : 0));
        }

        private void OnPuzzleSolved(FlipEventArgs args) {

            var info = args.info;
/*
            Debug.LogFormat("OnPuzzleSolved, puzzle: {0}, issolved: {1}, moves: {2}, level index: {3}, after solved state: {4}",
                args.puzzle, args.isSolved, args.Moves, args.puzzle.level.IndexInType, info.state);
*/
            // All earned, do nothing else
            if (earned.Count == NUM_BADGES) {
//                Debug.Log("All badges earned, do nothing");
                return;
            }
            // Star Badges
            int currentStars = manager.Stars;
            if (!HasEarned(Badge.StarGazer) && (currentStars >= STARS_STAR_GAZER && (currentStars - info.newStars) < STARS_STAR_GAZER)) {
                NewBadge(Badge.StarGazer);
            } else if (!HasEarned(Badge.StarCollector) && (currentStars >= STARS_STAR_COLLECTOR && (currentStars - info.newStars) < STARS_STAR_COLLECTOR)) {
                NewBadge(Badge.StarCollector);
            } else if (!HasEarned(Badge.Astronomer) && (currentStars >= STARS_STAR_ASTRONOMER && (currentStars - info.newStars) < STARS_STAR_ASTRONOMER)) {
                NewBadge(Badge.Astronomer);
            }

            var allLevels = manager.AllLevels;
            var levelStats = dbm.GetLevelStats(allLevels.Select(l => l.Ref));

            if (info.state.LevelMastered) {
                if (!HasEarned(Badge.Binary) && LevelMatches(args.puzzle.level, Level.LevelType.simple, LEVEL_INDEX_BINARY)) {
                    NewBadge(Badge.Binary);
                } else if (!HasEarned(Badge.RainbowWarrior) && LevelMatches(args.puzzle.level, Level.LevelType.simple, LEVEL_INDEX_RAINBOW_WARRIOR)) {
                    NewBadge(Badge.RainbowWarrior);
                }
            }

            if (info.state.LevelFinished || info.state.LevelMastered) {
                if (!HasEarned(Badge.Beginner) && LevelMatches(args.puzzle.level, Level.LevelType.simple, LEVEL_INDEX_BEGINNER)) {
                    NewBadge(Badge.Beginner);
                }

                var numFinishedLevels = levelStats.Values.Aggregate(0, (s, lstat) => {
                    return s + (((lstat.State == SolvedState.Finished) || (lstat.State == SolvedState.Mastered)) ? 1 : 0);
                });

                if (!HasEarned(Badge.Initiated) && numFinishedLevels == NUM_LEVELS_FINISHED_INITIATED) {
                    NewBadge(Badge.Initiated);
                } else if (!HasEarned(Badge.Advanced) && numFinishedLevels == NUM_LEVELS_FINISHED_ADVANCED) {
                    NewBadge(Badge.Advanced);
                }
            }

            if (info.state.TypeFinished) {
                // check only the finished that corresponds to this type
                var type = args.puzzle.level.type;
                var badge = typeFinishedBadges[type];
                if (!HasEarned(badge)) {
                    NewBadge(badge);
                }
            }

            // Type mastered badges
            if (info.state.TypeMastered) {

                // check only the finished that corresponds to this type
                var type = args.puzzle.level.type;
                var badge = typeMasteredBadges[type];
                if (!HasEarned(badge)) {
                    NewBadge(badge);
                }
            }

            // Done It All: finished at least one puzzle from each level
            if (!HasEarned(Badge.DoneItAll) && info.newStars > 0 &&
                dbm.GetLevelsContaining(PuzzleSolvedState.Ok, PuzzleSolvedState.Good, PuzzleSolvedState.Perfect).Count() == LevelManager.NUM_LEVELS) {
                NewBadge(Badge.DoneItAll);
            }

            if (!HasEarned(Badge.MasterOfTheUniverse) && info.state.AllLevelsMastered) {
                NewBadge(Badge.MasterOfTheUniverse);
            }
        }

        private void OnLevelSet(FlipEventArgs args) {
            if (!HasEarned(Badge.SeenItAll) && dbm.GetPlayedLevels().Count() == LevelManager.NUM_LEVELS) {
                NewBadge(Badge.SeenItAll);
            }
            if (!HasEarned(Badge.BackForMore) && args.gameMode == GameMode.RandomPuzzles) {
                NewBadge(Badge.BackForMore);
            }
        }

        public GameController Controller {
            set {
                if (controller != null) {
                    controller.FlipFinished -= OnFlipFinished;
                    controller.UndoFinished -= OnUndoFinished;
                    controller.PuzzleSet    -= OnPuzzleSet;
                    controller.PuzzleSolved -= OnPuzzleSolved;
                    controller.LevelSet     -= OnLevelSet;
                }
                controller = value;
                if (controller != null) {
                    controller.FlipFinished += OnFlipFinished;
                    controller.UndoFinished += OnUndoFinished;
                    controller.PuzzleSet    += OnPuzzleSet;
                    controller.PuzzleSolved += OnPuzzleSolved;
                    controller.LevelSet     += OnLevelSet;
                }
            }
        }

    }
}