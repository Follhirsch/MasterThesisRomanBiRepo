using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StaticGesture = VRfree.StaticGesture;

namespace VRfreePluginUnity {
    public class Paint3dScript : MonoBehaviour {
        public float[] smoothingKernel = new float[] { 1, 3, 4.5f, 3, 1 };

        // where the 3d ink will come out
        public Transform paintTip;
        public int maxNumPaintSegments = 100;
        public float segmentSize = 0.001f;
        public float maxMovement = 0.1f;
        public float lineWidth = 0.003f;
        public Color lineColor;
        public float emissonStrength = 0.5f;

        public Material sourceMaterial;

        public bool isPainting = false;
        [SerializeField]
        [ReadOnly]
        private bool isPaintingBefore = false;
        [SerializeField]
        [ReadOnly]
        private int currentSegmentIndex = 0;
        private Vector3 lastPosition;

        private List<GameObject> mLineRendererObjecs = new List<GameObject>();
        private LineRenderer currentLineRenderer;

        private StaticGesture point = new StaticGesture("point", new VRfree.HandAngles());

        public void StartPainting() {
            isPainting = true;
        }

        public void StopPainting() {
            isPainting = false;
        }

        // Use this for initialization
        void Start() {
            // normalize smoothing kernel
            float sum = 0;
            foreach (float f in smoothingKernel) {
                sum += f;
            }
            for (int i = 0; i < smoothingKernel.Length; i++) {
                smoothingKernel[i] /= sum;
            }
        }

        private void StartNewLine(bool continuous) {
            GameObject lineRendererObject = new GameObject("Line Renderer Object");
            lineRendererObject.transform.parent = transform;
            currentLineRenderer = lineRendererObject.AddComponent<LineRenderer>();
            currentLineRenderer.positionCount = maxNumPaintSegments;
            // set width curve to lineWidth for first n elements, then to 0
            Keyframe[] widthKeys = new Keyframe[4];
            widthKeys[0] = new Keyframe(0, lineWidth, 0, 0);
            widthKeys[1] = new Keyframe(-1/maxNumPaintSegments, lineWidth, 0, -lineWidth);
            widthKeys[2] = new Keyframe(0, 0, lineWidth, 0);
            widthKeys[3] = new Keyframe(1, 0, 0, 0);
            AnimationCurve widthCurve = new AnimationCurve(widthKeys);
            currentLineRenderer.widthCurve = widthCurve;
            currentLineRenderer.useWorldSpace = true;
            Material mat = new Material(sourceMaterial);
            //mat.color = lineColor;
            mat.SetColor("_BaseColor", lineColor);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", lineColor*emissonStrength);
            currentLineRenderer.material = mat;
            currentLineRenderer.numCapVertices = 3;
            currentLineRenderer.numCornerVertices = 0;
            // default is stretch, but this makes the width hack impossible
            currentLineRenderer.textureMode = LineTextureMode.DistributePerSegment;

            mLineRendererObjecs.Add(lineRendererObject);

            currentSegmentIndex = 0;

            // in case of a continuous line, add the last position as start
            if (continuous) {
                currentLineRenderer.SetPosition(currentSegmentIndex, lastPosition);
                currentSegmentIndex++;
            }
            currentLineRenderer.SetPosition(currentSegmentIndex, paintTip.position);
            SetWidthEnd(currentSegmentIndex);
            currentSegmentIndex++;

            lastPosition = paintTip.position;

            isPainting = true;
        }

        private void EndLine() {
            // set the number of positions to the actual used number and set the weight constant
            currentLineRenderer.positionCount = currentSegmentIndex;
            Keyframe[] widthKeys = new Keyframe[1];
            widthKeys[0] = new Keyframe(0, lineWidth, 0, 0);
            currentLineRenderer.widthCurve = new AnimationCurve(widthKeys);
        }

        public void RemoveAllLines() {
            foreach (GameObject lro in mLineRendererObjecs) {
                UnityEngine.Object.Destroy(lro);
            }
            mLineRendererObjecs.Clear();
        }

        public void RemoveLastLine() {
            if (mLineRendererObjecs.Count - 1 >= 0 && mLineRendererObjecs.Count - 1 < mLineRendererObjecs.Count) {
                GameObject last = mLineRendererObjecs[mLineRendererObjecs.Count - 1];
                UnityEngine.Object.Destroy(last);
                mLineRendererObjecs.Remove(last);
            }
        }

        /* Continuously varying the positionCount of the LineRenderer destroys performance.
         * Unfortunately, it doesn't seem like the array can be preallocated and only the front parts used,
         * UNLESS we just set the width of the front parts to 0;
         */
        private void SetWidthEnd(int index) {
            Keyframe[] keys = currentLineRenderer.widthCurve.keys;
            keys[1].time = ((float)(index - 1)) / ((float)maxNumPaintSegments-1f);
            keys[2].time = ((float)index) / ((float)maxNumPaintSegments-1f);
            currentLineRenderer.widthCurve = new AnimationCurve(keys);
            //Keyframe k1 = new Keyframe(((float)(index - 1)) / ((float)maxNumPaintSegments), -lineWidth, 0, lineWidth);
            //Keyframe k2 = new Keyframe(((float)index) / ((float)maxNumPaintSegments), 0, lineWidth, 0);
        }

        void FixedUpdate() {
            if (Input.GetKeyDown(KeyCode.N)) {
                RemoveAllLines();
            }

            if (isPainting && !isPaintingBefore) {
                StartNewLine(false);
            } else if (!isPainting && isPaintingBefore) {
                EndLine();
            }
            isPaintingBefore = isPainting;

            if (isPainting) {
                Vector3 movementVector = paintTip.position - lastPosition;
                float movementDistance = movementVector.magnitude;
                // check that we moved the minimum distance for a new paint segment
                // check that we didn't move too far (might be tracking glitch)
                if (movementDistance < maxMovement) {
                    if (currentSegmentIndex < currentLineRenderer.positionCount) {
                        currentLineRenderer.SetPosition(currentSegmentIndex, paintTip.position);
                        SetWidthEnd(currentSegmentIndex);
                    }
                    if (movementDistance > segmentSize) {
                        // create a new paint segment
                        if (currentSegmentIndex < currentLineRenderer.positionCount) {
                            currentLineRenderer.SetPosition(currentSegmentIndex, paintTip.position);
                            SetWidthEnd(currentSegmentIndex);
                            currentSegmentIndex++;
                            lastPosition = paintTip.position;
                            if (currentSegmentIndex > smoothingKernel.Length && smoothingKernel.Length > 1) {
                                Vector3 smoothed = Vector3.zero;
                                for (int i = 0; i < smoothingKernel.Length; i++) {
                                    smoothed += smoothingKernel[i]*currentLineRenderer.GetPosition(currentSegmentIndex - 1 - i);
                                }
                                currentLineRenderer.SetPosition(currentSegmentIndex - 1 - smoothingKernel.Length/2, smoothed);
                            }
                        } else {
                            // this line has reached max segments, start a new one
                            EndLine();
                            StartNewLine(true);
                        }
                    }
                } else {
                    // jumped too far
                    isPainting = false;
                }

            }
        }
    }
}