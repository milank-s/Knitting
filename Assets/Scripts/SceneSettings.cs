using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneSettings : MonoBehaviour
{
    // Start is called before the first frame update

    public Point startPoint;
    
    void Awake()
    {
        Services.StartPoint = startPoint;
        Services.PlayerBehaviour.curPoint = startPoint;
        Services.PlayerBehaviour.transform.position = startPoint.Pos;
        Services.PlayerBehaviour.Setup();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
