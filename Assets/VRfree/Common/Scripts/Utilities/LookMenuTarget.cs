using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VRfreePluginUnity {
    public class LookMenuTarget : MonoBehaviour {
        public enum Type {
            ShowPointer,
            Button
        };
        public Type type;
        public UnityEvent onSelected;
    }
}