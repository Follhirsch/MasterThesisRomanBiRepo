using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectManipulator : MonoBehaviour
{
    public bool loadFromFile = false;
    public TextAsset replayFile;
    public GameObject recorderSource;

    public GameObject sceneTarget;

    public int frame;
    private Vector3[][] posArray;
    private Quaternion[][] oriArray;
    // Start is called before the first frame update
    void Start()
    {
        frame = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("1"))
        {
            //get hand posees
            if(loadFromFile)
            {
                loadFromCSVFile();
            }
            else
            {
                loadFromGame();
            }

        }
        if (Input.GetKeyDown("2"))
        {
            StartCoroutine(replayObjects());
        }
        if (Input.GetKeyDown("3"))
        {
           playFrame();
        }
    }

    void loadFromGame()
    {
        posArray = recorderSource.GetComponent<ObjectRecorder>().posVectors.ToArray();
        oriArray = recorderSource.GetComponent<ObjectRecorder>().oriQuaternion.ToArray();

    }
    void loadFromCSVFile()
    {
        string[] dataLines = replayFile.text.Split("\n");
        string[] header = dataLines[0].Split(",");
        int frames = dataLines.Length;
        int objects = header.Length/3;
        
        posArray = new Vector3[frames][objects];
     
        for (int i = 1; i < dataLines.Length/3; i++)
        {
            string[] dataValues = dataLines[i].Split(",");
            for (int ii = 0; ii < dataValues.Length; ii= ii + 3)
            {
                Vector3 positionData = new Vector3(float.Parse(dataValues[ii]),float.Parse(dataValues[ii+1]),float.Parse(dataValues[ii+2]));
                posArray[i][ii] = positionData;
                //oriArray = 
            }


        }

    }


    void playFrame()
    {
        if (posArray != null && oriArray != null)
        {
            for (int ii = 0; ii < sceneTarget.transform.childCount; ii++)
            {
                sceneTarget.transform.GetChild(ii).transform.position = posArray[frame][ii];
                sceneTarget.transform.GetChild(ii).transform.rotation = oriArray[frame][ii];
            }
        }
        else
        {
            Debug.Log("Positions not loaded");
        }
    }

    IEnumerator replayObjects()
    {
        for (int i = 0; i < posArray.Length; i++)
        {

            // do right hand pose
            if (posArray != null && oriArray != null)
            {
                Debug.Log("Playframe"+ frame.ToString());
                for (int ii = 0; ii < sceneTarget.transform.childCount; ii++)
                {
                    sceneTarget.transform.GetChild(ii).transform.position = posArray[i][ii];
                    sceneTarget.transform.GetChild(ii).transform.rotation = oriArray[i][ii];
                }
            }
            else
            {
                Debug.Log("Positions not loaded");
            }
            yield return new WaitForSeconds(1 / recorderSource.GetComponent<ObjectRecorder>().samplingFrequency);
            
        }

    }
}
