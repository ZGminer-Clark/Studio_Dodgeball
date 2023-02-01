
using UdonSharp;
using System.Collections;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class augmentspawnercs : UdonSharpBehaviour
{
    public GameObject upgradePrefabs;
    public float upgradeRespawnTime = 15.0f;
    private bool isHoldingUpgrade;
    
    //Allows player to pick up upgrade if they don't already have one
    private void OnPlayerTriggerEnter(VRCPlayerApi player) {
        if (!isHoldingUpgrade) {
            upgradePrefabs.SetActive(false);
            SendCustomEventDelayedSeconds("upgradeRespawn", upgradeRespawnTime);
            //TODO: Implement upgrade
            Debug.Log("Upgrade picked up");
        }
    }

    //Respawns prefab after "upgradeRespawnTime" seconds of picking it up
    private void upgradeRespawn() {
        upgradePrefabs.SetActive(true);
    }

    //Spawns the prefabs for the first time *Random Selection Required*
    private void Start() {
        upgradeRespawn();
    } 
}

    //have delay before powerups spawn