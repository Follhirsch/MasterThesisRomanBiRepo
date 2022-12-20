using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRfreePluginUnity {
    public class VRfreeAdvancedSettings : MonoBehaviour {
        [Tooltip("Limit of extrapolation time in case of lost signal/tracking in ms")]
        public float maxExtrapolationTime = 100; //ms
        [Tooltip("Time in ms that is added to the position tracking extrapolation. Can reduce visible latency but increases noise.")]
        public float additionalPredictionTime = 10; //ms
        [Tooltip("Time in ms that is added to the rotation tracking extrapolation. Can reduce visible latency but increases noise.")]
        public float additionalPredictionTimeQuaternions = 0; //ms
        [Tooltip("When enabled, the position gets translated from head module space to world space using the hmd rotation and head module " +
            "offset rather than the head module rotation. Recommended for AR, but not necessarily for VR.")]
        public bool positionRelativeToHmd = false;

        // Start is called before the first frame update
        void Start() {
            ApplySettings();
        }

        // Update is called once per frame
        private void OnValidate() {
            ApplySettings();
        }

        public void ApplySettings() {
            VRfree.DebugAPI.SetPositionRelativeToHmd(positionRelativeToHmd);
            VRfree.VRfreeAPI.maxExtrapolationTime = maxExtrapolationTime;
            VRfree.VRfreeAPI.additionalPredictionTime = additionalPredictionTime;
            VRfree.VRfreeAPI.additionalPredictionTimeQuaternions = additionalPredictionTimeQuaternions;
        }
    }
}
