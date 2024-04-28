using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sound_manager : MonoBehaviour
{

    public AudioSource[] destroySound;

    public void PlayDestroySound()
    {
        int clipToPlay = 0;

        destroySound[clipToPlay].Play();
    }

}
