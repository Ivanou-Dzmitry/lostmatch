using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum GameType
{
    Moves,
    Time
}

[System.Serializable]
public class EndGameRequriments
{
    public GameType gameType;

    public int counterValue;
}


public class end_game_manager : MonoBehaviour
{
    public EndGameRequriments egRequrimentsClass;

    //panels
    public GameObject winPanel;
    public GameObject tryPanel;

    //UI
    public GameObject movesLabel;
    public TMP_Text counter;
    public int curCounterVal;
    
    //class
    private game_board gameBoardClass;


    // Start is called before the first frame update
    void Start()
    {
        gameBoardClass = GameObject.FindWithTag("GameBoard").GetComponent<game_board>();

        SetGameType();

        SetupGame();
    }

    public void SetGameType()
    {
        if (gameBoardClass != null)
        {
            if(gameBoardClass.level < gameBoardClass.worldClass.levels.Length)
            { 
                if (gameBoardClass.worldClass.levels[gameBoardClass.level] != null)
                {
                    egRequrimentsClass = gameBoardClass.worldClass.levels[gameBoardClass.level].egRequrimentsLVL;
                }
            }
        }
    }

    void SetupGame()
    {
        curCounterVal = egRequrimentsClass.counterValue;

        if(egRequrimentsClass.gameType == GameType.Moves)
        {
            movesLabel.SetActive(true);
        }

        counter.text = "" + curCounterVal;
    }


    public void DecreaseCounterVal()
    {
        if (gameBoardClass.currentState != GameState.pause)
        {
            curCounterVal--;
            counter.text = "" + curCounterVal;

            //for end game
            if (curCounterVal <= 0)
            {
                LoseGame();            
            }
        }
    }

    public void WinGame()
    {
        winPanel.SetActive(true);
        gameBoardClass.currentState = GameState.win;

        curCounterVal = 0;
        counter.text = "" + curCounterVal;
    }

    public void LoseGame()
    {
        tryPanel.SetActive(true);

        gameBoardClass.currentState = GameState.lose;
        curCounterVal = 0;
        counter.text = "" + curCounterVal;
    }


}
