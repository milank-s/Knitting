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
    
    public UnityEvent OnClicked;
    public UnityEvent Reset;

    bool clicked;
    Vector2 mousePos;
    Vector3 axis;
    public float sensitivity = 1000;

    float timeLastClicked;

    float rotation;

    void Start(){
        axis = transform.forward;
    }

    public void OnMouseDown(){
        
        
        if((Time.time - timeLastClicked) < 1){
            //Reset value
            ResetValue();
        }
        
        if(OnClicked != null){
            OnClicked.Invoke();
        }

        timeLastClicked = Time.time;

        clicked = true;
        mousePos = Input.mousePosition;
    }

    public void OnMouseUp(){
        clicked = false;
    }

    public void Rotate(float angle){
        rotation += angle;
    }

    void Update(){
        if(clicked){
            TrackInput();
        }

        Quaternion targetRot = Quaternion.Euler(rotation, -90, -90);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRot, Time.deltaTime * 5);
        
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
        
        delta *= sensitivity;
        rotation += -delta;

        delta /= 1000;


        if(ChangeValue != null){
            ChangeValue.Invoke(delta);
        }
    }
}
