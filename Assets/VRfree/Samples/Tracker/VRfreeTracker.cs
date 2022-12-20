using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.XR.Interaction;
using UnityEngine.SpatialTracking;

namespace VRfreePluginUnity {
    public class VRfreeTracker : BasePoseProvider {
        [Header("Settings")]
        public int trackerId;
        public bool rotationOnly = false;
        public GameObject hideWhenTrackingLost;

        [Header("Output")]
        public Vector3 trackerPosition;
        public Quaternion trackerRotation;
        public bool isTrackerPositionValid;
        public byte buttonPressed;

        [Header("Events")]
        public UnityEvent buttonPressedEvent;
        public UnityEvent buttonReleasedEvent;

        // Start is called before the first frame update
        public void Start() {
            isTrackerPositionValid = false;

            TrackedPoseDriver trackedPoseDriver = GetComponent<TrackedPoseDriver>();
            if (trackedPoseDriver == null) {
                trackedPoseDriver = gameObject.AddComponent<TrackedPoseDriver>();
                trackedPoseDriver.updateType = TrackedPoseDriver.UpdateType.BeforeRender;
                trackedPoseDriver.UseRelativeTransform = false;
            }
            trackedPoseDriver.poseProviderComponent = this;
        }

#if UNITY_2018
        public override bool TryGetPoseFromProvider(out Pose output) {
#else
        public override PoseDataFlags GetPoseFromProvider(out Pose output) {
#endif
            VRfree.Vector3 outPos;
            VRfree.Quaternion outQuat;
            byte newButtonPressed;
            bool isTrackerPositionValidNew;
            GetTrackerData(out outPos, out outQuat, out isTrackerPositionValidNew, out newButtonPressed);

            if (buttonPressed == 0 && newButtonPressed != 0) {
                buttonPressedEvent.Invoke();
            } else if (buttonPressed != 0 && newButtonPressed == 0) {
                buttonReleasedEvent.Invoke();
            }
            buttonPressed = newButtonPressed;

            if (outQuat.x == 0 && outQuat.y == 0 && outQuat.z == 0 && outQuat.w == 0) outQuat = VRfree.Quaternion.identity;
            trackerPosition = outPos.FromVRfree();
            trackerRotation = outQuat.FromVRfree();

            if (!isTrackerPositionValid && isTrackerPositionValidNew) {
                if (hideWhenTrackingLost != null) {
                    hideWhenTrackingLost.SetActive(true);
                }
            } else if (isTrackerPositionValid && !isTrackerPositionValidNew) {
                if (hideWhenTrackingLost != null) {
                    hideWhenTrackingLost.SetActive(false);
                }
            }
            isTrackerPositionValid = isTrackerPositionValidNew;

#if UNITY_2018
            output = new Pose(isTrackerPositionValid ? trackerPosition : transform.localPosition, trackerRotation);
            return true;
#else
            output = new Pose(trackerPosition, trackerRotation);
            return isTrackerPositionValid ? PoseDataFlags.Position | PoseDataFlags.Rotation : PoseDataFlags.Rotation;
#endif
        }

        protected virtual void GetTrackerData(out VRfree.Vector3 position, out VRfree.Quaternion rotation, out bool isTrackerPositionValid, out byte buttonPressed) {
            VRfree.VRfreeAPI.GetTrackerData(out position, out rotation, out isTrackerPositionValid, out buttonPressed, trackerId);
            if (transform.parent != null) {
                position = transform.parent.InverseTransformPoint(position.FromVRfree()).ToVRfree();
                rotation = Quaternion.Inverse(transform.parent.rotation).ToVRfree() * rotation;
            }
        }
    }
}