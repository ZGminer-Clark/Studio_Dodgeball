using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DodgeballController : UdonSharpBehaviour
{
    public bool hitFloor = true;
    public bool hasEliminatedPlayer = false;   //ensures each throw can only knock out one person
    public VRC_Pickup pickup;

    public bool wasHit = false;
    public VRCPlayerApi Thrower;
    public VRCPlayerApi playerHit;

    public Transform outZone;

    private void Start()
    {
        //pickup = this.gameObject.GetComponent<VRC_Pickup>();
    }

    //checks if the collision is the floor. if it is, change hitFloor to true.
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("[testing] OnCollisionEnter Called");
        Debug.Log("[testing] Collided with " + collision.gameObject.name);
        if (collision.gameObject.name == "floor")
        {
            Debug.Log("[testing] ball hit floor");
            hitFloor = true;
            hasEliminatedPlayer = false;
        }
    }

    // when the ball collides with a player, check if the ball collided with the floor or has eliminated a player already.
    // if neither of those are true, set wasHit to true and store the player as playerHit.
    public override void OnPlayerCollisionEnter(VRCPlayerApi player)
    {
        Debug.Log("[testing] OnPlayerCollisionEnter called on player " + player.playerId);
        if (!hitFloor && !hasEliminatedPlayer)
        {
            //if(player.GetPlayerTag("Team") != Thrower.GetPlayerTag("Team"))
            //{
                playerHit = player;
                wasHit = true;
                Debug.Log("[testing] player " + player.playerId + "was hit.");
            //}
        }
    }

    //when the ball stops colliding with a player, check if the player is marked as hit. if they are, move them to the outZone
    //and set hasEliminatedPlayer to true.
    public override void OnPlayerCollisionExit(VRCPlayerApi player)
    {
        Debug.Log("[testing] OnPlayerCollisionExit called on player "+ player.playerId);
        if (wasHit && playerHit != null)
        {
            playerHit.TeleportTo(outZone.position, outZone.rotation);
            playerHit = null;
            hasEliminatedPlayer = true;
            Debug.Log("[testing] Player " + playerHit.playerId + " sent to out.");
        }
    }

    //when the player picks up the ball check if the player is picking it up off the ground or catching it.
    //if it was on the ground, set the player as the thrower and set hitFloor to false.
    //if it was caught, send the previous player to the out zone and set the new player as the thrower.
    public override void OnPickup()
    {
        Debug.Log("[testing] OnPickup() called");
        if(hitFloor)
        {
            hitFloor = false;
            Thrower = pickup.currentPlayer;
            Debug.Log("[testing] Thrower is now player " + Thrower.playerId);
        }
        else
        {
            if (wasHit)
            {
                wasHit = false;
                Debug.Log("[testing] wasHit is now false");
            }

            if (Thrower.playerId != pickup.currentPlayer.playerId)
            {
                Thrower.TeleportTo(outZone.position, outZone.rotation);
                Thrower = pickup.currentPlayer;
                Debug.Log("[testing] Previous thrower (player " + Thrower.playerId + ") sent to out.");
                Debug.Log("[testing] Thrower is now player " + Thrower.playerId);
            }
            else
            {
                Debug.Log("[testing] The same player caught the ball.");
            }
        }
    }
}
