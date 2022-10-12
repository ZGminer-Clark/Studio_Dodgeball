
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;

public class RespawnObjectsScript : UdonSharpBehaviour
{
    public VRCObjectSync[] ObjectSyncs;



    public override void Interact()
    {
        for(int i = 0; i < ObjectSyncs.Length; i++)
        {
            Networking.SetOwner(Networking.LocalPlayer, ObjectSyncs[i].gameObject);
            ObjectSyncs[i].Respawn();
        }
    }
}
