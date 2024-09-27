using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        if(Services.main.state ==GameState.playing){
            
            resetTimer += Time.deltaTime;

            if(resetTimer > 45){

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
