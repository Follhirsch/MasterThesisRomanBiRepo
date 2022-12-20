using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRfreePluginUnity {
    public class ResetOnKeyPress : MonoBehaviour, IResettable {
        public string key = "n";
        private Vector3 startPos;
        private Quaternion startQuat;
        private Vector3 startScale;
        private bool isInitialized = false;
        private bool isReset = false;

        // Use this for initialization
        void Start() {
            startPos = transform.position;
            startQuat = transform.rotation;
            startScale = transform.localScale;
            isInitialized = true;
        }

        // Update is called once per frame
        void Update() {
            if(Input.GetKeyDown(key)) {
                reset();
            }
            isReset = false;
        }

        public void reset() {
            if(!isInitialized || isReset)
                return;
            isReset = true;
            transform.position = startPos;
            transform.rotation = startQuat;
            transform.localScale = startScale;
            Rigidbody rb = GetComponent<Rigidbody>();
            if(rb != null) {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            IResettable[] otherResettables = GetComponents<IResettable>();
            foreach(IResettable r in otherResettables) {
                if(r != (IResettable)this)
                    r.reset();
            }

        }

        public void resetAllChildren() {
            IResettable[] resetList = GetComponentsInChildren<IResettable>();
            foreach(IResettable rstObj in resetList) {
                rstObj.reset();
            }
        }

        public void resetAllChildrenNextUpdate() {
            StartCoroutine(resetAllChildrenNextUpdateCoroutine());
        }

        IEnumerator resetAllChildrenNextUpdateCoroutine() {
            yield return null;
            resetAllChildren();
        }
    }
}
