using System;
using System.Collections;
using System.Collections.Generic;
using System.IO; 
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

// handles scene, scanning setups
// in editcontent and runtime 
public enum PlayMode
{
    EditorSpawn,
    ManualScan
}


public class StoryTeller : MonoBehaviour
{
    [Header("Assigns")] 
    public DesignArea designArea;
    public LayoutArea layoutArea;
    public ModuleManager moduleManager;
    public Transform syncsParent;
    public Transform surroundSyncsParent;
    public Transform messParent;
   public GameObject fingerTappingModule;
    public GameObject handPosesObject;
    public GameObject handGesturesFist; 
    public GameObject controllerMenuRight;
    public GameObject controllerMenuLeft;
    public GameObject controllersInputAdapter;

    public void DebugMe(string hi)
    {
        Debug.Log(hi);
    }



    [Header("Settings")] 
    public bool executeOnStart;
    public bool disablePassthroughAfterScanning;
    
    
    private string _activeManualScan;
    private string _loadedSyncSetupName = "";


    public List<StoryObject> storyObjects = new List<StoryObject>();
    public UnityEvent onPipelineFinished = new UnityEvent();

    
    #if UNITY_EDITOR
    public void ClickStoryTellerWidget()
    {
        Selection.activeGameObject = this.gameObject;
        FindObjectOfType<ToolWindowSettings>().openEditContent = true;

    } 
#endif
    private void Start()
    { 
        designArea.MoveMeOutOfTheWay();
        layoutArea.ActivateMocks(false);
        layoutArea.flatPlane.gameObject.SetActive(false);
        if (executeOnStart)
            moduleManager.Execute();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.T))
            TapTable();
    }

    public void TapTable()
    {
        Debug.Log("TAP");
    }

    public void RegisterStoryObject(StoryObject storyObject)
    {
        if (storyObjects == null)
            storyObjects = new List<StoryObject>();
        storyObjects.Add(storyObject);
    }
  
    public void ActivateAllStoryObjects( )
    {
        foreach (var storyObject in storyObjects) 
                storyObject.Activate();
    }
    public void ReceivePipelineFinishSignal()
    {
        onPipelineFinished.Invoke();
    }
    public void DisableWristMenu()
    {
     //   fingerTappingModule.SetActive(false);
        handPosesObject.SetActive(false);
        handGesturesFist.SetActive(false);
    //    wristMenu.SetActive(false);
        controllerMenuRight.SetActive(false);
        controllerMenuLeft.SetActive(false);
        controllersInputAdapter.SetActive(false);
    }
    public void AttemptDisablePassthrough()
    {
        if (disablePassthroughAfterScanning)
               Camera.main.clearFlags = CameraClearFlags.Skybox;
    }
    public string GetLoadedSyncSetup()
    {
        if (_loadedSyncSetupName == null)
            _loadedSyncSetupName = "";
        return _loadedSyncSetupName;
    }
    public void LoadSyncSetup(string syncSetupName, bool deconstructExistingSpawns = true)
    {
        if (deconstructExistingSpawns)
            DeconstructSpawnedEnvironment();
        layoutArea.ActivateMocks(true);
        if (designArea.IsSyncSetupPresent(syncSetupName))
        {
            _loadedSyncSetupName = syncSetupName;
            moduleManager.Execute();
        } 
        else
        {
            _loadedSyncSetupName = "";
            Debug.LogWarning("Could not load a SyncSetup for name: " + syncSetupName);
        }
        layoutArea.ActivateMocks(false);
    }
    
   
   

 
   
    
    private void SpawnRegularPlayer()
    {
        
    }

    private void SpawnVRPlayer()
    {
        
    }

    private void SpawnVRManualScanPlayer()
    {
        
    }
    

    public void RunFromManualScanner(string scannerTempScan)
    {
        if (_activeManualScan == "")
        {
            if (scannerTempScan == "")
            {
                _activeManualScan = layoutArea.GetRoomLayout();
            }
            else
            {
                _activeManualScan = scannerTempScan;
            }
        } 
      
        LoadScanLayoutIntoLayoutArea(_activeManualScan);
        DesignAccess.UpdateRoomLayoutStorage();
        ExecuteCurrentSyncLayout();
    }
 

    private void LoadScanLayoutIntoLayoutArea(string tempLayoutText)
    {
        layoutArea.ReconstructFromScanLayout(tempLayoutText);

    }
    private void ExecuteCurrentSyncLayout()
    {
         DeconstructSpawnedEnvironment();
         moduleManager.Execute(); // run with current design area setup 
    }



    public void ReceiveScanFromManualScanner(string scanText)
    {
        _activeManualScan = scanText;
   //     File.WriteAllText("Assets/ReceivedScan.txt", _activeManualScan);
    }

   

    public void DeconstructSpawnedEnvironment()
    {
        if (Application.isPlaying)
        {
            foreach (var obj in  syncsParent.GetComponentsInChildren<Transform>())
                if (obj != null)
                    if (obj != syncsParent)
                      Destroy(obj.gameObject);
            foreach (var obj in  surroundSyncsParent.GetComponentsInChildren<Transform>())
                if (obj != null)
                    if (obj != surroundSyncsParent)
                            Destroy(obj.gameObject);
            foreach (var obj in  syncsParent.GetComponentsInChildren<Transform>())
                if (obj != null)
                    if (obj != syncsParent)
                        Destroy(obj.gameObject);
            foreach (var obj in  messParent.GetComponentsInChildren<Transform>())
                if (obj != null)
                    if (obj != messParent)
                        Destroy(obj.gameObject);
        }
        else
        {
            foreach (var obj in  syncsParent.GetComponentsInChildren<Transform>())
                if (obj != null)
                    if (obj != syncsParent)
                           DestroyImmediate(obj.gameObject);
            foreach (var obj in  surroundSyncsParent.GetComponentsInChildren<Transform>())
                if (obj != null)
                    if (obj != surroundSyncsParent)
                          DestroyImmediate(obj.gameObject);
            foreach (var obj in  messParent.GetComponentsInChildren<Transform>())
                if (obj != null)
                    if (obj != messParent)
                        DestroyImmediate(obj.gameObject);
        }
   
    }

    private DesignAccess _designAccess;
    public DesignAccess DesignAccess
    {
        get
        {
            if (_designAccess == null)
                _designAccess = FindObjectOfType<DesignAccess>();
            return _designAccess;
        }
    }


  
}
