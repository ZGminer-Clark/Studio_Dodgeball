using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnBarricade : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("respawner"))
        {
            gameObject.SetActive(false);
        }
    }
}
