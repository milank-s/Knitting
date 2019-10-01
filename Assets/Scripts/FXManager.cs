using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXManager : MonoBehaviour
{
    public enum FXType{fizzle, burst, rotate, pulse, cross}

    [SerializeField] private GameObject fxPrefab;
  private int index;
  private List<Animator> fxInstances = new List<Animator>();
 
  void Start()
  {
      for (int i = 0; i < 12; i++)
      {
          GameObject newFX = Instantiate(fxPrefab, Vector3.up * 1000, Quaternion.identity);
          fxInstances.Add(newFX.GetComponent<Animator>());   
      }
  }


  public void PlayAnimationAtPosition(FXType type, Transform t)
  {
      Animator fx = fxInstances[index];
      fx.SetTrigger(type.ToString());
      fx.transform.parent = t;
      fx.transform.position = t.position;
      index = (index + 1) % fxInstances.Count;
  }
  
}
