using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Analytics;
using Unity.Services.Core;
using Unity.Services.Core.Analytics;
using UnityEngine.SceneManagement;

public class Analytic : MonoBehaviour
{
    async void Start()
    {

        if(Application.isEditor) return;

        await UnityServices.InitializeAsync();
        AnalyticsService.Instance.StartDataCollection();
    }

}