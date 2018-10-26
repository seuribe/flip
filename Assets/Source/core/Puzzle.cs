using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.perroelectrico.flip.core {
    /// <summary>
    /// A Puzzle is an instance of a level with a specific starting configuration.
    /// It is what the player solves.
    /// </summary>
    [Serializable]
    public struct Puzzle {
        public readonly Level level;
        public readonly PuzzleState initial;

        public Level.LevelRef LevelRef {
            get { return level.Ref; }
        }

        public Puzzle(Level level, PuzzleState initial) {
            if (level == null)
                throw new ArgumentNullException("Level cannot be null for puzzle");

            if (initial == null)
                throw new ArgumentNullException("Initial PuzzleState cannot be null for puzzle");

            this.level = level;
            this.initial = initial;
        }

        public static Puzzle TutorialPuzzleOne(PuzzleState initial) {
            var solved = initial.Flip(new Move(0, Side.Right));
            Level level = Level.TutorialLevel(solved, new int[] { 1 }, 1);
            return new Puzzle(level, initial);
        }

        public static Puzzle TutorialPuzzleTwo() {
            return BuildTutorialLevel("0 1 1 2", "2 1 0 1", 2);
        }

        internal static Puzzle TutorialTwoFlips() {
            return BuildTutorialLevel("1 0", "0 1", 1);
        }

        internal static Puzzle TutorialTwoFlipsAgain() {
            return BuildTutorialLevel("0 1 2", "2 1 0", 1);
        }

        internal static Puzzle BuildTutorialLevel(string solvedString, string initialString, int numMoves) {
            var solved = PuzzleState.FromString(solvedString);
            var initial = PuzzleState.FromString(initialString);
            Level level = Level.TutorialLevel(solved, new int[] { 1 }, numMoves);
            return new Puzzle(level, initial);
        }

        internal bool IsSolution(PuzzleState state) {
            return level.IsSolution(state);
        }

        /// <summary>
        /// Find *one* matching between initial config and solution, meaning a relation between
        /// each config and solution coins. If the puzzle is a N-coins N-colors, there is only one.
        /// If there are more coins than colors, then it will pick just one possible.
        /// </summary>
        /// <returns></returns>
        private int[] MapToSolution() {
            var from = level.solved;
            var to = initial;

            HashSet<int> used = new HashSet<int>();
            int[] ret = new int[from.Count];
            for (int i = 0 ; i < from.Count ; i++) {
                int colorIndex = from.ColorIndex(i);
                int destIndex = 0;
                while (destIndex < to.Count && (used.Contains(destIndex) || to.ColorIndex(destIndex) != colorIndex)) {
                    destIndex++;
                }
                ret[i] = destIndex;
                used.Add(destIndex);
            }
            return ret;
        }

        public override string ToString() {
            return string.Format("[level:{0}, initial:{1}]", level, initial);
        }
    }
}
