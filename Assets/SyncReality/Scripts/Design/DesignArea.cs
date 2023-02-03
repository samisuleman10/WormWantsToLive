using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq; 
using UnityEditor; 
using UnityEngine;
using UnityEngine.Serialization;
using Object = System.Object;
using Random = System.Random;


public enum DesignMode
{
   Default,
   SyncLinking 
    
}

public class DesignArea : MonoBehaviour
{

   public void MoveMeOutOfTheWay()
   {
      transform.position = new Vector3(9999, 9999, 9999);
   }  
      
   private bool IsSyncSetupPresent(SyncSetup givenSyncSetup)
   {
      bool present = false;
      foreach (var syncSetup in GetPresentSyncSetups())
         if (syncSetup == givenSyncSetup)
            present = true;
      return present;
   }

   public bool IsSyncSetupPresent(string syncSetupName)
   {
      bool present = false;
      foreach (var syncSetup in GetPresentSyncSetups())
         if (syncSetup.gameObject.name == syncSetupName)
            present = true;
      return present;
   }
   public SyncSetup GetPresentSyncSetupForName(string givenName)
   {
      foreach (var syncSetup in GetPresentSyncSetups())
         if (syncSetup.gameObject.name == givenName)
            return syncSetup;
      Debug.LogWarning("Did not find a SyncSetup for name: " + givenName);
      return null;
   }
  
   public List<SyncSetup> GetPresentSyncSetups()
   {
      if (syncSetupHolder == null)
         syncSetupHolder = FindObjectOfType<SyncSetupHolder>(); 
      
      return syncSetupHolder.transform.GetComponentsInChildren<SyncSetup>(true).ToList();
   }
   public SyncSetup GetFirstActivePresentSyncSetupInRuntime()
   {
      foreach (var syncSetup in GetPresentSyncSetups() )
         if (syncSetup.gameObject.activeSelf)
            return syncSetup;
      // no active found, return first Present
      return  GetPresentSyncSetups()[0]; 
   }
   
   [Header("Assigns")] 
   public Compass compass;
   public Backdropper backdropper; 
   public GameObject messAnchorPrefab; 
   public SyncSetupHolder syncSetupHolder; 
   public FlexFloor flexFloor;
   public StoryTeller storyTeller; 
#if UNITY_EDITOR
   public SyncMaster syncMaster;
   public WallSplitter wallSplitter;
   public MessMaker messMaker;
   public GameObject surroundSyncAutoCreateFloor;
   [FormerlySerializedAs("designStorage")] public DesignAccess designAccess;
   public GameObject syncSetupPrefab;
 

   [Header("Settings")]
   [Tooltip("Don't trigger pipeline updates while snapping - setting this to true reduces lag")]
   public bool disablePipelineWhileSnapping = true;
   [Tooltip("Open a confirmation dialog before deleting a SyncSetup with Syncs/SSyncs/Anchors above threshold")]
   public bool deleteWarningDialog;
   [Tooltip("Threshold for above warning dialog")]
   public int deleteWarningThreshold = 5;
   [Tooltip("Size of SurroundSync Auto-Create-Floor extending beyond FlexFloor")] 
   public float surroundSyncFloorScale = 1.3f;
   [Tooltip("Amount of segments a wall is divided into on a new SyncSetup")] 
   public int defaultWallSegments = 10;  
   public Material wallPieceMaterial;  
   public string defaultSyncSetupName = "SyncSetup";
   public string syncSetupsFullPath;
   public string syncSetupsLoadPath; 
 
   public int infoWindowWidth;
   public int infoWindowHeight;
   public int infoWindowLeftSpacing = 5;
   public int infoWindowVerticalSpacing = 15; 
   
   [HideInInspector]  public DesignMode designMode;  //todo make this a bool as well
   [HideInInspector] public bool _snapping = false;

   private bool _wallPreviewToggled = false;


   private List<Sync> tempStoredSyncs = new List<Sync>();
   private List<SurroundSync> tempStoredSurroundSyncs = new List<SurroundSync>();
   private List<SyncLink> tempStoredSyncLinks = new List<SyncLink>();
   private ToolWindowSettings _toolWindowSettings;
   
   
  [HideInInspector] public Classification classificationFilter;

   public List<Classification> classificationFilterList
   {
      get
      {
         return Classification.GetValues(classificationFilter.GetType()).Cast<Classification>().Where(v=>classificationFilter.HasFlag(v)).ToList();
      }
   }
 

   public void ToggleSnapMode(bool forceDisable = false)
   {
      if (forceDisable)
         _snapping = false;
      else
         _snapping = !_snapping; 
      SendPipelineUpdateSignal();
   }
   public SyncSetup GetFirstActivePresentSyncSetup()
   {
      if (GetPresentSyncSetups().Count == 0)
      {
         if (GetStoredSyncSetups().Count != 0)
         {
            Debug.Log("No Present SyncSetup: activating " + GetStoredSyncSetups()[0].gameObject.name);
            ToggleSyncSetup(GetStoredSyncSetups()[0].gameObject.name, true);
         }
         else
         {
            Debug.Log("No SyncSetup Available:  auto-generating new ");
            GenerateNewSyncSetup();
         } 
      } 
      
      foreach (var syncSetup in GetPresentSyncSetups() )
         if (syncSetup.gameObject.activeSelf)
            return syncSetup;
      // no active found, return first Present
      return  GetPresentSyncSetups()[0]; 
   }
   public void InitializeDesignArea(bool redraw = true)
   { 
      surroundSyncAutoCreateFloor.hideFlags = HideFlags.NotEditable; 
      if (FlexFloorProp != null)
         FlexFloorProp.InitializeFlexFloor();
      if (WallSplitterProp != null)
         WallSplitterProp.InitializeWallSplitter();
      _wallPreviewToggled = false;
      if (redraw)
         RedrawDesignArea(); 
      SendPipelineUpdateSignal(); 
   }
   
   public void SwitchToSyncSetup(string syncSetupName)
   {
      GameObject syncSetupObj = GetPresentSyncSetupForName(syncSetupName).gameObject;
      foreach (var syncSetup in GetPresentSyncSetups())
         syncSetup.gameObject.SetActive(false);
      backdropper.DeleteBackdropSpawns();
      wallSplitter.RedrawWallPieces();
      syncSetupObj.SetActive(!syncSetupObj.activeSelf);
   }

   private void OnDisable ()
   {
      ToolShortcutManager.StopListening(SaveSyncSetup, KeyCode.LeftShift, KeyCode.S);
   }

   public List<SyncSetup> GetStoredSyncSetupsWithAssetDatabase()
   {
      List<SyncSetup> returnList = new List<SyncSetup>();
      string folderPath = "Assets/Resources/SyncSetups";
      string[] allPaths = Directory.GetFiles(folderPath, "*.prefab",  SearchOption.AllDirectories);
      foreach (string path in allPaths)
         returnList.Add((SyncSetup)AssetDatabase.LoadAssetAtPath(path.Replace("\\", "/"), typeof(SyncSetup)));
      return returnList;
   }
   public List<SyncSetup> GetStoredSyncSetups()
   {
      List<SyncSetup> returnList = new List<SyncSetup>();
      VerifySyncSetupFolderPresent();
      Object[] assets;
      assets = Resources.LoadAll(syncSetupsLoadPath, typeof(SyncSetup));
      
      foreach (var asset in assets)
         if (asset != null)
         {
            SyncSetup setup = (SyncSetup)asset;
            returnList.Add(setup);
         }
      return returnList;
   }

   
   public SyncSetup GetStoredSyncSetupForName(string givenName, bool report = true)
   {
      foreach (var syncSetup in GetStoredSyncSetups())
         if (syncSetup.gameObject.name == givenName)
            return syncSetup;
      if (report)
        Debug.LogWarning("Did not find SyncSetup in Resources/SyncSetups: " + givenName +". Please either Save or Delete it through the ToolWindow ControlUI");
      return null;
   }
   public bool IsSyncSetupSaved(string syncSetupName)
   {
      //todo make this more fail-safe than just checking amounts, perhaps with PrefabUtility.GetPropertyModifications
      if (IsSyncSetupPresent(syncSetupName) == false)
         return true;
      SyncSetup stored = GetStoredSyncSetupForName(syncSetupName);
      SyncSetup present = GetPresentSyncSetupForName(syncSetupName);
      if (stored == null || present == null)
         return false;
      return stored.GetElementCount() == present.GetElementCount();

   }
   public string ClickNewSyncSetup()
   {
      foreach (var syncSetup in GetPresentSyncSetups())
         syncSetup.gameObject.SetActive(false);
      string returnString = GenerateNewSyncSetup();
      backdropper.DeleteBackdropSpawns(); 
      if ( FindObjectOfType<ModuleManager>().allowExecute == true)
         InitializeDesignArea();
      SendPipelineUpdateSignal();
      _toolWindowSettings.forceSelectSyncSetupNameField = true;
      return returnString;
   }
   private string GenerateNewSyncSetup()
   {
      GameObject obj = PrefabUtility.InstantiatePrefab(syncSetupPrefab, syncSetupHolder.transform) as GameObject;
      var syncSetup = obj.GetComponent<SyncSetup>();
      syncSetup.wallPiecesOne = defaultWallSegments;
      syncSetup.wallPiecesTwo = defaultWallSegments;
      syncSetup.wallPiecesThree = defaultWallSegments;
      syncSetup.wallPiecesFour = defaultWallSegments;
      obj.name = defaultSyncSetupName + UnityEngine.Random.Range(101, 999);
      VerifySyncSetupFolderPresent();
    // PrefabUtility.SaveAsPrefabAssetAndConnect(obj, "Assets/Resources/SyncSetups/" +obj.name +".prefab", InteractionMode.UserAction);
      PrefabUtility.SaveAsPrefabAsset(obj, syncSetupsFullPath +obj.name +".prefab");
      PrefabUtility.UnpackPrefabInstance(syncSetup.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.UserAction);
 
      obj.transform.SetSiblingIndex(0);
      return obj.name;
   }

   private void VerifySyncSetupFolderPresent()
   {
      if ( AssetDatabase.IsValidFolder(syncSetupsFullPath  ) == false )
         AssetDatabase.CreateFolder("Assets/Resources", "SyncSetups");
   }
   public void SaveSyncSetup(string syncSetupName = "")
   {
      var syncSetup = GetFirstActivePresentSyncSetup();
      if (syncSetupName != "")
         syncSetup =  GetPresentSyncSetupForName(syncSetupName); 
      syncSetup.gameObject.SetActive(true); 
      //     PrefabUtility.SaveAsPrefabAsset(syncSetup.gameObject, syncSetupsFullPath +syncSetup.gameObject.name +".prefab");
      VerifySyncSetupFolderPresent();
      PrefabUtility.SaveAsPrefabAssetAndConnect(syncSetup.gameObject, syncSetupsFullPath + syncSetup.gameObject.name + ".prefab", InteractionMode.UserAction);
      PrefabUtility.UnpackPrefabInstance(syncSetup.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.UserAction); 
   }
   private void GenerateFromStoredSyncSetup(string syncSetupName)
   {
      // when you toggle from off to on
      PrefabUtility.InstantiatePrefab(GetStoredSyncSetupForName(syncSetupName), syncSetupHolder.transform);
      SyncSetup newlySpawned = GetPresentSyncSetupForName(syncSetupName);
      PrefabUtility.UnpackPrefabInstance(newlySpawned.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.UserAction);
      //    PrefabUtility.SaveAsPrefabAssetAndConnect(obj, "Assets/Resources/SyncSetups/" +obj.name +".prefab", InteractionMode.UserAction); 
   }
   
   public void RenamePresentSyncSetup(string oldName, string newName)
   {
      var syncSetup = GetPresentSyncSetupForName(oldName);
      VerifySyncSetupFolderPresent();

      PrefabUtility.SaveAsPrefabAssetAndConnect(syncSetup.gameObject, syncSetupsFullPath + syncSetup.gameObject.name + ".prefab", InteractionMode.UserAction);
      
      string assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(GetPresentSyncSetupForName(oldName).gameObject);
      AssetDatabase.RenameAsset(assetPath, newName);
      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh();
      GetPresentSyncSetupForName(oldName).gameObject.name = newName;
      
      PrefabUtility.UnpackPrefabInstance(syncSetup.gameObject, PrefabUnpackMode.Completely, InteractionMode.UserAction); 
      // PrefabUtility.GetCorrespondingObjectFromSource(syncSetupHolder.syncSetup).name = value;

   }
   public void RenameStoredSyncSetup(string oldName, string newName)
   { 
      var syncSetup = GetStoredSyncSetupForName(oldName); 
      VerifySyncSetupFolderPresent();

      // PrefabUtility.GetCorrespondingObjectFromSource(syncSetupHolder.syncSetup).name = value;
      string assetPath =  syncSetupsFullPath + syncSetup.gameObject.name + ".prefab";
      AssetDatabase.RenameAsset(assetPath, newName);
      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh(); 
   }
  
 
   public bool ToggleSyncSetup(string syncSetupName, bool newState)
   {
      bool allowed = true;

      if (newState == false)
      {
         if (GetPresentSyncSetups().Count == 1)
            if (IsSyncSetupPresent(syncSetupName))
               allowed = false;
         if (allowed)
            MakeSyncSetupNotPresent(syncSetupName);
         else
            Debug.LogWarning("Did not disable " + syncSetupName + ": it is the only present SyncSetup");
      }
      else if (newState == true)
      {
         GenerateFromStoredSyncSetup(syncSetupName);
      }
      return allowed;
   }

   private void MakeSyncSetupNotPresent(string syncSetupName)
   {
      SaveSyncSetup(syncSetupName);  
      wallSplitter.RedrawWallPieces();

      DestroyImmediate(GetPresentSyncSetupForName(syncSetupName).gameObject);
   }


   public static void SaveSyncSetup(CustomShortcutArguments args)
   {
      // TODO:: needs to be optimized
      FindObjectOfType<DesignArea>().SaveSyncSetup();
   }

   public void DeleteSyncSetup(string syncSetupName)
   {
      bool allowed = true;
      bool present = false;
      bool offerDialog = false;
      int count = 0;

      if (IsSyncSetupPresent(syncSetupName) )
         present = true;
      
      if (present)
      {
         count = GetPresentSyncSetupForName(syncSetupName).GetElementCount();
         if (count >= deleteWarningThreshold)
            offerDialog = true;
      }
      else
      {
         count = GetStoredSyncSetupForName(syncSetupName).GetElementCount();
         if (count >= deleteWarningThreshold)
            offerDialog = true; 
      }

      if (offerDialog)
      {
         if (deleteWarningDialog)
            allowed = EditorUtility.DisplayDialog(
               "Delete SyncSetup", // title
               "" +syncSetupName +" has " +count + " elements. Delete?", // message
               "Yes", 
               "No"  
            );
         
      }
        
      if (allowed)
      {
         if (IsSyncSetupPresent(syncSetupName))
            DestroyImmediate(GetPresentSyncSetupForName(syncSetupName).gameObject);
         if (GetStoredSyncSetupForName(syncSetupName) == null)
            return;
         string assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(GetStoredSyncSetupForName(syncSetupName));
         AssetDatabase.DeleteAsset(assetPath);
         AssetDatabase.Refresh();
      }  
   }

   
   public void SyncSetupChangesActiveState(SyncSetup syncSetup)
   {
      if (_toolWindowSettings == null)
         _toolWindowSettings = FindObjectOfType<ToolWindowSettings>();
      _toolWindowSettings.forceUpdateContentSections = true;
      SendPipelineUpdateSignal();
   }
   public string GetTooltipForAmountLabel(string syncSetupName)
   {
      var syncSetup = GetStoredSyncSetupForName(syncSetupName, false);
      if (IsSyncSetupPresent(syncSetup))
         syncSetup = GetPresentSyncSetupForName(syncSetupName);
      if (syncSetup == null)
         return "" + syncSetupName + " is not in storage! Please delete it from hierarchy";
      return syncSetup.GetToolTip();
   }
   private void SendPipelineUpdateSignal()
   {
      if (_toolWindowSettings == null)
         _toolWindowSettings = FindObjectOfType<ToolWindowSettings>();
      _toolWindowSettings.SendPipelineUpdateSignal();
   }

   public void ClickCompassWidget()
   {
      Selection.activeGameObject = compass.gameObject;
      _toolWindowSettings.openEditContent = true;
   }
   public void ClickMessWidget()
   {
      Selection.activeGameObject = messMaker.gameObject;
      _toolWindowSettings.openEditContent = true;
   } 
    
   
  
   public int GetIndexAmountForWallFace(WallFace face)
   {
      int number = compass.GetNumberForFace(face);
      if (number == 1)
         return GetFirstActivePresentSyncSetup().wallPiecesOne;
      if (number == 2)
         return GetFirstActivePresentSyncSetup().wallPiecesTwo;
      if (number == 3)
         return GetFirstActivePresentSyncSetup().wallPiecesThree;
      if (number == 4)
         return GetFirstActivePresentSyncSetup().wallPiecesFour;
      Debug.LogWarning("Did not find index value for WallFace: " + face );
      return 0;
   }
   
    
   private void RedrawDesignArea()
   { 
      surroundSyncAutoCreateFloor.transform.localScale = flexFloor.transform.localScale * surroundSyncFloorScale;
      surroundSyncAutoCreateFloor.transform.SetParent(flexFloor.transform);
      surroundSyncAutoCreateFloor.transform.localPosition = new Vector3(0, -0.2f, 0);
      surroundSyncAutoCreateFloor.transform.SetParent(this.transform);

      FlexFloorProp.RedrawFlexFloor();
      WallSplitterProp.RedrawWallPieces();
      compass.RedrawCompass();
      if (_wallPreviewToggled)
         CreateWallPreviews();
      else
         DeleteWallPreviews();
   }
   private void CreateWallPreviews()
   {
 //     FindObjectOfType<ModuleManager>().ExecuteSurrounderForDesignArea();
   }
   private void DeleteWallPreviews()
   {
      
   } 
   public void ToggleWallPreviews()
   {
      _wallPreviewToggled = !_wallPreviewToggled;
      RedrawDesignArea();
   }
   
   
   
   public void ToggleSyncLinkMode(bool forceDisable = false)
   {
      if (forceDisable == false)
      {
         if (!IsToggleAllowed()) return;
      
         if (designMode == DesignMode.Default)
            designMode = DesignMode.SyncLinking;
         else
            designMode = DesignMode.Default;
      }
      else // force disable
      {
         designMode = DesignMode.Default; 
      }
   
   }

   private bool IsToggleAllowed()
   {
      GameObject selected = Selection.activeGameObject;
      if (selected != null)
      {
         if (selected.GetComponent<Sync>() != null)
            return true; 
         if (selected.GetComponent<SyncLink>() != null)
            return true; 
         if (selected.GetComponent<WallMini>() != null)
            return true; 
         if (selected.GetComponent<FlexFloor>() != null)
            return true; 
         if (selected.GetComponent<DesignArea>() != null)
            return true; 
      } 
      return false;
   }
     
   
   public void RegisterChangeFromUI()
   {    
       SendPipelineUpdateSignal();
    //  InitializeDesignArea();
   }
   
   public void ReloadAndReselectWallMini(string ssID)
   {
      InitializeDesignArea();
      if (WallSplitterProp.GetWallMiniForSurroundSyncID(ssID) != null)
         Selection.activeGameObject = SyncMasterProp.GetSurroundSyncByID(ssID).gameObject;
      SendPipelineUpdateSignal();
   }

   int prev_wallPiecesAll;
   int prev_wallPiecesNorth;
   int prev_wallPiecesEast;
   int prev_wallPiecesSouth;
   int prev_wallPiecesWest;

   public void UpdateFromToolWindow()
   {
      CheckForChanges();
      
      if (compass.alwaysTurnTowardsCamera)
         compass.TurnTowardsCamera(); 
   }
   
     public void CheckForChanges()
     { 
        // check for SurroundSync changes
        wallSplitter.CheckForWallMiniChanges();
        SyncSetup syncSetup = GetFirstActivePresentSyncSetup();
        // check for designarea-settings changes
         if (DesignAccessProp == null)
        {
            Debug.LogError("[DesignArea](CheckForChanges): DesignStorage in DesignArea(" + this.gameObject.name + ") is not set correctly or points to an invalid gameObject.");
            return;
        }

         if (disablePipelineWhileSnapping)
            if (_snapping)
              return;  // reduce snapping lag
         
        if (prev_wallPiecesNorth != syncSetup.wallPiecesOne || prev_wallPiecesEast != syncSetup.wallPiecesTwo 
            || prev_wallPiecesSouth != syncSetup.wallPiecesThree || prev_wallPiecesWest != syncSetup.wallPiecesFour)
        { 
           prev_wallPiecesNorth = syncSetup.wallPiecesOne;
           prev_wallPiecesEast = syncSetup.wallPiecesTwo;
           prev_wallPiecesSouth = syncSetup.wallPiecesThree;
           prev_wallPiecesWest = syncSetup.wallPiecesFour;
           SendPipelineUpdateSignal();
           return;
        }
        
       // create reference for current active LP's & Mocks
       List<Sync> currentSyncs = GetSyncs().ToList();
       List<SurroundSync> currentSurroundSyncs = DesignAccessProp.GetSurroundSyncs().ToList();
       List<SyncLink> currentSyncLinks = SyncMasterProp.GetAllSyncLinks().ToList(); 

       void SynchronizeSyncs()
       {
          tempStoredSyncs = currentSyncs.ToList();
       }
       void SynchronizeSurroundSyncs()
       {
          tempStoredSurroundSyncs = currentSurroundSyncs.ToList();
       }
       void SynchronizeSyncLinks()
       {
          tempStoredSyncLinks = currentSyncLinks.ToList();
       }
       // check for null / empty lists
       if (tempStoredSyncs == null)
          SynchronizeSyncs();
       if (tempStoredSyncs.Count == 0)
          SynchronizeSyncs();
       if (tempStoredSurroundSyncs == null)
          SynchronizeSurroundSyncs();
       if (tempStoredSurroundSyncs.Count == 0)
          SynchronizeSurroundSyncs();
       if (tempStoredSyncLinks == null)
          SynchronizeSyncLinks();
       if (tempStoredSyncLinks.Count == 0)
          SynchronizeSyncLinks();
       
       // check for null elements 
       if (tempStoredSyncs.Count != 0)
          foreach (var sync in tempStoredSyncs)
             if (sync == null)
             {
                SendPipelineUpdateSignal();
                SynchronizeSyncs();
                return;
             }
       if (tempStoredSurroundSyncs.Count != 0)
          foreach (var ssync in tempStoredSurroundSyncs)
             if (ssync == null)
             {
                SendPipelineUpdateSignal();
                SynchronizeSurroundSyncs();
                return;
             }
       if (tempStoredSyncLinks.Count != 0)
          foreach (var slink in tempStoredSyncLinks)
             if (slink == null)
             {
                SendPipelineUpdateSignal();
                SynchronizeSyncLinks();
                return;
             }
       
       // check for size changes
       if (tempStoredSyncs.Count != currentSyncs.Count)
       {
          SendPipelineUpdateSignal();
          SynchronizeSyncs();
          return;
       }
       if (tempStoredSurroundSyncs.Count != currentSurroundSyncs.Count)
       {
          SendPipelineUpdateSignal();
          SynchronizeSurroundSyncs(); 
          return;
       }
       if (tempStoredSyncLinks.Count != currentSyncLinks.Count)
       {
          SendPipelineUpdateSignal();
          SynchronizeSyncLinks(); 
          return;
       }
       

       // check for transform changes 
       foreach (var sync in tempStoredSyncs)
          if (sync != null)
          {
             if (sync.transform.hasChanged)
             {
                SendPipelineUpdateSignal();
                sync.transform.hasChanged = false;
                SynchronizeSyncs();
                return;
             }
          }
       foreach (var ssync in tempStoredSurroundSyncs)
          if (ssync != null)
          {
             if (ssync.transform.hasChanged)
             {
                SendPipelineUpdateSignal();
                ssync.transform.hasChanged = false;
                SynchronizeSurroundSyncs();
                return;
             }
          } 

    }
  
   
   #region CalledFromUpdateFunctions
   
   
   // called when FlexFloor's scale changes
   public void FloorSizeChanged()
   {
      RedrawDesignArea();
   }
   public void WallPieceAmountChanged()
   {
      var syncSetup = GetFirstActivePresentSyncSetup();
      WallSplitterProp.RedrawWallPieces(); 
      EditorUtility.SetDirty(DesignAccessProp);
   }
  

   #endregion
   
   
   #region ButtonFunctions
   public void ResetDimensions(bool redraw = true)
   {
      if (FlexFloorProp != null)
         FlexFloorProp.ResetToDefaultScale();

      if (WallSplitterProp != null)
         WallSplitterProp.ResetToDefaultAmounts();
      if (redraw)
         RedrawDesignArea();
   }
//
 
   public void ResetDesignArea(bool alwaysSkipDialog = false)
   {
      

      FindObjectOfType<ModuleManager>().allowExecute = false;
    
      
      bool decision;
      if (alwaysSkipDialog)
         decision = true;
     else if (deleteWarningDialog) 
         decision = EditorUtility.DisplayDialog(
            "Reset DesignArea", // title
            "Removing all DesignArea content. Proceed?", // description
            "Yes", // OK button
            "No" // Cancel button
         );
      else
         decision = true;
    
      if (!decision) return;
 
   
     
      if (FlexFloorProp != null)
         FlexFloorProp.ResetFlexFloor(this);
      if (WallSplitterProp != null)
         WallSplitterProp.ResetWallSplitter();
      compass.ResetCompass();
      backdropper.ResetBackdropper();
      InitializeDesignArea(false);
      ResetDimensions(false);
      RedrawDesignArea();
      FindObjectOfType<ModuleManager>().allowExecute = true;

   }

  
   #endregion


  
 
   public void ClearReferencePrefabs()
   {
     
   }


   
#endif
  
   public List<Sync> GetSyncs(string syncSetupName = "")
   {
      SyncSetup syncSetup = null;
      if (Application.isPlaying == true)
      {
         syncSetup = GetFirstActivePresentSyncSetupInRuntime();
      }
      else
      {
         #if UNITY_EDITOR
         syncSetup = GetFirstActivePresentSyncSetup();
         #endif
      }
      if (syncSetupName != "")
         syncSetup = GetPresentSyncSetupForName(syncSetupName); 
      return syncSetup.syncsParent.GetComponentsInChildren<Sync>().ToList();
   }
   public List<SurroundSync> GetSurroundSyncs(string syncSetupName = "" )
   {
      SyncSetup syncSetup = null;
      if (Application.isPlaying == true)
      {
         syncSetup = GetFirstActivePresentSyncSetupInRuntime();
      }
      else
      {
#if UNITY_EDITOR
         syncSetup = GetFirstActivePresentSyncSetup();
#endif
      }
      if (syncSetupName != "")
         syncSetup = GetPresentSyncSetupForName(syncSetupName); 
      return syncSetup.surroundSyncsParent.GetComponentsInChildren<SurroundSync>().ToList(); 
   }
   public List<MessModule> GetMessModulesFromAnchors(string syncSetupName = "" )
   {  
      SyncSetup syncSetup = null;
      if (Application.isPlaying == true)
      {
         syncSetup = GetFirstActivePresentSyncSetupInRuntime();
      }
      else
      {
#if UNITY_EDITOR
         syncSetup = GetFirstActivePresentSyncSetup();
#endif
      }
      if (syncSetupName != "")
         syncSetup = GetPresentSyncSetupForName(syncSetupName);

      List<MessModule> returnList = new List<MessModule>();
      foreach (var anchor in  syncSetup.GetComponentsInChildren<MessAnchor>())
         if (anchor.anchoredModule != null)
            returnList.Add(anchor.anchoredModule);
      
      return returnList;
   }
   public Mesh GetFloorMesh(string syncSetupName = "")
   {
      SyncSetup syncSetup = null;
      if (Application.isPlaying == true)
      {
         syncSetup = GetFirstActivePresentSyncSetupInRuntime();
      }
      else
      {
#if UNITY_EDITOR
         syncSetup = GetFirstActivePresentSyncSetup();
#endif
      }
      if (syncSetupName != "")
         syncSetup = GetPresentSyncSetupForName(syncSetupName);
      return syncSetup.floorMesh;
   }
   public GameObject GetFloorMeshPrefab(string syncSetupName = "")
   {
      SyncSetup syncSetup = null;
      if (Application.isPlaying == true)
      {
         syncSetup = GetFirstActivePresentSyncSetupInRuntime();
      }
      else
      {
#if UNITY_EDITOR
         syncSetup = GetFirstActivePresentSyncSetup();
#endif
      }
      if (syncSetupName != "")
         syncSetup = GetPresentSyncSetupForName(syncSetupName);
      return syncSetup.floorMeshPrefab;
   }
   public Mesh GetCeilingMesh(string syncSetupName = "")
   {
      SyncSetup syncSetup = null;
      if (Application.isPlaying == true)
      {
         syncSetup = GetFirstActivePresentSyncSetupInRuntime();
      }
      else
      {
#if UNITY_EDITOR
         syncSetup = GetFirstActivePresentSyncSetup();
#endif
      }
      if (syncSetupName != "")
         syncSetup = GetPresentSyncSetupForName(syncSetupName);
      return syncSetup.ceilingMesh;
   }
   public GameObject GetCeilingMeshPrefab(string syncSetupName = "")
   {
      SyncSetup syncSetup = null;
      if (Application.isPlaying == true)
      {
         syncSetup = GetFirstActivePresentSyncSetupInRuntime();
      }
      else
      {
#if UNITY_EDITOR
         syncSetup = GetFirstActivePresentSyncSetup();
#endif
      }
      if (syncSetupName != "")
         syncSetup = GetPresentSyncSetupForName(syncSetupName);
      return syncSetup.ceilingMeshPrefab;
   }
   public Material GetFloorMaterial(int materialNumber , string syncSetupName = ""  )
   {
      SyncSetup syncSetup = null;
      if (Application.isPlaying == true)
      {
         syncSetup = GetFirstActivePresentSyncSetupInRuntime();
      }
      else
      {
#if UNITY_EDITOR
         syncSetup = GetFirstActivePresentSyncSetup();
#endif
      }
      if (syncSetupName != "")
         syncSetup = GetPresentSyncSetupForName(syncSetupName); 
      
      if (materialNumber == 2)
         return syncSetup.floorMaterial2;
      else
         return syncSetup.floorMaterial1;
   }  
   public Material GetCeilingMaterial(int materialNumber , string syncSetupName = "" )
   {
      SyncSetup syncSetup = null;
      if (Application.isPlaying == true)
      {
         syncSetup = GetFirstActivePresentSyncSetupInRuntime();
      }
      else
      {
#if UNITY_EDITOR
         syncSetup = GetFirstActivePresentSyncSetup();
#endif
      }
      if (syncSetupName != "")
         syncSetup = GetPresentSyncSetupForName(syncSetupName); 
      if (materialNumber == 2)
         return syncSetup.ceilingMaterial2;
      else
         return syncSetup.ceilingMaterial1; 
   }
   public float GetCeilingHeight(string syncSetupName = "" )
   {
      SyncSetup syncSetup = null;
      if (Application.isPlaying == true)
      {
         syncSetup = GetFirstActivePresentSyncSetupInRuntime();
      }
      else
      {
#if UNITY_EDITOR
         syncSetup = GetFirstActivePresentSyncSetup();
#endif
      }
      if (syncSetupName != "")
         syncSetup = GetPresentSyncSetupForName(syncSetupName); 
      return syncSetup.ceilingHeight;
   }
   public int GetWallPiecesForFace(WallFace surroundSyncFace, string syncSetupName)
   {
      SyncSetup syncSetup = null;
      if (Application.isPlaying == true)
      {
         syncSetup = GetFirstActivePresentSyncSetupInRuntime();
      }
      else
      {
#if UNITY_EDITOR
         syncSetup = GetFirstActivePresentSyncSetup();
#endif
      }
      if (syncSetupName != "")
         syncSetup = GetPresentSyncSetupForName(syncSetupName); 
      if (surroundSyncFace == WallFace.North)
         return syncSetup.wallPiecesOne;
      if (surroundSyncFace == WallFace.East)
         return syncSetup.wallPiecesTwo;
      if (surroundSyncFace == WallFace.South)
         return syncSetup.wallPiecesThree;
      if (surroundSyncFace == WallFace.West)
         return syncSetup.wallPiecesFour;
      return 0;
   }
   public Backdrop GetBackdropForWallFace(WallFace face, string loadedSyncSetup = "" )
   { 
      foreach (var backdrop in GetBackdrops(loadedSyncSetup))
         if (backdrop.mappedWallFace == face)
            return backdrop;
      Debug.LogWarning("Did not find a Backdrop for WallFace: " + face + " and SyncSetup " + loadedSyncSetup);
      return null;
   }
   private List<Backdrop> GetBackdrops(string syncSetupName = "" )
   {
      SyncSetup syncSetup = null;
      if (Application.isPlaying == true)
      {
         syncSetup = GetFirstActivePresentSyncSetupInRuntime();
      }
      else
      {
#if UNITY_EDITOR
         syncSetup = GetFirstActivePresentSyncSetup();
#endif
      }
      if (syncSetupName != "")
         syncSetup = GetPresentSyncSetupForName(syncSetupName); 
      List<Backdrop> returnList = new List<Backdrop>
      {
         syncSetup.backdrop1,
         syncSetup.backdrop2,
         syncSetup.backdrop3,
         syncSetup.backdrop4,
         syncSetup.backdropCenter
      }; 
      return returnList;
   }
  
   
   #if UNITY_EDITOR
   public void CreateMessModuleAnchor(MessModule lastClickedMessModule)
   { 
      GameObject obj = Instantiate(messAnchorPrefab, GetFirstActivePresentSyncSetup().messAnchorsParent );  
      obj.name = lastClickedMessModule.moduleID + "_Anchor"; 
      obj.GetComponent<MessAnchor>().anchoredModule = lastClickedMessModule; 
   }
   private void CreateMessModuleAnchor(string id )
   {  
      GameObject obj = Instantiate(messAnchorPrefab, GetFirstActivePresentSyncSetup().messAnchorsParent); 
      MessModule messModule = messMaker.GetMessModuleByID(id); 
      obj.name = messModule.moduleID + "_Anchor"; 
      obj.GetComponent<MessAnchor>().anchoredModule = messModule; 
   }
   

   public void DestroyMessModuleAnchor(MessModule lastClickedMessModule)
   {
      GameObject destroyAnchor = null;
         foreach (var anchor in GetFirstActivePresentSyncSetup().messAnchorsParent.GetComponentsInChildren<MessAnchor>())
            if (anchor.anchoredModule == lastClickedMessModule)
               destroyAnchor = anchor.gameObject;
      if (destroyAnchor != null)
         DestroyImmediate(destroyAnchor);
   }



   public DesignAccess DesignAccessProp
   {
      get
      {
         if (designAccess == null)
            designAccess = FindObjectOfType<DesignAccess>();
         return designAccess;
      }
   }
   public WallSplitter WallSplitterProp
   {
      get
      {
         if (wallSplitter == null)
            wallSplitter = FindObjectOfType<WallSplitter>();
         return wallSplitter;
      }
   }
   public SyncMaster SyncMasterProp
   {
      get
      {
         if (syncMaster == null)
            syncMaster = FindObjectOfType<SyncMaster>();
         return syncMaster;
      }
   }
   public FlexFloor FlexFloorProp
   {
      get
      {
         if (flexFloor == null)
            flexFloor = FindObjectOfType<FlexFloor>();
         return flexFloor;
      }
   }
 

   public void DeleteMessAnchorsForMessModule(MessModule messModule)
   {
      List<GameObject> deleteList = new List<GameObject>();
      foreach (var syncSetup in GetStoredSyncSetups())
      foreach (var messAnchor in syncSetup.messAnchorsParent.GetComponentsInChildren<MessAnchor>())
         if (messAnchor.anchoredModule == messModule)
            deleteList.Add(messAnchor.gameObject);
      foreach (var syncSetup in GetPresentSyncSetups())
      foreach (var messAnchor in syncSetup.messAnchorsParent.GetComponentsInChildren<MessAnchor>())
         if (messAnchor.anchoredModule == messModule)
            deleteList.Add(messAnchor.gameObject);
      foreach (var obj in deleteList)
         DestroyImmediate(obj, true);
      
   }
   public void ClickSyncSetupWidget()
   {
      Selection.activeGameObject = gameObject;
      ToolWindowSettings.Instance.openEditContent = true;
   }
#endif

  
}
