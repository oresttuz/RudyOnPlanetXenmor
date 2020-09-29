using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class LevelBrain : MonoBehaviour
{
    public GameObject pfSaveSceneData, pfPlayer, pfCanvas;

    [HideInInspector]
    public GameObject RefToRoomManager, levelPlayer, levelCanvas;
    
    [HideInInspector]
    public CinemachineVirtualCamera RefToCineCam;

    private SaveSceneData levelData;
    private PlayerMovement refToPlayerMovement;
    private bool ZoomedIn, ZoomingIn, ZoomingOut;

    private void Awake()
    {
        levelCanvas = Instantiate(pfCanvas, this.transform);
        levelPlayer = Instantiate(pfPlayer, this.transform);
        levelData = Instantiate(pfSaveSceneData, this.transform).GetComponent<SaveSceneData>();
        levelCanvas.GetComponent<Canvas>().worldCamera = RefToCineCam.GetComponentInParent<Camera>();
        RefToRoomManager.GetComponent<RoomManager>().Player = levelPlayer;
        RefToCineCam.m_Follow = levelPlayer.transform;
        RefToCineCam.m_LookAt = levelPlayer.transform;
        refToPlayerMovement = levelPlayer.GetComponent<PlayerMovement>();
        ZoomedIn = false;
    }

    private void Update()
    {
        if (ZoomingIn)
        {
            RefToCineCam.m_Lens.OrthographicSize -= 0.2f;
            if (RefToCineCam.m_Lens.OrthographicSize <= 5f) { ZoomingIn = false; }
        }
        else if (ZoomingOut)
        {
            RefToCineCam.m_Lens.OrthographicSize += 0.2f;
            if (RefToCineCam.m_Lens.OrthographicSize >= 10f) { ZoomingOut = false; }
        }
    }

    private void FixedUpdate()
    {
        if (refToPlayerMovement.AreThereEnemiesAroundMe())
        {
            if (!ZoomedIn)
            {
                ZoomedIn = true;
                ZoomingIn = true;
            }
        }
        else
        {
            if (ZoomedIn)
            {
                ZoomedIn = false;
                ZoomingOut = true;
            }
        }
    }

    public void GiveSceneGameData()
    {
        levelPlayer.GetComponent<PlayerMovement>().UpdatePlayerOnSceneLoad(levelData.currHp);
        int firstIndex = -1, secondIndex = -1;
        while (firstIndex == secondIndex)
        {
            firstIndex = Mathf.FloorToInt(Random.Range(0f, 3.99f));
            secondIndex = Mathf.FloorToInt(Random.Range(0f, 3.99f));
        }
        RefToRoomManager.GetComponent<RoomManager>().songTitles.Add(levelData.elementTypes[firstIndex]);
        RefToRoomManager.GetComponent<RoomManager>().songTitles.Add(levelData.elementTypes[secondIndex]);
    }

    public void EndLevel()
    {
        levelData.currHp = levelPlayer.GetComponent<PlayerMovement>().hb_instance.currHealth;
        ++levelData.currSceneNum;
        levelData.EndScene();
    }
}
