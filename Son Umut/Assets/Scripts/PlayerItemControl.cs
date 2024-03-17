using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PlayerItemControl : MonoBehaviour
{
    [SerializeField] private int playerMineral;
    public float mesafe;
    public float mineral;
    private RaycastHit hit;
    public Text canvatext;
    private int mineralsayisi = 0;
    public static int mineralCount = 0;
    public static bool hooker = false;
    
    private bool isNoteOpen = false;

   

    
    private void Update()
    {
        Debug.DrawLine(transform.position, hit.point, Color.red);
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.TryGetComponent(out Minerals minerals))
                {
                    hooker = true;
                    playerMineral += minerals.getItem();
                    mineralsayisi = playerMineral;
                    mineralCount = mineralsayisi;
                    canvatext.text = mineralsayisi.ToString();
                    
                }
                else
                {
                    hooker = false;
                }
            }
            
        }

        
    }
}

