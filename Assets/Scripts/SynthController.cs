using System.Collections;
using System.Collections.Generic;
using AudioHelm;
using UnityEngine;

public class SynthController : MonoBehaviour
{
    public HelmController movementSynth;

    public HelmController noiseySynth;
    // Start is called before the first frame update


    public List<HelmController> pads;
    public List<HelmController> keys;
    public bool hasStartedNoise;
    public static SynthController instance;
    private int[] notes = {60, 65, 70, 75};

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

        padNote = notes[Random.Range(0, notes.Length)];
        keys[0].NoteOn(padNote, Mathf.Clamp01(Services.PlayerBehaviour.flow), 1);
    }

    public void SwitchState(PlayerState s)
    {
	    LeaveState(Services.PlayerBehaviour.state);

	    switch (s)
	    {
		    case PlayerState.Traversing:

			    movementSynth.NoteOn(60);
					    
			    break;

		    case PlayerState.Flying:


			    break;

		    case PlayerState.Switching:

				

			    break;
	    }
    }

    public void LeaveState(PlayerState s)
		{
			
			
		}
    
    
    public void StopNote(int pad)
    {
        //pads[pad].NoteOff(padNote);
    }

    void Update()
    {
        float accuracy = Mathf.Clamp01(0.5f - Services.PlayerBehaviour.accuracy / 2);
        float accuracy2 = (accuracy - 0.75f);
       
        movementSynth.SetParameterValue(Param.kVolume, Mathf.Clamp01( Services.PlayerBehaviour.flow));
        movementSynth.SetParameterValue(Param.kOsc1Tune,  accuracy2 * 100f);
        
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
