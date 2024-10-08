﻿using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class GranularSynth : MonoBehaviour
{
    
    public enum SynthType{Flying, Moving, Stopping, Rewinding}
    
    public AudioMixer mixer;
    public float TimeScale = 1.0f;
    public List<AudioMixerSnapshot> snapShots;
    public SynthType myType;
    private float curSample;
    private float initVolume;

    public static List<GranularSynth> synths
    {
        get { return new List<GranularSynth> {flying, moving, stopping, rewinding}; }
    }
    
    private float _sample;
    private int sample;

    private bool isOn;
    private float t0;
    private bool play;
    public static GranularSynth flying, moving, stopping, rewinding;
    
    void Start()
    {
       mixer.GetFloat("Volume", out initVolume);
       
        t0 = Time.time;
        switch (myType)
        {
            case SynthType.Flying:
                flying = this;
                break;
            
            case SynthType.Moving:
                moving = this;
                break;
            
            case SynthType.Stopping:
                stopping = this;
                break;
            case SynthType.Rewinding:
                rewinding = this;
                break;
        }

        mixer.SetFloat("Volume", -80);
    }

    void Update()
    {
        switch (myType)
        {
            case SynthType.Moving:
               TraversingSynth();
                
                break;
            
            case SynthType.Flying:
                
                break;
            
            case SynthType.Stopping:
                break;
            
            case SynthType.Rewinding:
                break;
            
            
        }
    }
    
    IEnumerator SwitchSynth()
    {
        mixer.SetFloat("Volume", 0);
        while (Services.PlayerBehaviour.state == PlayerState.Switching)
        {
            if (Services.PlayerBehaviour.boostTimer == 0)
            {
                mixer.SetFloat("Volume", -80);
            }
            else
            {
                mixer.SetFloat("Volume", Services.PlayerBehaviour.boostTimer * 20 - 25);
                mixer.SetFloat("Speed", Mathf.Clamp(Services.PlayerBehaviour.boostTimer/2f + 0.25f, 0.25f, .75f));
            }

            yield return null;
        }
    }
    
    public void SetSample()
    {
            snapShots[sample].TransitionTo(0.1f);
        //change the audiomixer snapshot to the correct one
    }

    public void TurnOn()
    {
        isOn = true;
    }

    IEnumerator FadeIn()
    {
        float t = 0;
        
        while (t < 1)
        {
            t += Time.deltaTime;
            mixer.SetFloat("Volume", Mathf.Lerp(-80, 0, t));
            yield return null;
        }
    }
    public void TurnOff()
    {
        isOn = false;
        mixer.SetFloat("Volume", -80f);
    }

    public void FadeOut(float t)
    {
        float v;
        mixer.GetFloat("Volume", out v);
        mixer.SetFloat("Volume", Mathf.Lerp(v, -80, t));
    }

    public void TraversingSynth()
    {
        if (Services.PlayerBehaviour.state == PlayerState.Traversing)
        {
            float curVolume;

            if (isOn)
            {
                mixer.GetFloat("Volume", out curVolume);
                mixer.SetFloat("Volume",
                    Mathf.Lerp(curVolume, Mathf.Clamp(Services.PlayerBehaviour.flow * 20 - 50, -50, -15),
                        Time.deltaTime * 5));
            }
            
            //mixer.SetFloat("Rate", Mathf.Clamp(Services.PlayerBehaviour.clampedSpeed * 20f, 0, 50f));
             //using accuracy to make chord dissonant 
//            mixer.SetFloat("Speed", Vector3.Dot(Services.PlayerBehaviour.cursorDir, Vector3.up) / 2 + 1);

        }else if (Services.PlayerBehaviour.state == PlayerState.Switching)
        {
        }
        
        float speed;
        mixer.GetFloat("Speed", out speed);
        speed = Mathf.Lerp(speed,
            (-Mathf.Pow(Services.PlayerBehaviour.decelerationTimer, 3) / 10) +
            Mathf.Clamp01(Services.PlayerBehaviour.boostTimer / 2)/10f + 0.25f, Time.deltaTime * 5f);
        mixer.SetFloat("Speed", speed);
    }

    public void StoppingSynth()
    {
        StartCoroutine(SwitchSynth());
    }

}
