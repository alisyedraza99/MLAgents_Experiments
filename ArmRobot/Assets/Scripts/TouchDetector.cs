using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchDetector : MonoBehaviour
{
    public string touchTargetTag;
    //public string destinationTag;
    public bool hasTouchedTarget = false;
    public bool hasReachedDest = false;


    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.transform.gameObject.tag == destinationTag)
    //    {
    //        Debug.Log("Destiantion Reached!");
    //        hasReachedDest = true;
    //    }
    //}

    // Moving cube touch logic to every frame check for gripper and push tasks
    private void OnCollisionStay(Collision collision)
    {
        if (collision.transform.gameObject.tag == touchTargetTag)
        {
            //Debug.Log("Touch Detected!");
            hasTouchedTarget = true;
        }
    }

    //// set robot touched cube back to false after frame update. 
    //void LateUpdate()
    //{
    //    hasTouchedTarget = false;
    //}
}
