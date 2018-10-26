using UnityEngine;
using System.Collections;
using System;
using System.Text;
using com.perroelectrico.flip.util;
using com.perroelectrico.flip.core;
using com.perroelectrico.flip.controller.ui;

namespace com.perroelectrico.flip.controller {

    public class StatsMenuController : MonoBehaviour, IMenuController {

        public const string ID = "Stats";

        public TextMesh flipsDoneTitle;
        public TextMesh flipsUndoneTitle;
        public TextMesh totalPuzzlesTitle;
        public TextMesh flipsDone;
        public TextMesh flipsUndone;
        public TextMesh totalPuzzles;
        public TextMesh starCount;

        public LevelStateBar simpleBar;
        public LevelStateBar sidedBar;
        public LevelStateBar wiredBar;
        public LevelStateBar imageBar;

        private MenuManager mm;

        public string Id() {
            return ID;
        }

        void Start() {
            mm = GameObject.FindObjectOfType<MenuManager>() as MenuManager;
            var vngo = GameObject.Find("VersionNumber");
            if (vngo != null) {
                vngo.GetComponent<TextMesh>().text = Resources.Load<TextAsset>("Texts/version").text;
            }
        }

        public void DoBeforeArrival() {
            var manager = LevelManager.Instance;
            var stats = Statistics.Instance;

            simpleBar.SetStats(manager.GetTypeStats(Level.LevelType.simple));
            sidedBar.SetStats(manager.GetTypeStats(Level.LevelType.sided));
            wiredBar.SetStats(manager.GetTypeStats(Level.LevelType.wired));
            imageBar.SetStats(manager.GetTypeStats(Level.LevelType.image));

            flipsDone.text = "" + stats[Statistics.Stats.FlipsDone];
            flipsUndone.text = "" + stats[Statistics.Stats.FlipsUndone];
            totalPuzzles.text = "" + stats[Statistics.Stats.TotalPuzzlesPlayed];
            starCount.text = "" + LevelManager.Instance.Stars;
        }

        public void DoAfterArrival() { }

        public void DoOnLeaving() { }

        public void Back() {
            mm.ShowMenu(MainMenuController.ID);
        }

        public void Execute(string cmd) {
            switch (cmd) {
                case BadgesMenuController.ID:
                    mm.ShowMenu(BadgesMenuController.ID);
                    break;
            }
        }
    }
}