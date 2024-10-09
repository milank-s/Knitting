using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    public MenuController menu;
    
    bool menuOpen;

    void Start()
    {
        
    }

    public void Enter(){
        if(!menuOpen){
            //turn on screen
        }
    }
    
    public void Escape(){
        if(menuOpen){
            //turn off screen

        }else{
            Application.Quit();
        }
    }

    //turn dial on oscilloscope
    //select first level

    //escape turns off display
    //escape again quits

}
