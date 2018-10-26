using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Source.controller.ui {
    class FloatingStar : MonoBehaviour {

        public Transform target;
        public Vector3 minScale = new Vector3(0.1f, 0.1f, 0.1f);

        public float initialForce = 8;
        public float maxForce = 12;
        public float arriveDistance = 0.5f;
        [Range(0, 1f)]
        public float redirectionRate = 0.975f;

        private Vector3 force;

        void Start() {
            force = UnityEngine.Random.insideUnitCircle * initialForce;
            force += (target.position - transform.position);
        }

        public event Action OnArrival;

        void Reset() {
            gameObject.transform.localScale = new Vector3(1, 1, 1);
            gameObject.transform.position = Vector3.zero;
            gameObject.SetActive(false);
            force = UnityEngine.Random.insideUnitCircle * initialForce;
            force += (target.position - transform.position);
        }

        void Update() {
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance < arriveDistance) {
                Reset();
                if (OnArrival != null) {
                    OnArrival();
                }
                return;
            }
            transform.localScale = Vector3.Lerp(transform.localScale, minScale, redirectionRate * Time.deltaTime);
            Vector3 dir = target.position - transform.position;
            force = Vector3.Lerp(force, dir, redirectionRate * Time.deltaTime);
            if (force.magnitude > maxForce) {
                force *= (maxForce / force.magnitude);
            }
            transform.position += force * Time.deltaTime;
        }
    }
}
