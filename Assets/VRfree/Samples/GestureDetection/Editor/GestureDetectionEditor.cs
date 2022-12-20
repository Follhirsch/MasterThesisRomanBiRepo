#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VRfreePluginUnity {
    [CustomEditor(typeof(GestureDetection))]
    public class GestureDetectionEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            GestureDetection myScript = (GestureDetection)target;
            if(GUILayout.Button("Add Static Gestures")) {
                myScript.addStaticGestures();
            }
            if(GUILayout.Button("Remove Static Gesture at Index")) {
                myScript.removeStaticGesture();
            }
            if(GUILayout.Button("Add Multi Gestures")) {
                myScript.addMultiGestures();
            }
            if(GUILayout.Button("Remove Multi Gesture at Index")) {
                myScript.removeMultiGesture();
            }
        }
    }
}
#endif

