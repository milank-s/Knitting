using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoScript : MonoBehaviour
{
    

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R)){
            //you need to set the current level index I think
            SceneController.curLevel = 0;
            SceneController.instance.LoadDirect();
            
        }   
    }
}
