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
        gameDataClass = FindObjectOfType<game_data>();

        UpdateBar();
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
            if(score > gameBoardClass.scoreGoals[i] && numberStars < i + 1)
            {
                numberStars++;  
            }
        }

        if(gameDataClass != null)
        {
            int hiScore = gameDataClass.saveData.highScore[gameBoardClass.level];

            if (score > hiScore)
            {
                gameDataClass.saveData.highScore[gameBoardClass.level] = score;                
            }

            int currentStarsCount = gameDataClass.saveData.stars[gameBoardClass.level];

            if (numberStars > currentStarsCount)
            {
                gameDataClass.saveData.stars[gameBoardClass.level] = numberStars;
            }

            gameDataClass.Save();
        }

        UpdateBar();

    }

    private void OnApplicationPause()
    {
        if (gameDataClass != null)
        {
            gameDataClass.saveData.stars[gameBoardClass.level] = numberStars;
        }

        gameDataClass.Save();
    }
    


    private void UpdateBar()
    {
        if (gameBoardClass != null && scoreBar != null)
        {
            int scoreGoalsLength = gameBoardClass.scoreGoals.Length;
            scoreBar.value = (float)score / (float)gameBoardClass.scoreGoals[scoreGoalsLength - 1];
        }
    }

}
