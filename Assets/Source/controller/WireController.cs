using UnityEngine;
using System.Collections;
using System;
using com.perroelectrico.flip.core;
using com.perroelectrico.flip.util;

namespace com.perroelectrico.flip.controller {

    public class WireController : MonoBehaviour {

        public LineRenderer lineRenderer;
        public GameObject solWireEnd;
        public GameObject coinWireEnd;
        const float WIRE_Z = -0.1f;

        public static WireController[] Generate(GameController controller, GameObject[] coins, GameObject[] solCoins) {
            return Generate(controller.CurrentLevel, controller.Current, controller, controller.wirePrefab, coins, solCoins);
        }

        private static WireController[] Generate(Level level, PuzzleState cc, GameController controller, GameObject wirePrefab, GameObject[] coins, GameObject[] solCoins) {
            var wires = new WireController[level.NumCoins];
            for (int i = 0 ; i < level.NumCoins ; i++) {
                int targetCoin = level.solved.FirstIndexForColor(controller.Current.ColorIndex(i));

                var wire = (GameObject)Instantiate(controller.wirePrefab);
                var wcontrol = wire.GetComponent<WireController>() as WireController;
                if (controller != null) {
                    wcontrol.Attach(controller, coins[i], solCoins[targetCoin]);
                }
                wires[i] = wcontrol;
            }
            return wires;
        }

        public void Attach(GameController controller, GameObject coin, GameObject solCoin) {
            controller.RotationSet += OnRotationSet;


            var coinBounds = coin.GetComponent<Renderer>().bounds;
            lineRenderer.SetPosition(0, coin.transform.position + new Vector3(0, coinBounds.extents.y * 3 / 4, WIRE_Z));

            var solBounds = solCoin.GetComponent<Renderer>().bounds;
            lineRenderer.SetPosition(1, solCoin.transform.position + new Vector3(0, -(solBounds.extents.y * 3 / 4), WIRE_Z));

            solWireEnd.transform.parent = solCoin.transform;
            solWireEnd.transform.localPosition = new Vector3(0, -(solBounds.extents.y * 3 / 4), WIRE_Z);

            coinWireEnd.transform.parent = coin.transform;
            coinWireEnd.transform.position = coin.transform.position + new Vector3(0, coinBounds.extents.y * 3 / 4, WIRE_Z);
            controller.RotationSet += OnRotationSet;
        }

        public void Dettach(GameController controller) {
            controller.RotationSet -= OnRotationSet;
            DestroyImmediate(this.gameObject);
        }

        private void OnRotationSet(float angle) {
            var coin = coinWireEnd.transform.parent.gameObject;
            if (!coin.GetComponent<CoinController>().isMoving)
                return;

            var coinBounds = coin.GetComponent<Renderer>().bounds;

            var p1 = coin.transform.position + coin.transform.rotation * new Vector3(0, coinBounds.extents.y * 3 / 4, 0);
            var p2 = coin.transform.position - coin.transform.rotation * new Vector3(0, coinBounds.extents.y * 3 / 4, 0);
            var higher = p1.y > p2.y ? p1 : p2;
            float t = Math.Abs(angle - 90f) / 90;
            var pos = Vector3.Lerp(coin.transform.position, higher, t);

            coinWireEnd.transform.position = pos + new Vector3(0, 0, WIRE_Z);
        }

        public void Update() {
            lineRenderer.SetPosition(0, coinWireEnd.transform.position + new Vector3(0, 0, WIRE_Z));
        }

    }
}