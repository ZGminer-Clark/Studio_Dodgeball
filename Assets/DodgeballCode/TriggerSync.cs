
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TriggerSync : UdonSharpBehaviour
{

    public GameObject changeThis; //the object being changed

    [UdonSynced] bool isON;
    
    public override void Interact()
    {

        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "turnONorOFF");
    }

    public void turnONorOFF()
    {
        isON = !changeThis.activeSelf;
        changeThis.SetActive(isON);
    }

    public override void OnDeserialization()
    {
        changeThis.SetActive(isON);
    }
}
