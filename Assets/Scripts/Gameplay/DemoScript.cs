using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class DemoScript : MonoBehaviour
{
    

    // Update is called once per frame
    void Update()
    {
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
