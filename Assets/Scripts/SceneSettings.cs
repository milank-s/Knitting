using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneSettings : MonoBehaviour
{
    // Start is called before the first frame update

    public Point startPoint;
    
    void Start()
    {
        Services.StartPoint = startPoint;
        Services.main.InitializeLevel();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
