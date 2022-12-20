using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRfree;
using VRfreePluginUnity;

[RequireComponent(typeof(Paint3dScript))]
public class FingerPainting : MonoBehaviour
{
    // assign the hand controller in the editor, to read the hand movements
    public HandController handController;
    public bool enablePainting = true;
    private Paint3dScript paint3DScript;

    private StaticGesture point = new StaticGesture("point", new VRfree.HandAngles());

    // Start is called before the first frame update
    void Start() {
        paint3DScript = GetComponent<Paint3dScript>();

        for (int i = 0; i < 5; i++)
        {
            point.centerPose.fingerAngles0close[i] = (i == 1) ? 5 : -75;
            point.centerPose.fingerAngles1close[i] = (i == 1) ? 5 : -75;
            point.centerPose.fingerAngles2close[i] = (i == 1) ? 5 : -55;
        }

        point.ignoreFinger[0] = true;
        point.maxCloseAngleDeviation = 40;
        point.maxSideAngleDeviation = 40;
        point.useWristHandAngles = false;
    }

    // Update is called once per frame
    void FixedUpdate() {
        paint3DScript.isPainting = point.poseSatisfiesGesture(handController.handPose.RawHandAngles, handController.glove.isRightHand);
    }
}
