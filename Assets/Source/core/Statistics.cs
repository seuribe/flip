using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.perroelectrico.flip.core {

    public class Statistics {

        public delegate void StatChangeEvent(Stats st, int newValue);
        
        public enum Stats {
            FlipsDone,
            FlipsUndone,
            TotalPuzzlesPlayed,
            Stars,
            PuzzlesSolved,
        }

        private static Statistics statistics;
        public static Statistics Instance {
            get {
                return statistics = statistics ?? new Statistics();
            }
        }

        public event StatChangeEvent StatChanged;

        private Dictionary<Stats, int> stats;

        public int this[Stats key] {
            get {
                return stats.ContainsKey(key) ? stats[key] : 0;
            }
            set {
                SetValue(key, value);
                if (StatChanged != null) {
                    StatChanged(key, value);
                }
            }
        }

        private Statistics() {
            stats = new Dictionary<Stats, int>();
            foreach (Stats stat in Enum.GetValues(typeof(Stats))) {
                stats.Add(stat, PlayerPrefs.GetInt("STAT_" + stat.ToString(), 0));
            }
        }

        /// <summary>
        /// Sets a value without forcing any external change. Used to modify the value from steam or other
        /// external sources
        /// </summary>
        /// <param name="stats"></param>
        /// <param name="value"></param>
        internal void SetValue(Stats stat, int value) {
            if (stats.ContainsKey(stat)) {
                stats[stat] = value;
            } else {
                stats.Add(stat, value);
            }
            PlayerPrefs.SetInt("STAT_" + stat.ToString(), value);
        }
    }
}