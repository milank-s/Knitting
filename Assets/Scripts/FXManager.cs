using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXManager : MonoBehaviour
{
    public enum FXType{fizzle, burst, rotate, pulse, cross}

    public ParticleSystem popParticles, flyingParticles, speedParticles;
    public TrailRenderer playerTrail, flyingTrail;
    public MeshFilter flyingParticleMesh, flyingParticleTrailMesh, flyingTrailMesh, playerTrailMesh, brakeParticleMesh;
    
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
      
      flyingParticleMesh.mesh = new Mesh();
      flyingParticleTrailMesh.mesh = new Mesh();
      flyingTrailMesh.mesh = new Mesh();
      playerTrailMesh.mesh = new Mesh();
      brakeParticleMesh.mesh = new Mesh();
  }


  public void BakeTrail(TrailRenderer t, MeshFilter f)
  {
      t.emitting = false;
      Mesh m = new Mesh();
      Mesh m2 = f.mesh;
      CombineInstance[] c = new CombineInstance[2];    
     t.BakeMesh(m);
      c[0].mesh = m;    
      c[1].mesh = f.mesh;
        f.mesh = new Mesh();
      //c[0].transform = flyingParticles.transform.localToWorldMatrix;
      f.mesh.CombineMeshes(c, true, false);
     t.Clear();
  }
  
  public void BakeParticleTrail(ParticleSystem p, MeshFilter f)
  {
      p.Pause();
      Mesh m = new Mesh();
      Mesh m2 = f.mesh;
      CombineInstance[] c = new CombineInstance[2];    
      p.GetComponent<ParticleSystemRenderer>().BakeTrailsMesh(m);
      c[0].mesh = m;
      c[1].mesh = f.mesh;
      f.mesh = new Mesh();
      //c[0].transform = flyingParticles.transform.localToWorldMatrix;
      f.mesh.CombineMeshes(c, true, false);
  }
  
  

  public void BakeParticles(ParticleSystem p, MeshFilter f)
  {
      p.Pause();
      Mesh m = new Mesh();
      Mesh m2 = f.mesh;
      CombineInstance[] c = new CombineInstance[2];    
      p.GetComponent<ParticleSystemRenderer>().BakeMesh(m);
      c[0].mesh = m;
      c[1].mesh = f.mesh;
      f.mesh = new Mesh();
      //c[0].transform = flyingParticles.transform.localToWorldMatrix;
      f.mesh.CombineMeshes(c, true, false);
      p.Clear();
  }

  public void PlayAnimationAtPosition(FXType type, Transform t)
  {
      Animator fx = fxInstances[index];
      fx.SetTrigger(type.ToString());
      fx.transform.position = t.position;
      index = (index + 1) % fxInstances.Count;
  }

  public void PlayAnimationOnPlayer(FXType type)
  {
      Animator fx = fxInstances[index];
      fx.SetTrigger(type.ToString());

      fx.transform.position = Services.Player.transform.position;
      fx.transform.up = Services.Player.transform.up;
      index = (index + 1) % fxInstances.Count;
  }

  public void EmitRadialBurst(int i, float force, Transform t)
  {
      ParticleSystem.MainModule m = popParticles.main;
      m.startSpeedMultiplier = force;
      popParticles.transform.position = t.position;
      popParticles.Emit(i);
  }

  public void EmitLinearBurst(int i, float force, Transform t, Vector3 direction)
  {
      ParticleSystem.MainModule m = speedParticles.main;
      m.startSpeedMultiplier = force;
      speedParticles.transform.position = t.position;
      speedParticles.transform.up = direction;
      speedParticles.Emit(i);
  }
  
 
  
}
