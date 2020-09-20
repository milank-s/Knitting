using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class GameSettings : MonoBehaviour
{

    [SerializeField] private AudioMixer mainAudio;
    
    
    public void SetVolume(Single s)
    {
        mainAudio.SetFloat("Attenuation", s);
    }
}
