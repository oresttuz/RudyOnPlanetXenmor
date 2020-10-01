using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveSceneData : MonoBehaviour
{
    private LevelBrain RefToLevelBrain;

    public float currHp;
    public int currSceneNum;
    public bool GameWasReset;

    public string[] elementTypes;

    private void Start()
    {
        RefToLevelBrain = GetComponentInParent<LevelBrain>();
        if (RefToLevelBrain.Equals(null))
        {
            Debug.LogError("No LevelBrain was given to SaveSceneData");
            return;
        }
        ResetGame tempResetCheck = FindObjectOfType<ResetGame>();
        if (tempResetCheck != null)
        {
            if (tempResetCheck.gameWasReset)
            {
                tempResetCheck.gameObject.transform.SetParent(this.transform);
                SaveFile(new SceneData(4f, 1));
                GameWasReset = true;
            }
        }
        if (!LoadFile()) //no file was loaded
        {
            SaveFile();
            LoadFile();
        }
        if (currHp == 0f && currSceneNum == 1)
        {
            currHp = 4f;
        }
        GiveGameData();
    }

    public void EndScene()
    {
        SaveFile(new SceneData(currHp, currSceneNum));
    }

    public void GiveGameData()
    {
        RefToLevelBrain.GiveSceneGameData();
    }

    public void SaveFile()
    {
        string destination = Application.persistentDataPath + "/sceneData";
        FileStream file;

        if (File.Exists(destination)) { file = File.OpenWrite(destination); }
        else { file = File.Create(destination); }

        SceneData data = new SceneData(4f);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, data);
        file.Close();
    }

    public void SaveFile(SceneData sd)
    {
        string destination = Application.persistentDataPath + "/sceneData";
        FileStream file;

        if (File.Exists(destination)) { file = File.OpenWrite(destination); }
        else { file = File.Create(destination); }

        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, sd);
        file.Close();
    }

    public bool LoadFile()
    {
        string destination = Application.persistentDataPath + "/sceneData";
        FileStream file;

        if (File.Exists(destination)) { file = File.OpenRead(destination); }
        else
        {
            Debug.Log("File not found");
            return false;
        }

        BinaryFormatter bf = new BinaryFormatter();
        SceneData data = (SceneData)bf.Deserialize(file);
        file.Close();

        Debug.Log("CurrSceneNum: " + data.SceneNum);
        currHp = data.hp;
        currSceneNum = data.SceneNum;
        elementTypes = data.elemTypes;
        return true;
    }
}
