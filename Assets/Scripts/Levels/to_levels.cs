using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class to_levels : MonoBehaviour
{
    public string sceneToLoad;
    private game_data gameDataClass;
    private game_board gameBoardClass;

    private void Start()
    {
        //class
        gameDataClass = GameObject.FindWithTag("GameData").GetComponent<game_data>();
        gameBoardClass = GameObject.FindWithTag("GameBoard").GetComponent<game_board>();
    }

    public void WinOK()
    {
        if (gameDataClass != null)
        {
            gameDataClass.saveData.isActive[gameBoardClass.level + 1] = true;            
            gameDataClass.SaveToFile();
        }

        SceneManager.LoadScene(sceneToLoad);
    }

    public void LoseOK()
    {
        SceneManager.LoadScene(sceneToLoad);
    }

}
