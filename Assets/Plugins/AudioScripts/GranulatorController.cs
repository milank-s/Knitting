using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class GranulatorController : MonoBehaviour
{
    public AudioMixer mixer;
    public float TimeScale = 1.0f;

    public static int sample;

    private float _sample
    {
        get { return (float) sample;  }

    }
    private float t0;

    private bool play;
    void Start()
    {
        t0 = Time.time;
    }

    void Update()
    {
        float val;
        float t = (Time.time - t0) * TimeScale;
        mixer.SetFloat("WindowLen", Mathf.Clamp(t * 0.1f, 0.5f, 0.5f));
        //mixer.SetFloat("Offset", 0.2f - 0.2f * Mathf.Cos(t * 0.03f));
        float rndOffset = 0.2f * (1.5f - Mathf.Sin(t * 0.05f));
        mixer.SetFloat("RndOffset", rndOffset);
        mixer.SetFloat("RndSpeed", rndOffset + 0.1f - 0.1f * Mathf.Cos(t * 0.04f) * Mathf.Cos(t * 0.37f));
        
        if(Input.GetKeyDown(KeyCode.Space))
        {
            play = !play;
            if (!play)
            {
                mixer.SetFloat("Volume", -50);
            }
            else
            {
                mixer.SetFloat("Volume", 0);
            }
        }
    }
}
