using com.perroelectrico.flip.core;
using SQLite4Unity3d;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.perroelectrico.flip.db {

    class Puzzle {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Type { get; set; }
        public string SolvedConfig { get; set; }
        public string InitialConfig { get; set; }

        public int SolvedState { get; set; }
        public long LastPlayed { get; set; }

        public override string ToString() {
            return string.Format("[id: {5}, type: {0}, solved: {1}, initial: {2}, state: {3}, lastPlayed: {4}",
                Type.ToString(), SolvedConfig, InitialConfig, SolvedState, LastPlayed, Id);
        }
    }

    class Badge {
        [PrimaryKey, Unique]
        public int BadgeId { get; set; }
        public long Timestamp { get; set; }
    }

    class LevelState {
        public string Type { get; set; }
        public string SolvedConfig { get; set; }
        public int SolvedState { get; set; }
    }

    class TypeState {
        public string Type { get; set; }
        public int SolvedState { get; set; }
    }

    class LevelPlayed {
        [PrimaryKey, Unique]
        public string LevelRef { get; set; }
        public long Timestamp { get; set; }
    }

    class SQLDBManager : DBManager {

        private SQLiteConnection connection;
        private const string DatabaseName = "levels.db";

        private static object syncLock = new System.Object();
        private static SQLDBManager dbManager;
        public static new SQLDBManager Instance {
            get {
                if (dbManager == null) {
                    lock (syncLock) {
                        if (dbManager == null) {
                            dbManager = new SQLDBManager();
                        }
                    }
                }
                return dbManager;
            }
        }

        private SQLDBManager() {
#if UNITY_EDITOR
            var dbPath = System.IO.Path.Combine(Application.streamingAssetsPath, DatabaseName);
#else
            var dbPath = string.Format("{0}/{1}", Application.persistentDataPath, DatabaseName);
#endif
            Debug.Log("Final PATH: " + dbPath);
            connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
            connection.CreateTable<Puzzle>();
            connection.CreateTable<Badge>();
            connection.CreateTable<LevelPlayed>();
            connection.CreateCommand("CREATE VIEW IF NOT EXISTS LevelState AS SELECT DISTINCT type, SolvedConfig, min(SolvedState) as SolvedState FROM Puzzle GROUP BY type, SolvedConfig").ExecuteNonQuery();
            connection.CreateCommand("CREATE VIEW IF NOT EXISTS TypeState AS SELECT DISTINCT type, min(SolvedState) as SolvedState FROM Puzzle GROUP BY type").ExecuteNonQuery();
        }

        override public bool IsInitialized() {
            if (!Application.isPlaying) {
                return true;
            }
            return connection.Table<Puzzle>().Count() > 0;
        }

        private static Puzzle CoreToDBPuzzle(core.Puzzle puzzle, int solvedState) {
            return new Puzzle {
                Type = puzzle.level.type.ToString(),
                SolvedConfig = puzzle.level.solved.ToCompactString(),
                InitialConfig = puzzle.initial.ToCompactString(),
                SolvedState = solvedState
            };
        }

        override public bool AddPuzzles(List<core.Puzzle> puzzles) {
            try {
                var dbPuzzles = puzzles.Select(p => CoreToDBPuzzle(p, 0));
                return connection.InsertAll(dbPuzzles) == puzzles.Count;
            } catch (Exception e) {
                Debug.LogError("Could not insert puzzles in DB: " + e.ToString());
            }
            return false;
        }

        /// <summary>
        /// Updates the solved state of the puzzle in the db
        /// </summary>
        /// <param name="puzzle"></param>
        /// <param name="state"></param>
        override public void SetPuzzleState(core.Puzzle puzzle, PuzzleSolvedState state) {
            try {
                Puzzle dbPuzzle = GetPuzzle(puzzle);
                var storedState = GetStoredState(state);
                if (dbPuzzle != null) {
                    dbPuzzle.SolvedState = storedState;
                    connection.Update(dbPuzzle);
                } else {
                    dbPuzzle = CoreToDBPuzzle(puzzle, storedState);
                    connection.Insert(dbPuzzle);
                }
            } catch (Exception e) {
                Debug.LogError("Could not set puzzle state in DB: " + e.Message);
            }
        }

        private static int[] DbStoredStates(params PuzzleSolvedState[] states) {
            return states.Select(st => GetStoredState(st)).ToArray();
        }

        private List<Puzzle> QueryWithState(Level.LevelRef lref, PuzzleSolvedState[] states) {
            var typeStr = lref.type.ToString();
            var solvedStr = lref.cc.ToCompactString();
            var storedStates = DbStoredStates(states);

            var sb = new StringBuilder().Append('(');
            bool first = true;
            foreach (var st in storedStates) {
                if (!first) {
                    sb.Append(',');
                }
                sb.Append((int)st);
                first = false;
            }
            sb.Append(')');
            var result = connection.Query<Puzzle>(
                "SELECT * FROM Puzzle WHERE type = ? AND SolvedConfig = ? AND SolvedState in " + sb.ToString() + " ORDER BY SolvedState;",
                typeStr, solvedStr);
            return result;
        }

        /// <summary>
        /// returns a list of the puzzles whose state matches any of the listed ones.
        /// </summary>
        /// <param name="lref"></param>
        /// <param name="states"></param>
        /// <returns></returns>
        override public int PuzzlesWithState(Level.LevelRef lref, params PuzzleSolvedState[] states) {
            return QueryWithState(lref, states).Count();
        }

        override public List<core.Puzzle> GetPuzzlesWithState(Level level, params PuzzleSolvedState[] states) {
            return QueryWithState(level.Ref, states)
                .Select(
                    p => new core.Puzzle(level, PuzzleState.FromCompactString(p.InitialConfig)))
                .ToList();
        }

        /// <summary>
        /// if the puzzle is not present in the db, it's because it was never played,
        /// and therefore it's unsolved
        /// </summary>
        /// <param name="puzzle"></param>
        /// <returns></returns>
        override public PuzzleSolvedState GetPuzzleState(core.Puzzle puzzle) {
            Puzzle dbPuzzle = GetPuzzle(puzzle);
            if (dbPuzzle == null) {
                return PuzzleSolvedState.Unsolved;
            }
            return GetState(dbPuzzle.SolvedState);
        }

        private Puzzle GetPuzzle(core.Puzzle puzzle) {
            var config = puzzle.initial.ToCompactString();
            var typeStr = puzzle.level.type.ToString();
            var solvedStr = puzzle.level.solved.ToCompactString();

            var result = connection.Query<Puzzle>("SELECT * FROM Puzzle WHERE InitialConfig = ? AND type = ? AND SolvedConfig = ? LIMIT 1",
                config, typeStr, solvedStr);

            return result.Count() == 0 ? null : result.First();
        }
        
        private static int GetStoredState(PuzzleSolvedState state) {
            return
                state == PuzzleSolvedState.Ok ? LevelManager.STARS_OK :
                state == PuzzleSolvedState.Good ? LevelManager.STARS_GOOD :
                state == PuzzleSolvedState.Perfect ? LevelManager.STARS_PERFECT :
                            0;
        }

        private static PuzzleSolvedState GetState(int stars) {
            return
                stars == LevelManager.STARS_OK ? PuzzleSolvedState.Ok :
                stars == LevelManager.STARS_GOOD ? PuzzleSolvedState.Good :
                stars == LevelManager.STARS_PERFECT ? PuzzleSolvedState.Perfect :
                            PuzzleSolvedState.Unsolved;
        }

        override public void SetPlayed(core.Puzzle puzzle) {
            var dbPuzzle = GetPuzzle(puzzle);
            dbPuzzle.LastPlayed = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            connection.Update(dbPuzzle);
        }

        override public void Clear() {
            connection.DeleteAll<Puzzle>();
        }

        private class StatQuery {
            public int SolvedState { get; set; }
            public int Num { get; set; }
        }

        private static SolvedStats FillStats(List<StatQuery> stats) {
            int unsolved = 0;
            int ok = 0;
            int good = 0;
            int perfect = 0;
            foreach (var stat in stats) {
                switch (stat.SolvedState) {
                    case 0: unsolved = stat.Num; break;
                    case 1: ok = stat.Num; break;
                    case 2: good = stat.Num; break;
                    case 3: perfect = stat.Num; break;
                }
            }
            return new SolvedStats(unsolved, ok, good, perfect);
        }

        override public SolvedStats GetTypeStats(Level.LevelType type) {
            string q = "SELECT DISTINCT SolvedState, count(*) AS Num FROM Puzzle WHERE type = ? GROUP BY SolvedState;";
            var results = connection.Query<StatQuery>(q, new object[] { type.ToString() });
            return FillStats(results);
        }

        override public Dictionary<Level.LevelRef, SolvedStats> GetLevelStats(IEnumerable<Level.LevelRef> lrefs) {
            var ret = new Dictionary<Level.LevelRef, SolvedStats>();
            foreach (var lref in lrefs) {
                ret.Add(lref, GetLevelStats(lref));
            }
            return ret;
        }

        override public SolvedStats GetLevelStats(Level.LevelRef lref) {
            string q = "SELECT DISTINCT SolvedState, count(*) AS Num FROM Puzzle WHERE type = ? AND SolvedConfig = ? GROUP BY SolvedState;";
            var typeStr = lref.type.ToString();
            var solvedStr = lref.cc.ToCompactString();
            var results = connection.Query<StatQuery>(q, new object[] { typeStr, solvedStr });
            return FillStats(results);
        }

        override public IEnumerable<Level.LevelRef> GetLevelsContaining(params PuzzleSolvedState[] state) {
            var stStr = string.Join(",", state.Select((st) => { return ((int)st).ToString(); }).ToArray());
            string q = "SELECT DISTINCT Type, SolvedConfig FROM Puzzle WHERE SolvedState in ( " + stStr + ") GROUP BY Type, SolvedConfig;";
            var results = connection.Query<Puzzle>(q);
            return results.Select( p => Level.LevelRef.FromCompactString(p.Type + "/" + p.SolvedConfig));
        }

        override public void SetLevelPlayed(Level.LevelRef lref) {
            connection.InsertOrReplace(new LevelPlayed {
                LevelRef = lref.ToCompactString(),
                Timestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond
            });
        }

        override public int ClearBadges() {
            return connection.DeleteAll<Badge>();
        }

        override public int AddEarnedBadge(core.Badge badge) {
            try {
                
                return connection.Insert(new Badge {
                    BadgeId = (int)badge,
                    Timestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond
                });
            } catch (Exception e) {
                Debug.LogWarningFormat("Error inserting badge: {0}", e);
                return -1;
            }
        }

        override public IEnumerable<Level.LevelRef> GetPlayedLevels() {
            return connection.Table<LevelPlayed>()
                .ToList()
                .Select(dbLb => Level.LevelRef.FromCompactString(dbLb.LevelRef));
        }

        override public IEnumerable<core.Badge> GetEarnedBadges() {
            return connection.Table<Badge>()
                .ToList()
                .Select(dbBadge => (core.Badge)dbBadge.BadgeId);
        }

        public override void Close() {
            connection.Close();
        }
    }
}
