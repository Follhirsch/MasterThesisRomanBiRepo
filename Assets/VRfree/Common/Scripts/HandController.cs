/*
 * While the VRfreeGlove script can animate a hand model when its handTransforms are set, the Handcontroller allows more andvanced control.
 * As opposed to the VRfreeGlove script, where the hand is moved by simply setting the positions and rotations of the transforms, the HandController 
 * adds a rigidbody to every bone and moves them with Rigidbody.MovePosition and MoveRotation, which allows for better interaction with the physics engine.
 * HandController also creates a flattened hierarchy of the bones which allows scaling them to adapt to different hand sizes.
 * Finally, the ConstrainedHandPose allows to adapt the hand pose to (virtual) obstacles.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VRfreePluginUnity {
    public interface IPoseReceiver {
        void OnPoseChanged();
    }

    [ScriptOrder(-50)]
    [RequireComponent(typeof(VRfreeGlove))]
    public class HandController : MonoBehaviour, IPoseReceiver {
        [HideInInspector]
        public VRfreeGlove glove;

        public RigidbodyInterpolation rigidbodyInterpolation = RigidbodyInterpolation.None;
        public CollisionDetectionMode collisionDetectionMode = CollisionDetectionMode.Discrete;

        public HandTransformsWithEnd handTransforms;
        // during startup, empty copies of the handTransforms will be created in handMasterTransforms and all movement will be applied to them first
        // then their positions will be applied to the handTransforms 
        [HideInInspector]
        public HandTransformsWithEnd handMasterTransforms;

        public bool enableConstraints = true;
        public ConstrainedHandPose handPose = new ConstrainedHandPose();

        // only needed to copy colliders from other hand
        public HandController otherHandController;


        public HashSet<IPoseReceiver> poseReceivers = new HashSet<IPoseReceiver>();

        [ReadOnly]
        public bool isHandFlat;

        protected Rigidbody handRigidbody;
        protected Rigidbody wristRigidbody;
        protected Rigidbody[][] fingerRigidbodies;

        private bool isInitialized = false;

        // Use this for initialization
        protected virtual void Start() {
            init();
        }

        private void OnValidate() {
            if(!Application.isPlaying) {
                InitGloveLink();
            }
        }

        private void InitGloveLink() {
            glove = GetComponent<VRfreeGlove>();
            if(glove?.handTransforms.handTransform != null) {
                Debug.LogWarning("When using HandController, the handTransforms in the VRfreeGlove script should be unassigned!");
                glove.clearHandTransforms();
            }
            if(!glove.poseReceivers.Contains(this))
                glove.poseReceivers.Add(this);
        }

        public void init() {
            if(!isInitialized) {
                InitGloveLink();

                // during startup, empty copies of the handTransforms will be created in handMasterTransforms
                createMasterTransforms();

                // flatten hierarchy of handTransforms
                Transform flatParent = new GameObject("Flattened Hierarchy").transform;
                flatParent.parent = transform;
                if(handTransforms.wristTransform) handTransforms.wristTransform.parent = flatParent;
                if(handTransforms.handTransform) handTransforms.handTransform.parent = flatParent;
                for(int f = 0; f < 5; f++) {
                    for(int i = 0; i < 3; i++) {
                        if(handTransforms.getFingerTransform(f, i) != null) handTransforms.getFingerTransform(f, i).parent = flatParent;
                    }
                }

                // add rigidbodies
                if(handTransforms.handTransform) handRigidbody = addRigidbody(handTransforms.handTransform.gameObject);

                if(handTransforms.wristTransform) wristRigidbody = addRigidbody(handTransforms.wristTransform.gameObject);

                fingerRigidbodies = new Rigidbody[5][];
                for(int f = 0; f < 5; f++) {
                    fingerRigidbodies[f] = new Rigidbody[3];
                    for(int i = 0; i < 3; i++) {
                        if(handTransforms.getFingerTransform(f, i)) fingerRigidbodies[f][i] = addRigidbody(handTransforms.getFingerTransform(f, i).gameObject);
                    }
                }

                isInitialized = true;
            }
        }

        protected virtual Rigidbody addRigidbody(GameObject g) {
            Rigidbody rb = g.GetComponent<Rigidbody>();
            if(rb == null)
                rb = g.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.interpolation = rigidbodyInterpolation;
            rb.collisionDetectionMode = collisionDetectionMode;
            return rb;
        }

        public void OnPoseChanged() {
            if(Application.isPlaying) {
                if(enableConstraints && !glove.showingCalibrationPose && !VRfree.HandPoseCalibrationSettings.IsCalibrating) {
                    handPose.RawHandAngles = glove.handAngles;
                    handPose.setPositionRaw(glove.displayHandData.wristPosition);

                    foreach(IPoseReceiver pr in poseReceivers)
                        pr.OnPoseChanged();

                    // have to do this again to update constrainedHandAngles with new closedHandAngles
                    handPose.RawHandAngles = glove.handAngles;

                    HandData fromAngles = HandData.HandDataFromVRfree(VRfree.VRfreeAPI.GetHandDataFromAngles(glove.isRightHand, handPose.ConstrainedHandAngles));
                    if(glove.convertForSteamVrGlove)
                        fromAngles.convertToOpenVrQuaternions(glove.isRightHand);

                    fromAngles.wristPosition = handPose.getPosition();
                    handMasterTransforms.applyHandData(fromAngles);
                    moveHandTransforms();
                    isHandFlat = handPose.isHandFlat;
                } else {
                    handMasterTransforms.applyHandData(glove.displayHandData);
                    handMasterTransforms.wristTransform.position = glove.displayHandData.wristPosition;

                    moveHandTransforms();
                    foreach(IPoseReceiver pr in poseReceivers)
                        pr.OnPoseChanged();
                }
            } else {
                handTransforms.applyHandData(glove.displayHandData);
            }
        }

        public virtual void moveHandTransforms() {
            if(!Application.isPlaying)
                return;

            if(wristRigidbody) wristRigidbody.MovePosition(handMasterTransforms.wristTransform.position);
            if(handRigidbody) handRigidbody.MovePosition(handMasterTransforms.handTransform.position);

            for(int f = 0; f < 5; f++) {
                for(int i = 0; i < 3; i++) {
                    if(fingerRigidbodies[f][i]) fingerRigidbodies[f][i].MovePosition(handMasterTransforms.getFingerTransform(f, i).position);
                }
            }

            if(wristRigidbody) wristRigidbody.MoveRotation(handMasterTransforms.wristTransform.rotation);
            if(handRigidbody) handRigidbody.MoveRotation(handMasterTransforms.handTransform.rotation);

            for(int f = 0; f < 5; f++) {
                for(int i = 0; i < 3; i++) {
                    if(fingerRigidbodies[f][i]) fingerRigidbodies[f][i].MoveRotation(handMasterTransforms.getFingerTransform(f, i).rotation);
                }
            }
        }

        protected virtual void createMasterTransforms() {
            Transform masterTransformsParent = new GameObject("Master Transforms").transform;
            masterTransformsParent.parent = transform;
            if(handTransforms.wristTransform != null) {
                GameObject wristMasterObject = new GameObject("WristMaster");
                wristMasterObject.transform.parent = masterTransformsParent;
                wristMasterObject.transform.position = handTransforms.wristTransform.position;
                wristMasterObject.transform.rotation = handTransforms.wristTransform.rotation;
                handMasterTransforms.wristTransform = wristMasterObject.transform;
            }
            if(handTransforms.handTransform != null) {
                GameObject handMasterObject = new GameObject("Hand");
                handMasterObject.transform.parent = handMasterTransforms.wristTransform;
                handMasterObject.transform.position = handTransforms.handTransform.position;
                handMasterObject.transform.rotation = handTransforms.handTransform.rotation;
                handMasterTransforms.handTransform = handMasterObject.transform;
            }

            if(handTransforms.knucklesIndexTransform != null) {
                GameObject knucklesIndexMasterObject = new GameObject("knuckles_index");
                knucklesIndexMasterObject.transform.parent = handMasterTransforms.handTransform;
                knucklesIndexMasterObject.transform.position = handTransforms.knucklesIndexTransform.position;
                knucklesIndexMasterObject.transform.rotation = handTransforms.knucklesIndexTransform.rotation;
                handMasterTransforms.knucklesIndexTransform = knucklesIndexMasterObject.transform;
            }
            if(handTransforms.knucklesPinkyTransform != null) {
                GameObject knucklesPinkyMasterObject = new GameObject("knuckles_pinky");
                knucklesPinkyMasterObject.transform.parent = handMasterTransforms.handTransform;
                knucklesPinkyMasterObject.transform.position = handTransforms.knucklesPinkyTransform.position;
                knucklesPinkyMasterObject.transform.rotation = handTransforms.knucklesPinkyTransform.rotation;
                handMasterTransforms.knucklesPinkyTransform = knucklesPinkyMasterObject.transform;
            }

            for(int f = 0; f < 5; f++) {
                for(int i = -1; i < 4; i++) {
                    if(handTransforms.getFingerTransform(f, i) != null) {
                        GameObject fingerMasterObject;
                        if(i == -1) {
                            // metacarpals
                            fingerMasterObject = new GameObject("finger" + f + "meta");
                            fingerMasterObject.transform.parent = handMasterTransforms.handTransform;
                        } else if(i == 0) {
                            // first phalanges
                            fingerMasterObject = new GameObject("finger" + f + i);
                            fingerMasterObject.transform.parent = (handMasterTransforms.getFingerTransform(f, -1) != null) ? handMasterTransforms.getFingerTransform(f, -1) : handMasterTransforms.handTransform;
                        } else {
                            fingerMasterObject = new GameObject("finger" + f + ((f < 4) ? "" + i : "end"));
                            fingerMasterObject.transform.parent = handMasterTransforms.getFingerTransform(f, i - 1);
                        }
                        fingerMasterObject.transform.position = handTransforms.getFingerTransform(f, i).position;
                        fingerMasterObject.transform.rotation = handTransforms.getFingerTransform(f, i).rotation;
                        //Vector3 ls = handTransforms.getFingerTransform(f, i).lossyScale;
                        //Vector3 pls = fingerMasterObject.transform.parent.lossyScale;
                        //fingerMasterObject.transform.localScale = new Vector3(ls.x / pls.x, ls.y / pls.y, ls.z / pls.z);
                        handMasterTransforms.setFingerTransform(f, i, fingerMasterObject.transform);

                    }
                }
            }
        }

        public void findHandTransformsInChildren() {
            string rl = glove.isRightHand ? "_r" : "_l";
            handTransforms.wristTransform = transform.FindDeepChild("root" + rl);
            handTransforms.handTransform = transform.FindDeepChild("hand" + rl);
            string[] fingers = { "finger_thumb_", "finger_index_", "finger_middle_", "finger_ring_", "finger_pinky_" };
            for(int f = 0; f < 5; f++) {
                for(int i = 0; i < 5; i++) {
                    handTransforms.setFingerTransform(f, i, transform.FindDeepChild(fingers[f] + i + rl));
                }
            }
            for(int f = 0; f < 5; f++) {
                if(f != 0)
                    handTransforms.setFingerTransform(f, -1, transform.FindDeepChild(fingers[f] + "meta" + rl));
                handTransforms.setFingerTransform(f, 3, transform.FindDeepChild(fingers[f] + "end" + rl));
            }
            handTransforms.knucklesIndexTransform = transform.FindDeepChild("knuckles_index" + rl);
            handTransforms.knucklesPinkyTransform = transform.FindDeepChild("knuckles_pinky" + rl);

            GetComponent<VRfreeGlove>().clearHandTransforms();
        }

        public void clearHandTransforms() {
            handTransforms.wristTransform = null;
            handTransforms.handTransform = null;
            for(int f = 0; f < 5; f++) {
                for(int i = 0; i < 5; i++) {
                    handTransforms.setFingerTransform(f, i, null);
                }
            }
            for(int f = 0; f < 5; f++) {
                if(f != 0)
                    handTransforms.setFingerTransform(f, -1, null);
                handTransforms.setFingerTransform(f, 3, null);
            }
            handTransforms.knucklesIndexTransform = null;
            handTransforms.knucklesPinkyTransform = null;
        }

#if UNITY_EDITOR
        // Adds colliders to the hand. These are guesses based on the bones and will have to be adjusted to fit properly
        public void addHandColliders() {
            for(int finger = 0; finger < 5; finger++) {
                // first make a guess at the radius by looking at the length of the last phalanx
                Transform lastStart = handTransforms.getFingerTransform(finger, 2);
                Transform lastEnd = handTransforms.getFingerTransform(finger, 3);
                Vector3 lastLocalEnd = lastStart.InverseTransformPoint(lastEnd.position);
                float radius = 0.35f * lastLocalEnd.magnitude;

                for(int index = 0; index < 3; index++) {
                    Transform start = handTransforms.getFingerTransform(finger, index);
                    Transform end = handTransforms.getFingerTransform(finger, index + 1);
                    CapsuleCollider coll = start.GetComponent<CapsuleCollider>();
                    if(!coll) {
                        coll = Undo.AddComponent<CapsuleCollider>(start.gameObject);
                        Vector3 localEnd = start.InverseTransformPoint(end.position);
                        float height = localEnd.magnitude;
                        coll.radius = radius;
                        float startOverlap = (index == 0) ? 0 : 0.67f;
                        coll.height = height + startOverlap * radius;
                        coll.center = (1 - coll.height / (2 * height)) * localEnd;
                        coll.direction = (localEnd.x > localEnd.y && localEnd.x > localEnd.z) ? 0 : ((localEnd.y > localEnd.z) ? 1 : 2);
                    } else {
                        //Don't modify existing colliders
                        //Undo.RecordObject(coll, "Modified Finger Collider");
                    }
                }
            }
        }

        public void copyCollidersFromOtherHand() {
            for(int finger = 0; finger < 5; finger++) {
                for(int index = 0; index < 3; index++) {
                    CapsuleCollider other = otherHandController.handTransforms.getFingerTransform(finger, index).gameObject.GetComponent<CapsuleCollider>();
                    if(other == null)
                        continue;
                    Transform phalanx = handTransforms.getFingerTransform(finger, index);
                    CapsuleCollider coll = phalanx.GetComponent<CapsuleCollider>();
                    if(!coll) {
                        coll = Undo.AddComponent<CapsuleCollider>(phalanx.gameObject);
                    } else {
                        Undo.RecordObject(coll, "Modified Finger Collider");
                    }
                    coll.radius = other.radius;
                    coll.height = other.height;
                    coll.center = new Vector3(other.center.x, -other.center.y, other.center.z);
                    coll.direction = other.direction;

                }
            }
        }
#endif
    }
}