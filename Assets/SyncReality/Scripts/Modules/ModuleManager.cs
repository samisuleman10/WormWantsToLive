using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;

[ExecuteAlways]
/// <summary>
/// Manages all modules, e.g. to disable and enable them, to execute them in correct order, etc.
/// </summary>
public class ModuleManager : MonoBehaviour
{
    private DesignArea _designArea;
    private DesignAccess _designAccess;
    private LayoutArea _layoutArea; 
    
    public bool executePipelineInEditMode = false;

    public SurrounderModule surrounderModule;
    public MatcherModule matcherModule;
    public PlacerModule placerModule;
    public ResizerModule resizerModule;
    public GamifierModule gamifierModule;

    public UnityEvent<List<GameObject>> onPipelineExecuted = new UnityEvent<List<GameObject>>();

    [HideInInspector] public bool allowExecute = true;
    
    
    private DesignAccess DesignAccess
    {
        get
        {
            if (_designAccess == null)
                _designAccess = FindObjectOfType<DesignAccess>();
            return _designAccess;
        }
    }
    
    private DesignArea DesignArea
    {
        get
        {
            if (_designArea == null)
                _designArea = FindObjectOfType<DesignArea>();
            return _designArea;
        }
    }
    private LayoutArea LayoutArea
    {
        get
        {
            if (_layoutArea == null)
                _layoutArea = FindObjectOfType<LayoutArea>();
            return _layoutArea;
        }
    }
    public List<MockPhysical> currentMocks;

    /// <summary>
    /// Executes all Modules in correct order
    /// </summary>
    public void Execute(bool previewMode = false)
    { 
   //     Debug.Log("execute" + FindObjectOfType<LayoutArea>().GetLayoutPointsByTransform().Count);
        currentMocks = LayoutArea.GetMocksByTransform().ToList();
        List<ScanVolume> scanVolumes = LayoutArea.GetMocksByTransform().Select(m => new ScanVolume(m)).ToList();
        List<Sync> syncs = DesignAccess.GetSyncs().Where(s => s.quality != SyncQuality.Virtual).ToList();
        List<Sync> virtualSyncs = DesignAccess.GetSyncs().Where(s => s.quality == SyncQuality.Virtual).ToList();
        List<SurroundSync> surroundSyncs = DesignAccess.GetSurroundSyncs();
        RoomDimensions roomDimensions = new RoomDimensions();

        _layoutArea.rotateAllMocksToCenter();

        var smOut = surrounderModule.Execute((scanVolumes, roomDimensions, surroundSyncs));
        var mmOut = matcherModule.Execute((smOut.scanVolumes, syncs, smOut.wallInfos)); 
        var pmOut = placerModule.Execute(mmOut);
        var rmOut = resizerModule.Execute(pmOut);
        var gmOut = gamifierModule.Execute((rmOut, virtualSyncs));
        
        FindObjectOfType<MessMaker>().CreateMessFromMessModules();

        if (previewMode == false && Application.isPlaying)
            FindObjectsOfType<MockPhysical>(true).ToList().ForEach(m => m.gameObject.SetActive(false));

        if (Application.isPlaying)
            onPipelineExecuted.Invoke(gmOut);
//        Debug.Log("From ModuleManager, gmOut.Count = " + gmOut.Count);

    }

    public void ExecuteSurrounderForDesignArea()
    {
        RoomDimensions roomDimensions = new RoomDimensions();
        roomDimensions.layoutPointPositions = new List<Vector3>();
    //    roomDimensions.layoutPointPositions.Add(new Vector3);
         surrounderModule.Execute((new List<ScanVolume>(), roomDimensions, DesignAccess.GetSurroundSyncs()));

    }
  

#if UNITY_EDITOR
    public void UpdatePipeline()
    {
        if (Application.isPlaying == false && executePipelineInEditMode)
        {
            if (allowExecute)
            {
                Debug.Log("Execute Pipeline on " + DesignArea.GetFirstActivePresentSyncSetup().gameObject.name);
                Execute(previewMode: true);
            }
    

        }
     
    }
    #endif
}
