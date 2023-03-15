﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;
public class SpawnBarricade : UdonSharpBehaviour
{
    public GameObject FloorMatUpTextured;
    public GameObject spawner;
    void update()
    {

        if (FloorMatUpTextured.activeInHierarchy==true)
        {

            spawner.SetActive(false);
        }

        else if (FloorMatUpTextured.activeInHierarchy == false)
        {

            spawner.SetActive(true);
        }

    }
    public void interact()
    {
        spawner.SetActive(false);
        FloorMatUpTextured.SetActive(true);
    }
}
