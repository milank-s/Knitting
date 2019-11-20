using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

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
    
    void Update()
    {
        if (Services.PlayerBehaviour.state == PlayerState.Traversing)
        {
            mixer.SetFloat("Volume", Services.PlayerBehaviour.flow * 5 - 15);
            mixer.SetFloat("Rate", Services.PlayerBehaviour.flow * 40f);
            mixer.SetFloat("Speed", Mathf.Lerp(0.75f, 1.25f, Services.PlayerBehaviour.curSpeed));

        }
        float val;
        float t = (Time.time - t0) * TimeScale;
        mixer.SetFloat("WindowLen", Mathf.Clamp(t * 0.1f, 0.5f, 0.5f));
        //mixer.SetFloat("Offset", 0.2f - 0.2f * Mathf.Cos(t * 0.03f));
        float rndOffset = 0.2f * (1.5f - Mathf.Sin(t * 0.05f));
        //mixer.SetFloat("RndOffset", rndOffset);
        //mixer.SetFloat("RndSpeed", rndOffset + 0.1f - 0.1f * Mathf.Cos(t * 0.04f) * Mathf.Cos(t * 0.37f));
        
        if(Input.GetKeyDown(KeyCode.Space))
        {
            play = !play;
            if (!play)
            {
                sample = 1;
            }
            else
            {
                sample = 0;
            }
        }
    }
}
