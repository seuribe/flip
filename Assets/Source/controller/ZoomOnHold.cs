using UnityEngine;
using System.Collections;

namespace com.perroelectrico.flip.controller {

    public class ZoomOnHold : MonoBehaviour {
        /*
            private Vector3 originalPos;
            public Vector3 pressedPos;

            private Vector3 originalScale;
            public Vector3 pressedScale;

            public MeshRenderer targetRenderer;

            private bool pressed = false;

            void Start()
            {
                originalPos = gameObject.transform.localPosition;
                originalScale = gameObject.transform.localScale;
            }

            // Update is called once per frame
            void Update () {

                var rect = UIButtonsController.BoundsToScreenRect(targetRenderer.bounds);
        
                if (Input.touchCount > 0)
                {
                    var touch = Input.GetTouch(0);

                    if (touch.phase == TouchPhase.Began && rect.Contains(touch.position))
                    {
                        pressed = true;
                    }
                    else if (touch.phase == TouchPhase.Ended)
                    {
                        pressed = false;
                    }
                } else if (Input.GetMouseButtonDown(0) && rect.Contains(Input.mousePosition))
                {
                    pressed = true;
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    pressed = false;
                }

                var targetPosition = pressed ? pressedPos : originalPos;
                var targetScale = pressed ? pressedScale : originalScale;

                gameObject.transform.localPosition = Vector3.Lerp(gameObject.transform.localPosition, targetPosition, Time.deltaTime * 4);
                gameObject.transform.localScale = Vector3.Lerp(gameObject.transform.localScale, targetScale, Time.deltaTime * 4);
            }
         */
    }
}