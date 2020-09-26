using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToNextLevel : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);
        if (other.name == "Player(Clone)")
        {
            FindObjectOfType<LevelBrain>().EndLevel();
            if (SceneManager.GetActiveScene().name == "Main") { SceneManager.LoadScene("BossScene"); }
            else if (SceneManager.GetActiveScene().name == "BossScene") { SceneManager.LoadScene("Main"); }
        }
    }
}
