using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class SettingValue : MonoBehaviour
{
    public Setting _setting;
    public Text _text;
    public enum Setting{volume, resolution }

    public void ChangeValue(Single s)
    {
        _text.text = GameSettings.i.ChangeSetting((int)s, this);
    }
}
