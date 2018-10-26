using UnityEngine;
using com.perroelectrico.flip.util;
using com.perroelectrico.flip.core;

namespace com.perroelectrico.flip.controller {

    public class CoinController : MonoBehaviour {

        private readonly Color HeldColor = new Color(0.5f, 0.5f, 0.5f, 0);
        private readonly Color ReleasedColor = new Color(0, 0, 0, 0);

        private Color sourceColor = new Color(0, 0, 0, 0);
        private Color targetColor = new Color(0.5f, 0.5f, 0.5f, 0);

        public Material simpleMaterial;
        public Material plainMaterial;

        private readonly float HaloAnimationTime = 0.3f;
        private float animationTime = 1;

        public bool isMoving;

        private Level.LevelType type;
        private int colorIndex;

        private MeshRenderer mr;

        /// <summary>
        /// Bounds of the coin object before starting rotation
        /// </summary>
        public Rect InitialBound { get; private set; }

        void Awake() {
            mr = GetComponent<MeshRenderer>();
        }

        private static object matPoolLock = new object();
        private static MaterialPool<MaterialKey> matPool;
        private MaterialPool<MaterialKey> MatPool {
            get {
                lock (matPoolLock) {
                    if (matPool == null) {
                        poolMaterial = simpleMaterial;
                        matPool = new MaterialPool<MaterialKey>(GenerateMaterial);
                    }
                }
                return matPool;
            }
        }

        private struct MaterialKey {
            public Level.LevelType type;
            public int colorIndex;
        }

        public static Material poolMaterial;
        private static Material GenerateMaterial(MaterialKey cc) {
            Material mat = GameObject.Instantiate<Material>(poolMaterial);
            mat.SetInt("_SymbolIndex", cc.colorIndex);
            if (cc.type == Level.LevelType.sided) {
                mat.SetFloat("_SymbolPos", 0);
            }
            mat.SetColor("_ReplaceColor", ColorSet.colors[cc.colorIndex]);
            return mat;
        }

        public void Hold() {
            sourceColor = ReleasedColor;
            targetColor = HeldColor;
            animationTime = 0;
            mr = mr ?? GetComponent<MeshRenderer>();
            InitialBound = BoundsToScreenRect(mr.bounds);
        }

        private static Rect BoundsToScreenRect(Bounds bounds) {
            var topLeft = Camera.main.WorldToScreenPoint(bounds.center - bounds.extents);
            var bottomRight = Camera.main.WorldToScreenPoint(bounds.center + bounds.extents);
            return new Rect(topLeft.x, topLeft.y, (bottomRight - topLeft).x, (bottomRight - topLeft).y);
        }

        public void Release() {
            sourceColor = HeldColor;
            targetColor = ReleasedColor;
            animationTime = 0;
        }

        void Update() {
            if (animationTime < 1) {
                animationTime = Mathf.Min(1, animationTime + Time.deltaTime / HaloAnimationTime);
                Color newColor = Color.Lerp(sourceColor, targetColor, animationTime);
                mr.material.SetColor("_Halo", newColor);
                // if possible, revert to pooled material
                if (animationTime == 1 && targetColor == ReleasedColor && (type == Level.LevelType.sided || type == Level.LevelType.simple)) {
                    mr.material = MatPool.GetMaterial(new MaterialKey { colorIndex = colorIndex, type = type });
                }
            }
        }

        void Destroy() {
            if (mr != null) {
                Destroy(mr.material);
            }
        }

        public void Configure(Level.LevelType type, PuzzleState state, int index, Texture image = null) {
            this.type = type;
            if (type == Level.LevelType.sided || type == Level.LevelType.simple) {
                this.colorIndex = state.ColorIndex(index);
                mr.material = MatPool.GetMaterial(new MaterialKey { colorIndex = colorIndex, type = type });
            } else {
                mr.material = plainMaterial;
                if (type == Level.LevelType.image) {
                    mr.material.SetTexture("_MainTex", image);

                    float vInc = (float)(1f / state.Count);

                    float startV = vInc * state.ColorIndex(index);
                    var mesh = GetComponent<MeshFilter>().mesh;
                    mesh.uv = GeneratePiecesUVs(vInc, startV);
                    mr.material.color = Color.white;
                }
            }
        }

        private static Vector2[] GeneratePiecesUVs(float vInc, float startV) {
            return new Vector2[] {
                        new Vector2(startV, 0),
                        new Vector2(startV + vInc, 1),
                        new Vector2(startV + vInc, 0),
                        new Vector2(startV, 1)
                    };
        }
    }
}