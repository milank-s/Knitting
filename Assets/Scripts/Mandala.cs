using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class Mandala : MonoBehaviour
{
    public GameObject image;
    public int angle = 10;
    public int loops = 1;

    public float imageChangeSpeed = 100;
    public float imageCycleSpeed = 10;
    public Sprite[] images;

    private float spriteChangeTimer;
    private float timeIndex;
    private float[] timers;
    private int[] imageIndexes;
    private float spriteIndex;
    public bool shutItDown = true;
    private int index
    {
        get
        {
            return (int) timeIndex;
        }
    }
    public Vector3 rotation;
    public List<Transform> transforms;
    
    public IEnumerator Start()
    {

        float timer = 0;
        
        Color c = Color.white;
        
        bool white = false;
        int copies = Mathf.Clamp(360 / angle,1, 100);
        timers = new float[copies * loops];
        imageIndexes = new int[copies * loops];
        
        for (int i = 0; i < copies * loops; i++)
        {
            GameObject newObject = Instantiate(image, transform);
            newObject.transform.position += Vector3.forward * i  * 5f;
            newObject.transform.Rotate(0, i * 2, angle * i);
            if (i % 10 == 0)
            {
                white = !white;
                
                if (white)
                {
                    c = Color.white;
                }else{
//                    c = Color.black;        
                }
            }

            transforms.Add(newObject.transform);
            transforms[i].GetComponent<Image>().color = new Color(1,1,1,(((float)i + 1)/timers.Length));
            yield return new WaitForSeconds(imageChangeSpeed);
        }
        
        image.gameObject.SetActive(false);
    }


    void Update()
    {

        
//        spriteIndex += Time.deltaTime * 10f;
//        if (index >= transforms.Count)
//        {
//            timeIndex = 0;    
//        }

        
        float coefficient = Mathf.Pow(Mathf.Sin(Time.time / 2f), 2);

        coefficient = 1;
        
        if (spriteChangeTimer < coefficient)
        {
            
            spriteChangeTimer = 2;
            timeIndex = 0;
        }

        spriteChangeTimer -= Time.deltaTime;
        
        for (int i = 0; i < transforms.Count;i++)
        {
            
                timers[i] -= Time.deltaTime;
            

            if (timers[i] < 0)
            {
                timers[i] = imageCycleSpeed;

                if (coefficient > 0.75f || imageIndexes[i] < spriteIndex)
                {
                    imageIndexes[i]++;

                    if (imageIndexes[i] > spriteIndex)
                    {
                        spriteIndex = imageIndexes[i];
                    }

                    if (!shutItDown)
                    {
                        transforms[i].GetComponent<Image>().sprite = images[imageIndexes[i] % images.Length];
                        transforms[i].GetComponent<Image>().enabled = true;
                    }
                    else
                    {
                        transforms[i].GetComponent<Image>().enabled = false;
                    }
                }
            }
            
            transforms[i].Rotate(rotation * Time.deltaTime * coefficient);
        }
    }
}
