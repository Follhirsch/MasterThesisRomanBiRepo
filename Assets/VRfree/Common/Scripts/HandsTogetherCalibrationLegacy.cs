/*
 * The hands together calibration is more accurate than the simple calibration.
 * It is started by calling startCalibration() or by pressing the handsTogetherCalibrationKey.
 * The calibration pose for this is holding both hands and forearms together out flat forwards, 
 * with all fingers touching each other.
 * In the first step, the calibration pose is displayed through the VRfreeGlove. 
 * The user should then hold his hand exactly like displayed. After both wrist positions have 
 * stopped moving for holdStillTime, the first stage of the calibration is finished.
 * Since both hands are calibrated together, their orientations will match more precisely than
 * with the simple calibration. Furthermore, this can also calibrate the offset of the wrist tracker
 * in one direction.
 * Created by Hagen Seifert on 26.04.2018
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VRfreePluginUnity {
    public class HandsTogetherCalibrationLegacy : MonoBehaviour {
        [Header("References")]
        public VRfreeGlove rightGlove;
        public VRfreeGlove leftGlove;
        /* If the HandTransforms in the right and leftGlove aren't assigned, but there are HandControllers whose HandTrransforms are assigned,
         * set them here. Otherwise, leave rightHandController and leftHandController unassigned. */
        public HandController rightHandController;
        public HandController leftHandController;
        public ProgressBar progressBar;


        [Header("Configuration")]
        public string handsTogetherCalibrationKey = "z";

        public bool calibrateWristOffset = true;
        public bool twoStageWristOffsetCalibration = true;
        public bool headingCalibrationStep = true;

        public float wristOffsetY = 0.03f;
        public float holdStillTime = 3.0f;
        public float maxHoldStillMovement = 0.02f;

        public bool adaptInclination = true;
        public bool adaptHeading = false;
        public bool visualizeInclinationAndHeading = false;
        public float inclinationOffset = -1;
        public float headingOffset = 0;

        public float maxWristOffsetMagnitude = 0.1f;

        [Header("Calibration Poses")]
        public HandData Calibration1HandDataR;
        public HandData Calibration1HandDataL;

        public HandData Calibration2HandDataR;
        public HandData Calibration2HandDataL;

        public HandData Calibration3HandDataR;
        public HandData Calibration3HandDataL;

        public string calibrationPosesString = "";


        // private / not editor exposed variables
        public static bool IsCalibrating;
        private int calibrationStage = 0;
        private Vector3 wristOffsetR, wristOffsetL;
        private Quaternion calibrationDirection;

        private Vector3[] wristOffsetDirectionsR = new Vector3[3];
        private Vector3[] wristOffsetDirectionsL = new Vector3[3];

        private float timeSinceHoldStill = 0.0f;
        private Vector3 holdStillPositionR;
        private Vector3 holdStillPositionL;

        private bool wereConstraintsActive;

        public UnityEvent onCalibrationComplete;

        // Use this for initialization
        void Start() {
            progressBar.gameObject.SetActive(false);
            calibrationStage = 0;
        }

        // Update is called once per frame
        void Update() {
            if(Input.GetKeyDown(handsTogetherCalibrationKey)) {
                startCalibration();
            }
            //drawDebug();
            if(calibrationStage > 0) {
                IsCalibrating = true;

                //check if hands moved
                if((holdStillPositionR - rightGlove.handData.wristPosition).magnitude > maxHoldStillMovement
                        || (holdStillPositionL - leftGlove.handData.wristPosition).magnitude > maxHoldStillMovement) {
                    holdStillPositionR = rightGlove.handData.wristPosition;
                    holdStillPositionL = leftGlove.handData.wristPosition;

                    progressBar.progress = 0;
                    timeSinceHoldStill = 0.0f;
                }

                if(leftGlove.handData.isWristPositionValid && VRfree.DebugAPI.GetHandTrackingState(false) == VRfree.TrackingState.Tracking 
                    && rightGlove.handData.isWristPositionValid  && VRfree.DebugAPI.GetHandTrackingState(true) == VRfree.TrackingState.Tracking)
                    timeSinceHoldStill += Time.deltaTime;
                progressBar.progress = timeSinceHoldStill / holdStillTime;

                if(adaptHeading) {
                    calibrationDirection = getAverageHeading();
                }

                if(adaptInclination && visualizeInclinationAndHeading) {
                    showCalibrationPose(rightGlove, adaptHeading ? getAverageHeading() : calibrationDirection, calibrationStage, true);
                    showCalibrationPose(leftGlove, adaptHeading ? getAverageHeading() : calibrationDirection, calibrationStage, true);
                } else {
                    // make sure progress bar points upwards
                    progressBar.transform.rotation = Quaternion.LookRotation(Vector3.down, calibrationDirection*Vector3.forward);
                }

                if(timeSinceHoldStill > holdStillTime) {
                    if(calibrationStage == 1) {
                        if(adaptInclination && !visualizeInclinationAndHeading) {
                            showCalibrationPose(rightGlove, calibrationDirection, 1, true);
                            showCalibrationPose(leftGlove, calibrationDirection, 1, true);
                        }
                        calibrate1();

                        if(calibrateWristOffset && twoStageWristOffsetCalibration) {
                            calibrationStage = 2;

                            showCalibrationPose(rightGlove, calibrationDirection, 2, adaptInclination && visualizeInclinationAndHeading);
                            showCalibrationPose(leftGlove, calibrationDirection, 2, adaptInclination && visualizeInclinationAndHeading);

                            progressBar.progress = 0;
                            timeSinceHoldStill = 0.0f;
                        } else if(headingCalibrationStep) {
                            calibrationStage = 3;

                            showCalibrationPose(rightGlove, calibrationDirection, 3, adaptInclination && visualizeInclinationAndHeading);
                            showCalibrationPose(leftGlove, calibrationDirection, 3, adaptInclination && visualizeInclinationAndHeading);

                            wereConstraintsActive = VRfree.VRfreeAPI.GetConstraintsActive();
                            VRfree.VRfreeAPI.SetConstraintsActive(false);

                            progressBar.progress = 0;
                            timeSinceHoldStill = 0.0f;
                        } else {
                            StopCalibration();
                        }
                    } else if(calibrationStage == 2) {
                        calibrate2();

                        if(headingCalibrationStep) {
                            calibrationStage = 3;

                            showCalibrationPose(rightGlove, calibrationDirection, 3, adaptInclination && visualizeInclinationAndHeading);
                            showCalibrationPose(leftGlove, calibrationDirection, 3, adaptInclination && visualizeInclinationAndHeading);

                            //rightGlove.stopShowingCalibrationPose();
                            //leftGlove.stopShowingCalibrationPose();

                            wereConstraintsActive = VRfree.VRfreeAPI.GetConstraintsActive();
                            VRfree.VRfreeAPI.SetConstraintsActive(false);

                            progressBar.progress = 0;
                            timeSinceHoldStill = 0.0f;
                        } else {
                            // stop showing calibration pose
                            StopCalibration();
                        }
                    } else if(calibrationStage == 3) {
                        calibrate3();

                        StopCalibration();

                        VRfree.VRfreeAPI.SetConstraintsActive(wereConstraintsActive);
                    }
                }
            } else {
                IsCalibrating = false;
            }
        }

        private void StopCalibration() {
            rightGlove.stopShowingCalibrationPose();
            leftGlove.stopShowingCalibrationPose();

            progressBar.gameObject.SetActive(false);
            calibrationStage = 0;
            onCalibrationComplete.Invoke();
        }

        private void OnValidate() {
            twoStageWristOffsetCalibration = calibrateWristOffset && twoStageWristOffsetCalibration;
        }

        private float getInclinationAngle(Vector3 v) {
            Vector3 horizontal = new Vector3(v.x, 0, v.z);
            return Mathf.Sign(v.y) * Vector3.Angle(v, horizontal);
        }

        private float getAverageInclinationAngle() {
            float sumX = 0, sumY = 0;
            float angle;
            float radToDeg = 57.2957795131f;
            float degToRad = 1 / radToDeg;

            // see https://en.wikipedia.org/wiki/Mean_of_circular_quantities
            for(int h = 0; h < 2; h++) {
                VRfree.HandData raw = VRfree.VRfreeAPI.GetHandDataRaw(h == 0);

                Vector3 imuFwd = Vector3.right;

                angle = getInclinationAngle(HandData.QuaternionFromVRfree(raw.wristRotation.normalized) * imuFwd);
                //sumX += (float)Math.Sin(angle* degToRad); sumY += (float)Math.Cos(angle* degToRad);
                for(int f = 1; f < 5; f++) {
                    Vector3 fwd = HandData.QuaternionFromVRfree(raw.getFingerRotation(f, 0).normalized)* imuFwd
                                        + HandData.QuaternionFromVRfree(raw.getFingerRotation(f, 1).normalized) * imuFwd;
                    angle = getInclinationAngle(fwd);
                    sumX += (float)Math.Sin(angle* degToRad); sumY += (float)Math.Cos(angle* degToRad);
                    VRfreeGlove glove = (h == 0) ? rightGlove : leftGlove;
                    //Debug.DrawRay(glove.handTransforms.getFingerTransform(f, 0).position, 0.05f * (fwd));
                }
            }

            return -Mathf.Atan2(sumX, sumY) * radToDeg + inclinationOffset;
        }

        private float getHeadingAngle(Vector3 v) {
            Vector3 horizontal = new Vector3(v.x, 0, v.z);
            return Vector3.SignedAngle(Vector3.forward, horizontal, Vector3.up);
        }

        private Quaternion getAverageHeading() {
            float sumX = 0, sumY = 0;
            float angle;
            float radToDeg = 57.2957795131f;
            float degToRad = 1 / radToDeg;

            Quaternion headToCameraHeading = HandData.QuaternionFromVRfree(VRfree.VRfreeAPI.GetHeadModuleRotation()
                * VRfree.Quaternion.Inverse(VRfree.VRfreeAPI.GetHeadModuleRotationRaw()));

            // see https://en.wikipedia.org/wiki/Mean_of_circular_quantities
            for(int h = 0; h < 2; h++) {
                VRfree.HandData raw = VRfree.VRfreeAPI.GetHandDataRaw(h == 0);

                angle = getHeadingAngle(HandData.QuaternionFromVRfree(raw.wristRotation.normalized) * Vector3.forward);
                //sumX += (float)Math.Sin(angle* degToRad); sumY += (float)Math.Cos(angle* degToRad);
                for(int f = 1; f < 5; f++) {
                    Vector3 fwd = HandData.QuaternionFromVRfree(raw.getFingerRotation(f, 0).normalized)* Vector3.forward
                                        + HandData.QuaternionFromVRfree(raw.getFingerRotation(f, 1).normalized) * Vector3.forward;
                    angle = getHeadingAngle(fwd);
                    sumX += (float)Math.Sin(angle* degToRad); sumY += (float)Math.Cos(angle* degToRad);
                    VRfreeGlove glove = (h == 0) ? rightGlove : leftGlove;
                    Debug.DrawRay(glove.handTransforms.getFingerTransform(f, 0).position, 0.05f * (fwd));
                }
            }

            return headToCameraHeading * Quaternion.AngleAxis(Mathf.Atan2(sumX, sumY) * radToDeg + headingOffset, Vector3.up);
        }

        private Vector3 getAverageFingerDirection(HandData handData) {
            Vector3 sum = Vector3.zero;
            sum += 6 * (handData.handRotation * Vector3.forward);
            Vector3 fwd = rightGlove.convertForSteamVrGlove ? Vector3.right : Vector3.forward;
            for (int f = 1; f < 5; f++) {
                sum += handData.getFingerRotation(f, 0) * fwd + handData.getFingerRotation(f, 1) * fwd;
            }
            return sum.normalized;
        }

        public void startCalibration() {
            calibrationDirection = Quaternion.LookRotation(new Vector3(VRfreeCamera.Instance.transform.forward.x, 0, VRfreeCamera.Instance.transform.forward.z));

            showCalibrationPose(rightGlove, calibrationDirection, 1, adaptInclination && visualizeInclinationAndHeading);
            showCalibrationPose(leftGlove, calibrationDirection, 1, adaptInclination && visualizeInclinationAndHeading);

            if (calibrateWristOffset) {
                rightGlove.setWristOffset(Vector3.zero);
                leftGlove.setWristOffset(Vector3.zero);
            }

            holdStillPositionR = rightGlove.handData.wristPosition;
            holdStillPositionL = leftGlove.handData.wristPosition;

            progressBar.gameObject.SetActive(true);
            progressBar.progress = 0;
            timeSinceHoldStill = 0.0f;
            calibrationStage = 1;
        }

        public void setCurrentPoseCalibrationPose(int stage) {
            HandTransforms rightHandTransforms;
            if(rightGlove.handTransforms.wristTransform != null)
                rightHandTransforms = rightGlove.handTransforms;
            else if(rightHandController != null)
                rightHandTransforms = rightHandController.handTransforms;
            else
                return;

            HandData rightHD = new HandData {
                wristPosition = rightHandTransforms.wristTransform.position - VRfreeCamera.Instance.transform.position,
                wristRotation = rightHandTransforms.wristTransform.rotation,
                handRotation = rightHandTransforms.handTransform.rotation,
                thumb0Rotation = rightHandTransforms.thumb0Transform.rotation,
                thumb1Rotation = rightHandTransforms.thumb1Transform.rotation,
                thumb2Rotation = rightHandTransforms.thumb2Transform.rotation,
                index0Rotation = rightHandTransforms.index0Transform.rotation,
                index1Rotation = rightHandTransforms.index1Transform.rotation,
                index2Rotation = rightHandTransforms.index2Transform.rotation,
                middle0Rotation = rightHandTransforms.middle0Transform.rotation,
                middle1Rotation = rightHandTransforms.middle1Transform.rotation,
                middle2Rotation = rightHandTransforms.middle2Transform.rotation,
                ring0Rotation = rightHandTransforms.ring0Transform.rotation,
                ring1Rotation = rightHandTransforms.ring1Transform.rotation,
                ring2Rotation = rightHandTransforms.ring2Transform.rotation,
                pinky0Rotation = rightHandTransforms.pinky0Transform.rotation,
                pinky1Rotation = rightHandTransforms.pinky1Transform.rotation,
                pinky2Rotation = rightHandTransforms.pinky2Transform.rotation
            };
            rightHD = Quaternion.Inverse(VRfreeCamera.Instance.transform.rotation) * rightHD;

            HandTransforms leftHandTransforms;
            if(leftGlove.handTransforms.wristTransform != null)
                leftHandTransforms = leftGlove.handTransforms;
            else if(leftHandController != null)
                leftHandTransforms = leftHandController.handTransforms;
            else
                return;

            HandData leftHD = new HandData {
                wristPosition = leftHandTransforms.wristTransform.position - VRfreeCamera.Instance.transform.position,
                wristRotation = leftHandTransforms.wristTransform.rotation,
                handRotation = leftHandTransforms.handTransform.rotation,
                thumb0Rotation = leftHandTransforms.thumb0Transform.rotation,
                thumb1Rotation = leftHandTransforms.thumb1Transform.rotation,
                thumb2Rotation = leftHandTransforms.thumb2Transform.rotation,
                index0Rotation = leftHandTransforms.index0Transform.rotation,
                index1Rotation = leftHandTransforms.index1Transform.rotation,
                index2Rotation = leftHandTransforms.index2Transform.rotation,
                middle0Rotation = leftHandTransforms.middle0Transform.rotation,
                middle1Rotation = leftHandTransforms.middle1Transform.rotation,
                middle2Rotation = leftHandTransforms.middle2Transform.rotation,
                ring0Rotation = leftHandTransforms.ring0Transform.rotation,
                ring1Rotation = leftHandTransforms.ring1Transform.rotation,
                ring2Rotation = leftHandTransforms.ring2Transform.rotation,
                pinky0Rotation = leftHandTransforms.pinky0Transform.rotation,
                pinky1Rotation = leftHandTransforms.pinky1Transform.rotation,
                pinky2Rotation = leftHandTransforms.pinky2Transform.rotation
            };
            leftHD = Quaternion.Inverse(VRfreeCamera.Instance.transform.rotation) * leftHD;

            //rightCenterToWristOffset.x = (Calibration1HandDataR.wristPosition.x - Calibration1HandDataL.wristPosition.x) / 2;

            if(stage == 1) {
                Calibration1HandDataR = rightHD;
                Calibration1HandDataL = leftHD;
            } else if(stage == 2) {
                Calibration2HandDataR = rightHD;
                Calibration2HandDataL = leftHD;
            } else {
                Calibration3HandDataR = rightHD;
                Calibration3HandDataL = leftHD;
            }
        }

        public void copyRightPoseToLeft() {
            HandTransforms rightHandTransforms;
            if(rightGlove.handTransforms.wristTransform != null)
                rightHandTransforms = rightGlove.handTransforms;
            else if(rightHandController != null)
                rightHandTransforms = rightHandController.handTransforms;
            else
                return;

            HandTransforms leftHandTransforms;
            if(leftGlove.handTransforms.wristTransform != null)
                leftHandTransforms = leftGlove.handTransforms;
            else if(leftHandController != null)
                leftHandTransforms = leftHandController.handTransforms;
            else
                return;

            leftHandTransforms.wristTransform.position    = new Vector3(-rightHandTransforms.wristTransform.position.x, rightHandTransforms.wristTransform.position.y, rightHandTransforms.wristTransform.position.z);
            leftHandTransforms.wristTransform.rotation    = mirrorBoneQuaternion(rightHandTransforms.wristTransform.rotation);
            leftHandTransforms.handTransform.rotation     = mirrorBoneQuaternion(rightHandTransforms.handTransform.rotation);
            leftHandTransforms.thumb0Transform.rotation   = mirrorBoneQuaternion(rightHandTransforms.thumb0Transform.rotation);
            leftHandTransforms.thumb1Transform.rotation   = mirrorBoneQuaternion(rightHandTransforms.thumb1Transform.rotation);
            leftHandTransforms.thumb2Transform.rotation   = mirrorBoneQuaternion(rightHandTransforms.thumb2Transform.rotation);
            leftHandTransforms.index0Transform.rotation   = mirrorBoneQuaternion(rightHandTransforms.index0Transform.rotation);
            leftHandTransforms.index1Transform.rotation   = mirrorBoneQuaternion(rightHandTransforms.index1Transform.rotation);
            leftHandTransforms.index2Transform.rotation   = mirrorBoneQuaternion(rightHandTransforms.index2Transform.rotation);
            leftHandTransforms.middle0Transform.rotation  = mirrorBoneQuaternion(rightHandTransforms.middle0Transform.rotation);
            leftHandTransforms.middle1Transform.rotation  = mirrorBoneQuaternion(rightHandTransforms.middle1Transform.rotation);
            leftHandTransforms.middle2Transform.rotation  = mirrorBoneQuaternion(rightHandTransforms.middle2Transform.rotation);
            leftHandTransforms.ring0Transform.rotation    = mirrorBoneQuaternion(rightHandTransforms.ring0Transform.rotation);
            leftHandTransforms.ring1Transform.rotation    = mirrorBoneQuaternion(rightHandTransforms.ring1Transform.rotation);
            leftHandTransforms.ring2Transform.rotation    = mirrorBoneQuaternion(rightHandTransforms.ring2Transform.rotation);
            leftHandTransforms.pinky0Transform.rotation  = mirrorBoneQuaternion(rightHandTransforms.pinky0Transform.rotation);
            leftHandTransforms.pinky1Transform.rotation  = mirrorBoneQuaternion(rightHandTransforms.pinky1Transform.rotation);
            leftHandTransforms.pinky2Transform.rotation  = mirrorBoneQuaternion(rightHandTransforms.pinky2Transform.rotation);

        }

        public void resetTargetHandData(bool isRightHand) {
            float sign = isRightHand ? 1 : -1;
            Quaternion handRotation = Quaternion.Euler(7.4f, sign * 1.3f, sign * 10.4f);
            Quaternion wristRotation = Quaternion.Euler(0, 0, sign * (-100));
            HandData targetHandData = handRotation * (isRightHand ? HandData.calibrationHandDataR : HandData.calibrationHandDataL);
            targetHandData.wristRotation = Quaternion.Inverse(handRotation) * targetHandData.wristRotation;
            targetHandData = wristRotation * targetHandData;

            if(isRightHand ? rightGlove.convertForSteamVrGlove : leftGlove.convertForSteamVrGlove) {
                targetHandData.convertToOpenVrQuaternions(isRightHand);
            }

            targetHandData.wristPosition = new Vector3(isRightHand ? 0.06f : -0.06f, -0.5f, 0.4f);

            if(isRightHand)
                Calibration1HandDataR = targetHandData;
            else
                Calibration1HandDataL = targetHandData;
        }

        public void showCalibrationPose(VRfreeGlove glove, Quaternion directionRotation, int stage, bool adaptInclination) {
            HandData targetHandData = (stage == 1) ?
                (glove.isRightHand ? Calibration1HandDataR : Calibration1HandDataL) :
                (stage == 2) ?
                    (glove.isRightHand ? Calibration2HandDataR : Calibration2HandDataL) :
                    (glove.isRightHand ? Calibration3HandDataR : Calibration3HandDataL);
            if(adaptInclination)
                targetHandData = Quaternion.AngleAxis(getAverageInclinationAngle(), Vector3.right) * targetHandData;
            targetHandData = directionRotation * targetHandData;
            //targetHandData.wristPosition.x = glove.isRightHand ? rightCenterToWristOffset.x : -rightCenterToWristOffset.x;
            targetHandData.wristPosition = directionRotation * targetHandData.wristPosition + (VRfreeCamera.Instance != null ? VRfreeCamera.Instance.transform.position : Vector3.zero);

            glove.showCalibrationPose(targetHandData);

            progressBar.transform.rotation = Quaternion.LookRotation(Vector3.down, directionRotation*Vector3.forward);
        }

        public void calibrate3() {
            /* Step 1:
             * Correct holding hands in wrong direction/ head module heading mismatch during calibration 1
             * by comparing and aligning inclination of fingers of both hands
             */
            HandData rightHandData = rightGlove.handData;
            HandData leftHandData = leftGlove.handData;
            Vector3 fingerDirectionR = getAverageFingerDirection(rightHandData);
            Vector3 fingerDirectionL = getAverageFingerDirection(leftHandData);
            //Vector3 averageDirection = (fingerDirectionR + fingerDirectionL).normalized;

            Vector3 rotationAxisR = rightHandData.handRotation*Quaternion.Inverse(Calibration1HandDataR.handRotation)*Vector3.up;
            Vector3 rotationAxisL = leftHandData.handRotation*Quaternion.Inverse(Calibration1HandDataL.handRotation)*Vector3.up;

            Vector3 fingerDirectionRProjected = fingerDirectionR - rotationAxisR*Vector3.Dot(fingerDirectionR, rotationAxisR);
            Vector3 fingerDirectionLProjected = fingerDirectionL - rotationAxisL*Vector3.Dot(fingerDirectionL, rotationAxisL);

            float angleR = Vector3.SignedAngle(fingerDirectionRProjected, Vector3.Cross(Vector3.up, rotationAxisR), rotationAxisR);
            float angleL = Vector3.SignedAngle(fingerDirectionLProjected, -Vector3.Cross(Vector3.up, rotationAxisL), rotationAxisL);
            float angle = angleR + angleL;

            Debug.Log($"angleR: {angleR:f1}, angleL: {angleL:f1}, correcting calibration direction by " + angle/2 + " degrees");

            Quaternion corrR = Quaternion.AngleAxis(angle/2, rotationAxisR);
            Quaternion corrL = Quaternion.AngleAxis(angle/2, rotationAxisL);

            HandData rightTargetData = corrR * rightHandData;
            HandData leftTargetData = corrL * leftHandData;

            rightGlove.calibrateTargetHandData(rightTargetData);
            leftGlove.calibrateTargetHandData(leftTargetData);

            // update wristRotation2
            wristRotation2R = wristRotation2R * Quaternion.Inverse(rightHandData.wristRotation) * rightTargetData.wristRotation;
            wristRotation2L = wristRotation2L * Quaternion.Inverse(leftHandData.wristRotation) * leftTargetData.wristRotation;

            // fix wrist offsets to new calibration direction
            calibrationDirection = Quaternion.AngleAxis(angle/2, Vector3.up) * calibrationDirection;
            if(calibrateWristOffset) {
                calibrateWristOffset1();
                if(twoStageWristOffsetCalibration) {
                    calibrateWristOffset2();
                    calibrateWristOffset3(leftTargetData, rightTargetData);
                }
            }

            if(rightGlove.handTransforms.middle0Transform != null) {
                Debug.DrawRay(rightGlove.handTransforms.middle0Transform.position, 0.2f*fingerDirectionR, Color.green, 10);
                Debug.DrawRay(leftGlove.handTransforms.middle0Transform.position, 0.2f*fingerDirectionL, Color.green, 10);

                Debug.DrawRay(rightGlove.handTransforms.middle0Transform.position, 0.2f*rotationAxisR, Color.black, 10);
                Debug.DrawRay(leftGlove.handTransforms.middle0Transform.position, 0.2f*rotationAxisL, Color.gray, 10);

                Debug.DrawRay(rightGlove.handTransforms.middle0Transform.position, 0.2f*fingerDirectionRProjected, Color.blue, 10);
                Debug.DrawRay(leftGlove.handTransforms.middle0Transform.position, 0.2f*fingerDirectionLProjected, Color.blue, 10);
            }
        }

        public void showCalibrationPose() {
            showCalibrationPose(rightGlove, Quaternion.identity, 1, adaptInclination && visualizeInclinationAndHeading);
            showCalibrationPose(leftGlove, Quaternion.identity, 1, adaptInclination && visualizeInclinationAndHeading);
        }


        private void calibrate1() {
            // calibrate quaternions
            rightGlove.calibrateTargetHandData(rightGlove.displayHandData);
            leftGlove.calibrateTargetHandData(leftGlove.displayHandData);

            if(calibrateWristOffset) {
                wristPosition1R = rightGlove.GetWristTrackerPosition();
                wristPosition1L = leftGlove.handData.wristPosition;
                calibrateWristOffset1();
            }
        }

        Vector3 wristPosition1R;
        Vector3 wristPosition1L;
        private void calibrateWristOffset1() {
            showCalibrationPose(rightGlove, calibrationDirection, 1, adaptInclination);
            showCalibrationPose(leftGlove, calibrationDirection, 1, adaptInclination);

            Vector3 center = (wristPosition1R + wristPosition1L) * 0.5f - wristOffsetY*Vector3.up;

            float centerToWristDistance = (Calibration1HandDataR.wristPosition - Calibration1HandDataL.wristPosition).magnitude / 2;

            wristOffsetDirectionsR[0] = Quaternion.Inverse(Calibration1HandDataR.wristRotation) * Vector3.right;
            wristOffsetDirectionsR[1] = Quaternion.Inverse(Calibration1HandDataR.wristRotation) * Vector3.forward;
            wristOffsetDirectionsR[2] = Quaternion.Inverse(Calibration1HandDataR.wristRotation) * Vector3.up;

            wristOffsetDirectionsL[0] = Quaternion.Inverse(Calibration1HandDataL.wristRotation) * Vector3.left;
            wristOffsetDirectionsL[1] = Quaternion.Inverse(Calibration1HandDataL.wristRotation) * Vector3.forward;
            wristOffsetDirectionsL[2] = Quaternion.Inverse(Calibration1HandDataL.wristRotation) * Vector3.up;


            wristOffsetR = Quaternion.Inverse(rightGlove.displayHandData.wristRotation)
                * (center + calibrationDirection * Vector3.right * centerToWristDistance - wristPosition1R);
            wristOffsetL = Quaternion.Inverse(leftGlove.displayHandData.wristRotation)
                * (center + calibrationDirection * (-Vector3.right) * centerToWristDistance - wristPosition1L);
            if(wristOffsetL.magnitude < maxWristOffsetMagnitude && wristOffsetR.magnitude < maxWristOffsetMagnitude) {
                rightGlove.setWristOffset(wristOffsetR);
                leftGlove.setWristOffset(wristOffsetL);
            }
        }

        private void calibrate2() {
            wristPosition2R = rightGlove.GetWristTrackerPosition();
            wristPosition2L = leftGlove.GetWristTrackerPosition();
            wristRotation2R = rightGlove.handData.wristRotation;
            wristRotation2L = leftGlove.handData.wristRotation; ;

            calibrateWristOffset2();
        }

        Vector3 wristPosition2R;
        Vector3 wristPosition2L;
        Quaternion wristRotation2R;
        Quaternion wristRotation2L;

        private void calibrateWristOffset2() {
            showCalibrationPose(rightGlove, calibrationDirection, 2, adaptInclination);
            showCalibrationPose(leftGlove, calibrationDirection, 2, adaptInclination);

            // calibrate wrist offset along the lower arm
            Vector3 wristDirectionR = wristRotation2R * wristOffsetDirectionsR[1];
            Vector3 wristDirectionL = wristRotation2L * wristOffsetDirectionsL[1];

            Vector3 wristPositionR = wristPosition2R + wristRotation2R*wristOffsetR;
            Vector3 wristPositionL = wristPosition2L + wristRotation2L*wristOffsetL;

            Vector3 center = (wristPositionR + wristPositionL) * 0.5f;
            Vector3 correctionDirection = calibrationDirection * Vector3.right;

            // the distance in correction direction that both wrists should be from the center
            float centerToWristDistance = (rightGlove.displayHandData.wristPosition - leftGlove.displayHandData.wristPosition).magnitude / 2;

            // distance by which the wrist positions have to be adjusted to have the correct centerToWristDistance
            float corrValR = Vector3.Dot(center + correctionDirection * centerToWristDistance - wristPositionR, correctionDirection);
            float corrValL = Vector3.Dot(center - correctionDirection * centerToWristDistance - wristPositionL, correctionDirection);

            // we want to change left and right by the same amount
            float avgCorrection = 0.5f * (corrValR / Vector3.Dot(wristDirectionR, correctionDirection) + corrValL / Vector3.Dot(wristDirectionL, correctionDirection)); // we want to change left and right by the same amount
            Vector3 rightCorrectionVector = wristDirectionR * avgCorrection;
            Vector3 leftCorrectionVector = wristDirectionL * avgCorrection;

            wristOffsetR += Quaternion.Inverse(wristRotation2R) * rightCorrectionVector;
            wristOffsetL += Quaternion.Inverse(wristRotation2L) * leftCorrectionVector;

            if(wristOffsetL.magnitude < maxWristOffsetMagnitude && wristOffsetR.magnitude < maxWristOffsetMagnitude) {
                leftGlove.setWristOffset(wristOffsetL);
                rightGlove.setWristOffset(wristOffsetR);
            }
        }

        private void calibrateWristOffset3(HandData leftHandData, HandData rightHandData) {
            rightGlove.showCalibrationPose(rightHandData);
            leftGlove.showCalibrationPose(leftHandData);

            // first make boh hands have same height
            Vector3 dirR = rightHandData.wristRotation*wristOffsetDirectionsR[0];
            Vector3 dirL = leftHandData.wristRotation*wristOffsetDirectionsL[0];

            Vector3 centerR, centerL;
            if(rightHandController != null) {
                centerR = (rightHandController.handMasterTransforms.thumb2Transform.position + rightHandController.handMasterTransforms.index2Transform.position)/2;
                centerL = (leftHandController.handMasterTransforms.thumb2Transform.position + leftHandController.handMasterTransforms.index2Transform.position)/2;
            } else {
                centerR = (rightGlove.handTransforms.thumb2Transform.position + rightGlove.handTransforms.index2Transform.position)/2;
                centerL = (leftGlove.handTransforms.thumb2Transform.position + leftGlove.handTransforms.index2Transform.position)/2;
            }

            float dy = centerR.y - centerL.y;
            float dyProj = (dy/Vector3.Dot(dirR, Vector3.down) + dy/Vector3.Dot(dirL, Vector3.down))/2;

            wristOffsetR += 0.5f*dyProj * wristOffsetDirectionsR[0];
            wristOffsetL -= 0.5f*dyProj * wristOffsetDirectionsL[0];

            centerR += 0.5f*dyProj * dirR;
            centerL -= 0.5f*dyProj * dirL;

            // then make index fingers and thumbs touch
            dirR = rightHandData.wristRotation*wristOffsetDirectionsR[2];
            dirL = leftHandData.wristRotation*wristOffsetDirectionsL[2];

            Vector3 dirCombined = (dirR - dirL).normalized;
            float dist = Vector3.Dot(centerR - centerL, dirCombined) - 0.02f;

            wristOffsetR += 0.5f*dist/Vector3.Dot(dirR, dirCombined)*wristOffsetDirectionsR[2];
            wristOffsetL -= 0.5f*dist/Vector3.Dot(dirL, dirCombined)*wristOffsetDirectionsL[2];

            if(wristOffsetL.magnitude < maxWristOffsetMagnitude && wristOffsetR.magnitude < maxWristOffsetMagnitude) {
                rightGlove.setWristOffset(wristOffsetR);
                leftGlove.setWristOffset(wristOffsetL);
            }
        }

        private void drawDebug() {
            Vector3 trackerPosRTest = rightGlove.handData.wristPosition;
            Vector3 trackerPosLTest = leftGlove.handData.wristPosition;

            Vector3 centerTest = (trackerPosRTest + trackerPosLTest) * 0.5f;

            Debug.DrawRay(trackerPosRTest - 0.01f * Vector3.up, 0.02f * Vector3.up, Color.red);
            Debug.DrawRay(trackerPosRTest - 0.01f * Vector3.right, 0.02f * Vector3.right, Color.red);

            Debug.DrawRay(trackerPosLTest - 0.01f * Vector3.up, 0.02f * Vector3.up, Color.magenta);
            Debug.DrawRay(trackerPosLTest - 0.01f * Vector3.right, 0.02f * Vector3.right, Color.magenta);

            Debug.DrawRay(centerTest - 0.01f * Vector3.up, 0.02f * Vector3.up, Color.yellow);
            Debug.DrawRay(centerTest - 0.01f * Vector3.right, 0.02f * Vector3.right, Color.yellow);

            Quaternion calibrationDirectionTest = Quaternion.LookRotation(new Vector3(VRfreeCamera.Instance.transform.forward.x, 0, VRfreeCamera.Instance.transform.forward.z));

            //Debug.DrawRay(centerTest + calibrationDirectionTest * rightCenterToWristOffset - 0.01f * Vector3.up, 0.02f * Vector3.up, Color.green);
            //Debug.DrawRay(centerTest + calibrationDirectionTest * rightCenterToWristOffset - 0.01f * Vector3.right, 0.02f * Vector3.right, Color.green);
            //
            //Debug.DrawRay(centerTest + calibrationDirectionTest * new Vector3(-rightCenterToWristOffset.x, rightCenterToWristOffset.y, rightCenterToWristOffset.z) - 0.01f * Vector3.up, 0.02f * Vector3.up, Color.cyan);
            //Debug.DrawRay(centerTest + calibrationDirectionTest * new Vector3(-rightCenterToWristOffset.x, rightCenterToWristOffset.y, rightCenterToWristOffset.z) - 0.01f * Vector3.right, 0.02f * Vector3.right, Color.cyan);


            // calibrate wrist offset along the lower arm
            Vector3 wristDirectionR = rightGlove.handData.wristRotation * Quaternion.Inverse(Calibration1HandDataR.wristRotation) * Vector3.forward;
            Vector3 wristDirectionL = leftGlove.handData.wristRotation * Quaternion.Inverse(Calibration1HandDataL.wristRotation) * Vector3.forward;

            Debug.DrawRay(rightGlove.handData.wristPosition, 0.1f * wristDirectionR, Color.blue);
            Debug.DrawRay(leftGlove.handData.wristPosition, 0.1f * wristDirectionL, Color.blue);

            Vector3 center = (rightGlove.handData.wristPosition + leftGlove.handData.wristPosition) * 0.5f;
            Vector3 correctionDirection = calibrationDirection * Vector3.right;
            //float corrValR = Vector3.Dot(center + calibrationDirection * rightCenterToWristOffset - rightGlove.handData.wristPosition, correctionDirection);
            //float corrValL = Vector3.Dot(center + calibrationDirection * new Vector3(-rightCenterToWristOffset.x, rightCenterToWristOffset.y, rightCenterToWristOffset.z) - leftGlove.handData.wristPosition, correctionDirection);
            //
            //Debug.DrawRay(rightGlove.handData.wristPosition, corrValR * correctionDirection, Color.white);
            //Debug.DrawRay(leftGlove.handData.wristPosition, corrValL * correctionDirection, Color.white);
            //
            //Vector3 rightCorrectionVector = wristDirectionR * corrValR / Vector3.Dot(wristDirectionR, correctionDirection);
            //Vector3 leftCorrectionVector = wristDirectionL * corrValL / Vector3.Dot(wristDirectionL, correctionDirection);
            //
            //Debug.DrawRay(rightGlove.handData.wristPosition, rightCorrectionVector, Color.black);
            //Debug.DrawRay(leftGlove.handData.wristPosition, leftCorrectionVector, Color.black);
        }

        public Quaternion mirrorBoneQuaternion(Quaternion inQuaternion) {
            if(rightGlove.convertForSteamVrGlove) {
                Vector3 forward = inQuaternion * Vector3.forward;
                forward.x = -forward.x;
                Vector3 up = inQuaternion * Vector3.up;
                up.x = -up.x;
                return Quaternion.LookRotation(forward, -up);
            } else {
                Vector3 forward = inQuaternion * Vector3.forward;
                forward.x = -forward.x;
                Vector3 up = inQuaternion * Vector3.up;
                up.x = -up.x;
                return Quaternion.LookRotation(forward, up);
            }
        }

        [Serializable]
        private class ArrayWrapper {
            public HandData[] arr;
        }
        public void calibrationPosesToString() {
            ArrayWrapper wrapper = new ArrayWrapper();
            HandData c1r = Calibration1HandDataR;
            HandData c1l = Calibration1HandDataL;
            HandData c2r = Calibration2HandDataR;
            HandData c2l = Calibration2HandDataL;
            HandData c3r = Calibration3HandDataR;
            HandData c3l = Calibration3HandDataL;
            if(rightGlove.convertForSteamVrGlove) {
                c1r.convertFromOpenVrQuaternions(true);
                c1l.convertFromOpenVrQuaternions(false);
                c2r.convertFromOpenVrQuaternions(true);
                c2l.convertFromOpenVrQuaternions(false);
                c3r.convertFromOpenVrQuaternions(true);
                c3l.convertFromOpenVrQuaternions(false);
            }
            wrapper.arr = new HandData[] { c1r, c1l, c2r, c2l, c3r, c3l };
            calibrationPosesString = JsonUtility.ToJson(wrapper);
            //calibrationPosesString = JsonUtility.ToJson(Calibration1HandDataR);
        }

        public void calibrationPosesFromString() {
#if UNITY_EDITOR
            Undo.RecordObject(this, "Calibration Poses From String");
            ArrayWrapper fromString = JsonUtility.FromJson<ArrayWrapper>(calibrationPosesString);
            Calibration1HandDataR = fromString.arr[0];
            Calibration1HandDataL = fromString.arr[1];
            Calibration2HandDataR = fromString.arr[2];
            Calibration2HandDataL = fromString.arr[3];
            Calibration3HandDataR = fromString.arr[4];
            Calibration3HandDataL = fromString.arr[5];
            if(rightGlove.convertForSteamVrGlove) {
                Calibration1HandDataR.convertToOpenVrQuaternions(true);
                Calibration1HandDataL.convertToOpenVrQuaternions(false);
                Calibration2HandDataR.convertToOpenVrQuaternions(true);
                Calibration2HandDataL.convertToOpenVrQuaternions(false);
                Calibration3HandDataR.convertToOpenVrQuaternions(true);
                Calibration3HandDataL.convertToOpenVrQuaternions(false);
            }
#endif
        }
    }
}