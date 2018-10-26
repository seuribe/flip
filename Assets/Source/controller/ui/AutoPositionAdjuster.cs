using UnityEngine;
using System.Collections;

namespace com.perroelectrico.flip.controller.ui {


    /// <summary>
    /// On Start will test the current aspect ratio, and if it matches with some of the
    /// specified conditions, will adjust object position according to the displacement
    /// configured and stop checking the rest.
    /// 
    /// Made originally for Flip game (http://perroelectrico.com/flip) by Sebastian Uribe
    /// 
    /// </summary>
    public class AutoPositionAdjuster : MonoBehaviour {

        public enum ConditionOperator {
            // Will always match
            Always,
            // Match if current aspect ratio is greater than the one specified
            GreaterThat,
            // Match if current aspect ratio is equal to the one specified
            Equals,
            // Match if current aspect ratio is less than the one specified
            LessThat
        }

        [System.Serializable]
        public class Condition {
            public ConditionOperator condition;
            public int width;
            public int height;

            [Range(0, 1)]
            // Margin of error tolerable in the comparision
            public float margin = 0.1f;
            // How much should the object move if the condition is met
            public Vector3 displace;

            // Does this condition apply for screen width and height (w, h)
            internal bool Matches(int w, int h) {
                float lhand = w * height;
                float rhand = width * h;
                var actualMargin = lhand * margin;

                switch (condition) {
                    case ConditionOperator.Always:
                        return true;
                    case ConditionOperator.GreaterThat:
                        return lhand - actualMargin > rhand;
                    case ConditionOperator.Equals:
                        return lhand + actualMargin > rhand && lhand - actualMargin < rhand;
                    case ConditionOperator.LessThat:
                        return lhand + actualMargin < rhand;
                }
                return false;
            }
        }

        public Condition[] conditions;

        void Start() {

            foreach (var cond in conditions) {
                if (cond.Matches(Screen.width, Screen.height)) {
                    gameObject.transform.localPosition += cond.displace;
                    break;
                }
            }
        }
    }
}