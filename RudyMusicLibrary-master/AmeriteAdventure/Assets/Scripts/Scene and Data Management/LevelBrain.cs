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
    private bool ZoomedIn, ZoomingIn, ZoomingOut, finishedInit;

    private void Awake()
    {
        levelData = Instantiate(pfSaveSceneData, this.transform).GetComponent<SaveSceneData>();
    }

    private void Update()
    {
        if (!finishedInit) { return; }
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
        if (!finishedInit) { return; }
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
        if (levelData.GameWasReset)
        {
            levelCanvas = Instantiate(pfCanvas, this.transform);
            levelPlayer = Instantiate(pfPlayer, this.transform);
            levelPlayer.GetComponent<PlayerMovement>().UpdatePlayerOnSceneLoad(levelData.currHp);
        }
        else
        {
            levelCanvas = FindObjectOfType<Canvas>().gameObject;
            levelCanvas.transform.SetParent(transform);
            levelPlayer = FindObjectOfType<PlayerMovement>().gameObject;
            levelPlayer.transform.SetParent(transform);
        }
        
        levelCanvas.GetComponent<Canvas>().worldCamera = RefToCineCam.GetComponentInParent<Camera>();
        RefToRoomManager.GetComponent<RoomManager>().Player = levelPlayer;
        RefToCineCam.m_Follow = levelPlayer.transform;
        RefToCineCam.m_LookAt = levelPlayer.transform;
        refToPlayerMovement = levelPlayer.GetComponent<PlayerMovement>();
        ZoomedIn = false;

        int firstIndex = -1, secondIndex = -1;
        while (firstIndex == secondIndex)
        {
            firstIndex = Mathf.FloorToInt(Random.Range(0f, 3.99f));
            secondIndex = Mathf.FloorToInt(Random.Range(0f, 3.99f));
        }
        RefToRoomManager.GetComponent<RoomManager>().songTitles.Add(levelData.elementTypes[firstIndex]);
        RefToRoomManager.GetComponent<RoomManager>().songTitles.Add(levelData.elementTypes[secondIndex]);
        finishedInit = true;
    }

    public void EndLevel()
    {
        Debug.Log("Ending Level");
        levelData.currHp = levelPlayer.GetComponent<PlayerMovement>().hb_instance.currHealth;
        ++levelData.currSceneNum;
        levelPlayer.transform.GetChild(1).gameObject.SetActive(false);
        levelCanvas.transform.SetParent(null);
        levelPlayer.transform.SetParent(null);
        DontDestroyOnLoad(levelCanvas);
        DontDestroyOnLoad(levelPlayer);
        levelData.EndScene();
    }
}
