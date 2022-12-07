using UdonSharp;
using UnityEngine;
using System;
using VRC.SDKBase;
using VRC.Udon;

public class DodgeballController : UdonSharpBehaviour
{
    private bool hitFloor = true;
    private bool hasEliminatedPlayer = false;   //ensures each throw can only knock out one person
    private bool isBeingHeld = false;

    public VRC_Pickup pickup;

    public bool wasHit = false;
    public VRCPlayerApi Thrower;
    public VRCPlayerApi playerHit;

    public Transform outZone;

    private void Start()
    {
        if (pickup == null)
        {
            pickup = this.gameObject.transform.GetChild(0).GetComponent<VRC_Pickup>();
            Debug.Log("[testing] pickup set. (line 27)");
        }
    }

    //checks if the collision is the floor. if it is, change hitFloor to true.
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("[testing] OnCollisionEnter Called. Collided with " + collision.gameObject.name +". isBeingHeld = " + isBeingHeld+ ". (line 33)");
        if (collision.gameObject.name == "floor" && !isBeingHeld)
        {
            Debug.Log("[testing] ball hit floor and isnt being held (line 36)");
            hitFloor = true;
            Debug.Log("[testing] hitFloor = true (line 38)");
            hasEliminatedPlayer = false;
            Debug.Log("[testing] hasEliminatedPlayer = false (line 40)");
        }

        /*Debug.Log("[testing] was it a player? hitFloor = "+ hitFloor +" and hasEliminatedPlayer = "+ hasEliminatedPlayer +". (line 43)");
        if (!hitFloor && !hasEliminatedPlayer)
        {
            Debug.Log("[testing] since both are false... (line 46)");
            try
            {
                Debug.Log("[testing] try getting a VRCPLayerApi component from the collision... (line 49)");
                VRCPlayerApi temp = collision.gameObject.GetComponent<VRCPlayerApi>();
                playerHit = temp;
                wasHit = true;
                Debug.Log("[testing] playerHit ID = " +playerHit.playerId + ". Set WasHit to " + wasHit + ". (line 53)");
            }
            catch (NullReferenceException ex)
            {
                Debug.Log("there is no VRCPlayerApi attatched to this object. (line 57)");
            }
        }*/
    }

    /*private void OnCollisionExit(Collision collision)
    {
        Debug.Log("[testing] OnCollisionExit called. leaving " + collision.gameObject.name +". (line 64)");

        if (wasHit && playerHit != null)
        {
            try
            {
                VRCPlayerApi temp = collision.gameObject.GetComponent<VRCPlayerApi>();
                if (temp.playerId == playerHit.playerId)
                {
                    Debug.Log("[testing] playerHit was hit and is not null. (line 68)");
                    playerHit.TeleportTo(outZone.position, outZone.rotation);
                    Debug.Log("[testing] playerHit set to out. (line 70)");
                    playerHit = null;
                    Debug.Log("[testing] playerHit = null. (line 72)");
                    hasEliminatedPlayer = true;
                    Debug.Log("[testing] hasEliminatedPlayer = true. (line 74)");
                }
            }
            catch (NullReferenceException ex)
            {
                Debug.Log("there is no VRCPlayerApi attatched to this object.");
            }
        }
    }*/

        // when the ball collides with a player, check if the ball collided with the floor or has eliminated a player already.
        // if neither of those are true, set wasHit to true and store the player as playerHit.
    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
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
    public override void OnPlayerTriggerExit(VRCPlayerApi player)
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
        Debug.Log("[testing] OnPickup() called (line 113)");
        isBeingHeld = true;

        Debug.Log("[testing] hitFloor = " + hitFloor + " (line 116)");
        if (hitFloor)
        {
            Debug.Log("[testing] hitFloor definately = true (line 119)");
            hitFloor = false;
            Debug.Log("[testing] hitFloor = false (line 121)");
            Thrower = pickup.currentPlayer;
            Debug.Log("[testing] Thrower is now player " + Thrower.playerId + " (line 123)");
        }
        else
        {
            Debug.Log("[testing] wasHit = "+ wasHit +" (line 127)");
            if (wasHit)
            {
                Debug.Log("[testing] wasHit definately = true (line 130)");
                wasHit = false;
                Debug.Log("[testing] wasHit is now false (line 132)");
            }

            Debug.Log("[testing] Thrower.playerId ("+ Thrower.playerId + ") != pickup.currentPlayer.playerId("+ pickup.currentPlayer.playerId + ")?  (line 135)");
            if (Thrower.playerId != pickup.currentPlayer.playerId)
            {
                Debug.Log("[testing] Thrower is not the current player (line 138)");
                Thrower.TeleportTo(outZone.position, outZone.rotation);
                Debug.Log("[testing] Teleport thrower to out (line 140)");
                Thrower = pickup.currentPlayer;
                Debug.Log("[testing] Previous thrower (player " + Thrower.playerId + ") sent to out. (line 142)");
                Debug.Log("[testing] Thrower is now player " + Thrower.playerId + " (line 143)");
            }
            else
            {
                Debug.Log("[testing] The same player caught the ball. (line 147)");
            }
        }

        Debug.Log("[testing] OnPickup() finished");
    }

    public override void OnDrop()
    {
        isBeingHeld = false;
        Debug.Log("[testing] isBeingHeld = false (line 157)");
    }
}
