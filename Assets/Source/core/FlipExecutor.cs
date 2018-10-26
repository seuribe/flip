using com.perroelectrico.flip.controller;

namespace com.perroelectrico.flip.core {

    public class FlipEventArgs {
        public Puzzle puzzle;
        public GameState gameState;
        public AfterSolvedInfo info;
        public GameMode gameMode;
        public bool isSolved;

        public int Moves {
            get {
                return gameState.Moves;
            }
        }

        public PuzzleState Current {
            get {
                return gameState.cc;
            }
        }

        public override string ToString() {
            return string.Format("[puzzle:{0}, gameState:{1}, aftersolvedinfo:{2}, gameMode:{3}, solved:{4}]", puzzle, gameState, info, gameMode, isSolved);
        }

        public static FlipEventArgs PuzzleSolvedArgs(Puzzle puzzle, GameState gameState, GameMode gameMode, AfterSolvedInfo info) {
            return new FlipEventArgs() {
                puzzle = puzzle,
                gameState = gameState,
                gameMode = gameMode,
                info = info,
                isSolved = true,
            };
        }

        public FlipEventArgs() { }

        public FlipEventArgs(Puzzle puzzle, GameState gameState, GameMode gameMode, bool isSolved) {
            this.puzzle = puzzle;
            this.gameState = gameState;
            this.gameMode = gameMode;
            this.isSolved = isSolved;
            this.info = AfterSolvedInfo.NoChanges;
        }
    }

    public delegate void FlipEvent(FlipEventArgs args);

    public interface FlipExecutor {
        void Flip(Move move);

        event FlipEvent FlipStarted;
        event FlipEvent FlipFinished;
    }
}