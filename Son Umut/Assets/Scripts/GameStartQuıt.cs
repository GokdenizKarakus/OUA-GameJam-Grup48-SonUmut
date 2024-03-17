using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameStartQuıt : MonoBehaviour
{
    public void startgame()
    {
        SceneManager.LoadScene(1);
    }

    

    public void quıtGame()
    {
        Application.Quit();
    }
}
