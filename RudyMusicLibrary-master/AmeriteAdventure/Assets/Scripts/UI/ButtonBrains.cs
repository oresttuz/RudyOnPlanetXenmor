using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonBrains : MonoBehaviour
{
    public void BeginGame()
    {
        SceneManager.LoadScene("Main");
    }

    public void ExitGame()
    {
        Debug.Log("Exit");
        Application.Quit();
    }
}
