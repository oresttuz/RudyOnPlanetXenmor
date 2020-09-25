using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetGame : MonoBehaviour
{
    public bool gameWasReset;

    // Start is called before the first frame update
    void Start()
    {
        if (gameWasReset)
        {
            Debug.Log("Game was reset");
        }
    }
}
