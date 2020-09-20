using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class SettingValue : MonoBehaviour
{
    public UnityEvent valueChanged;
    

    public void ChangeValue(Single s)
    {
        if (valueChanged != null)
        {
            valueChanged.Invoke();
        }
    }

    public Selectable _selectable;

    public void Input()
    {
        bool selected = EventSystem.current.currentSelectedGameObject == gameObject;

        if (selected)
        {
            //unity input system 
        }
    }
}
