using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Analytics;
using Unity.Services.Core;
using Unity.Services.Core.Analytics;
using UnityEngine.SceneManagement;

public class DataConsent : MonoBehaviour
{
    public GameObject ui;
    public AudioSource sfx;
    public static bool hasConsented = false;
    bool gaveConsent = false;
    async void Start()
    {
        await UnityServices.InitializeAsync();

        // Show UI element asking the user for their consent OR retrieve prior consent from storage //

        if(PlayerPrefs.HasKey("dataCollectionConsent")){
            gaveConsent = PlayerPrefs.GetInt("dataCollectionConsent") == 0;
            // hasConsented = true;
        }

        if(hasConsented){
            ProcessConsent();

        }
    }

    public void GiveConsent(bool b){
        if(hasConsented) return;

        hasConsented = true;
        int c = b ? 0 : 1;
        gaveConsent = b;
        PlayerPrefs.SetInt("dataCollectionConsent", c);

        ProcessConsent();
        
    }

    IEnumerator WaitOneFrame(){
        yield return null;
        
        SceneManager.LoadSceneAsync("Main");
    }

    public void ProcessConsent(){

        if(gaveConsent){
            AnalyticsService.Instance.StartDataCollection();
        }

        ui.SetActive(false);
        sfx.Play();
        
        StartCoroutine(WaitOneFrame());
    }
}