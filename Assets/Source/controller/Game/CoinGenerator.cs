using UnityEngine;
using com.perroelectrico.flip.core;
using com.perroelectrico.flip.util;

namespace com.perroelectrico.flip.controller {

    /// <summary>
    /// Generates the pieces (GameObjects) for a puzzle configuration
    /// </summary>
    public class CoinGenerator : MonoBehaviour {
        public Vector3 center = new Vector3(0, 0, 0);
        public Vector3 scale = new Vector3(1, 1, 1);
        public float separation = 0.25f;
        public GameObject coinOriginal;
        public string baseCoinName;

        public string puzzleState;

        public bool cleanBeforeGenerating = true;

        private GameObject[] coins;
        private float defaultSeparation;

        public Bounds Bounds {
            get {
                if (coins == null) {
                    return new Bounds(center, Vector3.zero);
                }
                var first = coins[0];
                var last = coins[coins.Length - 1];
                var origBounds = first.GetComponentInChildren<MeshRenderer>().bounds;
                return new Bounds(center, (last.transform.position - first.transform.position) / 2 + origBounds.extents);
            }
        }

        private Animation anim;
        private Animation Anim {
            get {
                if (anim == null) {
                    anim = gameObject.GetComponent<Animation>();
                    if (anim == null) {
                        anim = gameObject.AddComponent<Animation>();
                    }
                }
                return anim;
            }
        }

        void Start() {
            defaultSeparation = separation;
            if (puzzleState != null && !puzzleState.Equals("")) {
                GenerateCoins(PuzzleState.FromString(puzzleState));
            }
        }

        public void ClearCoins() {
            if (coins != null) {
                foreach (var c in coins) {
                    GameObject.DestroyImmediate(c);
                }
            }
            coins = null;
        }

        /// <summary>
        /// Generates the coins for all possible puzzle types
        /// </summary>
        /// <param name="state"></param>
        /// <param name="level"></param>
        /// <param name="image">if specified, it will override the level image (only used for image type levels)</param>
        /// <returns></returns>
        public GameObject[] GeneratePuzzle(PuzzleState state, Level level, Texture image = null) {
            GenerateCoins(state);
            for (int i = 0 ; i < coins.Length ; i++) {
                var coin = coins[i];
                coin.GetComponent<CoinController>().Configure(level.type, state, i, image ?? level.Texture);
            }
            return coins;
        }

        private GameObject[] GenerateCoins(PuzzleState state) {
            if (cleanBeforeGenerating && coins != null) {
                ClearCoins();
            }

            int nCoins = state.Count;
            coins = new GameObject[nCoins];
            for (int i = 0 ; i < nCoins ; i++) {
                var coin = Instantiate(coinOriginal);
                coin.name = baseCoinName + "_" + i;
                coin.transform.parent = gameObject.transform;
                coin.transform.localPosition = GetCoinLocalPosition(i, nCoins, separation);
                coin.transform.localScale = scale;
                coin.transform.localRotation = state.Rotated(i) ? Quaternion.Euler(new Vector3(0, 0, 180)) : new Quaternion();
                coins[i] = coin;
            }
            return coins;
        }

        public Vector3 GetCoinLocalPosition(int index, int total, float separation) {
            Vector3 sep = new Vector3(separation * scale.x, 0, 0);
            Vector3 pos = center - (sep * ((float)(total - 1) / 2));
            return pos + (sep * index);
        }

        void OnDrawGizmos() {
            Gizmos.color = Color.white;
            Vector3 l = gameObject.transform.rotation * new Vector3(1.5f * scale.x, 0, 0);
            Gizmos.DrawLine(gameObject.transform.position + center - l, gameObject.transform.position + center + l);
        }

        private void ReplaceAndPlay(AnimationClip clip, string name) {
            Anim.AddClip(clip, name);
            Anim.Play(name);
        }

        private AnimationClip GetClip(string name) {
            var clip = Anim.GetClip(name);
            if (clip == null) {
                clip = new AnimationClip();
                clip.legacy = true;
                Anim.AddClip(clip, name);
            }
            return clip;
        }

        private void AnimateImage(float time, float separation, float ratio) {
            if (coins == null)
                return;

            Anim.Stop();
            Anim.wrapMode = WrapMode.Once;
            Anim.playAutomatically = false;
            Anim.Rewind();
            var clipBuilder = new ClipBuilder(GetClip("separate"));

            for (int i = 0 ; i < coins.Length ; i++) {
                var coin = coins[i];
                var coinLocalPosition = GetCoinLocalPosition(i, coins.Length, separation);
                clipBuilder.LocalPosition(coins[i], coin.transform.position, coinLocalPosition, time);
            }
            clipBuilder.LocalScale(gameObject.transform.localScale, Vector3.one * ratio, time);
            ReplaceAndPlay(clipBuilder.Clip, "separate");
        }

        public void AnimateBigImage(float time, float ratio) {
            AnimateImage(time, 1, ratio);
        }

        public void AnimateRestoreImage(float time) {
            AnimateImage(time, defaultSeparation, 1);
        }

        private void AnimateSeparation(float time, float newSeparation) {
            Anim.wrapMode = WrapMode.Once;
            Anim.playAutomatically = false;
            Anim.Rewind();

            var clipBuilder = new ClipBuilder(GetClip("separate"));
            for (int i = 0 ; i < coins.Length ; i++) {
                var coin = coins[i];
                var coinLocalPosition = GetCoinLocalPosition(i, coins.Length, newSeparation);
                clipBuilder.LocalPosition(coin, coin.transform.position, coinLocalPosition, time);
            }
            float scale = separation != 0 ? 0.95f : 1f;
            clipBuilder.LocalScale(gameObject.transform.localScale, Vector3.one * scale, time);
            ReplaceAndPlay(clipBuilder.Clip, "separate");
            separation = newSeparation;
        }

        public void AnimateSeparation(float time) {
            AnimateSeparation(time, 0);
        }

        public void AnimateRestoreSeparation(float time) {
            AnimateSeparation(time, defaultSeparation);
        }
    }
}