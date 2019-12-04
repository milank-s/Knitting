using System.Collections;
using System.Collections.Generic;
using AudioHelm;
using UnityEngine;

public class SynthController : MonoBehaviour
{
    public HelmController synth;
    // Start is called before the first frame update


    private bool a, b, c, d;
    // Update is called once per frame

    public void Start()
    {
       synth.SetPolyphony(5);
    }
    void Update()
    {
        if (Services.PlayerBehaviour.curSpeed > 0.25f && !a)
        {
            synth.FrequencyOn( 261.6f, 0.5f);
            a = true;
            
            
        }else if ((Services.PlayerBehaviour.curSpeed < 0.25f || Services.PlayerBehaviour.boostTimer > 0) && a)
        {
            synth.FrequencyOff(261.6f);
            a = false;
        }
        
        if (Services.PlayerBehaviour.curSpeed > 0.75f && !b)
        {
            synth.FrequencyOn( 261.6f * 1.5f, 0.5f);
            b = true;
            
        }else if ((Services.PlayerBehaviour.curSpeed < 0.75f || Services.PlayerBehaviour.boostTimer > 0) && b)
        
        {
            synth.FrequencyOff(261.6f * 1.5f);
            b = false;
        }
        
        if (Services.PlayerBehaviour.curSpeed > 1.5f && !c)
        {
            synth.FrequencyOn( 261.6f * 1.5f * 1.5f, 0.5f);
            c = true;
            
        }else if ((Services.PlayerBehaviour.curSpeed < 1.5f || Services.PlayerBehaviour.boostTimer > 0) && c)
        {
            synth.FrequencyOff(261.6f * 1.5f * 1.5f);
            c = false;
        }
        
        if (Services.PlayerBehaviour.boostTimer > 0 && !d)
        {
            synth.NoteOn(24, 1, 1);
            d = true;
            
        }else if (Services.PlayerBehaviour.boostTimer == 0 && d)
        {
            synth.NoteOff(12);
            d = false;
        }
    }
}
