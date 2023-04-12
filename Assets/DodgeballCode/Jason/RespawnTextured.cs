using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;


public class RespawnTextured : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject FloorMatUpSpawner;
    public GameObject FloorMatUpTextured;
    


    void Start()
    {
        
    }

    // Update is called once per frame
   
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entered Spawn Zone");

        if (Input.GetMouseButton(0))
        {
            Debug.Log("Pressed primary button.");
            FloorMatUpTextured.SetActive(true);
            
           

            Debug.Log("It Worked");

            FloorMatUpSpawner.SetActive(false);
        }

        Debug.Log("Its null bitch");
    }

}
