using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class pause_manager : MonoBehaviour
{
    public GameObject usedPanel;
    private game_board gameBoardClass;
    public bool paused = false;

    private game_data gameDataClass;
    private sound_manager soundManagerClass;

    public string sceneName;

    [Header("Sound")]
    public Button soundButton;
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;
    public Slider soundSlider;

    [Header("Music")]
    public Button musicButton;
    public Sprite musicOnSprite;
    public Sprite musicOffSprite;
    public Slider musicSlider;


    // Start is called before the first frame update
    void Start()
    {
        GameObject gameDataObject = GameObject.FindWithTag("GameData");
        gameDataClass = gameDataObject.GetComponent<game_data>();

        soundManagerClass = GameObject.FindWithTag("SoundManager").GetComponent<sound_manager>();

        if (gameDataClass != null)
        {
            LoadData();
        }

        usedPanel.SetActive(false);

        Scene currentScene = SceneManager.GetActiveScene();
        sceneName = currentScene.name;

        if (sceneName == "game_scene")
        {
            gameBoardClass = GameObject.FindWithTag("GameBoard").GetComponent<game_board>();
        }

    }


    private void LoadData()
    {
        //Debug.Log(gameDataClass.saveData.soundToggle + "/" + gameDataClass.saveData.musicToggle);

        //set sound
        if (gameDataClass.saveData.soundToggle)
        {
            soundButton.image.sprite = soundOnSprite;
            soundSlider.interactable = true;
        }
        else
        {
            soundButton.image.sprite = soundOffSprite;
            soundSlider.interactable = false;
        }

        //set music
        if (gameDataClass.saveData.musicToggle)
        {
            musicButton.image.sprite = musicOnSprite;
            musicSlider.interactable = true;
        }
        else
        {
            musicButton.image.sprite = musicOffSprite;
            musicSlider.interactable = false;
        }

        //load volume
        soundSlider.value = gameDataClass.saveData.soundVolume;
        musicSlider.value = gameDataClass.saveData.musicVolume;

    }


    public void SoundButton()
    {
        if (!gameDataClass.saveData.soundToggle)
        {
            gameDataClass.saveData.soundToggle = true;
            soundButton.image.sprite = soundOnSprite;
            soundSlider.interactable = true;
        }
        else
        {
            gameDataClass.saveData.soundToggle = false;
            soundButton.image.sprite = soundOffSprite;
            soundSlider.interactable = false;
        }
        
    }

    public void MusicButton()
    {
        if (!gameDataClass.saveData.musicToggle)
        {
            gameDataClass.saveData.musicToggle = true;
            musicButton.image.sprite = musicOnSprite;
            musicSlider.interactable = true;
        }
        else
        {
            gameDataClass.saveData.musicToggle = false;
            musicButton.image.sprite = musicOffSprite;
            musicSlider.interactable = false;
        }                
    }


    // Update is called once per frame
    void Update()
    {
        if (sceneName == "game_scene")
        {
            if (paused && !usedPanel.activeInHierarchy)
            {
                usedPanel.SetActive(true);
                gameBoardClass.currentState = GameState.pause;
            }

            if (!paused && usedPanel.activeInHierarchy)
            {
                usedPanel.SetActive(false);
                gameBoardClass.currentState = GameState.move;
            }
        }

    }

    public void PauseGame()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        paused = !paused;
    }

    public void SoundVolume()
    {
        gameDataClass.saveData.soundVolume = soundSlider.value;
        soundManagerClass.SetVolume("sound");       
    }

    public void MusicVolume()
    {
        gameDataClass.saveData.musicVolume = musicSlider.value;
        soundManagerClass.SetVolume("music");
    }


}
