using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class confirm_panel : MonoBehaviour
{
    [Header("Level Info")]
    public string levelToLoad;    
    public int level;
    private int activeStars;

    [Header("UI")]
    public Image[] stars;
    //public TMP_Text starText;
    public TMP_Text highScoreText;
    public TMP_Text headerText;
    private int highScore;

    [Header("Stars")]
    public Sprite starOffSprite;
    public Sprite starOnSprite;


    //class
    private game_data gameDataClass;


    // Start is called before the first frame update
    void OnEnable()
    {
        //class
        gameDataClass = GameObject.FindWithTag("GameData").GetComponent<game_data>();



        //deactivate stars
        for (int i = 0; i < activeStars; i++)
        {
            stars[i].sprite = starOffSprite;
        }

        LoadData(); //from file
        ActivateStars();
        SetText();
    }


    void LoadData()
    {
        //game data check
        if (gameDataClass != null)
        {
            activeStars = gameDataClass.saveData.stars[level - 1];
            highScore = gameDataClass.saveData.highScore[level - 1];
            
            //Debug.Log("activeStars: " + activeStars + "/" + highScore);
        }

        //load game immediatly
        if (highScore == 0)
        {
            Play();
        }

    }

    void SetText()
    {
        highScoreText.text = "Items collected: " + highScore;
        headerText.text = "Level " + level + " Records";
        //starText.text = "" + activeStars + "/3";
    }

    void ActivateStars()
    {
        for (int i = 0; i < activeStars; i++)
        {
            stars[i].sprite = starOnSprite;
        }
    }

    public void Play()
    {
        //gameDataClass.SaveToFile();
        PlayerPrefs.SetInt("Current_Level", level - 1);
        SceneManager.LoadScene(levelToLoad);
    }

}
