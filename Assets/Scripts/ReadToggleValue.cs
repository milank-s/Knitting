using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Slider = UnityEngine.UIElements.Slider;

public class ReadToggleValue : MonoBehaviour
{
    public bool val;

    [SerializeField] private SplineTurtle turtle;
    public Toggle toggle;

    void Start()
    {
        toggle.isOn = val;
    }
    
    public void ChangeValue(bool b)
    {

        val = b;
        turtle.RedrawTurtle();
    }
    
  
}
