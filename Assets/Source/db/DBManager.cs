using com.perroelectrico.flip.core;
using SQLite4Unity3d;
using System;
using System.Collections.Generic;
using System.Linq;

namespace com.perroelectrico.flip.db {

    public abstract class DBManager {
        public static DBManager Instance {
            get {
#if UNITY_WEBGL
                return MemoryDBManager.Instance;
#else
                return SQLDBManager.Instance;
#endif
            }
        }

        abstract public bool IsInitialized();
        abstract public bool AddPuzzles(List<core.Puzzle> puzzles);
        abstract public void SetPuzzleState(core.Puzzle puzzle, PuzzleSolvedState state);
        abstract public int PuzzlesWithState(Level.LevelRef lref, params PuzzleSolvedState[] states);
        abstract public List<core.Puzzle> GetPuzzlesWithState(Level level, params PuzzleSolvedState[] states);
        abstract public PuzzleSolvedState GetPuzzleState(core.Puzzle puzzle);
        abstract public void SetPlayed(core.Puzzle puzzle);
        abstract public void Clear();
        abstract public SolvedStats GetTypeStats(Level.LevelType type);
        abstract public Dictionary<Level.LevelRef, SolvedStats> GetLevelStats(IEnumerable<Level.LevelRef> lrefs);
        abstract public SolvedStats GetLevelStats(Level.LevelRef lref);
        abstract public IEnumerable<Level.LevelRef> GetLevelsContaining(params PuzzleSolvedState[] state);
        abstract public int ClearBadges();
        abstract public int AddEarnedBadge(core.Badge badge);
        abstract public IEnumerable<core.Badge> GetEarnedBadges();
        abstract public void SetLevelPlayed(Level.LevelRef lref);
        abstract public IEnumerable<Level.LevelRef> GetPlayedLevels();

        abstract public void Close();
    }
}
