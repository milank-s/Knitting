using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SimpleJSON;
using System.IO;
public class SaveGame
{
    
    //TODO
    //Overworld saves that player has finished level
    //Unlocks points based on last level finished
    
    public static void Save(){
        
        JSONObject saveFile = new JSONObject();
        JSONObject levelData = new JSONObject();

        int level =  StellationManager.instance.level;
        int checkpoint = StellationManager.instance.stellationSets[level].controllers.IndexOf(Services.main.activeStellation);
        int pointIndex = Services.main.activeStellation._points.IndexOf(Services.PlayerBehaviour.curPoint);
        levelData["checkpoint"].AsInt = checkpoint; 
        levelData["startPoint"].AsInt = pointIndex; 
        saveFile[SceneController.curLevelName] = levelData;

        JSONNode node = ReadJSONFromFile("saves");
        if(node == null){
            WriteJSONtoFile("saves", saveFile);
        }else{
            node[SceneController.curLevelName] = levelData;
            File.WriteAllText(Application.streamingAssetsPath + "/Saves/saves.json", node.ToString());
        }
     }

    public static void Load(){
        JSONNode json = ReadJSONFromFile("saves");
        if(json == null) return;

        if(StellationManager.instance != null){
            string levelName = SceneController.curLevelName;
            if(json[levelName] == null) return;
        
            StellationManager.instance.checkpoint = json[levelName]["checkpoint"];
            StellationManager.instance.startPoint = json[levelName]["startPoint"];
        }
        
        //do stuff
    }

    static void WriteJSONtoFile(string fileName, JSONObject json)
    {
        StreamWriter sw = new StreamWriter( Application.streamingAssetsPath + "/Saves/" + fileName + ".json");
        sw.Write(json.ToString());
        sw.Close();
    }

    static JSONNode ReadJSONFromFile(string fileName)
    {   
        string path = Application.streamingAssetsPath + "/Saves/" + fileName + ".json";
        
        if(!System.IO.File.Exists(path)) return null;

        StreamReader sr = new StreamReader(path);

        string resultstring = sr.ReadToEnd();

        sr.Close();

        JSONNode result = JSON.Parse(resultstring);

        return result;
    }
}
