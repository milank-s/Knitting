using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class controllerText : MonoBehaviour
{
    public string mouse;
    public string controller;

    private TextMesh text;
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TextMesh>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Main.usingJoystick)
        {
            text.text = controller;
        }
        else
        {
            text.text = mouse;
        }
    }
}
