using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Serialization;


#if UNITY_EDITOR
using UnityEditor;
#endif



// Handles sync (auto)creation and parents them
[ExecuteInEditMode]
public class SyncMaster : MonoBehaviour
{
    public DesignArea designArea; 
   public DesignAccess designAccess;


   public GameObject playfieldPrefab;
   [Header("Creation Settings")]
   [Tooltip("Sync auto-create only works on objects that have a MeshFilter")] 
   public bool requireMeshForSyncCreation = true;
   [Tooltip("Sync auto-create deletes the original asset that was converted")] 
   public bool autoDeleteOriginalAsset = true;
   [Header("SyncLink Settings")] 
   [Tooltip("Make SyncLink lines point to the middle of the object if true")] 
   public bool offsetSyncLinkLinesHeight = true;  
   
   //todo delete these
 [HideInInspector]  public bool useHandles;
 [HideInInspector]  public float defaultHandleOffset = 0.8f;
 [HideInInspector]  public float defaultHandleSize = 0.5f;
  

#if UNITY_EDITOR

    public FlexFloor flexFloor;

    private bool IsObjectInSyncPlaneBounds(GameObject activeGameObject)
   {
       float xPos = activeGameObject.transform.position.x  ;
       float zPos = activeGameObject.transform.position.z  ;
       var localScale = flexFloor.transform.localScale;
       float xMin = localScale.x  * designArea.transform.localScale.x * -5f+ designArea.FlexFloorProp.transform.position.x; // 5 because plane is 10 units length
       float xMax = localScale.x * designArea.transform.localScale.x * 5f  + designArea.FlexFloorProp.transform.position.x;
       float zMin = localScale.z * designArea.transform.localScale.z * -5f+ designArea.FlexFloorProp.transform.position.z;
       float zMax = localScale.z * designArea.transform.localScale.z *  5f + designArea.FlexFloorProp.transform.position.z;
      //    Debug.Log("xmin " + xMin  + "  xmax " + xMax); Debug.Log("zPos " + zPos   );  Debug.Log("zmin: " + zMin + "   zmax: " + zMax);
       if (xPos >= xMin && xPos <= xMax && zPos >= zMin && zPos <= zMax) 
           return true; 
       return false;
   }

   private bool IsObjectInSurroundSyncPlaneBounds(GameObject activeGameObject)
   { 
       float xPos = activeGameObject.transform.position.x  ;
       float zPos = activeGameObject.transform.position.z  ;
       var localScale = designArea.surroundSyncAutoCreateFloor.transform.localScale;
       var localParentScale = designArea.surroundSyncAutoCreateFloor.GetComponentInParent<Transform>().localScale;
       var flexFloorScale = designArea.flexFloor.transform.localScale;
       float xMin = localScale.x * localParentScale.x /flexFloorScale.x * designArea.transform.localScale.x * -5f+ designArea.surroundSyncAutoCreateFloor.transform.position.x;   
       float xMax = localScale.x * localParentScale.x /flexFloorScale.x* designArea.transform.localScale.x* 5f  + designArea.surroundSyncAutoCreateFloor.transform.position.x;
       float zMin = localScale.z * localParentScale.z /flexFloorScale.z* designArea.transform.localScale.z* -5f+ designArea.surroundSyncAutoCreateFloor.transform.position.z;
       float zMax = localScale.z *  localParentScale.z /flexFloorScale.z* designArea.transform.localScale.z * 5f + designArea.surroundSyncAutoCreateFloor.transform.position.z;
       if (xPos >= xMin && xPos <= xMax && zPos >= zMin && zPos <= zMax)
           return true;
       return false;
   }
   public void CheckForAutoCreation(GameObject activeGameObject)
   { 
      if (activeGameObject.GetComponentsInChildren<Sync>().Length != 0 ||
          activeGameObject.GetComponentsInParent<Sync>().Length != 0) return;
      if (activeGameObject.GetComponentsInChildren<WallMini>().Length != 0 ||
          activeGameObject.GetComponentsInParent<WallMini>().Length != 0) return;
      if (activeGameObject.GetComponentsInChildren<WallPiece>().Length != 0 ||
          activeGameObject.GetComponentsInParent<WallPiece>().Length != 0) return;
      if (activeGameObject.GetComponentsInChildren<FlexFloor>().Length != 0 ||
          activeGameObject.GetComponentsInParent<FlexFloor>().Length != 0) return;
      if (activeGameObject.GetComponentsInChildren<BackdropSpawn>().Length != 0 ||
          activeGameObject.GetComponentsInParent<BackdropSpawn>().Length != 0) return; 
      if (activeGameObject.GetComponentsInChildren<SurroundSync>().Length != 0 ||
          activeGameObject.GetComponentsInParent<SurroundSync>().Length != 0) return; 
      if ((requireMeshForSyncCreation != true || activeGameObject.GetComponentsInChildren<MeshFilter>().Length == 0) && requireMeshForSyncCreation != false) return;
      // All checks passed
      if (IsObjectInSyncPlaneBounds(activeGameObject))
         CreateSyncFromPlacement(activeGameObject);
      else   if (IsObjectInSurroundSyncPlaneBounds(activeGameObject))
         CreateSurroundSyncFromPlacement(activeGameObject);
   }

   private void CreateSyncFromPlacement(GameObject childObject)
   { 
#if UNITY_EDITOR
      childObject.transform.rotation = Quaternion.identity;
      string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(childObject);
   //   if (designArea.saveUsingReferencePrefabs)
   //      path = CreateSyncReferenceFile(childObject);  
      GameObject syncParentObj = new GameObject();  
      Undo.RegisterCreatedObjectUndo(syncParentObj, "Sync AutoCreate"); 
      syncParentObj.AddComponent<Sync>();
    //  GameObject syncChildObj = PrefabUtility.InstantiatePrefab(childObject as GameObject) as GameObject; 
      syncParentObj.transform.position = childObject.transform.position; 
      childObject.transform.SetParent(syncParentObj.transform);  
      childObject.transform.localPosition = Vector3.zero; 
      if (childObject.name.Length < 8)
         syncParentObj.name = "Sync" + childObject.name.Substring(0,childObject.name.Length);
      else
          syncParentObj.name = "Sync" + childObject.name.Substring(0,8); 
      Sync newSync = syncParentObj.GetComponent<Sync>();
      newSync.ID = Guid.NewGuid().ToString();
      newSync.handleOffset = defaultHandleOffset; 
      newSync.handleSize = defaultHandleSize; 
      newSync.CreateCombinedMesh();
       newSync.classification = Classification.Misc;
      newSync.referencePrefabName = childObject.name;
      newSync.referencePrefabPath = path;
      Selection.activeGameObject =  syncParentObj;    
          syncParentObj.transform.SetParent(designArea.GetFirstActivePresentSyncSetup().syncsParent.transform);
          /*if (autoDeleteOriginalAsset == true) 
             DestroyImmediate(childObject);*/
      Undo.RegisterCompleteObjectUndo(syncParentObj, "Sync AutoCreate"); // todo is this necessary? 

   }
 

#endif
   

   
   
   private void CreateSurroundSyncFromPlacement(GameObject activeGameObject)
   {
#if UNITY_EDITOR

       // find nearest WallPiece
       WallPiece nearestWallPiece = designArea.WallSplitterProp.FindNearestWallPiece(activeGameObject.transform.position);
    
       CreateSurroundSyncForWallPiece(nearestWallPiece, activeGameObject);
  

#endif
    }
   private void CreateSurroundSyncForWallPiece(WallPiece wallPiece, GameObject modelObject)
   {
#if UNITY_EDITOR 
       if (modelObject == null)
           Debug.LogWarning("You tried to create a SurroundSync but did not select a model! Please select one first!");
       else
       {      
               GameObject ssObj = new GameObject(); 
               ssObj.transform.position = Vector3.zero;
               SurroundSync ssync =   ssObj.AddComponent<SurroundSync>(); 
               ssync.Initialize(modelObject, wallPiece ); 
               string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(modelObject); 
               //if (designArea.saveUsingReferencePrefabs)
               //    path = CreateSurroundSyncReferenceFile(modelObject); 
               ssync.referencePrefabPath = path;
               ssync.referencePrefabName = modelObject.name; 
       }
       designArea.WallSplitterProp.RedrawWallPieces(wallPiece.index, wallPiece.face);
#endif
   }
 
   
    

#endif


  
   
   public List<SyncLink> GetAllSyncLinks()
   {
       return designArea.GetSyncs().SelectMany(sync => sync.GetSyncLinksFromComponents()).ToList();
   }
   public List<SyncLink> GetPassiveSyncLinksForSync(Sync givenSync)
   {
       return GetAllSyncLinks().Where(syncLink => syncLink.linkedObject == givenSync.gameObject).ToList();
   }
   public List<SyncLink> GetPassiveSyncLinksForSurroundSync(SurroundSync givenSurroundSync)
   {
       return GetAllSyncLinks().Where(syncLink => syncLink.linkedObject == givenSurroundSync.gameObject).ToList();

   }

   public Sync GetSyncByID(string givenID )
   { 
       foreach (var sync in designArea.GetSyncs())
           if (sync.ID == givenID)
               return sync;
       return null;
   }
   public SurroundSync GetSurroundSyncByID(string givenID )
   { 
       foreach (var ssync in designArea.GetSurroundSyncs())
           if (ssync.ID == givenID)
               return ssync; 
       return null;
   }
# if UNITY_EDITOR
    public void DeleteSyncLinksForSurroundSync(SurroundSync surroundSync)
   {
       foreach (var syncLink in GetPassiveSyncLinksForSurroundSync(surroundSync))
       {
           syncLink.parentSync.DestroySyncLink(syncLink);
       }
   } 
     
    public void DeleteSync(Sync sync)
   {
       if (sync != null)
       { 
           foreach (var syncLink in GetPassiveSyncLinksForSync(sync))
               syncLink.parentSync.DestroySyncLink(syncLink);
          // Debug.Log("destroy: " + sync.gameObject.name);
           DestroyImmediate(sync.gameObject);
       }
   }
#endif

     
}
