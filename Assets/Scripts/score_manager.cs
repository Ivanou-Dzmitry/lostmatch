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

        if(gameDataClass != null)
        {
            int hiScore = gameDataClass.saveData.highScore[gameBoardClass.level];
            if (score > hiScore)
            {
                gameDataClass.saveData.highScore[gameBoardClass.level] = score;
            }
            gameDataClass.Save();
        }

        UpdateBar();

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
