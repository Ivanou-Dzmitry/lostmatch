
using UnityEngine;
using System;
using System.IO;

using TMPro;
using UnityEngine.SceneManagement;


[Serializable]
public class SaveData
{
    public bool[] isActive;
    public int[] highScore;
    public int[] stars;
    public bool soundToggle;
    public bool musicToggle;
    public float soundVolume;
    public float musicVolume;
}

public class game_data : MonoBehaviour
{
    public static game_data gameData;
    public SaveData saveData;

    //public SaveData saveData = new SaveData();

    //path \AppData\LocalLow\frosofco\LostMatch
    public string fileName = "player_saves.json";    
    public TMP_Text debugText;

    private string myTxt;


    // Start is called before the first frame update
    void Awake()
    {
        if(gameData == null)
        {            
            DontDestroyOnLoad(this.gameObject);
            gameData = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        Load();
    }


    public void SaveToFile()
    {
        string savingData = JsonUtility.ToJson(gameData.saveData, true);
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        File.WriteAllText(filePath, savingData);
    }

    public void Load()
    {
        //check file
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        if (File.Exists(filePath))
        {            
            string loadedData = File.ReadAllText(filePath);
            saveData = JsonUtility.FromJson<SaveData>(loadedData);
        }
        else
        {
            AddDefaultData();
        }
    }


    public void AddDefaultData()
    {
        saveData = new SaveData();

        saveData.isActive = new bool[11];
        saveData.stars = new int[11];
        saveData.highScore = new int[11];
        saveData.isActive[0] = true;
        saveData.soundToggle = true;
        saveData.musicToggle = true;
        saveData.soundVolume = 1.0f;
        saveData.musicVolume = 0.5f;
    }

    public void DebugText(string text)
    {
        Scene scene = SceneManager.GetActiveScene();
        
        if (scene.name == "levels")
        {
            string tempText = debugText.text;
            debugText.text = "\n" + tempText + text;
        }
        else
        {
            Debug.Log(text);
        }
    }

    private void OnDisable()
    {
        SaveToFile();
    }

    private void OnApplicationQuit()
    {
        SaveToFile();
    }
}
