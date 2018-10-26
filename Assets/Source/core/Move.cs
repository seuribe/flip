using System.Collections.Generic;
using System.Text;

namespace com.perroelectrico.flip.core {

    public enum Side {
        None = 0,
        Left = -1, Right = 1
    }

    public struct Move {
        public readonly byte pos;
        public readonly Side side;

        public static readonly Move NoMove = new Move(0, Side.None);

        public Move(byte pos, Side side) {
            this.pos = pos;
            this.side = side;
        }

        public override bool Equals(object obj) {
            return (obj is Move && ((Move)obj).pos == pos && ((Move)obj).side.Equals(side));
        }

        public override int GetHashCode() {
            return pos.GetHashCode() * ((side == Side.Right) ? 7 : 13);
        }

        public static Move FromChar(char c) {
            if (c >= 'A' && c <= 'Z') {
                return new Move((byte)(c - 'A'), Side.Right);
            } else if (c >= 'a' && c <= 'z') {
                return new Move((byte)(c - 'a'), Side.Left);
            }
            return new Move();
        }

        public string ToCompactString() {
            return new string(new char[] { (char)((side == Side.Left ? 'a' : 'A') + pos) });
        }

        public override string ToString() {
            return "" + pos + (side == Side.Left ? "L" : "R");
        }

        public static Move FromString(string str) {
            str = str.Trim();
            string num = str.Substring(0, str.Length - 1);
            char side = str[str.Length - 1];
            return new Move(byte.Parse(num), side == 'R' ? Side.Right : Side.Left);
        }

        public static string ListToString(List<Move> moves) {
            if (moves == null) {
                return "";
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0 ; i < moves.Count ; i++) {
                if (i != 0) {
                    sb.Append(' ');
                }
                sb.Append(moves[i].ToString());
            }
            return sb.ToString();
        }

        public static List<Move> ListFromString(string str) {
            string[] strMoves = str.Split(' ');
            List<Move> moves = new List<Move>();
            foreach (string move in strMoves) {
                moves.Add(Move.FromString(move));
            }
            return moves;
        }

        public byte[] ToBinary() {
            byte[] ret = new byte[1];
            ret[0] = (byte)(pos | (side == Side.Left ? 0x80 : 0x00));
            return ret;
        }

        public static Move FromBinary(byte[] data) {
            byte pos = (byte)(data[0] & 0x7F);
            Side side = ((data[0] & 0x80) == 0x80) ? Side.Left : Side.Right;
            return new Move(pos, side);
        }
    }
}