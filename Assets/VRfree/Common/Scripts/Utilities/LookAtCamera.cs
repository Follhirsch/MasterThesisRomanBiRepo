using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRfreePluginUnity {
    public class LookAtCamera : MonoBehaviour {
        public Transform cameraTransform;

        // Start is called before the first frame update
        void Start() {
            if (cameraTransform == null && VRfreeCamera.Instance != null) {
                cameraTransform = VRfreeCamera.Instance.transform;
            }
        }

        // Update is called once per frame
        void Update() {
            if(cameraTransform != null) {
                transform.rotation = Quaternion.LookRotation(cameraTransform.position-transform.position, cameraTransform.up);
            }
        }
    }
}