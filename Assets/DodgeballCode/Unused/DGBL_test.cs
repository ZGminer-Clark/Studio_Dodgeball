using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DGBL_test : UdonSharpBehaviour
{
    private SphereCollider colliderSphere;

    public Transform outZone;


    // disable collider on pickup and reenable on drop
    public override void OnPickup()
    {
        base.OnPickup();
        {
            colliderSphere.enabled = !colliderSphere.enabled;
        }
    }
    public override void OnDrop()
    {
        base.OnDrop();
        {
            colliderSphere.enabled = !colliderSphere.enabled;
        }
    }
    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {

    }
}
