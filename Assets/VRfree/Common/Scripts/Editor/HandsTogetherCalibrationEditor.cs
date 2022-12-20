#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VRfreePluginUnity {
    [CustomEditor(typeof(HandsTogetherCalibrationLegacy))]
    public class HandsTogetherCalibrationEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            HandsTogetherCalibrationLegacy myScript = (HandsTogetherCalibrationLegacy)target;
            if(GUILayout.Button("Show Calibration Pose 1")) {
                myScript.showCalibrationPose(myScript.rightGlove, Quaternion.identity, 1, false);
                myScript.showCalibrationPose(myScript.leftGlove, Quaternion.identity, 1, false);
            }
            if(GUILayout.Button("Show Calibration Pose 2")) {
                myScript.showCalibrationPose(myScript.rightGlove, Quaternion.identity, 2, false);
                myScript.showCalibrationPose(myScript.leftGlove, Quaternion.identity, 2, false);
            }
            if(GUILayout.Button("Show Calibration Pose 3")) {
                myScript.showCalibrationPose(myScript.rightGlove, Quaternion.identity, 3, false);
                myScript.showCalibrationPose(myScript.leftGlove, Quaternion.identity, 3, false);
            }
            if(GUILayout.Button("Overwrite Calibration Pose 1")) {
                Undo.RecordObject(myScript, "Overwrite Calibration Pose 1");
                myScript.setCurrentPoseCalibrationPose(1);
            }
            if(GUILayout.Button("Overwrite Calibration Pose 2")) {
                Undo.RecordObject(myScript, "Overwrite Calibration Pose 2");
                myScript.setCurrentPoseCalibrationPose(2);
            }
            if(GUILayout.Button("Overwrite Calibration Pose 3")) {
                Undo.RecordObject(myScript, "Overwrite Calibration Pose 3");
                myScript.setCurrentPoseCalibrationPose(3);
            }
            if(GUILayout.Button("Copy Right Pose to Left")) {
                myScript.copyRightPoseToLeft();
            }
            if(GUILayout.Button("Reset Calibration Pose")) {
                Undo.RecordObject(myScript, "Reset Calibration Pose Right");
                myScript.resetTargetHandData(true);
                Undo.RecordObject(myScript, "Reset Calibration Pose Left");
                myScript.resetTargetHandData(false);
            }
            if(GUILayout.Button("Calibration Poses to String")) {
                myScript.calibrationPosesToString();
            }
            if(GUILayout.Button("Calibration Poses from String")) {
                myScript.calibrationPosesFromString();
            }
        }
    }
}
#endif
