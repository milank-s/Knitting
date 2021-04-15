using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Slider = UnityEngine.UIElements.Slider;

public class ReadToggleValue : MonoBehaviour
{
    public bool val;
    public bool updateTurtle;
    public bool updatePoints;

    public UnityEvent OnChangeValue;
    [SerializeField] private SplineTurtle turtle;
    public Toggle toggle;

    void Start()
    {
        toggle.isOn = val;
    }
    
    public void ChangeValue(bool b)
    {

        val = b;

        if(OnChangeValue != null){
            OnChangeValue.Invoke();
        }

        if(updateTurtle){
            turtle.RedrawTurtle();
        }

        if(updatePoints){
            turtle.updatePoints = true;
        }
    }
    
  
}
