using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class GameSettings : MonoBehaviour
{

    public static GameSettings i;
    
    [SerializeField] private AudioMixer mainAudio;

    public void ChangeSetting(int i, SettingValue.Setting s)
    {
        
    }
    
    public void SetVolume(Single s)
    {
        mainAudio.SetFloat("Attenuation", s);
    }
}
