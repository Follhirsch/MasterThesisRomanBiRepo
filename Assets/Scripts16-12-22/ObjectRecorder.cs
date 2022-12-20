using System.Collections;
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

public class ObjectRecorder : MonoBehaviour
{
    StreamWriter csvWriter;
    public List<Vector3[]> posVectors = new List<Vector3[]>();
    public List<Quaternion[]> oriQuaternion = new List<Quaternion[]>();
    public GameObject scene;
    public List<GameObject> objects = new List<GameObject>();
    public int samplingFrequency = 30;
    public bool recording = false;

    private float timer = 0.0f;
    private float samplingInterval;
    private int samples = 0;
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
        Vector3[] tempArray = new Vector3[scene.transform.childCount];
        Quaternion[] tempOriArray = new Quaternion[scene.transform.childCount];


            for (int i = 0; i < scene.transform.childCount; i++) //get position and orientation of objects
            {
                Vector3 pos = scene.transform.GetChild(i).position;
                Quaternion ori = scene.transform.GetChild(i).rotation;
                string positionString = pos.ToString();
                string modifiedPositionString = positionString.Substring(1, positionString.Length - 2);
                completeLine += modifiedPositionString + ",";
                tempArray[i] = pos;
                tempOriArray[i] = ori;

            }
            posVectors.Add(tempArray);
            oriQuaternion.Add(tempOriArray);

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
        

       for (int i = 0; i < scene.transform.childCount - 1; i++)
       {
            objects.Add(scene.transform.GetChild(i).gameObject);
            string childname = scene.transform.GetChild(i).name;
                headerconstruction += childname + ".x," + childname + ".y," + childname + ".z,";
       }

        return headerconstruction;

    }

}
