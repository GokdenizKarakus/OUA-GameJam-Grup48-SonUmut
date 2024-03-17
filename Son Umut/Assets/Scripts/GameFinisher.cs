using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFinisher : MonoBehaviour
{
    public GameObject triggerer;

    private void OnTriggerEnter(Collider other)
    {
        triggerer.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        triggerer.SetActive(false);
    }
}
