using UnityEngine;
using System.Collections;
using com.perroelectrico.flip.core;
using com.perroelectrico.flip.util;
using System;

namespace com.perroelectrico.flip.controller.ui {

    public class BadgeController : MonoBehaviour {

        public float UnobtainedAlpha = 0.25f;

        private const int BadgeColumns = 4;
        private const int BadgeRows = 8;

        public Color haloColor = new Color(0.2f, 0.2f, 0.2f, 0f);
        public Color haloOffColor = new Color(0f, 0f, 0f, 0f);

        private Color targetHalo;
        private Color currentHalo;

        private MeshRenderer mr;
        public TextMesh description;
        private TextResource tr;
        private TextTranslationController ttc;

        public Badge badge;
        private string badgeName;

        public Badge Badge {
            set {
                badge = value;
                Init();
            }
        }

        void Awake() {
            mr = GetComponent<MeshRenderer>();
            ttc = GetComponentInChildren<TextTranslationController>();
            if (mr == null || ttc == null) {
                Debug.LogErrorFormat("Wrong game object for Badge, mr:{0}, ttc:{1}", mr, ttc);
            }
            targetHalo = haloOffColor;
            currentHalo = haloOffColor;
        }

        void Start() {
            Init();
        }

        void Init() {
            float offsetX = (float)((int)badge % BadgeColumns) / BadgeColumns;
            float offsetY = (float)((int)badge / BadgeColumns) / BadgeRows;

            mr.material.SetTextureOffset("_MainTex", new Vector2(offsetX, offsetY));

            badgeName = Enum.GetName(typeof(Badge), badge);

            ttc.ResourceId = "badge." + badgeName + ".Title";
            tr = TextResource.Get("badges").Filter("badge");

            var bm = BadgeManager.Instance;
            if (!bm.HasEarned(badge)) {
                mr.material.SetFloat("_Alpha", UnobtainedAlpha);
            }
        }

        void OnMouseEnter() {
            // need to force reload of resources, as it does not seem to work here. -- Seu
            tr = TextResource.Get("badges").Filter("badge");

            targetHalo = haloColor;
            if (description != null) {
                description.text = tr[badgeName + ".Desc"];
            }
        }

        void OnMouseExit() {
            targetHalo = haloOffColor;
            if (description != null) {
                description.text = "";
            }
        }

        void OnMouseOver() { }

        void Update() {
            if (targetHalo.Equals(currentHalo)) {
                return;
            }
            currentHalo = Color.Lerp(currentHalo, targetHalo, 0.05f);
            mr.material.SetColor("_Halo", currentHalo);
        }
    }
}