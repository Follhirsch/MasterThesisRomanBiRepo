using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
//using RootMotion.Demos;
using Unity.VisualScripting;
using UnityEngine;
using Valve.VR;


public class BodyRecorder : MonoBehaviour
{
    StreamWriter csvWriter;
    public List<Vector3[]> rPosVectors = new List<Vector3[]>();
    public List<Quaternion[]> rOriQuaternion = new List<Quaternion[]>();
    public List<Vector3[]> lPosVectors = new List<Vector3[]>();
    public List<Quaternion[]> lOriQuaternion = new List<Quaternion[]>();
    public GameObject hands;
    public int samplingFrequency = 30;
    public bool recording = false;
    public bool recordHands;
    public bool recordHip;
    public bool recordRightFoot;
    public bool recordLeftFoot;
    public bool recordHead;

    private float timer = 0.0f;
    private float samplingInterval = 1.0f;
    private int samples = 0;

    public GameObject rightHand;
    public GameObject leftHand;
    //hands.transform.GetChild(4).gameObject;

    //GameObject leftHand = hands.transform.GetChild(1).gameObject;

    // Start is called before the first frame update
    void Start()
    {
        samplingInterval = 1 / samplingFrequency;
        locateObjects();


    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("r"))
        {
            toggleRecording();
        }
        if (Input.GetKeyDown("f"))
        {
            locateObjects();
        }

        if (recording)
        {
            timer += Time.deltaTime;
            if (timer > samplingInterval)
            {
                logData();
                timer = timer - samplingInterval;
            }
        }
    }

    void logData()
    {
        string completeLine = "";
        Vector3[] rTempArray = new Vector3[17];
        Vector3[] lTempArray = new Vector3[17];
        Quaternion[] rTempOriArray = new Quaternion[17];
        Quaternion[] lTempOriArray = new Quaternion[17];
        //int li = leftHand.transform.childCount;
        if (recordHands)
        {
            for (int ri = 0; ri < rightHand.transform.childCount; ri++) //get position of right hand
            {
                Vector3 pos = rightHand.transform.GetChild(ri).position;
                Quaternion ori = rightHand.transform.GetChild(ri).rotation;
                string positionString = pos.ToString();
                string modifiedPositionString = positionString.Substring(1, positionString.Length - 2);
                completeLine += modifiedPositionString + ",";
                rTempArray[ri] = pos;
                rTempOriArray[ri] = ori;

            }
            rPosVectors.Add(rTempArray);
            rOriQuaternion.Add(rTempOriArray);


            for (int li = 0; li < leftHand.transform.childCount; li++) //get position of left hand
            {
                Vector3 pos = leftHand.transform.GetChild(li).position;
                Quaternion ori = leftHand.transform.GetChild(li).rotation;
                string positionString = pos.ToString();
                string modifiedPositionString = positionString.Substring(1, positionString.Length - 2);
                completeLine += modifiedPositionString + ",";
                lTempArray[li] = pos;
                lTempOriArray[li] = ori;

            }
            lPosVectors.Add(lTempArray);
            lOriQuaternion.Add(lTempOriArray);


        }



        csvWriter.WriteLine(completeLine);
    }

    void toggleRecording()
    {
        if (recording) //sto recording
        {
            recording = false;
            csvWriter.Close();
            Debug.Log("recording stopped");
        }
        else // start recording
        {
            samples = 0;
            samplingInterval = 1 / samplingFrequency;

            string header = locateObjects();
            csvWriter = new StreamWriter("Assets/Recordings" + "/recoring" + "_" + System.DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv");
            csvWriter.WriteLine(header);

            recording = true;
            Debug.Log("recording started");
        }
    }

    string locateObjects()
    {
        string headerconstruction = "";
        if (recordHands)
        {
            GameObject rightHandObject = hands.transform.GetChild(0).gameObject;
            GameObject leftHandObject = hands.transform.GetChild(1).gameObject;

            rightHand = rightHandObject.transform.GetChild(4).gameObject;
            leftHand = leftHandObject.transform.GetChild(4).gameObject;

            for (int ri = 0; ri < rightHand.transform.childCount - 1; ri++)//right hand header
            {
                string childname = rightHand.transform.GetChild(ri).name;
                headerconstruction += childname + ".x," + childname + ".y," + childname + ".z,";
            }

            for (int li = 0; li < leftHand.transform.childCount - 1; li++)// left hand header
            {
                string childname = leftHand.transform.GetChild(4).name;
                headerconstruction += childname + ".x," + childname + ".y," + childname + ".z,";
            }

        }


        //add head,hip and feet tracking
        return headerconstruction;

    }

}
