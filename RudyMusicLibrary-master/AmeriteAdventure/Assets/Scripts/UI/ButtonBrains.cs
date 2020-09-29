using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonBrains : MonoBehaviour
{
    public GameObject ResetObject, AudioManager;

    public void BeginGame()
    {
        SceneManager.LoadScene("Main");
        DontDestroyOnLoad(ResetObject);
        DontDestroyOnLoad(AudioManager);
    }

    public void ExitGame()
    {
        Debug.Log("Exit");
        Application.Quit();
    }
}
