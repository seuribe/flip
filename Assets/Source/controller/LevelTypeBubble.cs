using UnityEngine;
using System.Collections.Generic;
using com.perroelectrico.flip.core;
using com.perroelectrico.flip.util;
using com.perroelectrico.flip.controller.ui;

namespace com.perroelectrico.flip.controller {

    public class LevelTypeBubble : MonoBehaviour {

        public Level.LevelType type;
        public CoinGenerator generator;
        public Transform levelMenuCenter;

        // TODO: move this prefab/dialog form here to another place.. probably the StartMenuController
        public static GameObject typeDialog;
        public GameObject typeDialogPrefab;
        public UIButtonsController clickStart;

        public GameObject leveInfoPrefab;

        private struct GridSize {
            public int columns;
            public int rows;
        }

        private const int FadeNone = 0;
        private const int FadeIn = 1;
        private const int FadeOut = -1;

        private static int fadeState = FadeNone;
        private static float fadeTime = 0;

        public static float LevelsDisplayTime = 0.4f;
        public static float FadeTime = 1f;
        public static float MaxBackAlpha = 0.9f;

        private static Dictionary<Level.LevelType, GridSize> grids;

        static LevelTypeBubble() {
            grids = new Dictionary<Level.LevelType, GridSize> {
                { Level.LevelType.simple, new GridSize { columns = 4, rows = 3 } },
                { Level.LevelType.sided, new GridSize { columns = 3, rows = 2 } },
                { Level.LevelType.wired, new GridSize { columns = 2, rows = 2 } },
                { Level.LevelType.image, new GridSize { columns = 2, rows = 2 } }
            };
        }

        void Start() {
            var tr = TextResource.Get("menu").Filter("menu");
            var level = LevelManager.Instance.GetLevel(type, 0);
            generator.GeneratePuzzle(level.solved, level);
            generator.gameObject.SetChildText("Title", tr["type." + type.ToString()]);
            clickStart.id = "";
            clickStart.OnClick += (id) => OpenTypeDialog();
        }

        public void OpenTypeDialog(bool now = false) {
            if (typeDialog != null)
                return;

            typeDialog = GameObject.Instantiate(typeDialogPrefab) as GameObject;
            typeDialog.transform.parent = levelMenuCenter;
            typeDialog.transform.localPosition = Vector3.back;

            var levels = LevelManager.Instance.GetLevels(type);
            var grid = grids[type];

            // use the back, to make sure that the boxes don't overlap
            Bounds linfoBounds = leveInfoPrefab.transform.Find("Back").GetComponent<MeshRenderer>().bounds;

            var coords = Distribute(grid.columns, grid.rows, linfoBounds, new Vector2(0.05f, 0.05f));

            for (int i = 0 ; i < levels.Count ; i++) {
                var linfo = Instantiate(leveInfoPrefab) as GameObject;
                linfo.transform.parent = typeDialog.transform;

                var info = linfo.GetComponent<LevelInfoController>();
                var level = LevelManager.Instance.GetLevel(type, i);

                info.SetLevel(level, coords[i], coords[i], now ? 0 : LevelsDisplayTime);
            }
            fadeState = FadeIn;
        }

        void Update() {
            if (fadeState == FadeNone || typeDialog == null) {
                return;
            }
            var mr = typeDialog.Child("Back").GetComponent<MeshRenderer>();
            float alpha = (fadeTime / FadeTime) * MaxBackAlpha;

            Color color = mr.material.color;
            color.a = alpha;
            mr.material.color = color;

            fadeTime += fadeState * Time.deltaTime;

            if (fadeTime < 0) {
                fadeState = FadeNone;
                fadeTime = 0;
                Destroy(typeDialog);
                typeDialog = null;
            } else if (fadeTime > FadeTime) {
                fadeTime = FadeTime;
                fadeState = FadeNone;
            }
        }

        public void Hide() {
            VisibilitySwitch.SetVisibility(gameObject, false);
        }

        private static Vector3[] Distribute(int cols, int rows, Bounds bounds, Vector2 separation) {
            var ret = new Vector3[rows * cols];

            var totalWidth = (cols * bounds.size.x) + (separation.x * (cols - 1));
            var totalHeight = (rows * bounds.size.y) + (separation.y * (rows - 1));

            Vector3 pos = new Vector3(-totalWidth / 2 + bounds.extents.x, +totalHeight / 2 - bounds.extents.y, -0.5f);
            for (int r = 0 ; r < rows ; r++) {
                for (int c = 0 ; c < cols ; c++) {
                    ret[r * cols + c] = pos;
                    pos.x += bounds.size.x + separation.x;
                }
                pos.y -= (bounds.size.y + separation.y);
                pos.x = - totalWidth / 2 + bounds.extents.x;
            }
            return ret;
        }

        internal static void CloseTypeDialog() {
            if (fadeState != FadeNone)
                return;

            var linfos = typeDialog.GetComponentsInChildren<LevelInfoController>();
            var targetScale = Vector3.one * 0.01f;
            foreach (var linfo in linfos) {
                var clip = new ClipBuilder().LocalScale(linfo.transform.localScale, targetScale, 0, LevelsDisplayTime).Clip;
                linfo.GetComponent<Animation>().AddClip(clip, "animback");
                linfo.GetComponent<Animation>().Play("animback");
            }
            fadeState = FadeOut;
        }
    }
}