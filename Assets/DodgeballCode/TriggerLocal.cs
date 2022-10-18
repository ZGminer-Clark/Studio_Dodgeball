
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TriggerLocal : UdonSharpBehaviour
{

    public GameObject changeThis;

    public override void Interact()
    {
        changeThis.SetActive(!changeThis.activeSelf);
    }
}
