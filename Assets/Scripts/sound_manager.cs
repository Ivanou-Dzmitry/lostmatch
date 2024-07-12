using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sound_manager : MonoBehaviour
{
    public static sound_manager Instance;

    [SerializeField] private AudioSource _effectsSource;
    [SerializeField] private AudioSource _musicSource;

    private AudioClip _clip;

    private game_data gameDataClass;

    private void Start()
    {
        GameObject gameDataObject = GameObject.FindWithTag("GameData");
        gameDataClass = gameDataObject.GetComponent<game_data>();

        if (gameDataClass != null)
        {
            LoadData();
        }

    }


    private void LoadData()
    {
        //load data
        if (!gameDataClass.saveData.soundToggle)
        {
            MuteSound(true);
        }


        if (!gameDataClass.saveData.musicToggle)
        {
            MuteMusic(true);
        }

        //load vol
        _effectsSource.volume = gameDataClass.saveData.soundVolume;
        _musicSource.volume = gameDataClass.saveData.musicVolume;
    }


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetVolume(string type)
    {
        if(gameDataClass != null)
        {
            if (type == "sound")
            {
                _effectsSource.volume = gameDataClass.saveData.soundVolume;
            }

            if (type == "music")
            {
                _musicSource.volume = gameDataClass.saveData.musicVolume;
            }
        }
    }

    public void PlaySound(AudioClip clip)
    {
       _effectsSource.PlayOneShot(clip);
    }

    public void ChangeMasterVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    public void MuteSound(bool muted)
    {
        _effectsSource.mute = !_effectsSource.mute;
    }

    public void MuteMusic(bool muted)
    {
        _musicSource.mute = !_musicSource.mute;
    }

}
