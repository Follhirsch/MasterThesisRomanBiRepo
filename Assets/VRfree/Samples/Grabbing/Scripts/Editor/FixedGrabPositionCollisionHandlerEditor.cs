#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VRfreePluginUnity {
    [CustomEditor(typeof(FixedGrabPositionCollisionHandler))]
    public class FixedGrabPositionCollisionHandlerEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            FixedGrabPositionCollisionHandler myScript = (FixedGrabPositionCollisionHandler)target;
            if(GUILayout.Button("Set Grab Position")) {
                Undo.RecordObject(myScript, "Set Grab Position");
                myScript.setGrabPosition();
            }
            if(GUILayout.Button("Move To Grab Position")) {
                Undo.RecordObject(myScript.transform, "Move To Grab Position");
                myScript.moveToGrabPosition();
            }
        }
    }
}
#endif