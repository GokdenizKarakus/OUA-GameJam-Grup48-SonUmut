using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFinisherController : MonoBehaviour
{
    private int kontrol;
    public GameObject Atenion;

    private void Update()
    {
        kontrol = PlayerItemControl.mineralCount;
        
    }


    private void OnTriggerEnter(Collider other)
    {
        if (kontrol >= 8)
        {
            SceneManager.LoadScene(3);
        }
        else
        {
            Atenion.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (Atenion == true)
        {
            Atenion.SetActive(false);
        }
    }

   
}
