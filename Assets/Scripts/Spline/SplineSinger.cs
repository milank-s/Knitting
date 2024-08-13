using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineSinger : MonoBehaviour
{

    [SerializeField] private AudioSource source;
    public bool isPlaying;
    
    public void Play()
    {
        source.Play();
        isPlaying = true;
    }   
    public void SetClipPlayback(float progress)
    {
        //source.time = (source.clip.length * progress)/source.clip.length;
        if (progress >= 1)
        {
            isPlaying = false;
        }
    }
}
