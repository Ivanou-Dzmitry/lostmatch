using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class pause_manager : MonoBehaviour
{
    public GameObject pausePanel;
    private game_board gameBoardClass;
    public bool paused = false;

    public Button soundButton;
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;


    // Start is called before the first frame update
    void Start()
    {
        pausePanel.SetActive(false);

        gameBoardClass = GameObject.FindWithTag("GameBoard").GetComponent<game_board>();
        

        if (PlayerPrefs.HasKey("Sound"))
        {
            if(PlayerPrefs.GetInt("Sound") == 0)
            {                
                soundButton.image.sprite = soundOffSprite;
            }
            else
            {
                soundButton.image.sprite = soundOnSprite;
            }
        }
        else
        {
            soundButton.image.sprite = soundOnSprite;
        }
    }

    public void SoundButton()
    {
        if (PlayerPrefs.HasKey("Sound"))
        {
            if (PlayerPrefs.GetInt("Sound") == 0)
            {
                soundButton.image.sprite = soundOnSprite;
                PlayerPrefs.SetInt("Sound", 1);
            }
            else
            {
                soundButton.image.sprite = soundOffSprite;
                PlayerPrefs.SetInt("Sound", 0);
            }
        }
        else
        {
            soundButton.image.sprite = soundOnSprite;
            PlayerPrefs.SetInt("Sound", 1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (paused && !pausePanel.activeInHierarchy)
        {
            pausePanel.SetActive(true);
            gameBoardClass.currentState = GameState.pause;
        }

        if (!paused && pausePanel.activeInHierarchy)
        {
            pausePanel.SetActive(false);
            gameBoardClass.currentState = GameState.move;
        }

    }

    public void PauseGame()
    {
        paused = !paused;
    }

}
