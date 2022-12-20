using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManipulator : MonoBehaviour
{
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
            loadFromGame();
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
