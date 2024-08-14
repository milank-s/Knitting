using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SimpleJSON;
using System.IO;
public class SaveGame
{
    
    public static void SaveProgress(){
        JSONObject saveFile = new JSONObject();

        JSONObject levelData = new JSONObject();
        levelData["levelName"] = SceneManager.GetActiveScene().name;

        int level =  StellationManager.instance.level;
        int checkpoint = StellationManager.instance.stellationSets[level].controllers.IndexOf(Services.main.activeStellation);
        int pointIndex = Services.main.activeStellation._points.IndexOf(Services.PlayerBehaviour.curPoint);
        levelData["checkpoint"].AsInt = checkpoint; 
        levelData["startPoint"].AsInt = pointIndex; 

        WriteJSONtoFile("saves", saveFile);
     }

    public static void Load(){
        JSONNode json = ReadJSONFromFile("saves/saves.json");

        if(StellationManager.instance != null){
            string levelName = SceneManager.GetActiveScene().name;
            StellationManager.instance.checkpoint = json[levelName]["checkpoint"];
            StellationManager.instance.startPoint = json[levelName]["startPoint"];
        }
        
        //do stuff
    }

    static void WriteJSONtoFile(string fileName, JSONObject json)
    {
        StreamWriter sw = new StreamWriter("saves/" + fileName);
        sw.Write(json.ToString());
        sw.Close();
    }

    static JSONNode ReadJSONFromFile(string fileName)
    {
        StreamReader sr = new StreamReader("saves/" + fileName);

        string resultstring = sr.ReadToEnd();

        sr.Close();

        JSONNode result = JSON.Parse(resultstring);

        return result;
    }
}
