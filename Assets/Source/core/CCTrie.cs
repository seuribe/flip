using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.perroelectrico.flip.core {

    public class CCTrie {
        private CCNode<Move> root;
        private static Random random = new System.Random(DateTime.Now.Millisecond);

        public int Count {
            get {
                return root.count;
            }
        }
        private PuzzleState sample;
        private int rndSeed;

        public List<Move> ReCreatePath(PuzzleState from, PuzzleState to, int max = 100) {
            if (!Contains(from)) {
                return null;
            }
            List<Move> moves = new List<Move>();
            while (!from.Equals(to) && max > 0) {
                Move move = new Move();
                GetExtra(from, out move);
                moves.Add(move);
                from = from.Flip(move);
                max--;
            }
            return moves;
        }

        public CCTrie(PuzzleState sample) {
            this.root = new CCNode<Move>(sample);
            this.sample = sample;
        }

        public bool Contains(PuzzleState cc) {
            return root.Contains(cc, 0);
        }

        public bool Add(PuzzleState cc, Move extra) {
            return root.Add(cc, extra, 0);
        }

        public bool Remove(PuzzleState cc) {
            return root.Remove(cc, 0);
        }

        public bool GetExtra(PuzzleState cc, out Move extra) {
            return root.GetExtra(cc, out extra, 0);
        }

        public PuzzleState One() {
            short[] coins = new short[sample.Count];
            rndSeed = random.Next();
            if (root.One(coins, rndSeed, 0) != null) {
                return new PuzzleState(coins, sample.numColors, sample.sided);
            }
            return null;
        }

        private class CCNode<T> {
            private readonly CCNode<T>[] next;
            private T extra;
            public int count;

            private bool Final {
                get {
                    return next == null;
                }
            }

            public CCNode(PuzzleState sample) {
                next = new CCNode<T>[sample.CoinValueRange];
            }

            public CCNode(PuzzleState sample, int index) {
                if (sample.Count != index) {
                    next = new CCNode<T>[sample.CoinValueRange];
                }
            }

            public bool Remove(PuzzleState cc, int index) {
                if (Final) {
                    return true;
                }
                int val = cc[index];
                var child = next[val];
                if (child == null) {
                    return false;
                }
                bool ret = child.Remove(cc, index + 1);
                if (ret) {
                    if (child.count == 0) {
                        next[val] = null;
                    }
                    count--;
                }
                return ret;
            }

            public bool Add(PuzzleState cc, T extra, int index) {
                if (Final) {
                    if (this.extra.Equals(default(T)))
                        this.extra = extra;

                    return false;
                }
                int val = cc[index];
                var child = next[val];
                bool noChild = child == null;
                if (noChild) {
                    child = new CCNode<T>(cc, index + 1);
                    next[val] = child;
                }
                bool wasAdded = child.Add(cc, extra, index + 1) || noChild;
                if (wasAdded) {
                    count++;
                }
                return wasAdded;
            }

            public bool Contains(PuzzleState cc, int index) {
                if (Final) {
                    return true;
                }
                int val = cc[index];
                var child = next[val];
                return child != null && child.Contains(cc, index + 1);
            }

            public bool GetExtra(PuzzleState cc, out T extra, int index) {
                extra = this.extra;
                if (Final) {
                    return true;
                }
                int val = cc[index];
                var child = next[val];
                return child != null && child.GetExtra(cc, out extra, index + 1);
            }

            public short[] One(short[] coins, int rndSeed, int index) {
                if (Final) {
                    return coins;
                }
                for (int i = 0 ; i < next.Length ; i++) {
                    int ind = (i + rndSeed) % next.Length;
                    var child = next[ind];
                    if (child != null) {
                        coins[index] = (short)ind;
                        return child.One(coins, rndSeed, index + 1);
                    }
                }
                return null;
            }
        }
    }

}

