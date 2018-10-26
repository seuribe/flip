using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using com.perroelectrico.flip.core;
using System.Collections.Generic;
using System;

namespace com.perroelectrico.flip.test {

    public class PuzzleStateTest {

        [SetUp]
        public void Init() {
        }

        [Test]
        public void TestCompactString() {
            var str = "abcd";
            var ps = PuzzleState.FromCompactString(str);
            Assert.AreEqual(ps.Count, 4, "State has 4 pieces");
            Assert.AreEqual(ps.numColors, 4, "State has 4 colors");
            Assert.IsFalse(ps.sided, "state is sided");

            var ps1 = ps.Flip(new Move(0, Side.Right));
            Assert.AreEqual("dcba", ps1.ToCompactString(), "Flipped compact representation matches");

            var sps = PuzzleState.FromCompactString("$abcd");
            Assert.AreEqual(sps.Count, 4, "State has 4 pieces");
            Assert.AreEqual(sps.numColors, 4, "State has 4 colors");
            Assert.IsTrue(sps.sided, "state is sided");

            var sps2 = sps.Flip(new Move(2, Side.Left));
            // new expected state: CBAd
            Assert.AreEqual(sps2.ColorIndex(0), 2, "piece 0 has color index 2");
            Assert.IsTrue(sps2.Rotated(0), "piece 0 is rotated");
            Assert.AreEqual(sps2.ColorIndex(1), 1, "piece 1 has color index 1");
            Assert.IsTrue(sps2.Rotated(1), "piece 1 is rotated");
            Assert.AreEqual(sps2.ColorIndex(2), 0, "piece 2 has color index 0");
            Assert.IsTrue(sps2.Rotated(2), "piece 2 is rotated");
            Assert.AreEqual(sps2.ColorIndex(3), 3, "piece 3 has color index 3");
            Assert.IsTrue(!sps2.Rotated(3), "piece 3 is NOT rotated");

            Assert.AreEqual("$CBAd", sps2.ToCompactString(), "Compact string matches expected");
        }

        [Test]
        public void TestStateBasics() {
            short[] simple = new short[]{0, 1, 2, 3};

            PuzzleState ps = new PuzzleState(simple, 4);
            Assert.AreEqual(ps.Count, 4, "State has 4 pieces");
            Assert.AreEqual(ps.numColors, 4, "State has 4 colors");
            Assert.IsFalse(ps.sided, "state is sided");

            for (int i = 0 ; i < ps.Count ; i++) {
                Assert.False(ps.Rotated(i), "piece not rotated");
                Assert.AreEqual(ps.ColorIndex(i), simple[i], "color index matches initial config");
            }

            var ps1 = ps.Flip(new Move(0, Side.Left));
            Assert.AreEqual(ps, ps1, "Flipping 1L on simple does not change state");
            var ps2 = ps.Flip(new Move(0, Side.Right)).Flip(new Move(0, Side.Right));
            Assert.AreEqual(ps, ps2, "Flipping 1R,1R returns same state as original");

        }

    }
}

