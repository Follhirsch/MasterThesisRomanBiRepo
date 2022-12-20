using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRfreePluginUnity {
    public class MoveWithCamera : MonoBehaviour {
        public Transform vrCamera;

        public Transform lookAtTransform;

        public Transform moveOtherTransform;

        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void Update() {
            if(moveOtherTransform != null)
                moveOtherTransform.position = vrCamera.position;
            else
                transform.position = vrCamera.position;

            if(lookAtTransform) {
                Vector3 dir = lookAtTransform.position - vrCamera.position;
                dir.y = 0;
                if(moveOtherTransform != null)
                    moveOtherTransform.rotation = Quaternion.LookRotation(dir);
                else
                    transform.rotation = Quaternion.LookRotation(dir);
            }
        }
    }
}