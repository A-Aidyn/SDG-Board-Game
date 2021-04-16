using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    
    public AudioSource source;

    public void PlaySound()
    {
        source.Play();
    }

    

}
