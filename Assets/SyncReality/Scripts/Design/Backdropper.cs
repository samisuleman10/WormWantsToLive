using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Backdropper : MonoBehaviour
{
    public DesignArea designArea;
    public Compass compass;
    public StoryTeller storyTeller;
    public float backdropDefaultDistance = 25f;
     
    
    
#if UNITY_EDITOR
    public Backdrop GetCenterBackdrop()
    { 
        return designArea.GetFirstActivePresentSyncSetup().backdropCenter; // no setupnumber needed because only called for editor-active work
    }
    
    public void ResetBackdropper()
    {
        ResetAllBackdropValues();
        designArea.GetFirstActivePresentSyncSetup().backdrop1.mappedWallFace = WallFace.North;
        designArea.GetFirstActivePresentSyncSetup().backdrop2.mappedWallFace = WallFace.East;
        designArea.GetFirstActivePresentSyncSetup().backdrop3.mappedWallFace = WallFace.South;
        designArea.GetFirstActivePresentSyncSetup().backdrop4.mappedWallFace = WallFace.West;
        DeleteBackdropSpawns(); 
    }

    private void ResetAllBackdropValues()
    { 
        ResetBackdropValues(designArea.GetFirstActivePresentSyncSetup().backdrop1);
        ResetBackdropValues(designArea.GetFirstActivePresentSyncSetup().backdrop2);
        ResetBackdropValues(designArea.GetFirstActivePresentSyncSetup().backdrop3);
        ResetBackdropValues(designArea.GetFirstActivePresentSyncSetup().backdrop4);
        ResetBackdropValues(designArea.GetFirstActivePresentSyncSetup().backdropCenter, true);
    }
    private void ResetBackdropValues(Backdrop backdrop, bool centerBackdrop = false)
    {
        backdrop.anchored = false;
        backdrop.assetReferencePath = "";
        if (centerBackdrop == false)
        {
            backdrop.backdropDistance = backdropDefaultDistance;
            backdrop.backdropVerticalOffset = 0;
        }
        else
        {
            backdrop.backdropDistance = 0; 
            backdrop.backdropVerticalOffset =   designArea.GetFirstActivePresentSyncSetup().ceilingHeight;
        }
        backdrop.backdropRotation = 0;
        backdrop.backdropHorizontalOffset = 0;
        backdrop.backdropAsset = null;
        backdrop.backdropSpawnedAsset = null;
    }
   
  
    public void SpawnCenterBackdrop(float dist, float rotate, float horOffset, float verOffset)
    { 
        if ( designArea.GetFirstActivePresentSyncSetup().backdropCenter.backdropSpawnedAsset != null)
            DestroyImmediate(designArea.GetFirstActivePresentSyncSetup().backdropCenter.backdropSpawnedAsset);
        GameObject obj = Instantiate(designArea.GetFirstActivePresentSyncSetup().backdropCenter.backdropAsset, this.transform, true);
        obj.AddComponent<BackdropSpawn>();
        designArea.GetFirstActivePresentSyncSetup().backdropCenter.backdropSpawnedAsset = obj;
         
        FlexFloor flexFloor = designArea.flexFloor;
        obj.transform.position =  flexFloor.transform.position + new Vector3(-horOffset,designArea.GetFirstActivePresentSyncSetup().ceilingHeight, -dist);
        obj.transform.rotation = Quaternion.Euler(0,rotate , 0);
        
    } 
  public void SpawnBackdrop(WallFace wallFace,WallNumber wallNumber, float dist, float rotate, float horOffset, float verOffset)
    { 
        int wallNr = wallNumber.GetWallNumberInt();
        Backdrop backdrop = designArea.GetBackdropForWallFace(wallFace);
        if ( backdrop.backdropSpawnedAsset != null)
            DestroyImmediate(backdrop.backdropSpawnedAsset);
        GameObject obj = Instantiate(backdrop.backdropAsset, this.transform, true);
        obj.AddComponent<BackdropSpawn>();
        backdrop.backdropSpawnedAsset = obj;
        
        float distance = dist;
        Vector3 changeVec = Vector3.zero;
        if (dist == 0)
            distance = backdropDefaultDistance;
        if (wallNr == 1)
            changeVec += new Vector3(-horOffset, verOffset, -distance);
        else      if (wallNr == 2)
            changeVec += new Vector3(-distance, verOffset, horOffset);
        else      if (wallNr == 3)
            changeVec += new Vector3( horOffset, verOffset, distance);
        else      if (wallNr == 4)
            changeVec += new Vector3(distance, verOffset, -horOffset);
                
        obj.transform.position = wallNumber.transform.position + changeVec;
        obj.transform.rotation = Quaternion.Euler(0,rotate + getRotationOfFace(wallFace), 0);
        
    } 

    float getRotationOfFace(WallFace face)
    {
        return ((int)face) * 90f;
    }
 
    public void DeleteBackdropSpawns()
    {
        designArea.GetFirstActivePresentSyncSetup().backdrop1.backdropSpawnedAsset = null;
        designArea.GetFirstActivePresentSyncSetup().backdrop2.backdropSpawnedAsset = null;
        designArea.GetFirstActivePresentSyncSetup().backdrop3.backdropSpawnedAsset = null;
        designArea.GetFirstActivePresentSyncSetup().backdrop4.backdropSpawnedAsset = null;
        designArea.GetFirstActivePresentSyncSetup().backdropCenter.backdropSpawnedAsset = null;
        foreach (var rt in transform.GetComponentsInChildren<BackdropSpawn>()) 
            if (rt != null)
                DestroyImmediate(rt.gameObject); 
   
    
    }
 

    public void SaveBackdropValues(WallFace wallFace, float backdropDistanceValue, float backdropRotationValue, float hor, float ver)
    {
        Backdrop backdrop = designArea.GetBackdropForWallFace(wallFace);
        backdrop.backdropDistance = backdropDistanceValue;
        backdrop.backdropRotation = backdropRotationValue;
        backdrop.backdropHorizontalOffset = hor;
        backdrop.backdropVerticalOffset = ver; 
    } 
    public void SaveCenterBackdropValues(  float backdropDistanceValue, float backdropRotationValue, float hor, float ver)
    {
         
        designArea.GetFirstActivePresentSyncSetup().backdropCenter.backdropDistance = backdropDistanceValue;
        designArea.GetFirstActivePresentSyncSetup().backdropCenter.backdropRotation = backdropRotationValue;
        designArea.GetFirstActivePresentSyncSetup().backdropCenter.backdropHorizontalOffset = hor;
        designArea.GetFirstActivePresentSyncSetup().backdropCenter.backdropVerticalOffset = ver; 
    }
#endif
  
}
