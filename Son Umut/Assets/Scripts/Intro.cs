using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Intro : MonoBehaviour
{
    public float waitime = -5f;
    void Start()
    {
        StartCoroutine(waitForİntro());
    }

    IEnumerator waitForİntro()
    {
        yield return new WaitForSeconds(waitime);

        SceneManager.LoadScene(2);
    }
}
