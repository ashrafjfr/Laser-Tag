using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class ARTrackingHandler : MonoBehaviour
{
    public bool canSeeEnemy = false;

    // This method will be connected to OnTargetFound event in the Unity Editor
    public void OnTargetFound()
    {
        canSeeEnemy = true;
    }

    // This method will be connected to OnTargetLost event in the Unity Editor
    public void OnTargetLost()
    {
        canSeeEnemy = false;
    }
    void Start()
    {
        
    }


void Update()
    {
        
    }
}
