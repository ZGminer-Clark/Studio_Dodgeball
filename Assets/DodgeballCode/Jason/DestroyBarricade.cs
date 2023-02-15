using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyBarricade : MonoBehaviour
{
    public int healthAmount = 3;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("damage"))
        {
            healthAmount = healthAmount - 1;
         
        }
        if (healthAmount <= 0)
            {
            other.gameObject.SetActive(false);
        }
    }
}
