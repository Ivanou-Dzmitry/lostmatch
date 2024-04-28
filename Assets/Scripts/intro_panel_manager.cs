using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class intro_panel_manager : MonoBehaviour
{
    [SerializeField] private GameObject introPanel;
    public void OK()
    {
        if (introPanel != null)
        {
            StartCoroutine(GameStratCo());
        }        
    }


    public void GameOver()
    {

    }

    IEnumerator GameStratCo()
    {
        yield return new WaitForSeconds(.5f);

        game_board gameBoardClass = FindObjectOfType<game_board>();
        gameBoardClass.currentState = GameState.move;

        introPanel.SetActive(false);
    }

}
