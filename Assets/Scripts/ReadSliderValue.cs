using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Slider = UnityEngine.UIElements.Slider;

public class ReadSliderValue : MonoBehaviour
{
    
    public InputField input;
    public Text text;
    [SerializeField] public UnityEngine.UI.Slider slider;

    public void Start()
    {
        slider = GetComponentInChildren<UnityEngine.UI.Slider>();
    }
    
    public void ChangeInputField(String s)
    {
        float f;
        float.TryParse(s, out f);
        f = Mathf.Clamp(f, slider.minValue, slider.maxValue);
        
        slider.SetValueWithoutNotify(f);
        input.SetTextWithoutNotify(f.ToString());
    }
    
    public void ChangeSlider(Single s)
    {
        input.SetTextWithoutNotify(s.ToString());
    }
    
  
}
