using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using com.perroelectrico.flip.core;
using System.Collections.Generic;
using System;
using com.perroelectrico.flip.db;

namespace com.perroelectrico.flip.test {

    public class MemoryDBTest {

        private MemoryDBManager memDB;

        [SetUp]
        public void Init() {
            memDB = MemoryDBManager.Instance;
            memDB.Clear();
        }

        private readonly PuzzleSolvedState[] AllStates = { PuzzleSolvedState.Unsolved, PuzzleSolvedState.Ok, PuzzleSolvedState.Good, PuzzleSolvedState.Perfect };

        [Test]
        public void TestAdd() {
            Assert.IsFalse(memDB.IsInitialized(), "Db not initialized");

            var puzzle = GenDummyPuzzle(Level.LevelType.simple, "abcd", "dcba");
            var puzzles = new List<Puzzle>();
            puzzles.Add(puzzle);

            Assert.IsTrue(memDB.AddPuzzles(puzzles), "Can add a puzzle to an empty db");
            Assert.IsTrue(memDB.IsInitialized(), "Db initialized");

            Assert.AreEqual(PuzzleSolvedState.Unsolved, memDB.GetPuzzleState(puzzle), "newly added puzzle is unsolved");
        }

        [Test]
        public void TestPuzzlesWithState() {
            var puzzle1 = GenDummyPuzzle(Level.LevelType.simple, "abcd", "dcba");
            var puzzle2 = GenDummyPuzzle(Level.LevelType.simple, "abcd", "bacd");
            var puzzle3 = GenDummyPuzzle(Level.LevelType.sided, "$abcd", "$bacd");
            var puzzles = new List<Puzzle>();
            puzzles.Add(puzzle1);
            puzzles.Add(puzzle2);
            puzzles.Add(puzzle3);
            memDB.AddPuzzles(puzzles);

            Assert.AreEqual(2, memDB.PuzzlesWithState(puzzle1.LevelRef, PuzzleSolvedState.Unsolved), "Two unsolved puzzles for " + puzzle1.LevelRef);
            Assert.AreEqual(1, memDB.PuzzlesWithState(puzzle3.LevelRef, PuzzleSolvedState.Unsolved), "One unsolved puzzles for " + puzzle3.LevelRef);
            memDB.SetPuzzleState(puzzle2, PuzzleSolvedState.Ok);
            memDB.SetPuzzleState(puzzle3, PuzzleSolvedState.Good);
            Assert.AreEqual(1, memDB.PuzzlesWithState(puzzle1.LevelRef, PuzzleSolvedState.Unsolved), "One unsolved puzzles for " + puzzle1.LevelRef);
            Assert.AreEqual(1, memDB.PuzzlesWithState(puzzle2.LevelRef, PuzzleSolvedState.Ok), "One ok puzzles for " + puzzle2.LevelRef);
            Assert.AreEqual(1, memDB.PuzzlesWithState(puzzle3.LevelRef, PuzzleSolvedState.Good), "One Good puzzles for " + puzzle3.LevelRef);
        }

        [Test]
        public void TestGetPuzzlesWithState() {
            var puzzle1 = GenDummyPuzzle(Level.LevelType.simple, "abcd", "dcba");
            var puzzle2 = GenDummyPuzzle(Level.LevelType.simple, "abcd", "bacd");
            var puzzles = new List<Puzzle>();
            puzzles.Add(puzzle1);
            puzzles.Add(puzzle2);
            memDB.AddPuzzles(puzzles);

            var ret = memDB.GetPuzzlesWithState(puzzle1.level, AllStates);
            Assert.AreEqual(2, ret.Count, "Two puzzles added to the db");

            ret = memDB.GetPuzzlesWithState(puzzle1.level, PuzzleSolvedState.Unsolved);
            Assert.AreEqual(2, ret.Count, "Two puzzles unsolved");
            Assert.Contains(puzzle1, ret, "puzzle 1 is returned in unsolved list");
            Assert.Contains(puzzle2, ret, "puzzle 2 is returned in unsolved list");

            memDB.SetPuzzleState(puzzle1, PuzzleSolvedState.Ok);

            Assert.AreEqual(PuzzleSolvedState.Ok, memDB.GetPuzzleState(puzzle1), "Puzzle state changed");
            Assert.AreEqual(PuzzleSolvedState.Unsolved, memDB.GetPuzzleState(puzzle2), "Puzzle state not changed for other puzzles");
        }

        [Test]
        public void TestClear() {
            var puzzle = GenDummyPuzzle(Level.LevelType.simple, "abcd", "dcba");
            var puzzles = new List<Puzzle>();
            puzzles.Add(puzzle);
            memDB.AddPuzzles(puzzles);

            var ret = memDB.GetPuzzlesWithState(puzzle.level, AllStates);
            Assert.IsNotEmpty(ret, "Some puzzles after clear");
            memDB.Clear();
            ret = memDB.GetPuzzlesWithState(puzzle.level, AllStates);
            Assert.IsEmpty(ret, "No more puzzles after clear");
        }

        [Test]
        public void TestBadges() {
            memDB.ClearBadges();
            memDB.AddEarnedBadge(Badge.Astronomer);
            var ret = new List<Badge>(memDB.GetEarnedBadges());
            Assert.AreEqual(1, ret.Count, "Badge added, only 1 there");
            Assert.Contains(Badge.Astronomer, ret, "Astronomer badge is there");

            memDB.AddEarnedBadge(Badge.Binary);
            ret = new List<Badge>(memDB.GetEarnedBadges());
            Assert.AreEqual(2, ret.Count, "Badge added, now 2 there");
            Assert.Contains(Badge.Astronomer, ret, "Astronomer badge is there");
            Assert.Contains(Badge.Binary, ret, "Binary badge is there");

            memDB.ClearBadges();
            ret = new List<Badge>(memDB.GetEarnedBadges());
            Assert.IsEmpty(ret, "No badges after clear");
        }

        [Test]
        public void TestLevelPlayed() {
            var puzzle1 = GenDummyPuzzle(Level.LevelType.simple, "abcd", "dcba");

            var played = new List<Level.LevelRef>(memDB.GetPlayedLevels());
            Assert.IsEmpty(played, "no levels played");
            memDB.SetLevelPlayed(puzzle1.LevelRef);
            played = new List<Level.LevelRef>(memDB.GetPlayedLevels());
            Assert.Contains(puzzle1.LevelRef, played, "level played is there");
            Assert.AreEqual(1, played.Count, "Only one level played");

            var puzzle2 = GenDummyPuzzle(Level.LevelType.sided, "$abcd", "$abdc");
            memDB.SetLevelPlayed(puzzle2.LevelRef);
            played = new List<Level.LevelRef>(memDB.GetPlayedLevels());
            Assert.Contains(puzzle1.LevelRef, played, "previous level played is there");
            Assert.Contains(puzzle2.LevelRef, played, "new levels played is there");
            Assert.AreEqual(2, played.Count, "Two level played");

            var puzzle3 = GenDummyPuzzle(Level.LevelType.sided, "$abcd", "$dcba");
            memDB.SetLevelPlayed(puzzle3.LevelRef);
            played = new List<Level.LevelRef>(memDB.GetPlayedLevels());
            Assert.Contains(puzzle1.LevelRef, played, "previous level played is there");
            Assert.Contains(puzzle2.LevelRef, played, "previous level played is there (2)");
            Assert.AreEqual(2, played.Count, "No new levels introduced, as puzzle2 and puzzle3 share level");

            memDB.Clear();
            played = new List<Level.LevelRef>(memDB.GetPlayedLevels());
            Assert.IsEmpty(played, "no played levels after db clear");
        }

        // fake puzzle & level data, will not be initialized anyway
        private Puzzle GenDummyPuzzle(Level.LevelType type, string solved, string initial) {
            var solvedState = PuzzleState.FromCompactString(solved);
            var initialState = PuzzleState.FromCompactString(initial);
            var level = new Level(type, solvedState, null, null, new int[1] { 1 }, 1, 0);
            return new Puzzle(level, initialState);
        }

        [Test]
        public void TestGetLevelsWithSomePerfect() {
            var lvls = new List<Level.LevelRef>(memDB.GetLevelsContaining(PuzzleSolvedState.Perfect));
            Assert.IsEmpty(lvls, "no level with some perfect after db clear");

            var puzzle1 = GenDummyPuzzle(Level.LevelType.simple, "abcd", "dcba");
            var puzzle2 = GenDummyPuzzle(Level.LevelType.simple, "abcd", "bacd");
            var puzzle3 = GenDummyPuzzle(Level.LevelType.sided, "$abcd", "$bacd");
            var puzzles = new List<Puzzle>();
            puzzles.Add(puzzle1);
            puzzles.Add(puzzle2);
            puzzles.Add(puzzle3);
            memDB.AddPuzzles(puzzles);

            memDB.SetPuzzleState(puzzle2, PuzzleSolvedState.Perfect);
            memDB.SetPuzzleState(puzzle3, PuzzleSolvedState.Good);

            lvls = new List<Level.LevelRef>(memDB.GetLevelsContaining(PuzzleSolvedState.Perfect));
            Assert.Contains(puzzle2.LevelRef, lvls, "Level with perfect puzzle returned");
            Assert.AreEqual(1, lvls.Count, "only one level returned");

            memDB.SetPuzzleState(puzzle3, PuzzleSolvedState.Perfect);
            lvls = new List<Level.LevelRef>(memDB.GetLevelsContaining(PuzzleSolvedState.Perfect));
            Assert.Contains(puzzle2.LevelRef, lvls, "Level with perfect puzzle returned");
            Assert.Contains(puzzle3.LevelRef, lvls, "Another Level with perfect puzzle returned");
            Assert.AreEqual(2, lvls.Count, "two levels returned");
        }

        [Test]
        public void TestGetLevelStats() {
            var puzzle1 = GenDummyPuzzle(Level.LevelType.simple, "abcd", "dcba");
            var puzzle2 = GenDummyPuzzle(Level.LevelType.simple, "abcd", "bacd");
            var puzzle3 = GenDummyPuzzle(Level.LevelType.simple, "abcd", "bcad");
            var puzzle4 = GenDummyPuzzle(Level.LevelType.simple, "abcd", "bcda");
            var puzzle5 = GenDummyPuzzle(Level.LevelType.simple, "aba", "aab");
            var puzzles = new List<Puzzle>();
            puzzles.Add(puzzle1);
            puzzles.Add(puzzle2);
            puzzles.Add(puzzle3);
            puzzles.Add(puzzle4);
            puzzles.Add(puzzle5);
            memDB.AddPuzzles(puzzles);

            var stats = memDB.GetLevelStats(puzzle1.LevelRef);
            Assert.AreEqual(0, stats.NumGood, "0 good");
            Assert.AreEqual(0, stats.NumOk, "0 ok");
            Assert.AreEqual(0, stats.NumPerfect, "0 perfect");
            Assert.AreEqual(4, stats.NumUnsolved, "4 unsolved");
            stats = memDB.GetLevelStats(puzzle5.LevelRef);
            Assert.AreEqual(0, stats.NumGood, "0 good");
            Assert.AreEqual(0, stats.NumOk, "0 ok");
            Assert.AreEqual(0, stats.NumPerfect, "0 perfect");
            Assert.AreEqual(1, stats.NumUnsolved, "1 unsolved");

            memDB.SetPuzzleState(puzzle1, PuzzleSolvedState.Ok);
            stats = memDB.GetLevelStats(puzzle1.LevelRef);
            Assert.AreEqual(0, stats.NumGood, "0 good");
            Assert.AreEqual(1, stats.NumOk, "1 ok");
            Assert.AreEqual(0, stats.NumPerfect, "0 perfect");
            Assert.AreEqual(3, stats.NumUnsolved, "3 unsolved");

            memDB.SetPuzzleState(puzzle2, PuzzleSolvedState.Good);
            memDB.SetPuzzleState(puzzle3, PuzzleSolvedState.Perfect);
            stats = memDB.GetLevelStats(puzzle1.LevelRef);
            Assert.AreEqual(1, stats.NumGood, "1 good");
            Assert.AreEqual(1, stats.NumOk, "1 ok");
            Assert.AreEqual(1, stats.NumPerfect, "1 perfect");
            Assert.AreEqual(1, stats.NumUnsolved, "1 unsolved");

            // TODO: test list version of the method -- Seu
        }

        [Test]
        public void TestGetTypeStats() {
            var stats = memDB.GetTypeStats(Level.LevelType.simple);
            foreach (var type in new Level.LevelType[] { Level.LevelType.simple, Level.LevelType.sided, Level.LevelType.wired, Level.LevelType.image }) {
                stats = memDB.GetTypeStats(type);
                Assert.AreEqual(0, stats.NumGood, "0 good");
                Assert.AreEqual(0, stats.NumOk, "0 ok");
                Assert.AreEqual(0, stats.NumPerfect, "0 perfect");
                Assert.AreEqual(0, stats.NumUnsolved, "0 unsolved");
            }

            var puzzle1 = GenDummyPuzzle(Level.LevelType.simple, "abcd", "dcba");
            var puzzle2 = GenDummyPuzzle(Level.LevelType.simple, "abc", "bac");
            var puzzle3 = GenDummyPuzzle(Level.LevelType.simple, "abcd", "bcad");
            var puzzle4 = GenDummyPuzzle(Level.LevelType.simple, "aba", "aab");
            var puzzles = new List<Puzzle>();
            puzzles.Add(puzzle1);
            puzzles.Add(puzzle2);
            puzzles.Add(puzzle3);
            puzzles.Add(puzzle4);
            memDB.AddPuzzles(puzzles);

            stats = memDB.GetTypeStats(Level.LevelType.simple);
            Assert.AreEqual(0, stats.NumGood, "0 good");
            Assert.AreEqual(0, stats.NumOk, "0 ok");
            Assert.AreEqual(0, stats.NumPerfect, "0 perfect");
            Assert.AreEqual(4, stats.NumUnsolved, "4 unsolved");

            foreach (var type in new Level.LevelType []{ Level.LevelType.sided, Level.LevelType.wired, Level.LevelType.image }) {
                stats = memDB.GetTypeStats(type);
                Assert.AreEqual(0, stats.NumGood, "0 good");
                Assert.AreEqual(0, stats.NumOk, "0 ok");
                Assert.AreEqual(0, stats.NumPerfect, "0 perfect");
                Assert.AreEqual(0, stats.NumUnsolved, "0 unsolved");
            }

        }

    }
}

