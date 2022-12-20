using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.IO;
using System;
using VRfreeAPI = VRfree.VRfreeAPI;
using GestureAPI = VRfree.GestureAPI;
using StaticGesture = VRfree.StaticGesture;
using MultiGesture = VRfree.MultiGesture;

namespace VRfreePluginUnity {
    public class GestureDetection : MonoBehaviour {
        public VRfreeGlove glove;

        public string subfolder;

        public string defineNewStaticGestureKey = "g";
        public List<bool> staticGesturesDetected = new List<bool>();
        public List<StaticGesture> staticGestures = new List<StaticGesture>();

        public string defineNewMultiGestureKey = "m";
        public List<bool> multiGesturesDetected = new List<bool>();
        public List<MultiGesture> multiGestures = new List<MultiGesture>();
        public float defineMultiGestureMaximumTimeInterval = 2;
        private float defineMultiGestureTimeElapsed = 100;

        [System.Serializable]
        public class UnityEventString : UnityEvent<string> { }
        public UnityEventString onStaticGestureDetected;
        public UnityEventString onMultiGestureDetected;

        public List<string> dllStaticGestures = new List<string>();
        public List<bool> dllStaticGesturesDetected = new List<bool>();

        public List<string> dllMultiGestures = new List<string>();
        public List<bool> dllMultiGesturesDetected = new List<bool>();

        public int removeIndexStatic = 0;
        public int removeIndexMulti = 0;

        // Use this for initialization
        void Start() {
            updateDllGestureNames();
        }

        // Update is called once per frame
        void Update() {
            if(Input.GetKeyDown(defineNewStaticGestureKey)) {
                staticGestures.Add(new StaticGesture("static gesture " + staticGestures.Count, glove.handAngles));
                staticGesturesDetected.Add(false);
            }

            if(Input.GetKeyDown(defineNewMultiGestureKey)) {
                if(defineMultiGestureTimeElapsed > defineMultiGestureMaximumTimeInterval) {
                    // create new MultiGesture
                    multiGestures.Add(new MultiGesture("multi gesture " + multiGestures.Count, glove.handAngles));
                    multiGesturesDetected.Add(false);
                    defineMultiGestureTimeElapsed = 0;
                } else {
                    // add additional gesture to multigesture
                    multiGestures[multiGestures.Count - 1].addPose(glove.handAngles, defineMultiGestureTimeElapsed);
                    defineMultiGestureTimeElapsed = 0;
                }
            }
            defineMultiGestureTimeElapsed += Time.deltaTime;

        }

        void FixedUpdate() {
            for(int i = 0; i < staticGestures.Count; i++) {
                bool detected = staticGestures[i].poseSatisfiesGesture(glove.handAngles, glove.isRightHand);
                if(detected && !staticGesturesDetected[i]) {
                    onStaticGestureDetected.Invoke(staticGestures[i].name);
                }
                staticGesturesDetected[i] = detected;
            }

            for(int i = 0; i < multiGestures.Count; i++) {
                if(multiGestures[i].poseSatisfiesGesture(glove.handAngles, Time.fixedDeltaTime, glove.isRightHand)) {
                    multiGesturesDetected[i] = true;
                    onMultiGestureDetected.Invoke(multiGestures[i].name);
                    StartCoroutine(setMultiGestureNotDetected(i));
                }
            }
            updateDllGesturesDetected();
        }

        public void updateDllGestureNames() {
            GestureAPI.LoadGestures(subfolder);
            int numStaticGestures = GestureAPI.GetNumStaticGestures();
            dllStaticGestures.Clear();
            dllStaticGesturesDetected.Clear();
            for(int i = 0; i < numStaticGestures; i++) {
                dllStaticGestures.Add(GestureAPI.GetStaticGestureName(i));
                dllStaticGesturesDetected.Add(false);
            }

            int numMultiGestures = GestureAPI.GetNumMultiGestures();
            dllMultiGestures.Clear();
            dllMultiGesturesDetected.Clear();
            for(int i = 0; i < numMultiGestures; i++) {
                dllMultiGestures.Add(GestureAPI.GetMultiGestureName(i));
                dllMultiGesturesDetected.Add(false);
            }
        }

        public void updateDllGesturesDetected() {
            int detectedStatic = glove.isRightHand ? GestureAPI.GetStaticGesturesDetectedRight() : GestureAPI.GetStaticGesturesDetectedLeft();
            for(int i = 0; i < dllStaticGesturesDetected.Count; i++) {
                dllStaticGesturesDetected[i] = (detectedStatic & (1 << i)) != 0;
            }

            int detectedMulti = glove.isRightHand ? GestureAPI.GetMultiGesturesDetectedRight() : GestureAPI.GetMultiGesturesDetectedLeft();
            for(int i = 0; i < dllMultiGesturesDetected.Count; i++) {
                dllMultiGesturesDetected[i] = (detectedMulti & (1 << i)) != 0;
            }
        }

        public void printDebug(string str) {
            Debug.Log(str);
        }

        IEnumerator setMultiGestureNotDetected(int index) {
            yield return new WaitForSeconds(0.5f);
            multiGesturesDetected[index] = false;
        }

        public void addStaticGestures() {
            foreach(StaticGesture gesture in staticGestures) {
                GestureAPI.AddStaticGesture(subfolder, gesture);
            }
            updateDllGestureNames();
        }

        public void removeStaticGesture() {
            if(removeIndexStatic >= 0 && removeIndexStatic < GestureAPI.GetNumStaticGestures())
                GestureAPI.RemoveStaticGesture(subfolder, removeIndexStatic);
            updateDllGestureNames();
        }

        public void addMultiGestures() {
            foreach(MultiGesture gesture in multiGestures) {
                GestureAPI.AddMultiGesture(subfolder, gesture);
            }
            updateDllGestureNames();
        }

        public void removeMultiGesture() {
            if(removeIndexMulti >= 0 && removeIndexMulti < GestureAPI.GetNumMultiGestures())
                GestureAPI.RemoveMultiGesture(subfolder, removeIndexMulti);
            updateDllGestureNames();
        }

    }
}