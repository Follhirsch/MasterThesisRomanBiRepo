using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VRfreePluginUnity {
    public class LookMenu : MonoBehaviour {
        public Transform vrCamera;
        public Transform pointerOrigin;
        public GameObject pointer;
        public ProgressBar progressBar;
        public LayerMask layerMask = ~0;

        public bool paused = false;
        public float raycastDistance = 5;
        public float activationTime = 2;
        public float deactivatedTime = 10;

        public float stopAfterHeadStillTime = 10;
        [ReadOnly]
        public bool stopped = true;

        private float timeSinceHeadStill = 0;
        private Quaternion lastcameraRotation = Quaternion.identity;
        private float timeSinceRunning = 0;

        private float timeSincePointing = 0;

        private bool isPointing = false;
        public UnityEvent onStartPointing;
        public UnityEvent onEndPointing;

        // Use this for initialization
        void Start() {
            pointer.SetActive(false);
            progressBar.gameObject.SetActive(false);
            timeSinceHeadStill = stopAfterHeadStillTime + 1;
            timeSinceRunning = 0;
            stopped = true;

            if (vrCamera == null && VRfreeCamera.Instance != null) {
                vrCamera = VRfreeCamera.Instance.transform;
            }

        }

        // Update is called once per frame
        void Update() {
            if(timeSinceRunning < 1) {
                timeSinceRunning += Time.deltaTime;
                lastcameraRotation = vrCamera.rotation;
                return;
            }
            if(lastcameraRotation == vrCamera.rotation) {
                timeSinceHeadStill += Time.deltaTime;
            } else {
                timeSinceHeadStill = 0;
            }
            lastcameraRotation = vrCamera.rotation;

            // don't cast ray if head did not move since more than stopAfterHeadStillTime
            if(stopAfterHeadStillTime != 0 && timeSinceHeadStill > stopAfterHeadStillTime) {
                stopped = true;
                return;
            }
            stopped = false;

            RaycastHit hit;
            bool isPointingNew = false;
            if(Physics.Raycast(vrCamera.position, vrCamera.forward, out hit, raycastDistance, layerMask)) {
                LookMenuTarget target = hit.transform.gameObject.GetComponent<LookMenuTarget>();
                if(target != null) {
                    isPointingNew = true;
                    pointerOrigin.localScale = new Vector3(hit.distance, hit.distance, hit.distance);
                    pointerOrigin.position = vrCamera.position;
                    pointerOrigin.rotation = vrCamera.rotation;

                    if(target.type == LookMenuTarget.Type.ShowPointer) {
                        pointer.SetActive(true);
                    } else {
                        pointer.SetActive(false);
                    }

                    if(target.type == LookMenuTarget.Type.Button) {
                        if(!progressBar.gameObject.activeSelf) {
                            // activate progress bar
                            progressBar.gameObject.SetActive(true);
                            timeSincePointing = 0;
                        }
                        // pointing at LookMenuTarget
                        if(!paused)
                            timeSincePointing += Time.deltaTime;
                        progressBar.progress = timeSincePointing / activationTime;
                        if(timeSincePointing > activationTime) {
                            target.onSelected.Invoke();
                            if(deactivatedTime > 0)
                                StartCoroutine(deactivateForDeactivatedTime(target));
                            timeSincePointing = 0;
                        }
                    } else {
                        progressBar.gameObject.SetActive(false);
                        progressBar.progress = 0;
                    }
                }
            } else {
                pointerOrigin.localScale = new Vector3(raycastDistance, raycastDistance, raycastDistance);
                pointerOrigin.position = vrCamera.position;
                pointerOrigin.rotation = vrCamera.rotation;
                pointer.SetActive(false);
                progressBar.gameObject.SetActive(false);
                progressBar.progress = 0;
            }

            if(isPointing && !isPointingNew) {
                onEndPointing.Invoke();
            } else if(!isPointing && isPointingNew) {
                onStartPointing.Invoke();
            }
            isPointing = isPointingNew;

        }

        IEnumerator deactivateForDeactivatedTime(LookMenuTarget target) {
            List<GameObject> objectsToReactivate = new List<GameObject>();
            Transform directChild = target.transform.parent;
            while(directChild.parent != null && directChild.parent != transform) {
                directChild = directChild.parent;
            }
            if(directChild.parent == null) {
                if(target.gameObject.activeSelf)
                    objectsToReactivate.Add(target.gameObject);
                target.gameObject.SetActive(false);
            } else {
                if(directChild.gameObject.activeSelf)
                    objectsToReactivate.Add(directChild.gameObject);
                directChild.gameObject.SetActive(false);
            }
            //for (int i = 0; i < transform.childCount; i++) {
            //    Transform child = transform.GetChild(i);
            //    if (child.gameObject.activeSelf) {
            //        child.gameObject.SetActive(false);
            //        objectsToReactivate.Add(child.gameObject);
            //    }
            //}
            yield return new WaitForSeconds(deactivatedTime);
            foreach(GameObject o in objectsToReactivate)
                o.SetActive(true);
        }
    }
}