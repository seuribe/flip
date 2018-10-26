using UnityEngine;
using com.perroelectrico.flip.core;

namespace com.perroelectrico.flip.controller {

    public class SolutionDisplayController : MonoBehaviour {
        private const float CoinSizeRatio = 4f;

        public float MaxHeight = 2f;

        public CoinGenerator generator;
        public MeshRenderer imageRenderer;

        public void Hide() {
            gameObject.SetActive(false);
        }

        public void Show() {
            gameObject.SetActive(true);
        }

        public GameObject[] SetPuzzle(Puzzle puzzle) {
            var level = puzzle.level;
            imageRenderer.enabled = level.IsImageLevel;
            generator.ClearCoins();
            if (level.IsImageLevel) {
                var texture = level.Texture;
                imageRenderer.material.mainTexture = texture;
                var ratio = (float)level.NumCoins / CoinSizeRatio;
                imageRenderer.gameObject.transform.localScale = new Vector3(MaxHeight * ratio, MaxHeight, 1);
                return null;
            }
            return generator.GeneratePuzzle(level.solved, level);
        }
    }
}