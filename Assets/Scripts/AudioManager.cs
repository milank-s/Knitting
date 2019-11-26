using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    // Start is called before the first frame update

    public static AudioManager instance;
    public AudioMixer SynthMaster;
    void Start()
    {
        instance = this;
    }
    public void MuteSynths(bool mute)
    {
        foreach (GranularSynth s in GranularSynth.synths)
        {
            if (mute)
            {
                s.TurnOff();
            }
            else
            {
                s.TurnOn();
            }
        }
    }

    void Update()
    {
        if (Services.PlayerBehaviour.state == PlayerState.Traversing)
        {
            SynthMaster.SetFloat("Distortion", Mathf.Clamp(0.5f - (Services.PlayerBehaviour.accuracy / 2f), 0, 0.9f));
        }
        else
        {
            SynthMaster.SetFloat("Distortion", 0);
        }
    }
    // Update is called once per frame
}
