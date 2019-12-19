using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneBank : MonoBehaviour
{
    
    
    // Start is called before the first frame update
    public void LoadEditor()
    {
        MapEditor.editing = !MapEditor.editing;
        Services.main.ToggleEditMode(MapEditor.editing);

        if (!MapEditor.editing)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void LoadLevel(string levelName)
    {
        Services.main.LoadLevelDelayed(levelName, 0);
    }
}
