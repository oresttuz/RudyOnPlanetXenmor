using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class LevelBrain : MonoBehaviour
{
    public GameObject pfSaveSceneData, pfPlayer, pfCanvas,
               RefToRoomManager,
               levelPlayer, levelCanvas;
    public CinemachineVirtualCamera RefToCineCam;
    private SaveSceneData levelData;

    private void Awake()
    {
        levelCanvas = Instantiate(pfCanvas, this.transform);
        levelPlayer = Instantiate(pfPlayer, this.transform);
        levelData = Instantiate(pfSaveSceneData, this.transform).GetComponent<SaveSceneData>();
        levelCanvas.GetComponent<Canvas>().worldCamera = RefToCineCam.GetComponentInParent<Camera>();
        RefToRoomManager.GetComponent<RoomManager>().Player = levelPlayer;
        RefToCineCam.m_Follow = levelPlayer.transform;
        RefToCineCam.m_LookAt = levelPlayer.transform;
    }

    public void GiveSceneGameData()
    {
        levelPlayer.GetComponent<PlayerMovement>().UpdatePlayerOnSceneLoad(levelData.currHp);
    }

    public void EndLevel()
    {
        levelData.currHp = levelPlayer.GetComponent<PlayerMovement>().hb_instance.currHealth;
        ++levelData.currSceneNum;
        levelData.EndScene();
    }
}
