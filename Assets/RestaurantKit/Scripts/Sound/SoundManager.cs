using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public GameObject mutedtoggle;
    public GameObject unMutedtoggle;
    public GameObject soundButton;
    public void MuteToggle(bool muted)
    {
        if(muted)
        {
            AudioListener.volume = 0;
            mutedtoggle.SetActive(true);
            unMutedtoggle.SetActive(false);
        }
        else
        {
            soundButton.GetComponent<AudioSource>().Play();
            AudioListener.volume = 1;
            unMutedtoggle.SetActive(true);
            mutedtoggle.SetActive(false);
        }
    }
}
