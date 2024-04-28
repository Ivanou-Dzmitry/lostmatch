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
        gameDataClass = FindObjectOfType<game_data>();
        gameBoardClass = FindObjectOfType<game_board>();
    }

    public void WinOK()
    {
        if (gameDataClass != null)
        {
            try
            {
                gameDataClass.saveData.isActive[gameBoardClass.level + 1] = true;
            }
            catch
            {
                throw;
            }
            
            gameDataClass.Save();
        }

        SceneManager.LoadScene(sceneToLoad);
    }

    public void LoseOK()
    {
        SceneManager.LoadScene(sceneToLoad);
    }

}
