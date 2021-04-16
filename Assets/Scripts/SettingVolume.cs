using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingVolume : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider slider;


    void Start()
    {
        slider.value = PlayerPrefs.GetFloat("volume", 0.5f);
        audioMixer.SetFloat("volume", PlayerPrefs.GetFloat("volume"));
    }

    public void SetVolume(float volume)//slider gets called to SetVolume
    {
        Debug.Log(volume);
        //to testand see debug log

        audioMixer.SetFloat("volume", volume);

        PlayerPrefs.SetFloat("volume", volume);
        //audioMixer.SetFloat("volume", PlayerPrefs.GetFloat("MVolume"));


    }

}

