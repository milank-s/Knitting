using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class SettingToggle : MonoBehaviour
{
    public GameSettings.Setting _setting;
    public Text _text;

    public void ChangeValue(Single s)
    {
        _text.text = GameSettings.i.ChangeSetting((int)s, _setting);
    }
}
