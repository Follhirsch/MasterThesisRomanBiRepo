/*
 * This allows exposig a field to the unity editor in read only mode
 * [ReadOnly] public int test1; //public field visible as read only in editor
 * [ReadOnly][SerializeField] private int test1; //private field visible as read only in editor
 */

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace VRfreePluginUnity {
    [CustomPropertyDrawer(typeof(ReadOnly))]
    public class ReadOnlyDrawer : PropertyDrawer {
        public override void OnGUI(Rect position,
            SerializedProperty property,
            GUIContent label) {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
}
#endif
