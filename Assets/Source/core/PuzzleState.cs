using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace com.perroelectrico.flip.core {

    /// <summary>
    /// immutable coins configuration
    /// </summary>
    [Serializable]
    public class PuzzleState {

        private static readonly char[] SEPARATION_CHARS = new char[] { ' ', '_' };
        public static readonly char IS_SIDED_CHAR = '$';

        private const int ROTATED_BIT = 0x8000;
        private const int COLOR_MASK  = 0x7FFF;

        private readonly short[] pieces;
        public readonly int numColors;
        public readonly bool sided;
        
        public int Count {
            get {
                return pieces.Length;
            }
        }

        /// <summary>
        /// WARNING! Only for use in CCTrie
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public int this[int i] {
            get {
                return Rotated(i) ? (Rotate(pieces[i]) + numColors) : pieces[i];
            }
        }

        public int CoinValueRange {
            get {
                return numColors + (sided ? numColors : 0);
            }
        }

        /// <summary>
        /// Warning: uses coin array as is, will not make a copy of it
        /// </summary>
        /// <param name="coins"></param>
        /// <param name="numColors"></param>
        /// <param name="sided"></param>
        public PuzzleState(short[] coins, int numColors, bool sided = false) {
            if (coins == null) {
                throw new ArgumentNullException("coins cannot be null for PuzzleState");
            }
            this.pieces = coins;
            this.sided = sided;
            this.numColors = numColors;
        }

        /// <summary>
        /// Returns the index from the first coin that has the given color. If not found, returns NumCoins
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public int FirstIndexForColor(int color) {
            for (int i = 0 ; i < pieces.Length ; i++) {
                if (ColorIndex(i) == color) {
                    return i;
                }
            }
            return pieces.Length;
        }

        /// <summary>
        /// Returns the color index corresponding to the coin at index.
        /// Might not correspond to the coin value, which may be influenced by orientation
        /// and/or other variables
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public int ColorIndex(int i) {
            return pieces[i] & COLOR_MASK;
        }

        /// <summary>
        /// true if the coin at index is upside down / rotated
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public bool Rotated(int i) {
            return IsRotated(pieces[i]);
        }

        private static bool IsRotated(short piece) {
            return (piece & ROTATED_BIT) != 0;
        }

        private static short Rotate(short piece) {
            return (short)(piece ^ ROTATED_BIT);
        }

        public PuzzleState Flip(Move move) {
            int pos = move.pos;
            var side = move.side;
            short[] newPieces = (short[])pieces.Clone();

            if (pos < 0) {
                pos = 0;
            } else if (pos > pieces.Length - 1) {
                pos = pieces.Length - 1;
            }
            int first = (side == Side.Left) ? 0 : pos;
            int n = (side == Side.Left) ? pos + 1 : pieces.Length - pos;
            Array.Reverse(newPieces, first, n);

            if (sided) {
                for (int i = first ; i < first + n ; i++) {
                    newPieces[i] = Rotate(newPieces[i]);
                }
            }
            return new PuzzleState(newPieces, numColors, sided);
        }

        public override bool Equals(object obj) {
            if (!(obj is PuzzleState) || obj == null) {
                return false;
            }
            PuzzleState other = obj as PuzzleState;
            bool ret = sided == other.sided && Enumerable.SequenceEqual(pieces, other.pieces);
            return ret;
        }

        public override int GetHashCode() {
            return (13 * pieces[0]) * 7 * pieces[1];
        }

        private char ToCompact(short c) {
            return (char)(IsRotated(c) ? ('A' + Rotate(c)) : ('a' + c));
        }

        private static bool IsRotatedChar(char c) {
            return c >= 'A' && c <= 'Z';
        }

        private static short FromCompact(char c) {
            if (IsRotatedChar(c)) {
                return Rotate((short)(c - 'A'));
            } else {
                return (short)(c - 'a');
            }
        }

        public string ToCompactString() {
            StringBuilder sb = new StringBuilder();
            if (sided) {
                sb.Append(IS_SIDED_CHAR);
            }
            for (int i = 0 ; i < pieces.Length ; i++) {
                sb.Append(ToCompact(pieces[i]));
            }
            return sb.ToString();
        }

        public static int NumColors(string piecesStr) {
            return piecesStr.ToLower().Distinct().Count();
        }

        public static PuzzleState FromCompactString(string str) {
            bool sided = (str[0] == IS_SIDED_CHAR);
            string coinsStr = (sided ? str.Substring(1) : str);
            int numColors = coinsStr.ToLower().Distinct().Count();
            short[] coins = new short[coinsStr.Length];
            for (int i = 0 ; i < coins.Length ; i++) {
                coins[i] = FromCompact(coinsStr[i]);
            }
            return new PuzzleState(coins, numColors, sided);
        }

        public static PuzzleState FromCompactString(string str, int numColors, bool sided) {
            short[] coins = new short[str.Length];
            for (int i = 0 ; i < coins.Length ; i++) {
                coins[i] = FromCompact(str[i]);
            }
            return new PuzzleState(coins, numColors, sided);
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            if (sided) {
                sb.Append(IS_SIDED_CHAR);
            }
            for (int i = 0 ; i < pieces.Length ; i++) {
                if (i != 0) {
                    sb.Append(SEPARATION_CHARS[0]);
                }
                if (Rotated(i)) {
                    sb.Append('-');
                }
                sb.Append(ColorIndex(i));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Sample representation:
        /// 
        /// [$][' ' or '_' separated color index list]
        /// 
        /// 0 2 3 1 0 1     - non sided, 6 coins, 4 colors (0, 1, 2, 3)
        /// $0 2 2 -1 1      - sided, 5 coins, 4th rotated, 3 colors (0, 1, 2)
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static PuzzleState FromString(string str) {
            bool sided = (str[0] == IS_SIDED_CHAR);
            if (sided) {
                str = str.Substring(1).Trim();
            }
            string[] strCoins = str.Split(SEPARATION_CHARS);
            short[] coins = new short[strCoins.Length];
            bool[] rot = new bool[strCoins.Length];
            List<int> nums = new List<int>();
            for (int i = 0 ; i < coins.Length ; i++) {
                rot[i] = strCoins[i][0] == '-'; // must do this because of possible -0 (will be parsed as 0 anyway)
                if (rot[i]) {
                    coins[i] = Rotate(Int16.Parse(strCoins[i].Substring(1)));
                } else {
                    coins[i] = Int16.Parse(strCoins[i]);
                }
                nums.Add(Math.Abs(coins[i]));
            }
            int numColors = nums.Distinct().Count();
            return new PuzzleState(coins, numColors, sided);
        }

        /// <summary>
        /// Given a move, what's the index of the first coin affected by the move
        /// </summary>
        public int FirstCoinAffectedIndex(Move move) {
            return (move.side == Side.Left) ? 0 : (int)move.pos;
        }

        /// <summary>
        /// Given a move, how many coins will be affected by it
        /// </summary>
        public int NumCoinsAffected(Move move) {
            return move.side == Side.Left ? move.pos + 1 : Count - move.pos;
        }

        private static Side[] allSides = new Side[] { Side.Left, Side.Right };
        public IEnumerable<Move> Moves {
            get {
                int startIndex = sided ? 0 : 1;

                for (int i = startIndex ; i < Count - 1 ; i++) {
                    foreach (Side side in allSides) {
                        yield return new Move((byte)i, side);
                    }
                }
                yield return new Move((byte)(Count - 1), Side.Left);
                if (sided) {
                    yield return new Move((byte)(Count - 1), Side.Right);
                }
            }
        }
    }
}