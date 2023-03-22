using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class GhostBall : UdonSharpBehaviour
{
  public GameObject dodgeballPrefab;

  public void GiveGhostball()
  {
    GameObject dodgeballInstance = Object.Instantiate(dodgeballPrefab);
  }
    //destroy if touches ground
}
