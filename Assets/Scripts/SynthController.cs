using System.Collections;
using System.Collections.Generic;
using AudioHelm;
using UnityEngine;
using UnityEngine.Audio;

public class SynthController : MonoBehaviour
{
    public HelmController movementSynth;

    public HelmController noiseySynth;
    public HelmController flyingSynth;
    // Start is called before the first frame update

    public AudioMixer synths;
    

    public List<HelmController> pads;
    public List<HelmController> keys;
    public bool hasStartedNoise;
    public static SynthController instance;
    private int[] notes = {60, 65, 70, 75, 80, 82, 90};

    private bool a, b, c, d;

    private int padNote;

    private int keyNote;
    // Update is called once per frame

    public void Start()
    {
        instance = this;

    }

    public void PlayNote(int i)
    {
        //how many should play?
        //which note and which patch?
        //how do you modulate the frequency once the note is being played?

        StopNote(i);

        padNote = 80;
        pads[0].NoteOn(padNote, 1, 0.05f);
    }

    public void SwitchState(PlayerState s)
    {
	    LeaveState(Services.PlayerBehaviour.state);

	    switch (s)
	    {
		    case PlayerState.Traversing:

			    movementSynth.NoteOn(32);
			    noiseySynth.NoteOn(50, 1);
			    break;

		    case PlayerState.Flying:
			    synths.SetFloat("Attenuation", -80);
			    flyingSynth.NoteOn(60, 1);
			    break;

		    case PlayerState.Switching:

				

			    break;
	    }
    }

    public void LeaveState(PlayerState s)
		{
			switch (s)
			{
				case PlayerState.Traversing:

					movementSynth.NoteOff(32);
					noiseySynth.NoteOff(50);
					break;

				case PlayerState.Flying:
					synths.SetFloat("Attenuation", 0);
					flyingSynth.NoteOff(60);
					break;

				case PlayerState.Switching:
					break;
			}
			
		}
    
    
    public void StopNote(int pad)
    {
        //pads[pad].NoteOff(padNote);
    }

    void Update()
    {
        float accuracy = Mathf.Clamp01(0.5f - Services.PlayerBehaviour.accuracy / 2);
        float accuracy2 = (accuracy - 0.75f);
	
        
        noiseySynth.SetParameterValue(Param.kVolume, Mathf.Lerp(0, Mathf.Clamp01( 1- Services.PlayerBehaviour.accuracy) * Mathf.Clamp01(Mathf.Pow(Services.PlayerBehaviour.flow,2)), Services.PlayerBehaviour.decelerationTimer));
		movementSynth.SetParameterValue(Param.kOsc2Tune,  accuracy2 * 100f);
		movementSynth.SetParameterValue(Param.kVolume,  Mathf.Lerp(movementSynth.GetParameterValue(Param.kVolume), Mathf.Clamp01(Services.PlayerBehaviour.flow - 0.25f) / 2f, Time.deltaTime));
//        if (!hasStartedNoise && Services.PlayerBehaviour.state == PlayerState.Traversing)
//        {
//            
//            movementSynth.FrequencyOn( 261.6f * 5f);
//            
//            hasStartedNoise = true;
//        }


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
