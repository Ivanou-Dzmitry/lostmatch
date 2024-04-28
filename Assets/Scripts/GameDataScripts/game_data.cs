using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class SaveData
{
    public bool[] isActive;
    public int[] highScore;
    public int[] stars;
}

public class game_data : MonoBehaviour
{
    public static game_data gameData;
    public SaveData saveData;
    public string fileName = "player_saves.dat";

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

        //load data
        Load();
    }

    private void Start()
    {
        
    }

    private void OnDisable()
    {
        Save();
    }

    private void OnApplicationQuit()
    {
        Save();
    }


    public void Save()
    {
        BinaryFormatter formatter = new BinaryFormatter();

        //to file
        FileStream file = File.Open(Application.persistentDataPath + "/" + fileName, FileMode.Create);

        //copy save data
        SaveData data = new SaveData();
        data = saveData;

        //save data to file
        formatter.Serialize(file, data);    

        //close data stream
        file.Close();

    }

    public void Load()
    {
        //check file

        if(File.Exists(Application.persistentDataPath + "/" + fileName))
        {
            //formatter
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/" + fileName, FileMode.Open);

            saveData = formatter.Deserialize(file) as SaveData;
            file.Close();
        }
        else
        {
            //default data
            saveData = new SaveData();
            saveData.isActive = new bool[100];
            saveData.stars = new int[100];
            saveData.highScore = new int[100];
            saveData.isActive[0] = true;
        }
    }


}
