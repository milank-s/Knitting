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
    public Setting _setting;
    public enum Setting{volume, resolution }

    public void ChangeValue(Single s)
    {
        GameSettings.i.ChangeSetting((int)s, _setting);
    }

    public Selectable _selectable;
}
