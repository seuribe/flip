using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using com.perroelectrico.flip.core;
using System.Collections.Generic;
using System;

namespace com.perroelectrico.flip.test {

    public class LevelTreeTest {

        List<Level> levels;

        [SetUp]
        public void Init() {
            var lm = LevelManager.Instance;
            levels = lm.AllLevels;
            foreach (var level in levels) {
                level.Init();
            }
        }

        [Test]
        public void TestInitialSolutions() {

            foreach (var level in levels) {
                Assert.IsTrue(level.Initialized, "Level is initialized");
                Assert.IsTrue(level.MinMoves > 0, "Min moves > 0");

                Assert.IsTrue(level.CanFindSolution || level.IsTutorial, "Level " + level + " either has tree, or is tutorial");

                if (!level.CanFindSolution) {
                    continue;
                }

                foreach (var ic in level.InitialConfigs) {
                    var moves = level.FindSolutionPath(ic);
                    Assert.NotNull(moves, "Solution path is not null");
                    Assert.AreEqual(level.MinMoves, moves.Count, "Number of moves in solution path for " + level + " is correct");
                }
            }
        }

        [Test]
        public void TestRandomPuzzles() {

            foreach (var level in levels) {
                Assert.IsTrue(level.Initialized, "Level is initialized");
                Assert.IsTrue(level.MinMoves > 0, "Min moves > 0");

                Assert.IsTrue(level.CanFindSolution || level.IsTutorial, "Level " + level + " either has tree, or is tutorial");

                if (!level.CanFindSolution) {
                    continue;
                }

                var randomPuzzles = level.GetRandomPuzzles(level.NumPuzzles);
                foreach (var puzzle in randomPuzzles) {
                    Assert.NotNull(puzzle, "Random Puzzle is not null");
                    var moves = level.FindSolutionPath(puzzle.initial);
                    Assert.NotNull(moves, "Solution path is not null");
                    Assert.AreEqual(level.MinMoves, moves.Count, "Number of moves in solution path for " + level + " is correct");
                }
            }
        }

    }
}

