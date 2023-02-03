using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
 
#if UNITY_EDITOR
public class ToolWindowSettings : MonoBehaviour
{
     private static ToolWindowSettings _Instance;

     public static ToolWindowSettings Instance
     {
          get
          {
               if (_Instance == null)
                    _Instance = FindObjectOfType<ToolWindowSettings>();
               return _Instance;
          }
          set => _Instance = value;
     }
     
     
     public PreviewRenderUtility pru;
     [Header("Window Settings")]
     public Vector2 MinSize;
     public Vector2 MaxSize;
     [Tooltip("Height of top selection tabs")]
     public int HeaderHeight = 30;
     public string EditorFolderPath; 
     [Tooltip("Amount of RoomLayouts shown in Recent list")]
     public int recentLayoutsAmount = 4;
     public int organizeRowHeight = 20;
     public float organizeColumnHeight = 10;
     [Header("Interaction")]
     [Tooltip("Open Edit tab on Sync/SSync selection or creation")]
     public bool autoSelectEdit;
     [Tooltip("Auto-sort SyncSetups in ControlContent by status")]
     public bool sortSyncSetupList;
     [Tooltip("Exclude DesignArea / LayoutArea selection from above")]
     public bool skipAutoSelectEditForAreas;
     [Tooltip("Show None content in EditUI (includes clicking in Project folders)")]
     public bool selectNoneUI = false;
     [Header("Colors")]
     public Color HeaderColor;
     public Color ContentColor;
     public Color HeaderSelectedColor;
     public Color HoverColor;
     public Color HeaderBorderColor; 
     public Color SyncSetupActiveColor;
     public Color SyncSetupPresentColor;
     public Color SyncSetupStoredColor;
     [Header("ChartView")] 
     public int miniMapWidth = 110;
     public int miniMapHeight = 65;
     public Color32 inputContainerColor;
     public Color32 outputContainerColor;


         [Header("Scene Icons")] 
     public Texture compassWidgetImage;
     public Texture messWidgetImage;
     public Texture storyTellerWidgetImage;
     public Texture syncSetupWidgetImage;
     public Texture syncLinkWidgetImageOn;
     public Texture syncLinkWidgetImageOff;
     public Texture snapWidgetImageOn;
     public Texture snapWidgetImageOff;
     public Texture compassWidgetImageGrey;
     public Texture messWidgetImageGrey;
     public Texture storyTellerWidgetImageGrey;
     public bool useGreyIcons;
     public int iconsSize; 
     public int iconsPaddingX; 
     public int iconsPaddingY;

     [Header("Preview Window")]
     public Color HoverPanelColor;

     [HideInInspector] public bool toolWindowBuilt;
     [HideInInspector] public bool forcePipelineUpdate;// general signal to update pipeline after a relevant change
     [HideInInspector] public int lastKnownIndex = 0;
     [HideInInspector] public bool forceUpdateContentSections = false;  
     [HideInInspector] public bool forceSelectSyncSetupNameField = false;  
     [HideInInspector] public bool forceUpdateControlForSyncLayout = false;  
     [HideInInspector] public bool forceUpdateControlForRoomLayout = false;
     [HideInInspector] public bool openEditContent = false;
     [HideInInspector] public string forceUpdateControlContentName;
    public class ObjectInfo
    {
        public int id;
        public GameObject objectToDraw;
        public Matrix4x4 transform;
        public Transform objectToFocusOnLeftClick;
        public Transform objectToFocusOnRightClick;
        public WallFace face;
        public bool includeChildren;
        public Material overrideMaterial;
        public ObjectInfo(int id, GameObject objectToDraw, Matrix4x4 transform, Transform objectToFocusOnLeftClick, Transform objectToFocusOnRightClick, WallFace face, bool includeChildren = true, Material overrideMaterial = null)
        {
            this.id = id;
            this.objectToDraw = objectToDraw;
            this.transform = transform;
            this.objectToFocusOnLeftClick = objectToFocusOnLeftClick; 
            this.objectToFocusOnRightClick = objectToFocusOnRightClick;
            this.face = face;
            this.includeChildren = includeChildren;
            this.overrideMaterial = overrideMaterial;
        }
    }

    public void SendPipelineUpdateSignal()
    { 
        forcePipelineUpdate = true;
    }
    
    public List<ObjectInfo> objectsToDrawInPreview;

    public string GetDebugInfoForMeshDrawInfo(ObjectInfo obj)
    {
        string str = "";
        str += "ID: " + obj.id;
        str += "\n";
        if(obj.objectToFocusOnLeftClick != null)
        {

            str += "Object: " + (obj.objectToFocusOnLeftClick.name.Contains("WallMini") ? obj.objectToFocusOnLeftClick.GetComponent<WallMini>().mappedSurroundSync.gameObject.name : obj.objectToFocusOnLeftClick.gameObject.name);
        }
        str += "\n";
        if (obj.objectToFocusOnRightClick != null)
            str += "Source: " + obj.objectToFocusOnRightClick.gameObject.name;
        return str;
    }
    

    public void ClickObjectInPreviewWindow(GameObject obj)
    {
        Selection.activeGameObject = obj;  
        SceneView.FrameLastActiveSceneView();
        SceneView.FocusWindowIfItsOpen(typeof(SceneView));
    }

    public void RegisterSyncLayoutSelection(string givenName)
    {
        forceUpdateControlContentName = givenName;
        forceUpdateControlForSyncLayout = true;
    }
    public void RegisterRoomLayoutSelection(string givenName)
    {
        forceUpdateControlContentName = givenName;
        forceUpdateControlForRoomLayout = true;
    }
    
    /// <summary>
    /// This is needed to setup the scene after merging
    /// </summary>
    private void OnValidate()
    {
        if(!ToolShortcutManager.Instance)
        {
            gameObject.AddComponent<ToolShortcutManager>();
        }
    }
}
#endif