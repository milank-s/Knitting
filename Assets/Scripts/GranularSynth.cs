using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using Vectrosity;

public class GranularSynth : MonoBehaviour
{
    
    public enum SynthType{Flying, Moving, Stopping, Rewinding}
    
    public AudioMixer mixer;
    public float TimeScale = 1.0f;
    public List<AudioMixerSnapshot> snapShots;
    public SynthType myType;



    public static List<GranularSynth> synths
    {
        get { return new List<GranularSynth> {flying, moving, stopping, rewinding}; }
    }
    
    private float _sample;
    private int sample;
    
    private float t0;
    private bool play;
    public static GranularSynth flying, moving, stopping, rewinding;
    
    void Start()
    {
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
    }

    public void SetSample()
    {
            snapShots[sample].TransitionTo(0.1f);
        //change the audiomixer snapshot to the correct one
    }

    public void TurnOn()
    {
        
    }

    public void TurnOff()
    {
        //turn volume to 0
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
            mixer.SetFloat("Volume", Mathf.Clamp(Services.PlayerBehaviour.flow * 10 - 25, -25, 0));
            mixer.SetFloat("Rate", Services.PlayerBehaviour.flow * 20f);
            mixer.SetFloat("Speed", Mathf.Lerp(0.7f, 1f, Services.PlayerBehaviour.curSpeed/2));
         
            //using accuracy to make chord dissonant 
//            mixer.SetFloat("Speed", Vector3.Dot(Services.PlayerBehaviour.cursorDir, Vector3.up) / 2 + 1);

        }
    }

}
