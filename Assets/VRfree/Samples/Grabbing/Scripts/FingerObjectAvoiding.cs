using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRfreePluginUnity {
    [ScriptOrder(100)]
    [RequireComponent(typeof(HandCollisionMaster))]
    public class FingerObjectAvoiding : MonoBehaviour {
        public bool drawDebugRays = false;
        public float changeCoefficient = 0.1f; // Between 0 and 1. Higher number means faster movement towards the correct grabbing pose, but might become unstable
        public float contactOffset = -0.001f; // positive: fingers will stop slightly outside of object; negative: fingers will penetrate into surface
                                              /* how to include the thumb side angle in the calculation of closedness of the thumb, i. e. 
                                               * thumbClosingAngle = (1 - Mathf.Abs(thumb0SideCloseFactor)) * fingerAngles0close[0] + thumb0SideCloseFactor * fingerAngles0side[0] */
        public float thumb0SideCloseFactor = 0.8f;
        HandCollisionMaster handCollisionMaster;
        private ConstrainedHandPose handPose;
        private float[,] phalanxOverlapTangents = new float[5, 3];
        private VRfree.HandBoneTransforms.HandDimensions dimensions;

        public float debugThumbCloseAngle;

        // Use this for initialization
        void Start() {
            handCollisionMaster = GetComponent<HandCollisionMaster>();
            if(!handCollisionMaster.handController.glove.isRightHand)
                thumb0SideCloseFactor *= -1;
            handPose = handCollisionMaster.handController.handPose;
            HandDimensionScript hds = gameObject.AddComponent<HandDimensionScript>();
            hds.handControllers.Add(handCollisionMaster.handController);
            hds.calculateInitialDimensions();
            dimensions = hds.initialDimensions;
            Destroy(hds);
        }

        void FixedUpdate() {
            debugThumbCloseAngle = (1 - Mathf.Abs(thumb0SideCloseFactor)) * handPose.RawHandAngles.fingerAngles0close[0] + thumb0SideCloseFactor * handPose.RawHandAngles.fingerAngles0side[0];

            /* In a first step, we iterate through all collisions with grabbed objects and find out the largest overlap
             * between each finger phalanx and the grabbed object(s). We normalize and use simplified math to assume 
             * every phalanx has a length of 1 and the overlap is located at the center of each phalanx.
             * If this returns false, no objects are grabbed. */
            if(!calculateFingerObjectOverlap()) {
                handPose.grabEnded();
                return;
            }

            /* In the second step we calculate, how the fingers need to move in order to decrease the overlap. 
             * This is an iterative algorithm, so it will take a couple of frames until the fingers reach the proper position. */
            calculateFingerDisplacement();
        }

        private void calculateFingerDisplacement() {
            handPose.thumb0SideCloseFactor = thumb0SideCloseFactor;
            float rawThumbClose = (1 - Mathf.Abs(thumb0SideCloseFactor)) * handPose.RawHandAngles.fingerAngles0close[0] + thumb0SideCloseFactor * handPose.RawHandAngles.fingerAngles0side[0];
            float constrainedThumbClose = (1 - Mathf.Abs(thumb0SideCloseFactor)) * handPose.ConstrainedHandAngles.fingerAngles0close[0] + thumb0SideCloseFactor * handPose.ConstrainedHandAngles.fingerAngles0side[0];

            for(int finger = 0; finger < 5; finger++) {
                float[] openingPotential, closingPotential;
                openingPotential = new float[]{ 10 - ((finger == 0) ? constrainedThumbClose : handPose.ConstrainedHandAngles.fingerAngles0close[finger]),
                                                5 - handPose.ConstrainedHandAngles.fingerAngles1close[finger],
                                                5 - handPose.ConstrainedHandAngles.fingerAngles2close[finger]};

                closingPotential = new float[]{ (finger == 0) ? rawThumbClose - constrainedThumbClose : handPose.RawHandAngles.fingerAngles0close[finger] - handPose.ConstrainedHandAngles.fingerAngles0close[finger],
                                                handPose.RawHandAngles.fingerAngles1close[finger] - handPose.ConstrainedHandAngles.fingerAngles1close[finger],
                                                handPose.RawHandAngles.fingerAngles2close[finger] - handPose.ConstrainedHandAngles.fingerAngles2close[finger] };

                for(int phalanx = 0; phalanx < 3; phalanx++) {
                    // calculate phalanx displacement
                    if(phalanxOverlapTangents[finger, phalanx] > 0) {
                        // overlap, open finger more
                        float overlapAngle = 57.3f * Mathf.Min(1f, phalanxOverlapTangents[finger, phalanx]); // approximated atan
                        float angleChange = Mathf.Min(changeCoefficient * overlapAngle, openingPotential[phalanx]);

                        if(phalanx == 0)
                            handPose.closedHandAngles.fingerAngles0close[finger] = angleChange + ((finger == 0) ? constrainedThumbClose : handPose.ConstrainedHandAngles.fingerAngles0close[finger]);
                        else if(phalanx == 1)
                            handPose.closedHandAngles.fingerAngles1close[finger] = angleChange + handPose.ConstrainedHandAngles.fingerAngles1close[finger];
                        else if(phalanx == 2)
                            handPose.closedHandAngles.fingerAngles2close[finger] = angleChange + handPose.ConstrainedHandAngles.fingerAngles2close[finger];
                    } else {
                        // separation, close finger more
                        float overlapAngle = 57.3f * Mathf.Max(-1f, phalanxOverlapTangents[finger, phalanx]); // approximated atan
                        float angleChange = Mathf.Max(changeCoefficient * overlapAngle, closingPotential[phalanx]);

                        if(phalanx == 0)
                            handPose.closedHandAngles.fingerAngles0close[finger] = Mathf.Min(
                                handPose.closedHandAngles.fingerAngles0close[finger],
                                angleChange + ((finger == 0) ? constrainedThumbClose : handPose.ConstrainedHandAngles.fingerAngles0close[finger]));
                        else if(phalanx == 1)
                            handPose.closedHandAngles.fingerAngles1close[finger] = Mathf.Min(
                                handPose.closedHandAngles.fingerAngles1close[finger],
                                angleChange + handPose.ConstrainedHandAngles.fingerAngles1close[finger]);
                        else if(phalanx == 2)
                            handPose.closedHandAngles.fingerAngles2close[finger] = Mathf.Min(
                                handPose.closedHandAngles.fingerAngles2close[finger],
                                angleChange + handPose.ConstrainedHandAngles.fingerAngles2close[finger]);
                    }
                }

                // remove restrictions from fingers that are open further than their closed angles
                if(finger == 0 && handPose.closedHandAngles.fingerAngles0close[finger] < rawThumbClose || finger != 0 && handPose.closedHandAngles.fingerAngles0close[finger] < handPose.RawHandAngles.fingerAngles0close[finger]) {
                    handPose.closedHandAngles.fingerAngles0close[finger] = -90;
                }
                if(handPose.closedHandAngles.fingerAngles1close[finger] < handPose.RawHandAngles.fingerAngles1close[finger]) {
                    handPose.closedHandAngles.fingerAngles1close[finger] = -90;
                }
                if(handPose.closedHandAngles.fingerAngles2close[finger] < handPose.RawHandAngles.fingerAngles2close[finger]) {
                    handPose.closedHandAngles.fingerAngles2close[finger] = -90;
                }
            }
        }

        private bool calculateFingerObjectOverlap() {
            for(int i = 0; i < phalanxOverlapTangents.GetLength(0); i++) {
                for(int j = 0; j < phalanxOverlapTangents.GetLength(1); j++) {
                    phalanxOverlapTangents[i, j] = -10;
                }
            }

            bool grabbedAnything = false;
            foreach(ContactItemList list in handCollisionMaster.collisionLists) {
                if(list.isGrabbed) {
                    grabbedAnything = true;
                    foreach(ContactItem item in list.contacts) {
                        // skip contacts on hand
                        if(item.finger < 0)
                            continue;

                        // a contact affects all phalanges below the phalanx it occurs on. E. g. a contact on the tip of the finger affects all 3 phalanges.
                        for(int phanlanx = 0; phanlanx <= item.phalanx; phanlanx++) {
                            calculateContactTangent(item.contact, item.finger, phanlanx);
                        }
                    }
                }
            }

            if(drawDebugRays) {
                for(int finger = 0; finger < 5; finger++) {
                    for(int phalanx = 0; phalanx < 3; phalanx++) {
                        Transform fingerTransform = handCollisionMaster.handController.handTransforms.getFingerTransform(finger, phalanx);
                        //Vector3 rotationAxis = handCollisionMaster.handController.glove.convertForSteamVrGlove ? fingerTransform.forward : fingerTransform.right;
                        Vector3 boneDirection = handCollisionMaster.handController.glove.convertForSteamVrGlove ? fingerTransform.right : fingerTransform.forward;
                        Vector3 openingDirection = (fingerTransform.up - 0.2f * boneDirection) * 0.98f;

                        if(phalanxOverlapTangents[finger, phalanx] != -10)
                            Debug.DrawRay(fingerTransform.position + boneDirection * (dimensions.getPhalanxLength(finger, phalanx) * 0.5f), openingDirection * phalanxOverlapTangents[finger, phalanx]* dimensions.getPhalanxLength(finger, phalanx), Color.red);
                    }
                }
            }

            return grabbedAnything;
        }

        void calculateContactTangent(ContactPoint contact, int finger, int phalanx) {
            Transform fingerTransform = handCollisionMaster.handController.handTransforms.getFingerTransform(finger, phalanx);
            //Vector3 rotationAxis = handCollisionMaster.handController.glove.convertForSteamVrGlove ? fingerTransform.forward : fingerTransform.right;
            Vector3 boneDirection = handCollisionMaster.handController.glove.convertForSteamVrGlove ? fingerTransform.right : fingerTransform.forward;
            Vector3 openingDirection = (!handCollisionMaster.handController.glove.isRightHand && handCollisionMaster.handController.glove.convertForSteamVrGlove) ? -fingerTransform.up : fingerTransform.up;
            openingDirection = (openingDirection - 0.2f * boneDirection) * 0.98f;

            float overlap = -contact.separation * Vector3.Dot(contact.normal, openingDirection) + contactOffset;
            Vector3 distanceVector = contact.point - fingerTransform.position;
            float distance = Vector3.Dot(distanceVector, boneDirection);
            float overlapTan = overlap / Mathf.Max(dimensions.getPhalanxLength(finger, phalanx), distance);

            phalanxOverlapTangents[finger, phalanx] = Mathf.Max(overlapTan, phalanxOverlapTangents[finger, phalanx]);

            if(drawDebugRays)
                Debug.DrawRay(contact.point, openingDirection * overlap, (phalanx == 0) ? Color.white : ((phalanx == 1) ? Color.gray : Color.black));
        }

    }
}