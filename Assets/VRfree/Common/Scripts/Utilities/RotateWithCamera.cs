using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRfreePluginUnity {
    public class RotateWithCamera : MonoBehaviour {
        public Transform vrCamera;
        public bool rotationOnly = false;

        // Use this for initialization
        void Start() {
            if (vrCamera == null && VRfreeCamera.Instance != null) {
                vrCamera = VRfreeCamera.Instance.transform;
            }
        }

        // Update is called once per frame
        void Update() {
            if(!rotationOnly)
                transform.position = vrCamera.position;
            transform.rotation = Quaternion.LookRotation(new Vector3(vrCamera.forward.x, 0, vrCamera.forward.z));
        }
    }
}
