/*
 * When using a VRfree Head Module that is attached to the HMD, attach this script to the VR camera Transform in the scene as a position and orientation reference.
 * The VRfree position tracking needs at least one VRfreeCamera or VRfreeFixedHeadModulePosition script in the scene!
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRfreePluginUnity;

namespace VRfreePluginUnity {     
    [ScriptOrder(-101)]
    public class VRfreeCamera : MonoBehaviour {
        public static VRfreeCamera Instance;

        [Header("Settings")]
        [Tooltip("When the VRfree head module is attached to an HMD, set this to false, when attached to a fixed position in space set this to true.")]
        public bool fixedHeadModule = false;

        [Tooltip("When true, disconnects the VRfree device and stops the corresponding thread when the app is paused "
         + "as opposed to when it is destroyed. Recommended on Android, since apps are not destroyed when exiting with "
         + "the home button, and continuing to be connected can unnecessarily drain the battery of the phone")]
        public bool disconnectOnPause = false;
    
        public bool allowOnlyHeadModuleUSB = true;

        private bool isPaused = true;

        [Header("Outputs")]
        public VRfree.StatusCode statusCode;
        public List<VRfree.DeviceType> connectedDevices;

        void Start() {
            if(Instance == null ) {
                Instance = this;
                Debug.Log("Registering VRfreeCamera Instance.");
            } else {
                this.enabled = false;
                Debug.Log("Another VRfreeCamera already active in scene, disabling.");
            }
        }

        void FixedUpdate() {
            VRfree.VRfreeAPI.UpdateCameraPose(HandData.Vector3ToVRfree(transform.position), HandData.QuaternionToVRfree(transform.rotation), fixedHeadModule);
            statusCode = VRfree.VRfreeAPI.StatusCode();
            connectedDevices = VRfree.VRfreeAPI.GetConnectedDevices();
        }

        public void OnEnable() {
            StartVRfree();
        }

        void OnApplicationFocus(bool hasFocus) {
            if (disconnectOnPause) {
                if (isPaused && hasFocus) {
                    isPaused = false;
                    Debug.Log("VRfreeGlove application gained focus");
                    StartVRfree();
                } else if (!isPaused && !hasFocus) {
                    isPaused = true;
                    Debug.Log("VRfreeGlove application lost focus");
                    ShutdownVRfree();
                }
            }
        }

        void OnApplicationPause(bool pauseStatus) {
            if (disconnectOnPause) {
                if (isPaused && !pauseStatus) {
                    Debug.Log("VRfreeGlove application unpaused");
                    StartVRfree();
                } else if (!isPaused && pauseStatus) {
                    Debug.Log("VRfreeGlove application paused");
                    ShutdownVRfree();
                }
            }
        }

        private void StartVRfree() {
#if WINDOWS_UWP
            VRfree.VRfreeAPI.CustomSaveFilePath = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
#endif
            VRfree.VRfreeAPI.Init();
            if (allowOnlyHeadModuleUSB) {
                VRfree.CalibrationAPI.SetAllowedDevices(new List<VRfree.VRfreePID>() { VRfree.VRfreePID.Head });
            }
            VRfree.VRfreeAPI.Start();
            isPaused = false;
        }

        private void ShutdownVRfree() {
            VRfree.VRfreeAPI.Shutdown();
            isPaused = true;
        }

        protected void OnDisable() {
            Debug.Log("VRfreeCamera OnDisable");
            //release the VRfree library to indicate that this instance does not use it anymore
            StopAllCoroutines();
            VRfree.VRfreeAPI.Shutdown();
        }

    }
}
