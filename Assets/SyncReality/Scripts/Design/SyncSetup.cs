using System;
using System.Collections;
using System.Collections.Generic; 
using UnityEngine;

// is an in-scene representation of a SyncLayout, pre-loaded (but only one active) for builds

[ExecuteInEditMode]
public class SyncSetup : MonoBehaviour
{
    [Header("Assigns")]
    public Transform syncsParent;
    public Transform surroundSyncsParent;  
    public Transform backdropsParent;
    public Transform messAnchorsParent;
    public Backdrop backdrop1;
    public Backdrop backdrop2;
    public Backdrop backdrop3;
    public Backdrop backdrop4;
    public Backdrop backdropCenter;
    public GameObject floorProxy;
    public GameObject ceilingProxy;
    
 

    [Header("Settings")]
    public Mesh floorMesh;
    public Mesh ceilingMesh;
    public GameObject floorMeshPrefab;
    public GameObject ceilingMeshPrefab;
    public Material floorMaterial1;
    public Material floorMaterial2;
    public Material ceilingMaterial1; 
    public Material ceilingMaterial2; 
    public float ceilingHeight = 5; 
    public int wallPiecesOne;
    public int wallPiecesTwo;
    public int wallPiecesThree;
    public int wallPiecesFour;
 
    
    
 #if UNITY_EDITOR
    private void OnDisable()
    {
        if (gameObject.activeSelf == false )
          FindObjectOfType<DesignArea>().SyncSetupChangesActiveState(this);
    }
    private void OnEnable()
    {
        if (gameObject.activeSelf == true )
            FindObjectOfType<DesignArea>().SyncSetupChangesActiveState(this);
    }
    
    #endif
    
    public int GetFloorComponentCount()
    {
        return floorProxy.GetComponents(typeof(Component)).Length-1;
    } 
    public int GetFloorChildCount()
    {
        return floorProxy.transform.childCount;
    } 
    public int GetCeilingComponentCount()
    {
        return ceilingProxy.GetComponents(typeof(Component)).Length-1;
    } 
    public int GetCeilingChildCount()
    {
        return ceilingProxy.transform.childCount;
    } 
    public int GetElementCount()
    {
        int count = syncsParent.GetComponentsInChildren<Sync>().Length;
        count += surroundSyncsParent.GetComponentsInChildren<SurroundSync>().Length;
        count += messAnchorsParent.GetComponentsInChildren<MessAnchor>().Length;
        return count;
    } 
    public string GetToolTip()
    {
        string str = "Syncs:    " + syncsParent.GetComponentsInChildren<Sync>().Length;
        str += "\nSSyncs:  " + surroundSyncsParent.GetComponentsInChildren<SurroundSync>().Length; 
        str += "\nAnchors: " + messAnchorsParent.GetComponentsInChildren<MessAnchor>().Length;
        return str;
    }

    public bool HasAnchorForMessModule(MessModule messModule)
    {
        foreach (var anchor in  messAnchorsParent.GetComponentsInChildren<MessAnchor>())
            if (anchor.anchoredModule == messModule)
                return true;
        return false;
    }
}
