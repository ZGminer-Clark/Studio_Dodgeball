using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DodgeballController : UdonSharpBehaviour
{
    private bool hitFloor = true;               
    private bool hasEliminatedPlayer = false;   //ensures each throw can only knock out one person
    private VRC_Pickup pickup;

    public bool wasHit = false;
    public VRCPlayerApi Thrower;
    private VRCPlayerApi playerHit;

    public Transform outZone;

    private void Start()
    {
        pickup = this.gameObject.GetComponent<VRC_Pickup>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "floor")
        {
            hitFloor = true;
            hasEliminatedPlayer = false;
        }
    }

    public override void OnPlayerCollisionEnter(VRCPlayerApi player)
    {
        if (!hitFloor && !hasEliminatedPlayer)
        {
            //if(player.GetPlayerTag("Team") != Thrower.GetPlayerTag("Team"))
            //{
                playerHit = player;
                wasHit = true;
            //}
        }
    }

    public override void OnPlayerCollisionExit(VRCPlayerApi player)
    {
        if(wasHit)
        {
            playerHit.TeleportTo(outZone.position, outZone.rotation);
            hasEliminatedPlayer = true;
        }
    }

    public override void OnPickup()
    {
        if(hitFloor)
        {
            hitFloor = false;
            Thrower = pickup.currentPlayer;
        }
        else
        {
            if (wasHit)
                wasHit = false;
            Thrower.TeleportTo(outZone.position, outZone.rotation);
            Thrower = pickup.currentPlayer;
        }
    }
}
