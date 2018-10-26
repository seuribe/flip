using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using com.perroelectrico.flip.core;
using System.Collections.Generic;
using System;

namespace com.perroelectrico.flip.test {

    public class TrieTest {

        [SetUp]
        public void Init() {
        }

        [Test]
        public void TestAdd() {
            PuzzleState st = new PuzzleState(new short[]{0, 1, 2, 3}, 4, true);

            var tree = new CCTrie(st);

            // flip 0R
            var mv1 = new Move(0, Side.Right);
            var st1 = st.Flip(mv1);
            tree.Add(st1, mv1);

            Assert.IsTrue(tree.Contains(st1), "Tree contains state just added");
            Move move = new Move();
            tree.GetExtra(st1, out move);
            Assert.AreEqual(mv1, move, "Stored state contains right move");

            var moves = tree.ReCreatePath(st1, st);
            Assert.NotNull(moves, "Solution is not null");
            Assert.AreEqual(1, moves.Count, "Solution has right length");
            Assert.AreEqual(mv1, moves[0], "Solution 1st move is right");


            var mv2 = new Move(2, Side.Left);
            var st2 = st1.Flip(mv2);
            tree.Add(st2, mv2);

            Assert.IsTrue(tree.Contains(st2), "Tree contains state just added");
            Assert.IsTrue(tree.Contains(st1), "Tree still contains the other state added");
            tree.GetExtra(st2, out move);
            Assert.AreEqual(mv2, move, "Stored state contains right move");

            moves = tree.ReCreatePath(st2, st);
            Assert.NotNull(moves, "Solution is not null");
            Assert.AreEqual(2, moves.Count, "Solution has right length");
            Assert.AreEqual(mv2, moves[0], "Solution 1st move is right");
            Assert.AreEqual(mv1, moves[1], "Solution 2nd move is right");

        }

    }
}

