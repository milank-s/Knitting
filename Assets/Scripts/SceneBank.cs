using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneBank : MonoBehaviour
{
    
    
    // Start is called before the first frame update
    public void LoadEditor()
    {
        Services.main.ToggleEditMode();

        if (!MapEditor.editing)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void LoadLevel(string levelName)
    {
        Services.main.Reset();
        //add logic for a set of levels
        if (MapEditor.editing)
        {
            Services.main.ToggleEditMode();
        }

        SceneController.instance.LoadNextStellation();
        Services.main.CloseMenu();
    }
}
