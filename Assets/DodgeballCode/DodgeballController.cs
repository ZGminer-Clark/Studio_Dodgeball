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

    //checks if the collision is the floor. if it is, change hitFloor to true.
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "floor")
        {
            hitFloor = true;
            hasEliminatedPlayer = false;
        }
    }

    // when the ball collides with a player, check if the ball collided with the floor or has eliminated a player already.
    // if neither of those are true, set wasHit to true and store the player as playerHit.
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

    //when the ball stops colliding with a player, check if the player is marked as hit. if they are, move them to the outZone
    //and set hasEliminatedPlayer to true.
    public override void OnPlayerCollisionExit(VRCPlayerApi player)
    {
        if(wasHit)
        {
            playerHit.TeleportTo(outZone.position, outZone.rotation);
            hasEliminatedPlayer = true;
        }
    }

    //when the player picks up the ball check if the player is picking it up off the ground or catching it.
    //if it was on the ground, set the player as the thrower and set hitFloor to false.
    //if it was caught, send the previous player to the out zone and set the new player as the thrower.
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
