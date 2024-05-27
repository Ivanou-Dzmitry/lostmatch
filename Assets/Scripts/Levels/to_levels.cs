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
