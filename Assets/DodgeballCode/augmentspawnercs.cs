using UdonSharp;
using System.Collections;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AugmentSpawner : UdonSharpBehaviour
{
  public GameObject[] upgradePrefabs;
  public Transform[] spawners;
  public bool holdingUpgrade;
  public float upgradeRespawnTime = 15.0f;
  private int[] activeUpgradeIndex;
  private float[] upgradeRespawnTimers;
  
  private void Start()
  {
    activeUpgradeIndex = new int[spawners.Length];
    upgradeRespawnTimers = new float[spawners.Length];

    for (int i = 0; i < upgradePrefabs.Length; i++)
    {
      upgradePrefabs[i].SetActive(false);
    }

    for (int i = 0; i < spawners.Length; i++)
    {
      int upgradeIndex = Random.Range(0, upgradePrefabs.Length);
      activeUpgradeIndex[i] = upgradeIndex;
      upgradePrefabs[upgradeIndex].SetActive(true);
    }
  }

  private void Update()
  {
    for (int i = 0; i < upgradeRespawnTimers.Length; i++)
    {
      if (upgradeRespawnTimers[i] > 0.0f)
      {
        upgradeRespawnTimers[i] -= Time.deltaTime;
        if (upgradeRespawnTimers[i] <= 0.0f)
        {
          SpawnNewUpgrade(i);
        }
      }
    }
  }

  private void OnPlayerTriggerEnter(VRCPlayerApi player, int spawnerIndex)
  {
    if (holdingUpgrade)
    {
      return;
    }

    if (upgradePrefabs[activeUpgradeIndex[spawnerIndex]].activeSelf)
    {
      upgradePrefabs[activeUpgradeIndex[spawnerIndex]].SetActive(false);
      SendCustomEvent(ApplyUpgrade(spawnerIndex));
      holdingUpgrade = true;
      upgradeRespawnTimers[spawnerIndex] = upgradeRespawnTime;
      Debug.Log("Upgrade picked up");
    }
  }

  private void SpawnNewUpgrade(int spawnerIndex)
  {
    int upgradeIndex = Random.Range(0, upgradePrefabs.Length);
    activeUpgradeIndex[spawnerIndex] = upgradeIndex;
    upgradePrefabs[upgradeIndex].SetActive(true);
  }

  private void ApplyUpgrade(int spawnerIndex)
  {
    switch (activeUpgradeIndex[spawnerIndex])
    {
      case 0:
        // Apply Upgrade 1 to the next thrown ball
        break;
      case 1:
        // Apply Upgrade 2 to the next thrown ball
        break;
      case 2:
        // Apply Upgrade 3 to the next thrown ball
        break;
      default:
        Debug.LogError("Invalid upgrade index!");
        break;
    }
  }
}
