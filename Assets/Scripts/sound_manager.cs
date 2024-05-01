using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sound_manager : MonoBehaviour
{

    public AudioSource[] destroySound;

    public void PlayDestroySound()
    {

        if (PlayerPrefs.HasKey("Sound"))
        {
            if(PlayerPrefs.GetInt("Sound") == 1)
            {
                int clipToPlay = 0;
                destroySound[clipToPlay].Play();
            }
        }
        else
        {
            int clipToPlay = 0;
            destroySound[clipToPlay].Play();
        }
    }
}
