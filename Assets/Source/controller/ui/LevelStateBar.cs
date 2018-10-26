using UnityEngine;
using System.Collections;
using com.perroelectrico.flip.core;

namespace com.perroelectrico.flip.controller.ui {

    public class LevelStateBar : MonoBehaviour {

        public void SetStats(SolvedStats stats) {
            int total = stats.NumUnsolved + stats.NumOk + stats.NumPerfect + stats.NumGood;
            Ok = (float)stats.NumOk / total;
            Good = (float)stats.NumGood / total;
            Perfect = (float)stats.NumPerfect / total;
        }

        public float Perfect {
            set {
                GetComponent<MeshRenderer>().material.SetFloat("_Percent3", value);
            }
        }
        public float Good {
            set {
                GetComponent<MeshRenderer>().material.SetFloat("_Percent2", value);
            }
        }
        public float Ok {
            set {
                GetComponent<MeshRenderer>().material.SetFloat("_Percent1", value);
            }
        }
    }
}