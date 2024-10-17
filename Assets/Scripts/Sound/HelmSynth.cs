using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioHelm;
public class HelmSynth : MonoBehaviour
{
    public float volume = 1;
   public int octave = 0;
    public bool hasStutter = false;
    public bool hasTremelo = false;
    public bool hasDistortion = false;
    public HelmController patch;

    public void Start(){
        patch.SetParameterPercent(Param.kVolume, volume);
    }
   public void PlayNote(int note, float duration = 102, float velocity = 1){
        // SetVolume(velocity);
        note += octave * 12;
        if(duration > 100){
            patch.NoteOn(note, velocity);
        }else{
            patch.NoteOn(note, velocity, duration);
        }
   }

   public void Mute(bool b){

       float val = b ? 0 : 1;
       patch.SetParameterValue(Param.kVolume, val);
    //    if(b){
    //         Stop();
    //    }
   }

   public void Stop(){
        patch.AllNotesOff();
   }

    public void SetVolume(float f){
        patch.SetParameterPercent(Param.kVolume, f*volume);
    }

   public void Modulate(){

    //vibrate note based on speed;
    
    //none of this worked, just change bpm
    
    if(hasStutter){
        
        float val = patch.GetParameterValue(Param.kStutterResampleFrequency);
        val += Input.mouseScrollDelta.y/10f;
        patch.SetParameterValue(Param.kStutterResampleFrequency, val);
        
    //    Debug.Log("stutter tempo: " 
    //                     + patch.GetParameterValue(Param.kStutterResampleTempo)
    //                     + " percent: "
    //                     + patch.GetParameterPercent(Param.kStutterResampleTempo)
    //                     );

        // Debug.Log("stutter frequency: " 
        //                 + patch.GetParameterValue(Param.kStutterResampleFrequency)
        //                 + " percent: "
        //                 + patch.GetParameterPercent(Param.kStutterResampleFrequency)
        //                 );
        
    }
    if(hasTremelo){
        
    float val = patch.GetParameterPercent(Param.kPolyLfoFrequency);
       val += Input.mouseScrollDelta.y/10f;

      patch.SetParameterPercent(Param.kPolyLfoFrequency, val); 

    //     Debug.Log("LFO tempo: " 
    //                     + patch.GetParameterValue(Param.kPolyLfoTempo)
    //                     + " percent: "
    //                     + patch.GetParameterPercent(Param.kPolyLfoTempo)
    //                     );

        // Debug.Log("LFO frequency: " 
        //                 + patch.GetParameterValue(Param.kPolyLfoFrequency)
        //                 + " percent: "
        //                 + patch.GetParameterPercent(Param.kPolyLfoFrequency)
        //                 );
       
    }
   }
   
}
