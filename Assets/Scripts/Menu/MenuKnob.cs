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
            
            transform.localEulerAngles = new Vector3(-90, 0, 0);
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
        }else{
            // Quaternion targetRot = Quaternion.AngleAxis(rotation, -Vector3.forward);
            // transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * 5);
            
            transform.localEulerAngles = new Vector3(Mathf.Lerp(transform.localEulerAngles.x, rotation, Time.deltaTime), 90, -90);
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
