﻿using System.Collections;
using System.Collections.Generic;
using AudioHelm;
using UnityEngine;

public class SynthController : MonoBehaviour
{
    public HelmController bassySynth;
    public HelmController noiseySynth;
    // Start is called before the first frame update


    public bool hasStartedNoise;
    public static SynthController instance;
    
    private bool a, b, c, d;
    // Update is called once per frame

    public void Start()
    {
        instance = this;
    }
    void Update()
    {
        float accuracy = Mathf.Clamp01(0.5f - Services.PlayerBehaviour.accuracy / 2);
        noiseySynth.SetParameterValue(Param.kVolume, Mathf.Clamp(Mathf.Lerp(0, Services.PlayerBehaviour.curSpeed + 1f, Services.PlayerBehaviour.decelerationTimer),0, 1f));
        noiseySynth.SetParameterValue(Param.kDistortionMix, Services.PlayerBehaviour.decelerationTimer + Services.PlayerBehaviour.boostTimer);

        if (!hasStartedNoise && Services.PlayerBehaviour.state == PlayerState.Traversing)
        {
            noiseySynth.FrequencyOn( 261.6f);
            hasStartedNoise = true;
        }
        
        
//            noiseySynth.SetParameterValue(Param.kDistortionMix, 1);

//        if (Services.PlayerBehaviour.curSpeed > 0.25f && !a)
//        {
//            bassySynth.FrequencyOn( 261.6f, 0.5f);
//            a = true;
//            
//        }else if ((Services.PlayerBehaviour.curSpeed < 0.25f || Services.PlayerBehaviour.boostTimer > 0) && a)
//        {
//            bassySynth.FrequencyOff(261.6f);
//            a = false;
//        }
//        
//        if (Services.PlayerBehaviour.curSpeed > 0.75f && !b)
//        {
//            bassySynth.FrequencyOn( 261.6f * 1.5f, 0.5f);
//            b = true;
//            
//        }else if ((Services.PlayerBehaviour.curSpeed < 0.75f || Services.PlayerBehaviour.boostTimer > 0) && b)
//        
//        {
//            bassySynth.FrequencyOff(261.6f * 1.5f);
//            b = false;
//        }
//        
//        if (Services.PlayerBehaviour.curSpeed > 1.5f && !c)
//        {
//            bassySynth.FrequencyOn( 261.6f * 1.5f * 1.5f, 0.5f);
//            c = true;
//            
//        }else if ((Services.PlayerBehaviour.curSpeed < 1.5f || Services.PlayerBehaviour.boostTimer > 0) && c)
//        {
//            bassySynth.FrequencyOff(261.6f * 1.5f * 1.5f);
//            c = false;
//        }
//        
//        if (Services.PlayerBehaviour.boostTimer > 0 && !d)
//        {
//            bassySynth.NoteOn(24, 1, 1);
//            d = true;
//            
//        }else if (Services.PlayerBehaviour.boostTimer == 0 && d)
//        {
//            bassySynth.NoteOff(24);
//            d = false;
//        }
    }
}