using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

public class GameSettings : MonoBehaviour
{
    public enum Setting{volume, vibration, gamepad, resolution }
    
    public static GameSettings i;
    public List<SettingValue> settings;
    [SerializeField] private AudioMixer mainAudio;

    void Awake()
    {
        settings = GetComponentsInChildren<SettingValue>().ToList();
        i = this;
    }
    public string ChangeSetting(int i, Setting s)
    {
        string toReturn = "";
        switch (s)
        {
            case Setting.volume:
                toReturn = SetVolume(i).ToString("F0");
                break;
            
            case Setting.gamepad:
                toReturn = SetGamepad(i);
                break;
            
            case Setting.vibration:
                toReturn = SetVibration(i);
                break;
            
            case Setting.resolution:
                toReturn = SetResolution(i);
                break;
            
        }
        
        
        PlayerPrefs.Save();
        return toReturn;

    }

    public void SetSettingText(string t, Setting s)
    {
        foreach (SettingValue v in settings)
        {
            if (v._setting == s)
            {
                v._text.text = t;
                break;
            }
        }   
    }
    
   
    public void InitializeSettings()
    {
        if (PlayerPrefs.HasKey("GameVolume"))
        {
            mainAudio.SetFloat("Volume", Mathf.Lerp(-80, 0, PlayerPrefs.GetFloat("GameVolume")));   
            SetSettingText((PlayerPrefs.GetFloat("GameVolume") * 10f).ToString("F0") , Setting.volume);
        }
        else
        {
            float curVolume = 0;
            mainAudio.GetFloat("Volume", out curVolume);
            curVolume = 1-Mathf.Abs(curVolume / 80f);
            PlayerPrefs.SetFloat("GameVolume", curVolume);
        }

        if (PlayerPrefs.HasKey("UseGamepad"))
        {
            Services.main.useGamepad = PlayerPrefs.GetInt("UseGamepad") > 0;
            string t = Services.main.useGamepad ? "yes" : "no";
            SetSettingText(t, Setting.gamepad);
        }
        else
        {
            PlayerPrefs.SetFloat("UseGameped", 1);
        }
        
        if (PlayerPrefs.HasKey("UseVibration"))
        {
            Services.main.useVibration = PlayerPrefs.GetInt("UseVibration") > 0;
            string t = Services.main.useVibration ? "yes" : "no";
            SetSettingText(t, Setting.vibration);
        }
        else
        {
            PlayerPrefs.SetFloat("UseVibration", 1);
        }

        if (PlayerPrefs.HasKey("ResolutionWidth") && PlayerPrefs.HasKey("ResolutionHeight"))
        {
            Screen.SetResolution(PlayerPrefs.GetInt("ResolutionWidth"), PlayerPrefs.GetInt("ResolutionHeight"), FullScreenMode.ExclusiveFullScreen);
        }
        else
        {
            PlayerPrefs.SetInt("ResolutionWidth", Screen.currentResolution.width);
            PlayerPrefs.SetInt("ResolutionHeight", Screen.currentResolution.height);
            
        }
        
        PlayerPrefs.Save();
    }

    public string SetResolution(Single s)
    {
        Resolution[] resolutions = Screen.resolutions;
        Resolution curResolution = Screen.currentResolution;
        int indexof = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == curResolution.width && resolutions[i].height == curResolution.height)
            {
                indexof = i;
                break;
            }
        }

        int newIndex = indexof + (int) s;
        if (newIndex > resolutions.Length)
        {
            newIndex = 0;
        }else if (newIndex < 0)
        {
            newIndex = resolutions.Length - 1;
        }

        Screen.SetResolution(resolutions[newIndex].width, resolutions[newIndex].height, FullScreenMode.ExclusiveFullScreen);
        
        return resolutions[newIndex].width + " " + resolutions[newIndex].height;
    }
    
    public float SetVolume(Single s)
    {
        float curVolume;
        float diff = s / 10f;
        mainAudio.GetFloat("Volume", out curVolume);
        curVolume = 1-Mathf.Abs(curVolume / 80f);
        float newVolume = Mathf.Clamp01(curVolume + diff);
        mainAudio.SetFloat("Volume", Mathf.Lerp(-80, 0, newVolume));

        if (PlayerPrefs.HasKey("GameVolume"))
        {
            PlayerPrefs.SetFloat("GameVolume", newVolume);
        }

        return newVolume * 10f;
    }

    public string SetVibration(Single s)
    {
        if (s > 0)
        {
            Services.main.useVibration = true;
            return "yes";
        }else
        {
            Services.main.useVibration = false;
            if (Services.main.hasGamepad)
            {
                Services.main.gamepad.ResetHaptics();
            }

            return "no";
        }
    }

    string SetGamepad(Single s)
    {
        Services.main.useGamepad = s > 0;
        //Services.main.EnableGamepad();
        return s > 0 ? "yes" : "no";
    }
}
