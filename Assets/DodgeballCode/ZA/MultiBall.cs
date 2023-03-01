using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MultiBall : UdonSharpBehaviour
{
  public GameObject dodgeballPrefab; // Assign a prefab for the extra dodgeballs in the inspector
  public GameObject originalDodgeball;

  public void SpawnExtraDodgeballs(VRCPlayerApi player, Transform originalDodgeballPos)
  {
    if (player != null && originalDodgeballPos != null)
    {
      // Get the position of original ball
      Vector3 spawnPos = originalDodgeballPos.position;

      // Spawn two extra dodgeballs with a slight offset from the spawn position
      GameObject dodgeball1 = Object.Instantiate(dodgeballPrefab, spawnPos + Vector3.left * 0.5f + Vector3.forward * 2f, Quaternion.identity);
      GameObject dodgeball2 = Object.Instantiate(dodgeballPrefab, spawnPos + Vector3.right * 0.5f + Vector3.forward * 2f, Quaternion.identity);

      // Give the extra dodgeball's the original's force
      Rigidbody rb1 = dodgeball1.GetComponent<Rigidbody>();
      Rigidbody rb2 = dodgeball2.GetComponent<Rigidbody>();
      rb1.velocity = originalDodgeball.GetComponent<Rigidbody>().velocity;
      rb2.velocity = originalDodgeball.GetComponent<Rigidbody>().velocity;
    }
  }
}

/* 
MultiBall multiBallComponent = GetComponent<MultiBall>();
multiBallComponent.SpawnExtraDodgeballs(player, originalDodgeballPos);
*/