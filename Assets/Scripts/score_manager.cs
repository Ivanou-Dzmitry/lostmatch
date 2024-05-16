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
    private int numberStars;

    //class
    private game_data gameDataClass;

    // Start is called before the first frame update
    void Start()
    {
        //class
        gameBoardClass = GameObject.FindWithTag("GameBoard").GetComponent<game_board>();
        gameDataClass = GameObject.FindWithTag("GameData").GetComponent<game_data>();

        //gameDataClass.Load();

        UpdateBar();

        if (gameDataClass != null)
        {
            gameDataClass.Load();
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
            if (score > gameBoardClass.scoreGoals[i] && numberStars < i + 1)
            {
                numberStars++;
            }
        }

        //DebugText("ReadFromsaveData:" + gameDataClass.saveData.isActive.Length);

        if (gameDataClass != null)
        {
            int hiScore = gameDataClass.saveData.highScore[gameBoardClass.level];

            DebugText("Score:" + hiScore + "-" + score);

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
        debugText.text = debugText.text + "/_/" + text;
    }

}
