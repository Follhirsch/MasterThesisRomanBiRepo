using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandAngles = VRfree.HandAngles;

namespace VRfreePluginUnity {
    public class ConstrainedHandPose {
        /* raw HandAngles as coming from the VRfreeGlove */
        private HandAngles rawHandAngles = new HandAngles().init();
        public HandAngles RawHandAngles {
            get {
                return rawHandAngles;
            }

            set {
                rawHandAngles = value;

                // thumb
                constrainedHandAngles.thumbAngle1side = rawHandAngles.thumbAngle1side;

                float rawThumbClose = (1 - Mathf.Abs(thumb0SideCloseFactor)) * rawHandAngles.fingerAngles0close[0] + thumb0SideCloseFactor * rawHandAngles.fingerAngles0side[0];

                float closedThumbClose = closedHandAngles.fingerAngles0close[0];
                if(rawThumbClose < closedThumbClose) {
                    float correctedDelta = (closedThumbClose - rawThumbClose) / ((1 - Mathf.Abs(thumb0SideCloseFactor)) * (1 - Mathf.Abs(thumb0SideCloseFactor)) + thumb0SideCloseFactor * thumb0SideCloseFactor);
                    constrainedHandAngles.fingerAngles0side[0] = rawHandAngles.fingerAngles0side[0] + correctedDelta * thumb0SideCloseFactor;
                    constrainedHandAngles.fingerAngles0close[0] = rawHandAngles.fingerAngles0close[0] + correctedDelta * (1 - Mathf.Abs(thumb0SideCloseFactor));
                    float constrainedThumbClose = (1 - Mathf.Abs(thumb0SideCloseFactor)) * constrainedHandAngles.fingerAngles0close[0] + thumb0SideCloseFactor * constrainedHandAngles.fingerAngles0side[0];
                    if(constrainedThumbClose < closedThumbClose - 0.1f)
                        Debug.Log("math problem!");
                } else {
                    constrainedHandAngles.fingerAngles0side[0] = rawHandAngles.fingerAngles0side[0];
                    constrainedHandAngles.fingerAngles0close[0] = rawHandAngles.fingerAngles0close[0];
                }
                constrainedHandAngles.fingerAngles1close[0] = Mathf.Max(rawHandAngles.fingerAngles1close[0], closedHandAngles.fingerAngles1close[0]);
                constrainedHandAngles.fingerAngles2close[0] = Mathf.Max(rawHandAngles.fingerAngles2close[0], closedHandAngles.fingerAngles2close[0]);

                // other fingers
                for(int finger = 1; finger < 5; finger++) {
                    constrainedHandAngles.fingerAngles0side[finger] = rawHandAngles.fingerAngles0side[finger];
                    constrainedHandAngles.fingerAngles0close[finger] = Mathf.Max(rawHandAngles.fingerAngles0close[finger], closedHandAngles.fingerAngles0close[finger]);
                    constrainedHandAngles.fingerAngles1close[finger] = Mathf.Max(rawHandAngles.fingerAngles1close[finger], closedHandAngles.fingerAngles1close[finger]);
                    constrainedHandAngles.fingerAngles2close[finger] = Mathf.Max(rawHandAngles.fingerAngles2close[finger], closedHandAngles.fingerAngles2close[finger]);
                }

                // calculate if hand is flat
                for(int i = 0; i < 5; i++) {
                    if(rawHandAngles.fingerAngles0close[i] < -20 || rawHandAngles.fingerAngles1close[i] < -30) {
                        isHandFlat = false;
                        return;
                    }
                }
                isHandFlat = true;
            }
        }

        /* HandAngles that are in some way constrained, for examble from holding something that prevents the fingers from closing further */
        private HandAngles constrainedHandAngles = new HandAngles().init();
        public HandAngles ConstrainedHandAngles {
            get {
                return constrainedHandAngles;
            }
        }

        /* HandAngles that are in some way constrained, for examble from holding something that prevents the fingers from closing further */
        public HandAngles closedHandAngles = new HandAngles().init();

        /* how to include the thumb side angle in the calculation of closedness of the thumb, i. e. 
         * thumbClosingAngle = (1 - Mathf.Abs(thumb0SideCloseFactor)) * fingerAngles0close[0] + thumb0SideCloseFactor * fingerAngles0side[0] */
        public float thumb0SideCloseFactor = 0.5f;

        /* whether or not the hand is flat (all closing angles below 20 degrees) */
        public bool isHandFlat = false;

        /* Vector3 containing the raw position of the root joint of the hand (provided by the tracking) */
        private Vector3 positionRaw;

        /* Vector3 containing the guided position of the root joint of the hand (when the allowed hand movement is constrained by an object) */
        private Vector3 positionConstrained;

        /* This delegate can be set from outside to calculate the positionConstrained whenever setPositionRaw is called. */
        public delegate Vector3 ConstrainHandPosition(Vector3 positionRaw);
        private ConstrainHandPosition positionConstraint;

        public ConstrainedHandPose() {
            grabEnded();
        }

        /* Gets the raw tracking position */
        public Vector3 getPositionRaw() {
            return positionRaw;
        }

        /* 
         * Gets the constrained position, which is equal to the raw position when the hand can move freely
         * (setPositionConstraint has not been called or removePositionConstraint was called). Otherwise this 
         * position is determined from the raw position by the positionConstraint delegate.
         */
        public Vector3 getPosition() {
            return positionConstrained;
        }

        /*
         * Set the tracking position here. If the positionConstraint is set, this automatically calculates the positionConstrained.
         */
        public void setPositionRaw(Vector3 position) {
            positionRaw = position;
            if(positionConstraint == null) {
                // the hand can move freely and therefore positionConstrained is the same as positionRaw
                positionConstrained = position;
            } else {
                // the hand position is constrained and the hand can't move freely. Call positionConstraint delegate to get constrained position
                positionConstrained = positionConstraint(position);
            }
        }

        public void setPositionConstraint(ConstrainHandPosition constraint) {
            positionConstraint = constraint;
        }

        public void removePositionConstraint() {
            positionConstraint = null;
        }

        /*
         * Quaternion containing the orientation of the root joint of the hand
         */
        public Quaternion rotation = Quaternion.Euler(0, 0, 0);

        /*
         * Quaternion containing the orientation of the lowe arm joint
         */
        public Quaternion wristRotation = Quaternion.Euler(0, 0, 0);

        public void grabEnded() {
            for(int i = 0; i < 5; i++) {
                closedHandAngles.fingerAngles0close[i] = -90;
                closedHandAngles.fingerAngles1close[i] = -90;
                closedHandAngles.fingerAngles2close[i] = -90;
            }
        }
    }
}