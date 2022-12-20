using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandPoseManipulation : MonoBehaviour
{
    public GameObject handSource;

    public GameObject handTarget;

    public int frame;
    private Vector3[][] rPosArray;
    private Quaternion[][] rOriArray;
    private Vector3[][] lPosArray;
    private Quaternion[][] lOriArray;
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
            //loadFromGame();
        }
        if (Input.GetKeyDown("2"))
        {
            //StartCoroutine( replayHands());
        }
    }

    void loadFromGame()
    {
        rPosArray = handSource.GetComponent<BodyRecorder>().rPosVectors.ToArray();
        rOriArray = handSource.GetComponent<BodyRecorder>().rOriQuaternion.ToArray();
        lPosArray = handSource.GetComponent<BodyRecorder>().lPosVectors.ToArray();
        lOriArray = handSource.GetComponent<BodyRecorder>().lOriQuaternion.ToArray();
    }

    IEnumerator replayHands()
    {
        for (int i = 0; i < rPosArray.Length; i++)
        {

            // do right hand pose
            if (rPosArray != null && rOriArray != null)
            {
                GameObject rightHandObject = handTarget.transform.GetChild(0).gameObject;
                GameObject rightHandTarget = rightHandObject.transform.GetChild(4).gameObject;
                
                for (int ri = 0; ri < rightHandTarget.transform.childCount; ri++)
                {
                    rightHandTarget.transform.GetChild(ri).transform.position = rPosArray[i][ri];
                    rightHandTarget.transform.GetChild(ri).transform.rotation = rOriArray[i][ri];
                }
            }
            // do left hand pose
            if (lPosArray != null && lOriArray != null)
            {
                GameObject leftHandObject = handTarget.transform.GetChild(1).gameObject;
                
                GameObject lefthandTarget = leftHandObject.transform.GetChild(4).gameObject;
                for (int li = 0; li < lefthandTarget.transform.childCount; li++)
                {
                    lefthandTarget.transform.GetChild(li).transform.position = lPosArray[i][li];
                    lefthandTarget.transform.GetChild(li).transform.rotation = lOriArray[i][li];
                }
            }
            
            
            else
            {
                Debug.Log("Positions not loaded");
            }
            yield return new WaitForSeconds(1/handSource.GetComponent<BodyRecorder>().samplingFrequency);
        }
        
    }

}
