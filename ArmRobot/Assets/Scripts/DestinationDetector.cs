using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationDetector : MonoBehaviour
{
    public string destinationTag;

    private void OnTriggerStay(Collider collider)
    {
        if (collider.transform.gameObject.tag == destinationTag)
        {
            TouchDetector td = collider.gameObject.GetComponent<TouchDetector>();
            Debug.Log("Destiantion Reached!");
            td.hasReachedDest = true;
        }
    }
}
