using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimation : MonoBehaviour
{
    public float framerate = 12;

    public Sprite[] frames;
    [SerializeField] private SpriteRenderer r;
    private int index;

    private float timer;
    // Update is called once per frame
    void Start()
    {
        timer = 1f / framerate;
        r.sprite = frames[0];
    }
    void Update()
    {
        
        if (Time.time > timer)
        {
            index++;
            if (index >= frames.Length)
            {
                index = 0;
            }
            
            r.sprite = frames[index];
            timer = Time.time + 1f / framerate;
        }
    }
}
