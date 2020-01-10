using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class Mandala : MonoBehaviour
{
    public GameObject image;
    public int angle = 10;
    void Start()
         {
             Initialize();
         }

    void Initialize()
    {
        Color c = Color.white;
        bool white = false;
        int copies = Mathf.Clamp(360 / angle,1, 100);
        
        for (int i = 0; i <= copies; i++)
        {
            GameObject newObject = Instantiate(image, transform);
            newObject.transform.Rotate(0, i * 2, angle * i);
            if (i % 10 == 0)
            {
                white = !white;
                
                if (white)
                {
                    c = Color.white;
                }else{
                    c = Color.black;        
                }
            }
            
            newObject.GetComponent<Image>().color = c;
        }
    }
    
}
