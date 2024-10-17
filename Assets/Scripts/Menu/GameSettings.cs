using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

public class GameSettings : MonoBehaviour
{

    public AudioSource changeSettingFX;
    public enum Setting{volume, vibration, gamepad, resolution, sensitivity}

    private int newWidth;
    private int newHeight;
    public static GameSettings i;
    public List<SettingValue> settings;
    [SerializeField] private AudioMixer mainAudio;

    void Awake()
    {
        settings = GetComponentsInChildren<SettingValue>().ToList();
        i = this;
        gameObject.SetActive(false);
    }
    public void ChangeSetting(int i, Setting s)
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

             case Setting.sensitivity:
                toReturn = SetSensitivity(i);
                break;
            
        }
        
        SetSettingText(toReturn, s, i);

        PlayerPrefs.Save();

    }

    public void SetSettingText(string t, Setting s, int i = 0)
    {
        
        foreach (SettingValue v in settings)
        {
            if (v._setting == s)
            {
                if (v._text.text != t)
                {
                    changeSettingFX.Play();
                    Services.menu.RotateXKnob(-i);
                }
                v._text.text = t;
                break;
            }
        }   
    }

    public string SetSensitivity(int f){
        float curSense = 0.5f;

        if (PlayerPrefs.HasKey("Sensitivity")){
            curSense = PlayerPrefs.GetFloat("Sensitivity");
        }

        float senseDiff = ((float)f)/10f;

        curSense = Mathf.Clamp(curSense + senseDiff, 0.1f, 1f);
        String displayedSensitivity = ((int)(curSense * 10)).ToString();

        PlayerPrefs.SetFloat("Sensitivity", curSense);

        Services.PlayerBehaviour.cursorMoveSpeed = curSense;

        return displayedSensitivity;

    }

    public void SubmitSettingChanges()
    {
        Screen.SetResolution(newWidth, newHeight, FullScreenMode.ExclusiveFullScreen);
        PlayerPrefs.SetInt("ResolutionWidth", newWidth);
        PlayerPrefs.SetInt("ResolutionHeight", newHeight);
        
        Services.menu.OpenSettings();
        PlayerPrefs.Save();
    }
    
    public void InitializeSettings()
    {
        float curVolume;
        if (PlayerPrefs.HasKey("GameVolume"))
        {
            curVolume = PlayerPrefs.GetFloat("GameVolume");
            AudioManager.instance.SetVolume(curVolume);
        }
        else
        {   
            PlayerPrefs.SetFloat("GameVolume", AudioManager.volume);
        }

        SetSettingText((PlayerPrefs.GetFloat("GameVolume") * 10f).ToString("F0") , Setting.volume);

        if (PlayerPrefs.HasKey("UseGamepad"))
        {
            Services.main.useGamepad = PlayerPrefs.GetInt("UseGamepad") > 0;
        }
        else
        {
            PlayerPrefs.SetFloat("UseGameped", 1);
        }

         if (PlayerPrefs.HasKey("Sensitivity"))
        {
            float sense = PlayerPrefs.GetFloat("Sensitivity");
            Services.PlayerBehaviour.cursorMoveSpeed = sense;
            SetSettingText(((int)(sense * 10)).ToString(), Setting.sensitivity);
            
        }else{
            
            SetSettingText(5.ToString(), Setting.sensitivity);
        }
        
        string t = Services.main.useGamepad ? "yes" : "no";
        SetSettingText(t, Setting.gamepad);
        
        
        if (PlayerPrefs.HasKey("UseVibration"))
        {
            Services.main.useVibration = PlayerPrefs.GetInt("UseVibration") > 0;
            
        }
        else
        {
            PlayerPrefs.SetFloat("UseVibration", 1);
        }
        
        t = Services.main.useVibration ? "yes" : "no";
        SetSettingText(t, Setting.vibration);
        

        if (PlayerPrefs.HasKey("ResolutionWidth") && PlayerPrefs.HasKey("ResolutionHeight"))
        {
            Screen.SetResolution(PlayerPrefs.GetInt("ResolutionWidth"), PlayerPrefs.GetInt("ResolutionHeight"), FullScreenMode.ExclusiveFullScreen);
        }
        else
        {
            PlayerPrefs.SetInt("ResolutionWidth", Screen.currentResolution.width);
            PlayerPrefs.SetInt("ResolutionHeight", Screen.currentResolution.height);
            
        }

        newWidth = Screen.currentResolution.width;
        newHeight = Screen.currentResolution.height;
        t = PlayerPrefs.GetInt("ResolutionWidth") + " " + PlayerPrefs.GetInt("ResolutionHeight");
        SetSettingText(t, Setting.resolution);
        
    }

    public string SetResolution(Single s)
    {
        Resolution[] resolutions = Screen.resolutions;
       
        int indexof = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == newWidth && resolutions[i].height == newHeight)
            {
                indexof = i;
                break;
            }
        }

        int newIndex = indexof + (int) s;
        if (newIndex >= resolutions.Length)
        {
            newIndex = 0;
        }else if (newIndex < 0)
        {
            newIndex = resolutions.Length - 1;
        }

        newWidth = resolutions[newIndex].width;
        newHeight = resolutions[newIndex].height;
        
        return resolutions[newIndex].width + " " + resolutions[newIndex].height;
    }
    
    public float SetVolume(Single s)
    {
        float curVolume = AudioManager.volume;
        float diff = s / 10f;
        float newVolume = Mathf.Clamp01(curVolume + diff);

        AudioManager.instance.SetVolume(newVolume);

        if (PlayerPrefs.HasKey("GameVolume"))
        {
            PlayerPrefs.SetFloat("GameVolume", newVolume);
        }

        return newVolume * 10f;
    }

    public string SetVibration(Single s)
    {

        Services.main.useVibration = !Services.main.useVibration;
            
      
            if (Services.main.hasGamepad)
            {
                Services.main.gamepad.ResetHaptics();
            }

            return Services.main.useVibration ? "yes" : "no";
    }

    string SetGamepad(Single s)
    {
        Services.main.useGamepad = s > 0;
        //Services.main.EnableGamepad();
        return s > 0 ? "yes" : "no";
    }
}
