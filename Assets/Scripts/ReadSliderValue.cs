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
    [SerializeField] public Slider slider;
    
    

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeInputField(String s)
    {
        float f;
        float.TryParse(s, out f);
        f = Mathf.Clamp(f, slider.lowValue, slider.highValue);
        
        slider.SetValueWithoutNotify(f);
    }
    
    public void ChangeSlider(Single s)
    {
        input.SetTextWithoutNotify(s.ToString());
    }
    
  
}
