using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minerals : MonoBehaviour
{
    [SerializeField] private int Mineral;
    
    public int getItem()
    {
        Destroy(gameObject);
        return Mineral;
    }
}
