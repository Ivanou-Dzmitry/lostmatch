using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class score_manager : MonoBehaviour
{
    //classes
    private game_board gameBoardClass;

    public TMP_Text scoreText;
    public TMP_Text debugText;
    public int score;

    public Slider scoreBar;


    public Image[] levelStars;
    public Sprite[] levelStarsSpite;

    private int scoreBarLenght;
    private int numberStars;
    

    //class
    private game_data gameDataClass;

    // Start is called before the first frame update
    void Start()
    {
        //class
        gameBoardClass = GameObject.FindWithTag("GameBoard").GetComponent<game_board>();

        GameObject gameDataObject = GameObject.FindWithTag("GameData");

        if (gameDataObject == null)
        {
            Debug.LogError("GameData object not found. Make sure there is a GameObject with the tag 'GameData' in the scene.");
            return;
        }

        gameDataClass = gameDataObject.GetComponent<game_data>();

        if (gameDataClass == null)
        {
            Debug.LogError("game_data component not found on GameData object. Make sure the game_data script is attached to the GameObject with the tag 'GameData'.");
            return;
        }


        if (gameBoardClass != null)
        {
            GetScoreData();
        }        

        UpdateBar();

        if (gameDataClass != null)
        {
            gameDataClass.Load();
        }

        for (int i = 0; i < levelStars.Length; i++)
        {
            levelStars[i].sprite = levelStarsSpite[1];
            
        }
    }

    private void GetScoreData()
    {
        int scoreGoalsLength = gameBoardClass.scoreGoals.Length;

        scoreBarLenght = 0;

        for (int i = 0; i < scoreGoalsLength; i++)
        {
            scoreBarLenght = scoreBarLenght + gameBoardClass.scoreGoals[i];
        }
    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = score.ToString();
    }


    public void IncreaseScore(int amountToIncrease)
    {
        score += amountToIncrease;        

        for (int i = 0; i < gameBoardClass.scoreGoals.Length; i++)
        {
            //Debug.Log("Score Goals " +i+": "+ gameBoardClass.scoreGoals[i]);

            if (score >= gameBoardClass.scoreGoals[i] && numberStars < i + 1)
            {
                numberStars++;
            }
        }

        //turn on stars
        for (int i = 0; i < numberStars; i++)
        {
            levelStars[i].sprite = levelStarsSpite[0];
        }

        if (gameDataClass != null)
        {
            int hiScore = gameDataClass.saveData.highScore[gameBoardClass.level];

            //DebugText("Score:" + hiScore + "-" + score);

            if (score > hiScore)
            {
                gameDataClass.saveData.highScore[gameBoardClass.level] = score;                
            }

            int currentStarsCount = gameDataClass.saveData.stars[gameBoardClass.level];

            if (numberStars > currentStarsCount)
            {
                gameDataClass.saveData.stars[gameBoardClass.level] = numberStars;
            }

            gameDataClass.SaveToFile();
         }

        UpdateBar();

        //DebugText(""+score);

    }

    private void OnApplicationPause()
    {
        if (gameDataClass != null)
        {
            gameDataClass.saveData.stars[gameBoardClass.level] = numberStars;
            
            gameDataClass.SaveToFile();
        }        
    }
    


    private void UpdateBar()
    {
        if (gameBoardClass != null && scoreBar != null)
        {
            int scoreGoalsLength = gameBoardClass.scoreGoals.Length;

            scoreBar.value = (float)score / (float)gameBoardClass.scoreGoals[scoreGoalsLength - 1];                        
        }
    }

    private void DebugText(string text)
    {
        debugText.text = debugText.text + ", " + text;
    }

}
