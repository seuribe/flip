using UnityEngine;
using com.perroelectrico.flip.controller.ui;
using com.perroelectrico.flip.util;

namespace com.perroelectrico.flip.controller {

    public class MovesDisplayController : MonoBehaviour {

        public ButtonController movesSign;

        private int current;
        private int max;
        private TextResource tr;

        public int Moves => current;

        void Start() {
            tr = TextResource.Get("gui").Filter("game.gui");
        }

        public void Hide() {
            movesSign.Hide();
        }

        public void Show() {
            movesSign.Show();
        }

        public void Init(int current, int max, bool solved) {
            SetMax(max);
            SetValue(current, solved);
        }

        public void SetMax(int max) {
            this.max = max;
            UpdateMovesText();
        }

        private void UpdateMovesText() {
            movesSign.Text = string.Format("{0}: {1} / {2}", tr["Moves"], current, max);
        }

        public void SetValue(int val, bool solved) {
            var old = current;
            current = val;
            UpdateMovesText();
            if (val == max && old < val && !solved) {
                movesSign.Blink();
            }
        }
    }
}