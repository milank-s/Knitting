using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneBank : MonoBehaviour
{

    [SerializeField]public List<string> sceneDictionary;
   
    // Update is called once per frame
    public void LoadScene(int m)
    {
        Services.main.LoadLevelDelayed(sceneDictionary[m]);
    }
    
    
}
