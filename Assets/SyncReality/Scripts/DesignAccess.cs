using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Object = UnityEngine.Object;


// holds design data so that the other design objects can be if-unity-editored away from build 

[ExecuteInEditMode]
public class DesignAccess : MonoBehaviour
{
    public GameObject spawnedGroundPlane;
    private LayoutArea layoutArea;
    private DesignArea designArea;
    private SyncMaster syncMaster;
    private StoryTeller storyTeller;
    //   public List<Sync> syncList;
    //   public List<SurroundSync> surroundSyncList;
     private List<LayoutPoint> _layoutPoints;


     [HideInInspector] public RoomLayout activeLayout;

     private LayoutArea LayoutArea
    {
        get
        {
            if (layoutArea == null)
                layoutArea = FindObjectOfType<LayoutArea>();

            return layoutArea;
        }
    }

     private DesignArea DesignArea
    {
        get
        {
            if (designArea == null)
                designArea = FindObjectOfType<DesignArea>();
            return designArea;
        }
    }
    public SyncMaster SyncMaster
    {
        get
        {
            if (syncMaster == null)
                syncMaster = FindObjectOfType<SyncMaster>(); 
            return syncMaster;
        }
    }
    private StoryTeller StoryTeller
    {
        get
        {
            if (storyTeller == null)
                storyTeller = FindObjectOfType<StoryTeller>(); 
            return storyTeller;
        }
    }

    
    public Mesh GetFloorMesh( )
    {
        return DesignArea.GetFloorMesh( StoryTeller.GetLoadedSyncSetup() );
    }
    public GameObject GetFloorMeshPrefab( )
    {
        return DesignArea.GetFloorMeshPrefab( StoryTeller.GetLoadedSyncSetup() );
    }
    public Mesh GetCeilingMesh( )
    {
        return DesignArea.GetCeilingMesh( StoryTeller.GetLoadedSyncSetup() );
    }
    public GameObject GetCeilingMeshPrefab( )
    {
        return DesignArea.GetCeilingMeshPrefab( StoryTeller.GetLoadedSyncSetup() );
    }
    public Material GetFloorMaterial1( )
    {
        return DesignArea.GetFloorMaterial(1, StoryTeller.GetLoadedSyncSetup() );
    }
    public Material GetFloorMaterial2( )
    {
        return DesignArea.GetFloorMaterial(2, StoryTeller.GetLoadedSyncSetup());
    }
    public Material GetCeilingMaterial1()
    {
        return DesignArea.GetCeilingMaterial(1, StoryTeller.GetLoadedSyncSetup());
    }
    public Material GetCeilingMaterial2()
    {
        return DesignArea.GetCeilingMaterial(2, StoryTeller.GetLoadedSyncSetup());
    }
    public float GetCeilingHeight()
    {
        return designArea.GetCeilingHeight(StoryTeller.GetLoadedSyncSetup());
    }
    public List<Sync> GetSyncs()
    {  
        return DesignArea.GetSyncs(StoryTeller.GetLoadedSyncSetup());
    }
    public List<SurroundSync> GetSurroundSyncs( )
    {
        return DesignArea.GetSurroundSyncs(StoryTeller.GetLoadedSyncSetup());
    }
    public Dictionary<WallFace, Backdrop> GetActiveBackdrops(bool getCenter = false )
    {
        Dictionary<WallFace, Backdrop> returnDict = new Dictionary<WallFace, Backdrop>();
        if (DesignArea.GetBackdropForWallFace(WallFace.North, StoryTeller.GetLoadedSyncSetup()) != null)
        {
            returnDict.Add(WallFace.North,DesignArea.GetBackdropForWallFace(WallFace.North, StoryTeller.GetLoadedSyncSetup() ) );
            returnDict.Add(WallFace.East,DesignArea.GetBackdropForWallFace(WallFace.East , StoryTeller.GetLoadedSyncSetup()) );
            returnDict.Add(WallFace.South,DesignArea.GetBackdropForWallFace(WallFace.South, StoryTeller.GetLoadedSyncSetup()) );
            returnDict.Add(WallFace.West,DesignArea.GetBackdropForWallFace(WallFace.West, StoryTeller.GetLoadedSyncSetup()) );
        }
        return returnDict; 
    }
    public List<MessModule> GetMessModules()
    {
        return DesignArea.GetMessModulesFromAnchors(StoryTeller.GetLoadedSyncSetup());
    }

    public Backdrop GetCenterBackdrop()
    {
        if (Application.isPlaying == true)
        {
            return DesignArea.GetFirstActivePresentSyncSetupInRuntime().backdropCenter;
        }
        else
        {
            #if UNITY_EDITOR
            return DesignArea.GetFirstActivePresentSyncSetup().backdropCenter; 
            #endif
        }
#pragma warning disable  
        return null; 
#pragma warning restore  
    }

    
    public int GetWallPiecesForFace(WallFace surroundSyncFace)
    {
        return DesignArea.GetWallPiecesForFace(surroundSyncFace, StoryTeller.GetLoadedSyncSetup());
    }


    
    public RoomLayout GetActiveRoomLayout()
    {
        if (activeLayout == null)
            UpdateRoomLayoutStorage();
        else if (activeLayout.roomDimensions.layoutPointPositions == null)
            UpdateRoomLayoutStorage();
        else if (activeLayout.roomDimensions.layoutPointPositions.Count == 0)
            UpdateRoomLayoutStorage();
        else if (activeLayout.mockPhysicals == null)
            UpdateRoomLayoutStorage();
        else if (activeLayout.mockPhysicals.Count == 0)
            UpdateRoomLayoutStorage();

        return activeLayout;
    }

    public List<LayoutPoint> GetStoredLayoutPoints()
    {
        if (_layoutPoints == null)
            UpdateRoomLayoutStorage();
        if (_layoutPoints.Count == 0 )
            UpdateRoomLayoutStorage();
        return _layoutPoints;
    }
    public void UpdateRoomLayoutStorage()
    { 
        activeLayout = new RoomLayout();
        activeLayout.roomDimensions.layoutPointPositions = (from lp in LayoutArea.GetLayoutPointsByTransform() where lp != null select lp.transform.position).ToList();
        activeLayout.mockPhysicals = LayoutArea.GetMocksByTransform().ToList();
 
        _layoutPoints = LayoutArea.GetLayoutPointsByTransform();
    }

    
    public GameObject AssembleRuntimeFloor(GameObject newFloor)
    {  
        GameObject sourceFloor = designArea.GetFirstActivePresentSyncSetupInRuntime().floorProxy;
       if (sourceFloor != null && newFloor != null)
       {
           foreach (var comp in sourceFloor.GetComponents(typeof(Component)))
               if (comp is Transform == false && comp is MeshFilter == false && comp is MeshRenderer == false)
                   CopyComponent(comp, newFloor);

           foreach (var child in  sourceFloor.GetComponentsInChildren<Transform>())
               if (child != null)
                   if (child != sourceFloor.transform) 
                       Instantiate(child.gameObject, newFloor.transform); 
       } 
        
        return newFloor; 
    }
    public GameObject AssembleRuntimeCeiling(GameObject newCeiling)
    {  
        GameObject sourceCeiling = designArea.GetFirstActivePresentSyncSetupInRuntime().ceilingProxy;
        if (sourceCeiling != null && newCeiling != null)
        {
            foreach (var comp in sourceCeiling.GetComponents(typeof(Component)))
                if (comp is Transform == false && comp is MeshFilter == false && comp is MeshRenderer == false)
                    CopyComponent(comp, newCeiling);

            foreach (var child in sourceCeiling.GetComponentsInChildren<Transform>())
                if (child != null)
                    if (child != sourceCeiling.transform)
                        Instantiate(child.gameObject, newCeiling.transform);
        }

        return newCeiling;
    }

    private void CopyComponent(Component original, GameObject destination)
    {
        System.Type type = original.GetType();
        Component copy = destination.AddComponent(type);
        // Copied fields can be restricted with BindingFlags
        System.Reflection.FieldInfo[] fields = type.GetFields(); 
        foreach (System.Reflection.FieldInfo field in fields)
        {
            field.SetValue(copy, field.GetValue(original));
        } 
    }

   
}
