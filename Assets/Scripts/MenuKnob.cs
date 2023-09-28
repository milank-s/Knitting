using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[System.Serializable]
public class MyFloatEvent : UnityEvent<float>
{

}

public class MenuKnob : MonoBehaviour
{
    public MyFloatEvent ChangeValue;
    public UnityEvent Reset;
    
    bool clicked;
    Vector2 mousePos;
    public float sensitivity = 1000;

    float timeLastClicked;

    public void OnMouseDown(){
        
        if((Time.time - timeLastClicked) < 1){
            //Reset value
            ResetValue();
        }

        timeLastClicked = Time.time;
        clicked = true;
        mousePos = Input.mousePosition;
    }

    public void OnMouseUp(){
        clicked = false;
    }

    void Update(){
        if(clicked){
            TrackInput();
        }
    }

    void ResetValue(){
        if(Reset != null){
            Reset.Invoke();
        }
    }

    void TrackInput(){
        
        float delta = 0;
        Vector2 m = Input.mousePosition;
        delta = m.x - mousePos.x;
        mousePos = m;
        
        transform.Rotate(0,-delta,0);
        delta *= sensitivity;

        if(ChangeValue != null){
            ChangeValue.Invoke(delta);
        }
    }
}
