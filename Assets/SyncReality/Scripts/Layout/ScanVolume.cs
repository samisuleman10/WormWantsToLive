using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


 

// abstracted object with geometry and anchoring
[Serializable]
public class ScanVolume
{
   
   
    
    
    public Transform transform;
    public string ID;
    public Bounds bounds;
    public float width;
    public float widthScaled;
    public float height;
    public float heightScaled;
    public float depth; 
    public float depthScaled; 
    public Vector3 position;
    public Vector3 scale;
    public Quaternion rotation;
    public Classification classification;
    
    
    // add: mesh data
    
    public ScanVolume(MockPhysical mockPhyiscal)
    { 
        Transform rt = mockPhyiscal.transform;
        transform = rt;
        ID = "SV_" + mockPhyiscal.gameObject.name;
        bounds = mockPhyiscal.GetComponent<MeshRenderer>().bounds; 
        width =   bounds.size.x;
        widthScaled =   width*rt.localScale.x;
        height =   bounds.size.y;
        heightScaled =   height*rt.localScale.y;
        depth =   bounds.size.z;
        depthScaled =   depth*rt.localScale.z;
        position = rt.position;
        scale = rt.localScale;
        rotation = rt.rotation;
        classification = mockPhyiscal.Classification;

        /*Debug.Log("widthscaled: " + widthScaled  + " .. " + mockPhyiscal.GetComponent<MeshFilter>().sharedMesh.bounds.size.x);
        Debug.Log("width : " + width  + " .. " + mockPhyiscal.GetComponent<MeshFilter>().sharedMesh.bounds.size.x);
        Debug.Log("heightScaled: " + heightScaled  + " .. " + mockPhyiscal.GetComponent<MeshFilter>().sharedMesh.bounds.size.y);
        Debug.Log("depthscaled: " + depthScaled  + " .. " + mockPhyiscal.GetComponent<MeshFilter>().sharedMesh.bounds.size.z);*/
    }
 
}
