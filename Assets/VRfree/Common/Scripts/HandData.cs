using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace VRfreePluginUnity {
    public static class VRfreeUnityExtensions {
        public static Vector3 FromVRfree(this VRfree.Vector3 v) {
            return new Vector3(v.x, v.y, v.z);
        }

        public static VRfree.Vector3 ToVRfree(this Vector3 v) {
            return new VRfree.Vector3(v.x, v.y, v.z);
        }

        public static Quaternion FromVRfree(this VRfree.Quaternion q) {
            return new Quaternion(q.x, q.y, q.z, q.w);
        }

        public static VRfree.Quaternion ToVRfree(this Quaternion q) {
            return new VRfree.Quaternion(q.x, q.y, q.z, q.w);
        }

        public static VRfree.HandData ToVRfree(this HandData h) {
            return new VRfree.HandData(
                h.cameraTimestamp, h.timeSinceLastLeftHandData, h.timeSinceLastRightHandData, h.isWristPositionValid, h.wristPosition.ToVRfree(),
                h.wristRotation.ToVRfree(),     h.handRotation.ToVRfree(),
                h.thumb0Rotation.ToVRfree(),    h.thumb1Rotation.ToVRfree(),    h.thumb2Rotation.ToVRfree(),
                h.index0Rotation.ToVRfree(),    h.index1Rotation.ToVRfree(),    h.index2Rotation.ToVRfree(),
                h.middle0Rotation.ToVRfree(),   h.middle1Rotation.ToVRfree(),   h.middle2Rotation.ToVRfree(),
                h.ring0Rotation.ToVRfree(),     h.ring1Rotation.ToVRfree(),     h.ring2Rotation.ToVRfree(),
                h.pinky0Rotation.ToVRfree(),    h.pinky1Rotation.ToVRfree(),    h.pinky2Rotation.ToVRfree());
        }

        public static HandData FromVRfree(VRfree.HandData h) {
            return new HandData(
                h.cameraTimestamp, h.timeSinceLastLeftHandData, h.timeSinceLastRightHandData, h.isWristPositionValid, h.wristPosition.FromVRfree(),
                h.wristRotation.FromVRfree(),      h.handRotation.FromVRfree(),
                h.thumb0Rotation.FromVRfree(),     h.thumb1Rotation.FromVRfree(),     h.thumb2Rotation.FromVRfree(),
                h.index0Rotation.FromVRfree(),     h.index1Rotation.FromVRfree(),     h.index2Rotation.FromVRfree(),
                h.middle0Rotation.FromVRfree(),    h.middle1Rotation.FromVRfree(),    h.middle2Rotation.FromVRfree(),
                h.ring0Rotation.FromVRfree(),      h.ring1Rotation.FromVRfree(),      h.ring2Rotation.FromVRfree(),
                h.pinky0Rotation.FromVRfree(),     h.pinky1Rotation.FromVRfree(),     h.pinky2Rotation.FromVRfree());
        }
    }

    [System.Serializable]
    public struct HandData {
        public uint cameraTimestamp;
        public int timeSinceLastLeftHandData;
        public int timeSinceLastRightHandData;
        public bool isWristPositionValid;
        public Vector3 wristPosition;
        public Quaternion wristRotation;
        public Quaternion handRotation;
        public Quaternion thumb0Rotation;
        public Quaternion thumb1Rotation;
        public Quaternion thumb2Rotation;
        public Quaternion index0Rotation;
        public Quaternion index1Rotation;
        public Quaternion index2Rotation;
        public Quaternion middle0Rotation;
        public Quaternion middle1Rotation;
        public Quaternion middle2Rotation;
        public Quaternion ring0Rotation;
        public Quaternion ring1Rotation;
        public Quaternion ring2Rotation;
        public Quaternion pinky0Rotation;
        public Quaternion pinky1Rotation;
        public Quaternion pinky2Rotation;

        public HandData(uint cameraTimestamp, int timeSinceLastLeftHandData, int timeSinceLastRightHandData,
        bool isWristPositionValid, Vector3 wristPosition,
        Quaternion wristRotation, Quaternion handRotation,
        Quaternion thumb0Rotation, Quaternion thumb1Rotation, Quaternion thumb2Rotation,
        Quaternion index0Rotation, Quaternion index1Rotation, Quaternion index2Rotation,
        Quaternion middle0Rotation, Quaternion middle1Rotation, Quaternion middle2Rotation,
        Quaternion ring0Rotation, Quaternion ring1Rotation, Quaternion ring2Rotation,
        Quaternion pinky0Rotation, Quaternion pinky1Rotation, Quaternion pinky2Rotation) {
            this.cameraTimestamp = cameraTimestamp;
            this.timeSinceLastLeftHandData = timeSinceLastLeftHandData;
            this.timeSinceLastRightHandData = timeSinceLastRightHandData;
            this.isWristPositionValid = isWristPositionValid;
            this.wristPosition = wristPosition;
            this.wristRotation = wristRotation;
            this.handRotation = handRotation;
            this.thumb0Rotation = thumb0Rotation;
            this.thumb1Rotation = thumb1Rotation;
            this.thumb2Rotation = thumb2Rotation;
            this.index0Rotation = index0Rotation;
            this.index1Rotation = index1Rotation;
            this.index2Rotation = index2Rotation;
            this.middle0Rotation = middle0Rotation;
            this.middle1Rotation = middle1Rotation;
            this.middle2Rotation = middle2Rotation;
            this.ring0Rotation = ring0Rotation;
            this.ring1Rotation = ring1Rotation;
            this.ring2Rotation = ring2Rotation;
            this.pinky0Rotation = pinky0Rotation;
            this.pinky1Rotation = pinky1Rotation;
            this.pinky2Rotation = pinky2Rotation;
        }

        public static HandData operator *(Quaternion lhs, HandData rhs) {
            HandData returnData = rhs;
            returnData.wristRotation = lhs * rhs.wristRotation;
            returnData.handRotation = lhs * rhs.handRotation;
            returnData.thumb0Rotation = lhs * rhs.thumb0Rotation;
            returnData.thumb1Rotation = lhs * rhs.thumb1Rotation;
            returnData.thumb2Rotation = lhs * rhs.thumb2Rotation;
            returnData.index0Rotation = lhs * rhs.index0Rotation;
            returnData.index1Rotation = lhs * rhs.index1Rotation;
            returnData.index2Rotation = lhs * rhs.index2Rotation;
            returnData.middle0Rotation = lhs * rhs.middle0Rotation;
            returnData.middle1Rotation = lhs * rhs.middle1Rotation;
            returnData.middle2Rotation = lhs * rhs.middle2Rotation;
            returnData.ring0Rotation = lhs * rhs.ring0Rotation;
            returnData.ring1Rotation = lhs * rhs.ring1Rotation;
            returnData.ring2Rotation = lhs * rhs.ring2Rotation;
            returnData.pinky0Rotation = lhs * rhs.pinky0Rotation;
            returnData.pinky1Rotation = lhs * rhs.pinky1Rotation;
            returnData.pinky2Rotation = lhs * rhs.pinky2Rotation;

            return returnData;
        }

        public void setFingerRotation(int finger, int joint, Quaternion q) {
            if(finger == 0 && joint == 0)
                thumb0Rotation = q;
            else if(finger == 0 && joint == 1)
                thumb1Rotation = q;
            else if(finger == 0 && joint == 2)
                thumb2Rotation = q;

            else if(finger == 1 && joint == 0)
                index0Rotation = q;
            else if(finger == 1 && joint == 1)
                index1Rotation = q;
            else if(finger == 1 && joint == 2)
                index2Rotation = q;

            else if(finger == 2 && joint == 0)
                middle0Rotation = q;
            else if(finger == 2 && joint == 1)
                middle1Rotation = q;
            else if(finger == 2 && joint == 2)
                middle2Rotation = q;

            else if(finger == 3 && joint == 0)
                ring0Rotation = q;
            else if(finger == 3 && joint == 1)
                ring1Rotation = q;
            else if(finger == 3 && joint == 2)
                ring2Rotation = q;

            else if(finger == 4 && joint == 0)
                pinky0Rotation = q;
            else if(finger == 4 && joint == 1)
                pinky1Rotation = q;
            else if(finger == 4 && joint == 2)
                pinky2Rotation = q;
        }

        public Quaternion getFingerRotation(int finger, int joint) {
            if(finger == 0 && joint == 0)
                return thumb0Rotation;
            else if(finger == 0 && joint == 1)
                return thumb1Rotation;
            else if(finger == 0 && joint == 2)
                return thumb2Rotation;

            else if(finger == 1 && joint == 0)
                return index0Rotation;
            else if(finger == 1 && joint == 1)
                return index1Rotation;
            else if(finger == 1 && joint == 2)
                return index2Rotation;

            else if(finger == 2 && joint == 0)
                return middle0Rotation;
            else if(finger == 2 && joint == 1)
                return middle1Rotation;
            else if(finger == 2 && joint == 2)
                return middle2Rotation;

            else if(finger == 3 && joint == 0)
                return ring0Rotation;
            else if(finger == 3 && joint == 1)
                return ring1Rotation;
            else if(finger == 3 && joint == 2)
                return ring2Rotation;

            else if(finger == 4 && joint == 0)
                return pinky0Rotation;
            else if(finger == 4 && joint == 1)
                return pinky1Rotation;
            else if(finger == 4 && joint == 2)
                return pinky2Rotation;
            else
                return new Quaternion();
        }

        public void convertToOpenVrQuaternions(bool isRightHand) {
            Quaternion zRot = new Quaternion(0, 0, 0.7071068f, 0.7071068f); // -90 around z axis
            wristRotation = wristRotation * zRot;
            handRotation = handRotation * zRot;

            Quaternion yRot = new Quaternion(0, -0.7071068f, 0, 0.7071068f); // 90 around y axis
            if(!isRightHand)
                yRot = yRot * new Quaternion(1, 0, 0, 0); //180 around x axis

            thumb0Rotation = thumb0Rotation * yRot * Quaternion.Euler(isRightHand ? 40 : -40, 0, 0);
            thumb1Rotation = thumb1Rotation * yRot;
            thumb2Rotation = thumb2Rotation * yRot;

            index0Rotation = index0Rotation * yRot;
            index1Rotation = index1Rotation * yRot;
            index2Rotation = index2Rotation * yRot;

            middle0Rotation = middle0Rotation * yRot;
            middle1Rotation = middle1Rotation * yRot;
            middle2Rotation = middle2Rotation * yRot;

            ring0Rotation = ring0Rotation * yRot;
            ring1Rotation = ring1Rotation * yRot;
            ring2Rotation = ring2Rotation * yRot;

            pinky0Rotation = pinky0Rotation * yRot;
            pinky1Rotation = pinky1Rotation * yRot;
            pinky2Rotation = pinky2Rotation * yRot;
        }

        public void convertFromOpenVrQuaternions(bool isRightHand) {
            Quaternion zRot = new Quaternion(0, 0, 0.7071068f, -0.7071068f); // -90 around z axis
            wristRotation = wristRotation * zRot;
            handRotation = handRotation * zRot;

            Quaternion yRot = new Quaternion(0, -0.7071068f, 0, -0.7071068f); // 90 around y axis
            if(!isRightHand)
                yRot = new Quaternion(1, 0, 0, 0) * yRot; //180 around x axis

            thumb0Rotation = thumb0Rotation * Quaternion.Euler(isRightHand ? -40 : 40, 0, 0) * yRot;
            thumb1Rotation = thumb1Rotation * yRot;
            thumb2Rotation = thumb2Rotation * yRot;

            index0Rotation = index0Rotation * yRot;
            index1Rotation = index1Rotation * yRot;
            index2Rotation = index2Rotation * yRot;

            middle0Rotation = middle0Rotation * yRot;
            middle1Rotation = middle1Rotation * yRot;
            middle2Rotation = middle2Rotation * yRot;

            ring0Rotation = ring0Rotation * yRot;
            ring1Rotation = ring1Rotation * yRot;
            ring2Rotation = ring2Rotation * yRot;

            pinky0Rotation = pinky0Rotation * yRot;
            pinky1Rotation = pinky1Rotation * yRot;
            pinky2Rotation = pinky2Rotation * yRot;
        }

        public String toString() {
            return "" +
                cameraTimestamp + ", " +
                timeSinceLastLeftHandData + ", " +
                timeSinceLastRightHandData + ", " +
                isWristPositionValid + ", \n" +

                wristPosition + "\n" +
                wristRotation + "\n" +
                handRotation + "\n" +

                thumb0Rotation + "\n" +
                index0Rotation + "\n" +
                middle0Rotation + "\n" +
                ring0Rotation + "\n" +
                pinky0Rotation + "\n";
        }

        public static readonly HandData calibrationHandDataR = new HandData {
            wristPosition = Vector3.zero,
            wristRotation = Quaternion.identity,
            handRotation = Quaternion.identity,
            thumb0Rotation = Quaternion.Euler(0, -32, 68),
            thumb1Rotation = Quaternion.Euler(0, -32, 68)*Quaternion.Euler(25, 12, 0),
            thumb2Rotation = Quaternion.Euler(0, -32, 68)*Quaternion.Euler(25, 12, 0)*Quaternion.Euler(-5, 0, 0),
            index0Rotation = Quaternion.Euler(0, 3, 0),
            index1Rotation = Quaternion.Euler(0, 3, 0),
            index2Rotation = Quaternion.Euler(0, 3, 0),
            middle0Rotation = Quaternion.Euler(0, 0, 0),
            middle1Rotation = Quaternion.Euler(0, 0, 0),
            middle2Rotation = Quaternion.Euler(0, 0, 0),
            ring0Rotation = Quaternion.Euler(0, -3, 0),
            ring1Rotation = Quaternion.Euler(0, -3, 0),
            ring2Rotation = Quaternion.Euler(0, -3, 0),
            pinky0Rotation = Quaternion.Euler(0, -5, 0),
            pinky1Rotation = Quaternion.Euler(0, -5, 0),
            pinky2Rotation = Quaternion.Euler(0, -5, 0)
        };

        public static readonly HandData calibrationHandDataL = new HandData {
            wristPosition = Vector3.zero,
            wristRotation = Quaternion.identity,
            handRotation = Quaternion.identity,
            thumb0Rotation = Quaternion.Euler(0, 32, -68),
            thumb1Rotation = Quaternion.Euler(0, 32, -68)*Quaternion.Euler(25, -12, 0),
            thumb2Rotation = Quaternion.Euler(0, 32, -68)*Quaternion.Euler(25, -12, 0)*Quaternion.Euler(-5, 0, 0),
            index0Rotation = Quaternion.Euler(0, -2, 0),
            index1Rotation = Quaternion.Euler(0, -2, 0),
            index2Rotation = Quaternion.Euler(0, -2, 0),
            middle0Rotation = Quaternion.Euler(0, 0, 0),
            middle1Rotation = Quaternion.Euler(0, 0, 0),
            middle2Rotation = Quaternion.Euler(0, 0, 0),
            ring0Rotation = Quaternion.Euler(0, 3, 0),
            ring1Rotation = Quaternion.Euler(0, 3, 0),
            ring2Rotation = Quaternion.Euler(0, 3, 0),
            pinky0Rotation = Quaternion.Euler(0, 6, 0),
            pinky1Rotation = Quaternion.Euler(0, 6, 0),
            pinky2Rotation = Quaternion.Euler(0, 6, 0)
        };

        public static VRfree.Vector3 Vector3ToVRfree(Vector3 v) {
            return new VRfree.Vector3(v.x, v.y, v.z);
        }

        public static Vector3 Vector3FromVRfree(VRfree.Vector3 v) {
            return new Vector3(v.x, v.y, v.z);
        }

        public static void Vector3ToVRfree(Vector3 inVec, ref VRfree.Vector3 outVec) {
            outVec.x = inVec.x;
            outVec.y = inVec.y;
            outVec.z = inVec.z;
        }

        public static void Vector3FromVRfree(VRfree.Vector3 inVec, ref Vector3 outVec) {
            outVec.x = inVec.x;
            outVec.y = inVec.y;
            outVec.z = inVec.z;
        }

        public static VRfree.Quaternion QuaternionToVRfree(Quaternion q) {
            return new VRfree.Quaternion(q.x, q.y, q.z, q.w);
        }

        public static Quaternion QuaternionFromVRfree(VRfree.Quaternion q) {
            return new Quaternion(q.x, q.y, q.z, q.w);
        }

        public static void QuaternionToVRfree(Quaternion inQ, ref VRfree.Quaternion outQ) {
            outQ.x = inQ.x;
            outQ.y = inQ.y;
            outQ.z = inQ.z;
            outQ.w = inQ.w;
        }

        public static void QuaternionFromVRfree(VRfree.Quaternion inQ, ref Quaternion outQ) {
            outQ.x = inQ.x;
            outQ.y = inQ.y;
            outQ.z = inQ.z;
            outQ.w = inQ.w;
        }

        public static VRfree.HandData HandDataToVRfree(HandData h) {
            return new VRfree.HandData(
                h.cameraTimestamp, h.timeSinceLastLeftHandData, h.timeSinceLastRightHandData, h.isWristPositionValid, Vector3ToVRfree(h.wristPosition),
                QuaternionToVRfree(h.wristRotation), QuaternionToVRfree(h.handRotation),
                QuaternionToVRfree(h.thumb0Rotation), QuaternionToVRfree(h.thumb1Rotation), QuaternionToVRfree(h.thumb2Rotation),
                QuaternionToVRfree(h.index0Rotation), QuaternionToVRfree(h.index1Rotation), QuaternionToVRfree(h.index2Rotation),
                QuaternionToVRfree(h.middle0Rotation), QuaternionToVRfree(h.middle1Rotation), QuaternionToVRfree(h.middle2Rotation),
                QuaternionToVRfree(h.ring0Rotation), QuaternionToVRfree(h.ring1Rotation), QuaternionToVRfree(h.ring2Rotation),
                QuaternionToVRfree(h.pinky0Rotation), QuaternionToVRfree(h.pinky1Rotation), QuaternionToVRfree(h.pinky2Rotation));
        }

        public static HandData HandDataFromVRfree(VRfree.HandData h) {
            return new HandData(
                h.cameraTimestamp, h.timeSinceLastLeftHandData, h.timeSinceLastRightHandData, h.isWristPositionValid, Vector3FromVRfree(h.wristPosition),
                QuaternionFromVRfree(h.wristRotation), QuaternionFromVRfree(h.handRotation),
                QuaternionFromVRfree(h.thumb0Rotation), QuaternionFromVRfree(h.thumb1Rotation), QuaternionFromVRfree(h.thumb2Rotation),
                QuaternionFromVRfree(h.index0Rotation), QuaternionFromVRfree(h.index1Rotation), QuaternionFromVRfree(h.index2Rotation),
                QuaternionFromVRfree(h.middle0Rotation), QuaternionFromVRfree(h.middle1Rotation), QuaternionFromVRfree(h.middle2Rotation),
                QuaternionFromVRfree(h.ring0Rotation), QuaternionFromVRfree(h.ring1Rotation), QuaternionFromVRfree(h.ring2Rotation),
                QuaternionFromVRfree(h.pinky0Rotation), QuaternionFromVRfree(h.pinky1Rotation), QuaternionFromVRfree(h.pinky2Rotation));
        }

        public static void HandDataFromVRfree(VRfree.HandData h, ref HandData outData) {
            outData.cameraTimestamp = h.cameraTimestamp;
            outData.timeSinceLastLeftHandData = h.timeSinceLastLeftHandData;
            outData.timeSinceLastRightHandData = h.timeSinceLastRightHandData;
            outData.isWristPositionValid = h.isWristPositionValid;
            Vector3FromVRfree(h.wristPosition, ref outData.wristPosition);
            QuaternionFromVRfree(h.wristRotation, ref outData.wristRotation);
            QuaternionFromVRfree(h.handRotation, ref outData.handRotation);
            QuaternionFromVRfree(h.thumb0Rotation, ref outData.thumb0Rotation);
            QuaternionFromVRfree(h.thumb1Rotation, ref outData.thumb1Rotation);
            QuaternionFromVRfree(h.thumb2Rotation, ref outData.thumb2Rotation);
            QuaternionFromVRfree(h.index0Rotation, ref outData.index0Rotation);
            QuaternionFromVRfree(h.index1Rotation, ref outData.index1Rotation);
            QuaternionFromVRfree(h.index2Rotation, ref outData.index2Rotation);
            QuaternionFromVRfree(h.middle0Rotation, ref outData.middle0Rotation);
            QuaternionFromVRfree(h.middle1Rotation, ref outData.middle1Rotation);
            QuaternionFromVRfree(h.middle2Rotation, ref outData.middle2Rotation);
            QuaternionFromVRfree(h.ring0Rotation, ref outData.ring0Rotation);
            QuaternionFromVRfree(h.ring1Rotation, ref outData.ring1Rotation);
            QuaternionFromVRfree(h.ring2Rotation, ref outData.ring2Rotation);
            QuaternionFromVRfree(h.pinky0Rotation, ref outData.pinky0Rotation);
            QuaternionFromVRfree(h.pinky1Rotation, ref outData.pinky1Rotation);
            QuaternionFromVRfree(h.pinky2Rotation, ref outData.pinky2Rotation);
        }
    }

    [System.Serializable]
    public class HandTransforms {
        public Transform wristTransform;    //this transform will receive position and rotation, while all others just receive rotations
        public Transform handTransform;
        public Transform thumb0Transform;
        public Transform thumb1Transform;
        public Transform thumb2Transform;
        public Transform index0Transform;
        public Transform index1Transform;
        public Transform index2Transform;
        public Transform middle0Transform;
        public Transform middle1Transform;
        public Transform middle2Transform;
        public Transform ring0Transform;
        public Transform ring1Transform;
        public Transform ring2Transform;
        public Transform pinky0Transform;
        public Transform pinky1Transform;
        public Transform pinky2Transform;

        public void applyHandData(HandData handData) {
            if(wristTransform != null) { if(!float.IsNaN(handData.wristPosition.x)) wristTransform.position = handData.wristPosition; wristTransform.rotation = handData.wristRotation; }
            if(handTransform != null) { handTransform.rotation = handData.handRotation; }
            if(thumb0Transform != null) { thumb0Transform.rotation = handData.thumb0Rotation; }
            if(thumb1Transform != null) { thumb1Transform.rotation = handData.thumb1Rotation; }
            if(thumb2Transform != null) { thumb2Transform.rotation = handData.thumb2Rotation; }
            if(index0Transform != null) { index0Transform.rotation = handData.index0Rotation; }
            if(index1Transform != null) { index1Transform.rotation = handData.index1Rotation; }
            if(index2Transform != null) { index2Transform.rotation = handData.index2Rotation; }
            if(middle0Transform != null) { middle0Transform.rotation = handData.middle0Rotation; }
            if(middle1Transform != null) { middle1Transform.rotation = handData.middle1Rotation; }
            if(middle2Transform != null) { middle2Transform.rotation = handData.middle2Rotation; }
            if(ring0Transform != null) { ring0Transform.rotation = handData.ring0Rotation; }
            if(ring1Transform != null) { ring1Transform.rotation = handData.ring1Rotation; }
            if(ring2Transform != null) { ring2Transform.rotation = handData.ring2Rotation; }
            if(pinky0Transform != null) { pinky0Transform.rotation = handData.pinky0Rotation; }
            if(pinky1Transform != null) { pinky1Transform.rotation = handData.pinky1Rotation; }
            if(pinky2Transform != null) { pinky2Transform.rotation = handData.pinky2Rotation; }
        }

        /*
         * finger going from 0 (thumb) to 4 (pinky finger), index going from 0 to 2
         */
        public Transform getFingerTransform(int finger, int index) {
            if(finger == 0 && index == 0)
                return thumb0Transform;
            else if(finger == 0 && index == 1)
                return thumb1Transform;
            else if(finger == 0 && index == 2)
                return thumb2Transform;
            else if(finger == 1 && index == 0)
                return index0Transform;
            else if(finger == 1 && index == 1)
                return index1Transform;
            else if(finger == 1 && index == 2)
                return index2Transform;
            else if(finger == 2 && index == 0)
                return middle0Transform;
            else if(finger == 2 && index == 1)
                return middle1Transform;
            else if(finger == 2 && index == 2)
                return middle2Transform;
            else if(finger == 3 && index == 0)
                return ring0Transform;
            else if(finger == 3 && index == 1)
                return ring1Transform;
            else if(finger == 3 && index == 2)
                return ring2Transform;
            else if(finger == 4 && index == 0)
                return pinky0Transform;
            else if(finger == 4 && index == 1)
                return pinky1Transform;
            else if(finger == 4 && index == 2)
                return pinky2Transform;
            else
                return null;
        }

        /*
     * finger going from 0 (thumb) to 4 (pinky finger), index going from 0 to 2
     */
        public void setFingerTransform(int finger, int index, Transform transform) {
            if(finger == 0 && index == 0)
                thumb0Transform = transform;
            else if(finger == 0 && index == 1)
                thumb1Transform = transform;
            else if(finger == 0 && index == 2)
                thumb2Transform = transform;
            else if(finger == 1 && index == 0)
                index0Transform = transform;
            else if(finger == 1 && index == 1)
                index1Transform = transform;
            else if(finger == 1 && index == 2)
                index2Transform = transform;
            else if(finger == 2 && index == 0)
                middle0Transform = transform;
            else if(finger == 2 && index == 1)
                middle1Transform = transform;
            else if(finger == 2 && index == 2)
                middle2Transform = transform;
            else if(finger == 3 && index == 0)
                ring0Transform = transform;
            else if(finger == 3 && index == 1)
                ring1Transform = transform;
            else if(finger == 3 && index == 2)
                ring2Transform = transform;
            else if(finger == 4 && index == 0)
                pinky0Transform = transform;
            else if(finger == 4 && index == 1)
                pinky1Transform = transform;
            else if(finger == 4 && index == 2)
                pinky2Transform = transform;
        }
    };
}