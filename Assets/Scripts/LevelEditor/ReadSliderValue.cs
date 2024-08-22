using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Slider = UnityEngine.UIElements.Slider;

public class ReadSliderValue : MonoBehaviour
{
    [SerializeField] private SplineTurtle turtle;
    public float val;
    public bool updateTurtle = true;
    public bool updatePoints = false;
    public InputField input;
    public Text text;
    [SerializeField] public UnityEngine.UI.Slider slider;
    
    public UnityEvent OnChangeValue;
    public void Start()
    {
        val = slider.value;
    }
    
    public void ChangeInputField(String s)
    {

        if (float.TryParse(s, out val))
        {
            //val = Mathf.Clamp(val, slider.minValue, slider.maxValue);

            slider.SetValueWithoutNotify(val);
            input.SetTextWithoutNotify(val.ToString());

            if (updateTurtle)
            {
                turtle.RedrawTurtle();
            }
        }
    }
    
    public void ChangeSlider(Single s)
    {
        input.SetTextWithoutNotify(s.ToString());
        val = s;

        if (updateTurtle)
        {
            turtle.RedrawTurtle();
        }

        if(updatePoints){
            turtle.updatePoints = true;
        }

        if(OnChangeValue != null){
            OnChangeValue.Invoke();
        }
    }

        public void ChangeValue(Single s)
    {
        input.SetTextWithoutNotify(s.ToString());
        slider.SetValueWithoutNotify(s);
        val = s;
    }
}
