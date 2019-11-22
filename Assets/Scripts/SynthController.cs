using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using Vectrosity;

public class SynthController : MonoBehaviour
{
    
    public AudioMixer mixer;
    public float TimeScale = 1.0f;
    public List<AudioMixerSnapshot> snapShots;
    
    public static int sample
    {
        set
        {
            instance._sample = value;
            instance.SetSample();
        }
        get { return (int) instance._sample; }
    }

    private float _sample;
        
    private float t0;
    private bool play;
    public static SynthController instance;
    
    void Start()
    {
        t0 = Time.time;
        instance = this;
    }

    public void SetSample()
    {
        snapShots[sample].TransitionTo(0.1f);
        //change the audiomixer snapshot to the correct one
    }


    public void TurnOff()
    {
        //turn volume to 0
    }

    public static void FadeOut(float t)
    {
        float v;
        instance.mixer.GetFloat("Volume", out v);
        instance.mixer.SetFloat("Volume", Mathf.Lerp(v, -80, t));
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
