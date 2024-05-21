using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class level_button : MonoBehaviour
{
    [Header("Stuff")]
    public bool isActive;
    private Button myButton;
    private int activeStars;

    [Header("Level UI")]
    public Image[] stars;
    public TMP_Text levelText;
    public TMP_Text debugText;
    public int level;
    public GameObject confirmPanel;

    [Header("Stars")]
    public Sprite starOffSprite;
    public Sprite starOnSprite;


    private game_data gameDataClass;

    // Start is called before the first frame update
    void Start()
    {
        //class
        gameDataClass = GameObject.FindWithTag("GameData").GetComponent<game_data>();
       
        myButton = GetComponent<Button>();

        LoadData();
        ShowLevel();
        ChooseSprite();
        ActivateStars();
    }

    void LoadData()
    {
        //game data check
        if (gameDataClass != null)
        {           
            if (gameDataClass.saveData.isActive[level - 1])
            {
                isActive = true;
            }
            else
            {
                isActive = false;
            }
        }

        //active stars
        activeStars = gameDataClass.saveData.stars[level -1];
    }

    void ChooseSprite()
    {
        if(isActive)
        {
            myButton.interactable = true;
            levelText.enabled = true;
        }
        else
        {
            myButton.interactable = false;
            levelText.enabled = false;
        }
    }

    void ShowLevel()
    {
        levelText.text = "" + level;
    }

    void ActivateStars()
    {
        //show stars
        for (int i = 0; i < activeStars; i++)
        {
            stars[i].sprite = starOnSprite;
        }
    }

    public void ConfirmPanel(int level)
    {
        confirmPanel.GetComponent<confirm_panel>().level = level;
        confirmPanel.SetActive(true);
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

}
