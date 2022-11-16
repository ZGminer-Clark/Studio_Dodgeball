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
        pickup = this.gameObject.GetComponent<VRC_Pickup>();
        if (pickup == null)
            Debug.Log("[testing] pickup was not properly set");
        else
            Debug.Log("[testing] pickup set");
    }

    //checks if the collision is the floor. if it is, change hitFloor to true.
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("[testing] OnCollisionEnter Called. Collided with " + collision.gameObject.name);
        if (collision.gameObject.name == "floor")
        {
            Debug.Log("[testing] ball hit floor (line 30)");
            hitFloor = true;
            Debug.Log("[testing] hitFloor = true (line 32)");
            hasEliminatedPlayer = false;
            Debug.Log("[testing] hasEliminatedPlayer = false (line 34)");
        }

       /* VRCPlayerApi temp = collision.gameObject.GetComponent(typeof(VRCPlayerApi));

        if ()
        {
            Debug.Log("[testing] OnPlayerCollisionEnter called on player ");
            if (!hitFloor && !hasEliminatedPlayer)
            {
                //if(player.GetPlayerTag("Team") != Thrower.GetPlayerTag("Team"))
                //{
                //playerHit = player;
                wasHit = true;
                Debug.Log("[testing] player  was hit.");
                //}
            }
        }*/
    }

    /*private void OnCollisionExit(Collision collision)
    {
        
    }*/

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
        Debug.Log("[testing] OnPickup() called (line 92)");

        Debug.Log("[testing] hitFloor = " + hitFloor + " (line 94)");
        if (hitFloor)
        {
            Debug.Log("[testing] hitFloor definately = true (line 97)");
            hitFloor = false;
            Debug.Log("[testing] hitFloor = false (line 99)");
            Thrower = pickup.currentPlayer;
            Debug.Log("[testing] Thrower is now player " + Thrower.playerId + " (line 101)");
        }
        else
        {
            Debug.Log("[testing] wasHit = "+ wasHit +" (line 105)");
            if (wasHit)
            {
                Debug.Log("[testing] wasHit definately = true (line 108)");
                wasHit = false;
                Debug.Log("[testing] wasHit is now false (line 110)");
            }

            Debug.Log("[testing] Thrower.playerId ("+ Thrower.playerId + ") != pickup.currentPlayer.playerId("+ pickup.currentPlayer.playerId + ")?  (line 113)");
            if (Thrower.playerId != pickup.currentPlayer.playerId)
            {
                Debug.Log("[testing] Thrower is not the current player (line 116)");
                Thrower.TeleportTo(outZone.position, outZone.rotation);
                Debug.Log("[testing] Teleport thrower to out (line 118)");
                Thrower = pickup.currentPlayer;
                Debug.Log("[testing] Previous thrower (player " + Thrower.playerId + ") sent to out.");
                Debug.Log("[testing] Thrower is now player " + Thrower.playerId);
            }
            else
            {
                Debug.Log("[testing] The same player caught the ball.");
            }
        }

        Debug.Log("[testing] OnPickup() finished");
    }
}
