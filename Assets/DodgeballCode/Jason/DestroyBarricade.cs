using UdonSharp;
using VRC.Udon;
using VRC.SDKBase;
using UnityEngine;

public class DestroyBarricade : MonoBehaviour
{
    public int maxHealth = 3;
    public int currentHealth;
    public GameObject FloorMatUp;
    public GameObject FloorMatUpPlaceholder;
    public GameObject EmptyGameObjectSpawner;
    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;

       

        FloorMatUpPlaceholder.SetActive(false);

        Debug.Log("FloorMatUpPlaceholder is now off");
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


            FloorMatUpPlaceholder.SetActive(true);
          
            Debug.Log("You turned on your placeholder");
            currentHealth = maxHealth;
            FloorMatUp.SetActive(false);

          

          
        }

      

    }
   
}
