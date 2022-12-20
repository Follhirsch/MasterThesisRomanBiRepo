#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VRfreePluginUnity {
    [CustomEditor(typeof(HandController))]
    public class HandControllerEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            HandController myScript = (HandController)target;
            if(GUILayout.Button("Find Hand Transforms in Children")) {
                Undo.RecordObject(myScript, "Find Hand Transforms in Children");
                myScript.findHandTransformsInChildren();
            }
            if(GUILayout.Button("Clear Hand Transforms")) {
                Undo.RecordObject(myScript, "Clear Hand Transforms");
                myScript.clearHandTransforms();
            }
            if(GUILayout.Button("Add Hand Colliders")) {
                myScript.addHandColliders();
            }
            if(GUILayout.Button("Copy Colliders From Other Hand")) {
                myScript.copyCollidersFromOtherHand();
            }
        }
    }
}
#endif
