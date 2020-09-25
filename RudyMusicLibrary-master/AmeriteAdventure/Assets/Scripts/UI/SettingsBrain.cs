using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsBrain : MonoBehaviour
{
    public GameObject settings;
    public bool settingsOpen;

    private void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            settingsOpen = !settingsOpen;
            settings.SetActive(settingsOpen);
        }
    }
}
