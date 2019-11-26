using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Start is called before the first frame update

    public static AudioManager instance;

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

    // Update is called once per frame
}
