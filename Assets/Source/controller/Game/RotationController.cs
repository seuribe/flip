using UnityEngine;
using com.perroelectrico.flip.core;

namespace com.perroelectrico.flip.controller {
    public class RotationController {
        private GameObject rotatingBase;
        private readonly GameObject[] pieceObjects;

        public RotationController(PuzzleState puzzleState, Move currentMove, GameObject[] pieceObjects, ref Vector2 rotationScreenCenter) {
            this.pieceObjects = pieceObjects;

            int first = puzzleState.FirstCoinAffectedIndex(currentMove);
            int n = puzzleState.NumCoinsAffected(currentMove);

            GameObject firstCoin = pieceObjects[first];
            GameObject lastCoin = pieceObjects[first + n - 1];
            Vector3 originalPos = (firstCoin.transform.position + lastCoin.transform.position) / 2;

            rotatingBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rotatingBase.transform.position = originalPos;
            rotatingBase.transform.localScale = new Vector3((lastCoin.transform.position - firstCoin.transform.position).x + 0.1f, 0.1f, 1f);
            rotatingBase.GetComponent<Renderer>().enabled = false;

            for (int i = first ; i < first + n ; i++) {
                pieceObjects[i].transform.parent = rotatingBase.transform;
                pieceObjects[i].GetComponent<CoinController>().isMoving = true;
            }
            rotationScreenCenter = Camera.main.WorldToScreenPoint(rotatingBase.GetComponent<Renderer>().bounds.center);
            rotatingBase.transform.position -= Vector3.forward;
        }

        /// <summary>
        /// Destroys the dummy rotation object, and detaches coins from it
        /// </summary>
        public void ReleasePieces(GameObject piecesParent) {
            rotatingBase.transform.position += Vector3.forward;

            rotatingBase.transform.DetachChildren();
            if (rotatingBase != null) {
                GameObject.Destroy(rotatingBase);
                foreach (GameObject cc in pieceObjects) {
                    cc.GetComponent<CoinController>().isMoving = false;
                    cc.transform.parent = piecesParent.transform;
                }
                rotatingBase = null;
            }
        }

        public void SetAngle(float shownAngle) {
            Quaternion newRot = Quaternion.Euler(new Vector3(0, 0, shownAngle));
            rotatingBase.transform.rotation = newRot;
        }

        public void AttachFingerTracking(GameObject fingerTrack) {
            fingerTrack.transform.parent = rotatingBase.transform;
        }
    }
}