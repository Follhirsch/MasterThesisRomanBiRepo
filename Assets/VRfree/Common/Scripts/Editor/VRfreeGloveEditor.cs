#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VRfreePluginUnity {
    [CustomEditor(typeof(VRfreeGlove))]
    public class VRfreeGloveEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            VRfreeGlove myScript = (VRfreeGlove)target;
            if(GUILayout.Button("Find Hand Transforms in Children")) {
                Undo.RecordObject(myScript, "Find Hand Transforms in Children");
                myScript.findHandTransformsInChildren();
            }
            if(GUILayout.Button("Clear Hand Transforms")) {
                Undo.RecordObject(myScript, "Clear Hand Transforms");
                myScript.clearHandTransforms();
            }
        }
    }
}
#endif
