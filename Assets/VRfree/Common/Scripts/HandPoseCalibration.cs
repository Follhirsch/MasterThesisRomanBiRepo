using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace VRfreePluginUnity {
    public class HandPoseCalibration : MonoBehaviour {
        [Header("References")]
        public ProgressBar progressbar;
        public Text errorText;

        [Header("Settings")]
        public bool adaptHeading = false;
        public bool calibrateHeadModuleHeadingOffset = true;

        [Header("Output")]
        public bool isCalibrating;
        public int currentCalibrationStep;
        public int totalNumCalibrationSteps;
        public float progress;

        [Header("Events")]
        public UnityEvent onCalibrationFinished;

        // Start is called before the first frame update
        void Start() {

        }

        // Update is called once per frame
        void Update() {

        }

        void OnValidate() {
            VRfree.HandPoseCalibrationSettings.adaptHeading = adaptHeading;
            VRfree.HandPoseCalibrationSettings.calibrateHeadModuleHeadingOffset = calibrateHeadModuleHeadingOffset;

        }

        public void StartCalibration() {
            Debug.Log("HandPoseCalibrationDll StartCalibration");

            VRfree.HandPoseCalibrationSettings.adaptHeading = adaptHeading;
            VRfree.HandPoseCalibrationSettings.calibrateHeadModuleHeadingOffset = calibrateHeadModuleHeadingOffset;

            VRfree.CalibrationAPI.StartPoseCalibration(CalibrationStatusUpdate, CalibrationErrorCallback);
            if(progressbar != null) progressbar.gameObject.SetActive(true);
            if (errorText != null) errorText.text = "";
        }

        private void CalibrationStatusUpdate(int currentCalibrationStep, int totalNumCalibrationSteps, float progress) {
            this.currentCalibrationStep = currentCalibrationStep;
            this.totalNumCalibrationSteps = totalNumCalibrationSteps;
            this.progress = progress;
            if (progressbar != null) progressbar.progress = progress;
            isCalibrating = VRfree.HandPoseCalibrationSettings.IsCalibrating;
            if(!isCalibrating) {
                if(progressbar != null) progressbar.gameObject.SetActive(false);
                onCalibrationFinished.Invoke();
            }
            if(VRfree.HandPoseCalibrationSettings.adaptHeading && currentCalibrationStep > 1) {
                VRfree.HandPoseCalibrationSettings.adaptHeading = false;
            }
        }

        private void CalibrationErrorCallback(string errorMessage) {
            if (errorText != null) errorText.text = errorMessage;
        }

    }
}
