#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace VRfreePluginUnity {
    [CustomEditor(typeof(HandDimensionScript))]
    public class HandDimensionScriptEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            if(Application.isPlaying) {
                HandDimensionScript myScript = (HandDimensionScript)target;
                if(GUILayout.Button("Calculate Initial Dimensions")) {
                    myScript.calculateInitialDimensions();
                }
                if(GUILayout.Button("Set Initial as Desired Dimensions")) {
                    myScript.setIntialAsDesiredDimensions();
                }
                if(GUILayout.Button("Apply Desired Dimensions")) {
                    myScript.applyDesiredHandDimensions();
                }
                if(GUILayout.Button("Apply Desired Wrist to Middle End Length")) {
                    myScript.applyDesiredWristToMiddleEndLength();
                }

                if(GUILayout.Button("Save Dimensions to File")) {
                    myScript.saveHandDimensions();
                }

                if(GUILayout.Button("Load Dimensions from File")) {
                    myScript.loadHandDimensions();
                }
            }
        }
    }
}
#endif