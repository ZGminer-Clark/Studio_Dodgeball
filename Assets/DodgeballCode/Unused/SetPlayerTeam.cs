
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SetPlayerTeam : UdonSharpBehaviour
{
    public bool isGameStarting;
    public string TeamName;
    public override void OnPlayerCollisionEnter(VRCPlayerApi player)
    {
        player.SetPlayerTag("Team", TeamName);
    }

    public override void OnPlayerCollisionExit(VRCPlayerApi player)
    {
        if (!isGameStarting)
        {
            player.SetPlayerTag("Team", null);
        }
    }
}
