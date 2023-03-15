using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class GhostBall : MonoBehaviour
{
    public GameObject dodgeballPrefab;

    public void GiveDodgeball()
    {
        GameObject dodgeballInstance = VRCInstantiate(dodgeballPrefab);
    }
    //destroy if touches ground
}
