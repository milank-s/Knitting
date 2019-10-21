using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;
using UnityEngine.UI;

public class FXManager : MonoBehaviour
{
    public enum FXType{fizzle, burst, rotate, pulse, cross}

    public ParticleSystem popParticles, flyingParticles, speedParticles, trailParticles;
    public TrailRenderer playerTrail, flyingTrail;
    public MeshFilter flyingParticleMesh, flyingParticleTrailMesh, flyingTrailMesh, playerTrailMesh, brakeParticleMesh;
    public GameObject MeshPrefab;
    public Text readout;
    
    private int lineIndex;
    private VectorLine line;
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
      
      
      line = new VectorLine (name, new List<Vector3> (20), 2, LineType.Continuous, Vectrosity.Joins.Weld);
      line.color = new Color(1,1,1,0.25f);
      line.smoothWidth = true;
      line.smoothColor = true;

      flyingParticleMesh.mesh = new Mesh();
      flyingParticleTrailMesh.mesh = new Mesh();
      flyingTrailMesh.mesh = new Mesh();
      playerTrailMesh.mesh = new Mesh();
      brakeParticleMesh.mesh = new Mesh();
      
  }

  public void Reset()
  {
      flyingParticleMesh.mesh = new Mesh();
      flyingParticleTrailMesh.mesh = new Mesh();
      flyingTrailMesh.mesh = new Mesh();
      playerTrailMesh.mesh = new Mesh();
      brakeParticleMesh.mesh = new Mesh();
      
      VectorLine.Destroy(ref line);
      
      line = new VectorLine (name, new List<Vector3> (20), 2, LineType.Continuous, Vectrosity.Joins.Weld);
      line.color = new Color(1,1,1,0.25f);
      line.smoothWidth = true;
      line.smoothColor = true;
      
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
  
  public void EmitTracerBurst(int i, float force, Transform t)
  {
      ParticleSystem.MainModule m = trailParticles.main;
      trailParticles.transform.position = t.position;
      trailParticles.Emit(i);
  }

  public void EmitLinearBurst(int i, float force, Transform t, Vector3 direction)
  {
      ParticleSystem.MainModule m = speedParticles.main;
      m.startSpeedMultiplier = force;
      speedParticles.transform.position = t.position;
      speedParticles.transform.up = direction;
      speedParticles.Emit(i);
  }

  public void DrawLine()
  {
      lineIndex += (int)(50 * Time.deltaTime);
      List<Vector3> pos = new List<Vector3>();
      Vector3[] positions = new Vector3[playerTrail.positionCount];
      playerTrail.GetPositions(positions);
      int indices = Mathf.Clamp(playerTrail.positionCount, 0, 50);
      for (int i = 0; i < indices; i++)
      {
          if (Random.Range(0, 100) < 100)
          {
              pos.Add(positions[(indices + lineIndex - i)%playerTrail.positionCount] +  (Vector3) Random.insideUnitCircle /100);
//             
          }
      } 
      line.points3 = pos;
      line.Draw3D();
  }
 
  
}
