using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PowerUps : UdonSharpBehaviour
{
    public void ActivateMultiBall(VRCPlayerApi player, Transform originalDodgeballPos)
    {
     // MultiBall.SpawnExtraDodgeballs(player, originalDodgeballPos, originalDodgeball, dodgeballPrefab);
    }

    // Add more power up methods as needed
}


//SendCustomEvent for node->script