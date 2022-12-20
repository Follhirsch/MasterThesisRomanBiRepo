using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VRfreePluginUnity;

public class ArHeadModuleOffsetCalibration : MonoBehaviour
{
    [Header("Button")]
    public bool startOffsetCalibration = false;

    [Header("References")]
    public List<Transform> targetTransforms = new List<Transform>();
    public ProgressBar progressBar;

    [Header("Settings")]
    public float holdStillTime = 1;
    public float maxHoldPositionDeviation = 0.01f;
    [System.Serializable]
    public enum CalibrationSource { RightHand, LeftHand, Tracker0 };
    public CalibrationSource calibrationSource;

    [Header("Events")]
    public UnityEvent onStartCalibration;
    public UnityEvent onFinishCalibration;

    [Header("Debug")]
    public int calibrationStage = -1;
    public float holdStillTimeElapsed = 0;

    public Vector3 avgPosition = Vector3.zero;
    public int avgCounter = 0;

    public Vector3 holdStillPosition;

    [System.Serializable]
    public struct OffsetCalibrationPoint {
        public Vector3 measuredPosition;
        public Vector3 targetPosition;
    }
    public List<OffsetCalibrationPoint> offsetCalibrationPoints = new List<OffsetCalibrationPoint>();

    public Vector3 headModuleOffset;
    public Quaternion headModuleRotationGuess;
    public Vector3 headModuleRotationEuler;
    public Quaternion headModuleRotation;
    public float calibrationError;

    // Start is called before the first frame update
    void Start()
    {
        foreach(Transform target in targetTransforms) {
            target.gameObject.SetActive(false);
        }
        progressBar.transform.parent.gameObject.SetActive(false);

#if UNITY_EDITOR
        //if (offsetCalibrationPoints.Count > 0)
        //    Calibrate();
#endif
    }

    // Update is called once per frame
    void Update()
    {
        if(startOffsetCalibration) {
            startOffsetCalibration = false;
            StartOffsetCalibration();
        }

        if(calibrationStage >= 0) {
            progressBar.transform.parent.position = targetTransforms[calibrationStage].position;

            // record average position
            if(holdStillTimeElapsed/holdStillTime > 0.5f) {
                if (GetTrackingState() == VRfree.TrackingState.Tracking) {
                    avgCounter++;
                    float weight = 1.0f/((float)avgCounter);
                    avgPosition = weight*GetTrackerPositionAbsolute() + (1-weight)*avgPosition;
                }
            } else {
                avgPosition = Vector3.zero;
                avgCounter = 0;
            }

            if (CheckHoldstill()) {
                RecordCalibrationPosition();
                calibrationStage++;
                if(calibrationStage < targetTransforms.Count) {
                    // next stage
                    targetTransforms[calibrationStage].gameObject.SetActive(true);
                } else {
                    // finished
                    Calibrate();
                    calibrationStage = -1;
                    progressBar.transform.parent.gameObject.SetActive(false);
                    onFinishCalibration.Invoke();
                }
            }
        }
    }

    private Vector3 GetTrackerPositionAbsolute() {
        switch (calibrationSource) {
            case CalibrationSource.RightHand:
                return VRfree.VRfreeAPI.GetWristTrackerPositionAbsolute(true).FromVRfree();
            case CalibrationSource.LeftHand:
                return VRfree.VRfreeAPI.GetWristTrackerPositionAbsolute(false).FromVRfree();
            case CalibrationSource.Tracker0:
                VRfree.VRfreeAPI.GetTrackerDataAbsolute(out VRfree.Vector3 trackerPos, out _, out _, out _, 0, false);
                return trackerPos.FromVRfree();
            default:
                return Vector3.zero;
        }
    }

    private VRfree.TrackingState GetTrackingState() {
        switch (calibrationSource) {
            case CalibrationSource.RightHand:
                return VRfree.DebugAPI.GetHandTrackingState(true);
            case CalibrationSource.LeftHand:
                return VRfree.DebugAPI.GetHandTrackingState(false);
            case CalibrationSource.Tracker0:
                return VRfree.DebugAPI.GetTrackerTrackingState(0);
            default:
                return VRfree.TrackingState.NotConnected;
        }
    }

    public void StartOffsetCalibration() {
        holdStillTimeElapsed = 0;
        calibrationStage = 0;
        targetTransforms[0].gameObject.SetActive(true);
        progressBar.transform.parent.gameObject.SetActive(true);
        offsetCalibrationPoints.Clear();
        onStartCalibration.Invoke();
    }

    private void RecordCalibrationPosition() {
        offsetCalibrationPoints.Add(new OffsetCalibrationPoint {
            measuredPosition = avgPosition,
            targetPosition = Quaternion.Inverse(VRfreeCamera.Instance.transform.rotation)
            *(targetTransforms[calibrationStage].position - VRfreeCamera.Instance.transform.position)
        });

        targetTransforms[calibrationStage].gameObject.SetActive(false);
        holdStillTimeElapsed = 0;
        avgPosition = Vector3.zero;
        avgCounter = 0;
    }

    private bool CheckHoldstill() {
        Vector3 relativePosition = Quaternion.Inverse(VRfreeCamera.Instance.transform.rotation)
            *(GetTrackerPositionAbsolute() - VRfreeCamera.Instance.transform.position);

        if ((holdStillPosition-relativePosition).sqrMagnitude < maxHoldPositionDeviation*maxHoldPositionDeviation) {
            if (GetTrackingState() == VRfree.TrackingState.Tracking) 
                holdStillTimeElapsed += Time.deltaTime;
        } else {
            holdStillTimeElapsed = 0;
            holdStillPosition = relativePosition;
        }

        progressBar.progress = holdStillTimeElapsed/holdStillTime;

        return holdStillTimeElapsed > holdStillTime;
    }

    private void Calibrate() {
        // 1st step: approximate rotation from first 2 positions
        Vector3 v1_t = offsetCalibrationPoints[1].targetPosition - offsetCalibrationPoints[0].targetPosition;
        Vector3 v2_t = offsetCalibrationPoints[2].targetPosition - offsetCalibrationPoints[0].targetPosition;
        Vector3 v1_m = offsetCalibrationPoints[1].measuredPosition - offsetCalibrationPoints[0].measuredPosition;
        Vector3 v2_m = offsetCalibrationPoints[2].measuredPosition - offsetCalibrationPoints[0].measuredPosition;

        Quaternion q_t = Quaternion.LookRotation(v1_t, v2_t);
        Quaternion q_m = Quaternion.LookRotation(v1_m, v2_m);


        float offsetStep = 0.001f;
        float eulerStep = 0.1f;
        float delta = 1;
        float ox = 0, oy = 0, oz = 0;
        float ex = 0, ey = 0, ez = 0;
        float preErr = GetError(ox, oy, oz, ex, ey, ez);
        Debug.Log($"pre error 0 {preErr}");

        headModuleRotationGuess = q_t*Quaternion.Inverse(q_m);
        preErr = GetError(ox, oy, oz, ex, ey, ez);
        Debug.Log($"pre error 1 {preErr}");

        // 2nd step: approximate offset from first position
        Vector3 o_init = offsetCalibrationPoints[0].targetPosition - headModuleRotationGuess*offsetCalibrationPoints[0].measuredPosition;
        ox = o_init.x; oy = o_init.y; oz = o_init.z;
        preErr = GetError(ox, oy, oz, ex, ey, ez);
        Debug.Log($"pre error 2 {preErr}");


        // 3rd step: optimization
        float iterErr = preErr;
        int iterations = 0;
        while (delta > 0.000001f) {
            iterations++;
            float err = GetError(ox + offsetStep, oy, oz, ex, ey, ez);
            if (err < preErr) {
                ox = ox + offsetStep;
                preErr = err;
            } else {
                err = GetError(ox - offsetStep, oy, oz, ex, ey, ez);
                if (err < preErr) {
                    ox = ox - offsetStep;
                    preErr = err;
                }
            }

            err = GetError(ox, oy + offsetStep, oz, ex, ey, ez);
            if (err < preErr) {
                oy = oy + offsetStep;
                preErr = err;
            } else {
                err = GetError(ox, oy - offsetStep, oz, ex, ey, ez);
                if (err < preErr) {
                    oy = oy - offsetStep;
                    preErr = err;
                }
            }

            err = GetError(ox, oy, oz + offsetStep, ex, ey, ez);
            if (err < preErr) {
                oz = oz + offsetStep;
                preErr = err;
            } else {
                err = GetError(ox, oy, oz - offsetStep, ex, ey, ez);
                if (err < preErr) {
                    oz = oz - offsetStep;
                    preErr = err;
                }
            }

            err = GetError(ox, oy, oz, ex + eulerStep, ey, ez);
            if (err < preErr) {
                ex = ex + eulerStep;
                preErr = err;
            } else {
                err = GetError(ox, oy, oz, ex - eulerStep, ey, ez);
                if (err < preErr) {
                    ex = ex - eulerStep;
                    preErr = err;
                }
            }

            err = GetError(ox, oy, oz, ex, ey + eulerStep, ez);
            if (err < preErr) {
                ey = ey + eulerStep;
                preErr = err;
            } else {
                err = GetError(ox, oy, oz, ex, ey - eulerStep, ez);
                if (err < preErr) {
                    ey = ey - eulerStep;
                    preErr = err;
                }
            }

            err = GetError(ox, oy, oz, ex, ey, ez + eulerStep);
            if (err < preErr) {
                ez = ez + eulerStep;
                preErr = err;
            } else {
                err = GetError(ox, oy, oz, ex, ey, ez - eulerStep);
                if (err < preErr) {
                    ez = ez - eulerStep;
                    preErr = err;
                }
            }

            delta = iterErr - preErr;
            iterErr = preErr;
        }
        Debug.Log($"post error {preErr}, delta {delta}, iterations {iterations}");

        headModuleOffset = new Vector3(ox, oy, oz);
        headModuleRotationEuler = new Vector3(ex, ey, ez);
        headModuleRotation = headModuleRotationGuess*Quaternion.Euler(headModuleRotationEuler);
        VRfree.CalibrationAPI.SetHeadModuleAndNeckOffset(headModuleOffset.ToVRfree(), headModuleRotation.ToVRfree(), Vector3.zero.ToVRfree());
    }

    private float GetError(float offset_x, float offset_y, float offset_z, float euler_x, float euler_y, float euler_z) {
        Vector3 offset = new Vector3(offset_x, offset_y, offset_z);
        Quaternion rotation = headModuleRotationGuess*Quaternion.Euler(euler_x, euler_y, euler_z);

        float error = 0;
        foreach(OffsetCalibrationPoint p in offsetCalibrationPoints) {
            error += (rotation*p.measuredPosition + offset - p.targetPosition).magnitude;
        }
        error /= offsetCalibrationPoints.Count;
        return error;
    }
}
