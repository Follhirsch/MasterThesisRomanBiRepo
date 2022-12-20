/*
 * This, together with Editor/ReadOnlyDrawer.cs allows exposig a field to the unity editor in read only mode
 * [ReadOnly] public int test1; //public field visible as read only in editor
 * [ReadOnly][SerializeField] private int test1; //private field visible as read only in editor
 */ 
using UnityEngine;

namespace VRfreePluginUnity {
    public class ReadOnly : PropertyAttribute {

    }
}