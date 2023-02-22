using UdonSharp;
using VRC.Udon;
using VRC.SDKBase;
using UnityEngine;

public class DestroyBarricade : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;
    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;

    }


    // Update is called once per frame

    private void OnTriggerEnter(Collider other)
    {

     
        if (other.gameObject.CompareTag("damage"))
        {
            currentHealth--;
        }

        if (currentHealth <= 0)
        {

            other.gameObject.CompareTag("respawner");

            gameObject.SetActive(true);

            gameObject.SetActive(false);

          
        }

      

    }
}
