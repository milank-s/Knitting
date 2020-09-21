using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class GameSettings : MonoBehaviour
{
    public static GameSettings i;
    
    [SerializeField] private AudioMixer mainAudio;

    public string ChangeSetting(int i, SettingValue s)
    {
        return i.ToString();
    }
    
    public void SetVolume(Single s)
    {
        float curVolume;
        mainAudio.GetFloat("Attenuation", out curVolume);
        
        mainAudio.SetFloat("Attenuation", Mathf.Clamp01(curVolume + s));
    }
}
