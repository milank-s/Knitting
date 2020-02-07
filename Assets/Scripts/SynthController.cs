using System.Collections;
using System.Collections.Generic;
using AudioHelm;
using UnityEngine;

public class SynthController : MonoBehaviour
{
    public HelmController movementSynth;
    public HelmController noiseySynth;
    // Start is called before the first frame update


    public List<HelmController> synths;
    public bool hasStartedNoise;
    public static SynthController instance;
    private int[] notes = {60,65,70,75};
    
    private bool a, b, c, d;
    // Update is called once per frame

    public void Start()
    {
        instance = this;
        foreach (HelmController h in synths)
        {
            
            h.NoteOn(notes[Random.Range(0, notes.Length)], 1, 0.01f);
        }
    }

    public void PlayNote(int i)
    {
        float time = 0.8f;
        foreach (HelmController h in synths)
        {
            time -= 0.1f;
            h.NoteOn(notes[Random.Range(0, notes.Length)], 1, time);
        }
    }
    void Update()
    {
        float accuracy = Mathf.Clamp01(0.5f - Services.PlayerBehaviour.accuracy / 2);
        noiseySynth.SetParameterValue(Param.kVolume, Mathf.Clamp(Mathf.Lerp(0, Services.PlayerBehaviour.curSpeed + 1f, Services.PlayerBehaviour.decelerationTimer),0, 1f));
        noiseySynth.SetParameterValue(Param.kDistortionMix, Services.PlayerBehaviour.decelerationTimer + Services.PlayerBehaviour.boostTimer);

        movementSynth.SetParameterValue(Param.kVolume, Mathf.Clamp01(Mathf.Lerp(movementSynth.GetParameterValue(Param.kVolume),
            Services.PlayerBehaviour.curSpeed, Time.deltaTime * 3)));
        
        movementSynth.SetParameterValue(Param.kDistortionMix, Mathf.Clamp(Mathf.Lerp(0, Services.PlayerBehaviour.curSpeed + 1f, Services.PlayerBehaviour.decelerationTimer),0, 1f));
        
        if (!hasStartedNoise && Services.PlayerBehaviour.state == PlayerState.Traversing)
        {
            
            noiseySynth.FrequencyOn( 261.6f);
            movementSynth.FrequencyOn( 261.6f * 5f);
            
            hasStartedNoise = true;
        }
        
        
//            noiseySynth.SetParameterValue(Param.kDistortionMix, 1);

//        if (Services.PlayerBehaviour.curSpeed > 0.25f && !a)
//        {
//            movementSynth.FrequencyOn( 261.6f, 0.5f);
//            a = true;
//            
//        }else if ((Services.PlayerBehaviour.curSpeed < 0.25f || Services.PlayerBehaviour.boostTimer > 0) && a)
//        {
//            movementSynth.FrequencyOff(261.6f);
//            a = false;
//        }
//        
//        if (Services.PlayerBehaviour.curSpeed > 0.75f && !b)
//        {
//            movementSynth.FrequencyOn( 261.6f * 1.5f, 0.5f);
//            b = true;
//            
//        }else if ((Services.PlayerBehaviour.curSpeed < 0.75f || Services.PlayerBehaviour.boostTimer > 0) && b)
//        
//        {
//            movementSynth.FrequencyOff(261.6f * 1.5f);
//            b = false;
//        }
//        
//        if (Services.PlayerBehaviour.curSpeed > 1.5f && !c)
//        {
//            movementSynth.FrequencyOn( 261.6f * 1.5f * 1.5f, 0.5f);
//            c = true;
//            
//        }else if ((Services.PlayerBehaviour.curSpeed < 1.5f || Services.PlayerBehaviour.boostTimer > 0) && c)
//        {
//            movementSynth.FrequencyOff(261.6f * 1.5f * 1.5f);
//            c = false;
//        }
//        
//        if (Services.PlayerBehaviour.boostTimer > 0 && !d)
//        {
//            movementSynth.NoteOn(24, 1, 1);
//            d = true;
//            
//        }else if (Services.PlayerBehaviour.boostTimer == 0 && d)
//        {
//            movementSynth.NoteOff(24);
//            d = false;
//        }
    }
}
