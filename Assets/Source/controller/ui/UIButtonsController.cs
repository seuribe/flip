using UnityEngine;
using com.perroelectrico.flip.audio;
using com.perroelectrico.flip.util;
using UnityEngine.SceneManagement;

namespace com.perroelectrico.flip.controller.ui {

    public class UIButtonsController : MonoBehaviour {

        public const string SHARE_LEVEL_MASTERED = "ShareLevelMastered";
        public const string SHARE_LEVEL_FINISHED = "ShareLevelFinished";

        private const string CommandGamePrefix = "Game";
        private const string CommandCloseDialog = "CloseDialog";
        private const string CommandBack = "Back";
        private const string CommandGotoMenu = "GotoMenu";

        public string id;
        public Color normalColor = ColorUtils.ColorFromInt(0xFF, 0xA1, 0x10, 0xFF);
        public Color overColor = ColorUtils.ColorFromInt(0xFF, 0x44, 0x44, 0xFF);
        public EffectsManager.AudioEvent eventType;
        private EffectsManager effectsManager;

        private TextMesh text;

        public delegate void ButtonEvent(string id);
        public event ButtonEvent OnClick;

        void Awake() {
            effectsManager = EffectsManager.Instance;
        }

        void Start() {
            text = gameObject.GetComponentInChildren<TextMesh>();
            if (text != null) {
                text.color = normalColor;
            }
        }

#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX
        void OnMouseEnter() {
            if (text != null)
                gameObject.GetComponent<MeshRenderer>().material.color = overColor;
        }

        void OnMouseExit(){
            if (text != null)
                gameObject.GetComponent<MeshRenderer>().material.color = normalColor;
        }
#endif

        void Update() {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) {
                ProcessInput(Input.GetTouch(0).position);
            } else if (Input.GetMouseButtonDown(0)) {
                ProcessInput(Input.mousePosition);
            }
        }

        private void ProcessInput(Vector2 pos) {
            Ray ray = Camera.main.ScreenPointToRay(pos);

            if (GetComponent<Collider>().bounds.IntersectRay(ray)) {
                if (effectsManager != null && eventType != EffectsManager.AudioEvent.None) {
                    effectsManager.PlayEvent(eventType);
                }
                if (id != null && !id.Equals("")) {
                    Execute(id);
                }
                OnClick?.Invoke(id);
            }
        }

        public static void Execute(string id) {
            Debug.Log("Execute cmd: " + id);
            int _index = id.IndexOf('_');
            var prefix = (_index != -1) ? id.Substring(0, _index) : id;
            var remainder = (_index != -1) ? id.Substring(_index + 1) : id;

            if (MenuManager.Current != null && MenuManager.Current.IsValidMenuId(prefix)) {
                MenuManager.Current[prefix].Execute(remainder);
            } else if (prefix.Equals(CommandGamePrefix)) {
                GameController controller = FindObjectOfType<GameController>() as GameController;
                controller?.Execute(remainder);
            } else {
                switch (id) {
                    case CommandBack:
                        MenuManager.Current.Back();
                        break;
                    case CommandGotoMenu:
                        SceneManager.LoadSceneAsync("Main");
                        break;
                }
            }
        }

        public void ClearOnClick() {
            OnClick = null;
        }
    }
}