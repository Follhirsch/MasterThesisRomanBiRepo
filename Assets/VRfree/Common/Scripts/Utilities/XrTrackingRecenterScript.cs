using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRfreePluginUnity {
    public class XrTrackingRecenterScript : MonoBehaviour {
#if UNITY_STANDALONE_WIN
        public Valve.VR.ETrackingUniverseOrigin trackingUniverseOrigin = Valve.VR.ETrackingUniverseOrigin.TrackingUniverseSeated;
#endif
        public bool recenterOnStart = true;
        public string recenterKey = "c";

        // Use this for initialization
        void Start() {
#if UNITY_STANDALONE_WIN
            Valve.VR.CVRCompositor compositor = Valve.VR.OpenVR.Compositor;
            if(compositor != null)
                compositor.SetTrackingSpace(trackingUniverseOrigin);
#endif
            if (recenterOnStart) {
                recenter();
            }
        }

        void Update() {
            if(Input.GetKeyDown(recenterKey)) {
                recenter();
            }
        }

        public void recenter() {
            UnityEngine.XR.InputTracking.Recenter();
        }

    }
}