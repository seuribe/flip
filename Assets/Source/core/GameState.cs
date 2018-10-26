using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.perroelectrico.flip.core {

    public class GameState {
        private static readonly char SEPARATOR = ':';
        public PuzzleState cc;
        public Stack<Move> history;

        public GameState()
            : this(PuzzleState.FromString("0")) {
        }

        public GameState(PuzzleState cc, IEnumerable<Move> moves) {
            this.cc = cc;
            this.history = new Stack<Move>(moves);
        }

        public GameState(PuzzleState cc)
            : this(cc, new List<Move>()) {
        }

        public int Moves {
            get {
                return history.Count;
            }
        }

        public GameState Back {
            get {
                var newStack = new Stack<Move>(history);
                var last = newStack.Pop();
                return new GameState(cc.Flip(last), newStack);
            }
        }

        public PuzzleState Apply(Move move) {
            return cc = cc.Flip(move);
        }

        public static GameState FromString(string str) {
            var parts = str.Split(SEPARATOR);
            GameState gs = new GameState(
                PuzzleState.FromString(parts[0]),
                Move.ListFromString(parts[1])
            );
            return gs;
        }

        public override string ToString() {
            return cc.ToString() + SEPARATOR + Move.ListToString(history.ToList());
        }
    }
}
