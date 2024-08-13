using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class SettingValue : MonoBehaviour
{
    public GameSettings.Setting _setting;
    public Text _text;

    public void ChangeValue(Single s)
    {
        GameSettings.i.ChangeSetting((int)s, _setting);
    }
}
