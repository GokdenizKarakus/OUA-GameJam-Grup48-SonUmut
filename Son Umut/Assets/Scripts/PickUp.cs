using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    public GameObject trigger;
    


    public void OnTriggerEnter(Collider other)
    {
        trigger.SetActive(true);
    }

    public void OnTriggerExit(Collider other)
    {
        trigger.SetActive(false);
    }


}
