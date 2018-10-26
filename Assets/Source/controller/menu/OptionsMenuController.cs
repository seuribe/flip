using UnityEngine;
using System.Collections;
using System.Linq;
using com.perroelectrico.flip.util;
using com.perroelectrico.flip.core;
using com.perroelectrico.flip.controller.ui;
using UnityEngine.Audio;

namespace com.perroelectrico.flip.controller {

    public class OptionsMenuController : MonoBehaviour, IMenuController {

        public const string ID = "Options";

        public AudioMixer mixer;

        public MultiOptionSwitch languageSwitch;
        public AudioOptionsController audioOptionsController;

        private TextResource tr;

        void Awake() {

            languageSwitch.options = Localization.LANG_DESC.Select(
                    (lang) => { return new MultiOptionSwitch.Option { display = lang.name, value = lang.id };}
                ).ToArray();

            languageSwitch.OnValueChanged += (val) => {
                Settings.Instance.SetLanguage(val);
                Tracking.Track(Tracking.OPTIONS_LANGUAGE + ":" + val);
                SetLanguage(val);
            };
        }

        void Start() {
            tr = TextResource.Get("menu");
        }

        public string Id() {
            return ID;
        }

        private void SetLanguage(string newLanguage) {
            languageSwitch.SetTexts(tr.Filter("menu.options.Language"));
        }

        public void DoBeforeArrival() {
            languageSwitch.Set(Settings.Instance.Language);
            SetLanguage(null);
        }

        public void DoAfterArrival() { }
        public void DoOnLeaving() { }
        public void Execute(string cmd) {
            switch (cmd) {
                case "Reset":
                    resetDialog = Dialog.ShowDialog(tr["dialog.Reset"],
                        new Dialog.DialogButton[] {
                            new Dialog.DialogButton { id = "Yes", onClick = ResetPlayData, text = tr["general.Yes"]},
                            new Dialog.DialogButton { id = "Back", text = tr["general.No"]}
                        }).GetComponent<Dialog>();
                    audioOptionsController.musicVolumeBar.AllowInput = false;
                    audioOptionsController.effectsVolumeBar.AllowInput = false;
                    resetDialog.OnClose += () => {
                        StartCoroutine(ReEnableInput());
                    };
                    break;
            }
        }

        IEnumerator ReEnableInput() {
            yield return new WaitForSeconds(0.1f);
            audioOptionsController.musicVolumeBar.AllowInput = true;
            audioOptionsController.effectsVolumeBar.AllowInput = true;
            yield return null;
        }

        private Dialog resetDialog;
        private void ResetPlayData(string id) {
            Debug.Log("Reset play data");
            resetDialog.Close();
            LevelManager.Instance.ResetPlayData();

            if (Application.isEditor) {
                BadgeManager.Instance.Reset();
            }
        }

        public void Back() {
            var mm = FindObjectOfType<MenuManager>() as MenuManager;
            mm.ShowMenu(MainMenuController.ID);
        }
    }
}