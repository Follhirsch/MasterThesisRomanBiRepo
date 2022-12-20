using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRfreePluginUnity {
    //[ExecuteInEditMode]
    public class RadialProgressBar : ProgressBar {
        Material mMaterial;

        void Start() {
            mMaterial = GetComponent<Renderer>().material;
        }

        void LateUpdate() {
            mMaterial.SetFloat("_Cutoff", 1.0001f - Mathf.Min(progress, 1));
        }
    }
}