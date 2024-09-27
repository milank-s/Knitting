using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
public class DemoScript : MonoBehaviour
{
    
    public GameObject[] menuButtonToggles;

    float resetTimer = 0;
    // Update is called once per frame

    void Start(){

        foreach(GameObject g in menuButtonToggles){
            g.SetActive(false);
        }
    }

    void Update()
    {
        
        InputSystem.onAnyButtonPress.Call(CurrentAction => {resetTimer = 0;});

        if(Services.main.state ==GameState.playing){
            
            if(Input.anyKey){
                resetTimer = 0;    
            }

            resetTimer += Time.deltaTime;

            if(resetTimer > 30){

                SceneController.instance.FinishLevelSet();
                resetTimer = 0;
            }
        }

        if(!MapEditor.typing && Input.GetKeyDown(KeyCode.R)){
            
            if(SceneManager.sceneCount > 1){
                
                SceneController.instance.LoadScene(SceneManager.GetSceneAt(SceneManager.sceneCount -1).name);
            }else{
                //you need to set the current level index I think
                SceneController.curLevel = 0;
                SceneController.instance.LoadDirect();
            }
            
        }   
    }
}
