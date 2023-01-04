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

    private int framerate = 30;
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
        framerate = recorderSource.GetComponent<ObjectRecorder>().framerate;
        posArray = recorderSource.GetComponent<ObjectRecorder>().posVectors.ToArray();
        oriArray = recorderSource.GetComponent<ObjectRecorder>().oriQuaternion.ToArray();

    }
    void loadFromCSVFile()
    {
        //syntax csv object1.x,object1.y,object1.z,object1.rx,object1.ry,object1.rz...
        string[] dataLines = replayFile.text.Split("\n");
        string[] recorderOtionStrings = dataLines[1].Split(",");
        string[] header = dataLines[1].Split(",");
        int frames = dataLines.Length;
        framerate = int.Parse(recorderOtionStrings[1]); 
        int objects = int.Parse(recorderOtionStrings[3]);;

        //List<Vector3[]> tempPosVectorList = new List<Vector3[]>();
        //List<Quaternion[]> tempOriList = new List<Quaternion[]>();
        Vector3[] tempPosFrame = new Vector3[objects];
        Quaternion[] tempOriFrame = new Quaternion[objects];

        for (int i = 1; i < frames; i++)
        {
            string[] dataValues = dataLines[i].Split(",");
            for (int ii = 0; ii < objects/3; ii= ii + 3)
            {
                Vector3 positionData = new Vector3(float.Parse(dataValues[ii]),float.Parse(dataValues[ii+1]),float.Parse(dataValues[ii+2]));
                tempPosFrame[ii] = positionData;
                //oriArray = 
                posArray[i] = tempPosFrame;
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
            yield return new WaitForSeconds(1 / framerate);
            
        }

    }
}
