using UdonSharp;
using System.Collections;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AugmentSpawner : UdonSharpBehaviour
{
    public GameObject[] upgradePrefabs;
    public float upgradeRespawnTime = 15.0f;
    private bool isHoldingUpgrade;

    // Allows player to pick up upgrade if they don't already have one
    private void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (!isHoldingUpgrade)
        {
            int upgradeIndex = Random.Range(0, upgradePrefabs.Length);
            upgradePrefabs[upgradeIndex].SetActive(false);
            SendCustomEvent("ApplyUpgrade", upgradeIndex);
            SendCustomEventDelayedSeconds("UpgradeRespawn", upgradeRespawnTime, upgradeIndex);
            Debug.Log("Upgrade picked up");
        }
    }

    // Respawns prefab after "upgradeRespawnTime" seconds of picking it up
    private void UpgradeRespawn(int upgradeIndex)
    {
        upgradePrefabs[upgradeIndex].SetActive(true);
    }

    // Spawns the prefabs for the first time
    private void Start()
    {
        for (int i = 0; i < upgradePrefabs.Length; i++)
        {
            upgradePrefabs[i].SetActive(false);
        }
        int upgradeIndex = Random.Range(0, upgradePrefabs.Length);
        upgradePrefabs[upgradeIndex].SetActive(true);
    }

    // Apply the upgrade
    private void ApplyUpgrade(int upgradeIndex)
    {
        switch (upgradeIndex)
        {
            case 0:
                // Apply Upgrade 1
                /* Ex:
                 Multiball multiball = gameObject.GetComponent<Multiball>();
              if (multiball != null)
              {
                multiball.Activate();
              }
              else
              {
                Debug.LogError("Multiball script not found");
              } */
                break;
            case 1:
                // Apply Upgrade 2
                break;
            case 2:
                // Apply Upgrade 3
                break;
            default:
                Debug.LogError("Invalid upgrade index!");
                break;
        }
    }
}
