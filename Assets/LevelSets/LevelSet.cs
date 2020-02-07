using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/LevelSet", order = 1)]
public class LevelSet : ScriptableObject
{
    public string title = "Stellation";
    public string description = "Good going";
    public Sprite image;
    
    public List<string> levels;
}
