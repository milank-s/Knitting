using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
public class MenuSelectable : MonoBehaviour, ISelectHandler, ISubmitHandler
{

    public AudioClip selectSFX;
    public AudioClip inputSFX;

    public UnityEvent OnSelectEvent;
    public UnityEvent OnInputEvent;
    public void OnSelect (BaseEventData eventData) 
	{
		if(OnSelectEvent != null){
            OnSelectEvent.Invoke();
        }
	}

     public void OnSubmit (BaseEventData eventData) 
	{
		if(OnInputEvent != null){
            OnInputEvent.Invoke();
        }
	}
}
