using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.perroelectrico.flip.core {

    /// <summary>
    /// Reproduces a Move list on the CoinController
    /// </summary>
    class FlipPlayer {
        private FlipExecutor controller;
        private List<Move> moves;

        public FlipEvent Finished;

        public FlipPlayer(FlipExecutor controller) {
            this.controller = controller;
        }

        public void Play(List<Move> moves) {
            if (this.moves != null) {
                return;
            }
            this.moves = new List<Move>(moves);
            controller.FlipFinished += DoNextFlip;
            DoNextFlip(new FlipEventArgs());
        }

        internal void DoNextFlip(FlipEventArgs args) {
            if (moves == null || moves.Count == 0) {
                Stop();
                if (Finished != null) {
                    Finished(new FlipEventArgs());
                }
                return;
            }
            Move next = moves[0];
            moves.RemoveAt(0);
            controller.Flip(next);
        }

        public void Stop() {
            moves = null;
            controller.FlipFinished -= DoNextFlip;
        }

        public void Pause() {
            controller.FlipFinished -= DoNextFlip;
        }

        public void Resume() {
            controller.FlipFinished += DoNextFlip;
            DoNextFlip(new FlipEventArgs());
        }
    }
}