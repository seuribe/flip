using com.perroelectrico.flip.core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.perroelectrico.flip.db {

    public class MemoryDBManager : DBManager {
        private Dictionary<core.Puzzle, PuzzleSolvedState> solvedState = new Dictionary<core.Puzzle, PuzzleSolvedState>();
        private HashSet<core.Puzzle> puzzles = new HashSet<core.Puzzle>();
        private HashSet<core.Badge> badges = new HashSet<core.Badge>();
        private HashSet<core.Puzzle> played = new HashSet<core.Puzzle>();
        private HashSet<Level.LevelRef> levelsPlayed = new HashSet<Level.LevelRef>();

        private static System.Object syncLock = new System.Object();
        private static MemoryDBManager dbManager;
        public static new MemoryDBManager Instance {
            get {
                if (dbManager == null) {
                    lock (syncLock) {
                        if (dbManager == null) {
                            dbManager = new MemoryDBManager();
                        }
                    }
                }
                return dbManager;
            }
        }

        override public bool IsInitialized() {
            return puzzles.Count > 0;
        }

        override public bool AddPuzzles(List<core.Puzzle> puzzles) {
            Debug.LogFormat("AddPuzzles #:{0}", puzzles.Count);
            foreach (var puzzle in puzzles) {
                this.puzzles.Add(puzzle);
                solvedState.Add(puzzle, PuzzleSolvedState.Unsolved);
            }
            return true;
        }

        /// <summary>
        /// Updates the solved state of the puzzle in the db
        /// </summary>
        /// <param name="puzzle"></param>
        /// <param name="state"></param>

        override public void SetPuzzleState(core.Puzzle puzzle, PuzzleSolvedState state) {
            solvedState[puzzle] = state;

        }

        /// <summary>
        /// returns a list of the puzzles whose state matches any of the listed ones.
        /// </summary>
        /// <param name="lref"></param>
        /// <param name="states"></param>
        /// <returns></returns>
        override public int PuzzlesWithState(Level.LevelRef lref, params PuzzleSolvedState[] states) {
            int count = 0;
            foreach (var puzzle in solvedState.Keys) {
                if (puzzle.LevelRef.Equals(lref) && Array.IndexOf(states, solvedState[puzzle]) != -1) {
                    count++;
                }
            }
            return count;
        }

        override public List<core.Puzzle> GetPuzzlesWithState(Level level, params PuzzleSolvedState[] states) {
            var ret = new List<core.Puzzle>();
            var lref = level.Ref;
            foreach (var puzzle in solvedState.Keys) {
                var state = solvedState[puzzle];
                if (puzzle.LevelRef.Equals(lref) && Array.IndexOf(states, state) != -1) {
                    ret.Add(puzzle);
                }
            }
            return ret;
        }

        /// <summary>
        /// if the puzzle is not present in the db, it's because it was never played,
        /// and therefore it's unsolved
        /// </summary>
        /// <param name="puzzle"></param>
        /// <returns></returns>
        override public PuzzleSolvedState GetPuzzleState(core.Puzzle puzzle) {
            return solvedState[puzzle];
        }

        override public void SetPlayed(core.Puzzle puzzle) {
            played.Add(puzzle);
        }

        override public void Clear() {
            puzzles.Clear();
            badges.Clear();
            solvedState.Clear();
            played.Clear();
            levelsPlayed.Clear();
        }

        override public SolvedStats GetTypeStats(Level.LevelType type) {
            var count = new Dictionary<PuzzleSolvedState, int>();
            count[PuzzleSolvedState.Unsolved] = 0;
            count[PuzzleSolvedState.Ok] = 0;
            count[PuzzleSolvedState.Good] = 0;
            count[PuzzleSolvedState.Perfect] = 0;

            foreach (var ps in solvedState) {
                if (ps.Key.level.type == type) {
                    count[ps.Value]++;
                }
            }

            return new SolvedStats(
                count[PuzzleSolvedState.Unsolved],
                count[PuzzleSolvedState.Ok],
                count[PuzzleSolvedState.Good],
                count[PuzzleSolvedState.Perfect]);
        }

        override public Dictionary<Level.LevelRef, SolvedStats> GetLevelStats(IEnumerable<Level.LevelRef> lrefs) {
            var ret = new Dictionary<Level.LevelRef, SolvedStats>();
            foreach (var lref in lrefs) {
                ret[lref] = GetLevelStats(lref);
            }
            return ret;
        }

        override public SolvedStats GetLevelStats(Level.LevelRef lref) {
            var count = new Dictionary<PuzzleSolvedState, int>();
            count[PuzzleSolvedState.Unsolved] = 0;
            count[PuzzleSolvedState.Ok] = 0;
            count[PuzzleSolvedState.Good] = 0;
            count[PuzzleSolvedState.Perfect] = 0;

            foreach (var ps in solvedState) {
                var puzzle = ps.Key;
                var state = ps.Value;
                if (lref.Equals(puzzle.LevelRef)) {
                    count[state]++;
                }
            }

            return new SolvedStats(
                count[PuzzleSolvedState.Unsolved],
                count[PuzzleSolvedState.Ok],
                count[PuzzleSolvedState.Good],
                count[PuzzleSolvedState.Perfect]);
        }

        override public IEnumerable<Level.LevelRef> GetLevelsContaining(params PuzzleSolvedState[] states) {
            var lrefs = new HashSet<Level.LevelRef>();
            foreach (var puzzle in solvedState.Keys) {
                var st = solvedState[puzzle];
                if (states.Contains(st)) {
                    lrefs.Add(puzzle.LevelRef);
                }
            }
            return lrefs;
        }


        override public int ClearBadges() {
            int count = badges.Count;
            badges.Clear();
            return count;
        }

        override public int AddEarnedBadge(core.Badge badge) {
            if (badges.Contains(badge)) {
                return 0;
            }
            badges.Add(badge);
            return 1;
        }

        override public IEnumerable<core.Badge> GetEarnedBadges() {
            return new List<core.Badge>(badges);
        }


        override public void SetLevelPlayed(Level.LevelRef lref) {
            levelsPlayed.Add(lref);
        }

        override public IEnumerable<Level.LevelRef> GetPlayedLevels() {
            return new List<Level.LevelRef>(levelsPlayed);
        }

        public override void Close() { }
    }
}
