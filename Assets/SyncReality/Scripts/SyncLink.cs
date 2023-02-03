using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using System;

 

public class SyncLink : MonoBehaviour
{
    public string ID;
    public Sync parentSync; // = this.Getcomponent<Sync>();
    public GameObject linkedObject; // Sync or SurroundSync
    public bool linkedObjectIsSurroundSync = false;  

    public float minDistance;
    public float preferredDistance;
    public float maxDistance;
    public bool topOf;
    public bool bottomOf;
    public bool rotationConstraint;
    public bool rotationOutside;
    public float minAngle; 
    public float maxAngle;

    public Vector3 anchorVector; // relative to bounding box
    public Vector3 offsetVector; // relative to bounding box
    public bool scaleWithSync;
    public bool onEveryClutter = false;
     
    
  

    public SyncLinkData GetSyncLinkData()
    {
        return new SyncLinkData(this);
    }
}
