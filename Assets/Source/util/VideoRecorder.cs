using UnityEngine;

namespace com.perroelectrico.flip.util {

    public class VideoRecorder : MonoBehaviour {

        public string baseFilename;
        public int fps;
        public bool recording = false;
        public KeyCode startRecordingKey = KeyCode.R;
        public KeyCode stopRecordingKey = KeyCode.S;

        private int imageIndex;
        private float lastFrameTime = 0;

        private string GenerateFilename() {
            return string.Format("{0}_{1,5:D5}.png", baseFilename, imageIndex);
        }

        void Update() {
            if (Input.GetKeyDown(startRecordingKey)) {
                recording = true;
            } else if (Input.GetKeyDown(stopRecordingKey)) {
                recording = false;
            }

            if (recording == true) {
                RecordImages();
            }
        }

        void RecordImages() {
            if (lastFrameTime < Time.time + (1 / fps)) {
                ScreenCapture.CaptureScreenshot(GenerateFilename());
                imageIndex++;
                lastFrameTime = Time.time;
            }
        }
    }
}