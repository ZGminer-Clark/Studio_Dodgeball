using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnBarricade : MonoBehaviour
{
    // Start is called before the first frame update
    void Start(Collider other)
    {
               other.gameObject.CompareTag("respawner");

               gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
       
    }
}
