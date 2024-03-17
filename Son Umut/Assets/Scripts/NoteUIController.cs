using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;

public class NoteUIController : MonoBehaviour
{
    [SerializeField] private GameObject paperObject;
    [SerializeField] private TMP_Text noteText;

    public static NoteUIController Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void setText(string noteStr)
    {
        noteText.text = noteStr;
        paperObject.SetActive(true);
    }

    public void closeNote()
    {
        paperObject.SetActive(false);
    }
}
