using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteItem : MonoBehaviour
{
   [SerializeField] [TextArea] public string noteString;

   public string getNote()
   {
      return noteString;
   }
   
   

}
