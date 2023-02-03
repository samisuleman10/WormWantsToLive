#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.WSA;


// more clicking options: https://answers.unity.com/questions/1271403/onclick-handles-spherecap.html 

 
// synclinks are shown in top bar and can be hover-released to select
// if none selected: only the Link option appears
// if one selected: autoselect it, do show in top row
// if multi selected: autoselect latest created. top line is every SyncLink that it already has   
// a Sync will, if it has at least 1 Link, always have one 'active'. Start now with first in list, maybe later store last active
 
 

// HANDLE IDs
// 0 = MainHandle / LinkHandle / CenterHandle / SyncHandle
// 1 = LinkCreatorHandle
// 2 = CreationIndicatorHandle


class LinkHandle
{
    public int ID;
    public Vector3 position; 
    public float handleSize; 
    public float offsetX;
    public float offsetY;
    public float offsetZ;
    public Color defaultColor;
    public Color activeColor; 
}

[CustomEditor(typeof(Sync))]
public class SyncEditor : Editor
{
    private Sync _sync;
    private SyncLink activeLink;
    private List<LinkHandle> handles = new List<LinkHandle>();
    private List<LinkHandle> existorHandles = new List<LinkHandle>();
    private SyncMaster _syncMaster;
    private DesignArea _designArea;
    
    // Handle States
    private bool hoveringObject = false;
    private bool hoveringSurrounderObject = false;
    private GameObject hoveredObject;
    private GameObject hoveredSurrounderObject; 
    
    // Handle Positions
    private float mainHandleX = 0f;
    private float mainHandleY = 1.5f;
    private float mainHandleZ = 0f;
    private float creatorHandleX = 0f;
    private float creatorHandleY = 2.5f;
    private float creatorHandleZ = 0f;   
    private float existorHandleX = 0f;
    private float existorHandleY = 1f;
    private float existorHandleZ = 0f;   
    private float hoverHandleX = 0f;
    private float hoverHandleY = 1f;
    private float hoverHandleZ = 0f;

    // Handle Colors
    private Color mainDefaultColor = Color.green;
    private Color mainActiveColor = Color.gray;
    private Color creatorDefaultColor = Color.red;
    private Color creatorActiveColor = Color.magenta;
    private Color hoverDefaultColor = Color.blue;
    private Color hoverActiveColor = Color.gray;
    private Color existorHandleColor = Color.cyan;
    //   private Color creatorActiveColor = new Color(222, 111, 44, 255);
 
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("handleOffset"));
        serializedObject.ApplyModifiedProperties();
        
        EditorGUI.BeginChangeCheck();
    //    ((Sync)target).classification = (Classification)EditorGUILayout.MaskField("Classification Flag", (int)((Sync)target).classification, Enum.GetNames(typeof(Classification)));
        
        DrawDefaultInspector();

        if (target != null && GUILayout.Button("Set Bounds to Mesh Size"))
            ((Sync)target).CreateCombinedMesh();
    }
 
    private void OnEnable()
    { 
        ToolShortcutManager.StartListening(ToggleAlternateSnapLayoutMode, context: "SceneView", KeyCode.C);
    }
    private void OnDisable()
    {
        ToolShortcutManager.StopListening(ToggleAlternateSnapLayoutMode, context: "SceneView", KeyCode.C);
    }

    #region OnSceneGUI

    BoxBoundsHandle m_BoundsHandle = new BoxBoundsHandle();
    private void OnSceneGUI()
     {   
         _sync = (Sync) target;
         if (_sync == null) return;
         if (_syncMaster == null)
             _syncMaster = FindObjectOfType<SyncMaster>();
         if (_designArea == null)
             _designArea = FindObjectOfType<DesignArea>();
         handles = new List<LinkHandle>();
         existorHandles = new List<LinkHandle>(); 

      
      if (_designArea.designMode == DesignMode.SyncLinking) 
          HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));  
      
     

      if (_syncMaster.useHandles)
      {
          if (_designArea.designMode == DesignMode.SyncLinking)
          {
              DrawSyncLinkLines();
             //  CreateHandles(_sync.transform); 
              // DrawHandles();
          } 
      }
      
         CheckForInput();
        //    ButtonLogic();
        if(_sync.hasFreeArea)
            DrawFreeSpaceRect();
        if (_designArea.designMode != DesignMode.SyncLinking)
             DrawBoundsBox();
        if(_sync.useSizes)
            DrawMinAndMaxBoxes();

        // if (_designArea._designMode == DesignMode.Snap)
            Snap(); 
     //   else 
      //      Tools.current = lastTool;
     }

    private void DrawSyncLinkLines()
    {
        Handles.color = Color.white; 
        DrawActiveSyncLinkLines();
        Handles.color = Color.grey;
        DrawPassiveSyncLinkLines();
        Handles.color = Color.white;
        DrawHoverLines(); 
    }

    private Vector3 GetObjectMidVerticalPosition(GameObject givenObject )
    {
        Vector3 returnPos =givenObject.transform.position;;

        if (_syncMaster.offsetSyncLinkLinesHeight == false)
            return returnPos;
        if (givenObject.GetComponent<Sync>() != null)
            returnPos += new Vector3(0, givenObject.GetComponent<Sync>().bounds.extents.y /2f + givenObject.GetComponent<Sync>().bounds.center.y/2f, 0);
        else         if (givenObject.GetComponent<SurroundSync>() != null)
            returnPos += new Vector3(0, givenObject.GetComponent<SurroundSync>().bounds.extents.y /2f + givenObject.GetComponent<SurroundSync>().bounds.center.y/2f, 0);
        
        return returnPos;
    }
    private void DrawActiveSyncLinkLines()
    {
        Vector3 syncOffsetPosition = GetObjectMidVerticalPosition( _sync.gameObject);
        List<Vector3> primaryTargetPositions = new List<Vector3>();
        for (int i = 0; i < _sync.GetSyncLinksFromComponents().Count; i++) 
            primaryTargetPositions.Add( GetObjectMidVerticalPosition(  _sync.GetSyncLinksFromComponents()[i].linkedObject.gameObject)); 
        if (primaryTargetPositions.Count > 0)
            foreach (var pos in primaryTargetPositions)
                if (hoveringObject == true )
                  {
                      if (GetObjectMidVerticalPosition(hoveredObject.gameObject) != pos)
                          Handles.DrawAAPolyLine(syncOffsetPosition, pos);
                  }
                  else if (hoveringSurrounderObject == true)
                {
                    if (GetObjectMidVerticalPosition(hoveredSurrounderObject.gameObject) != pos)
                        Handles.DrawAAPolyLine(syncOffsetPosition, pos);
                }
                else
                   Handles.DrawAAPolyLine(syncOffsetPosition, pos); 
    }
    private void DrawPassiveSyncLinkLines()
    {  
        Vector3 syncOffsetPosition = GetObjectMidVerticalPosition( _sync.gameObject); 
        List<Vector3> passiveTargetPositions = new List<Vector3>(); 
        for (int i = 0; i < _sync.GetPassiveSyncLinks().Count; i++) 
            passiveTargetPositions.Add(GetObjectMidVerticalPosition(_sync.GetPassiveSyncLinks()[i].parentSync.gameObject)); 
        if (passiveTargetPositions.Count > 0)
            foreach (var pos in passiveTargetPositions)
                Handles.DrawAAPolyLine(syncOffsetPosition, pos); 
    }

    private void DrawHoverLines()
    {
        Vector3 syncOffsetPosition = GetObjectMidVerticalPosition( _sync.gameObject);  
        if (hoveringObject == true )
        {
            Vector3 targetPos = GetObjectMidVerticalPosition(hoveredObject.gameObject);
            
            if (_sync.GetSyncLinksFromComponents().Count == 0)
            {
                // Selected sync has no SyncLinks, so draw a create-line
                Handles.DrawDottedLine(syncOffsetPosition, targetPos, 3);
            }
            
            else if (_sync.HasSyncLinkForGameObject(hoveredObject))
            {
                // Selected sync has a synclink for this object, so draw a delete-line
                Handles.color = Color.red;
                Handles.DrawDottedLine(syncOffsetPosition, targetPos, 3); 
            } 
            else
            {
                /*bool allowLine = true;
                foreach (var syncLink in _sync.GetPassiveSyncLinks().Where(syncLink => syncLink.parentSync == _sync))
                    allowLine = false; 
                
                // Selected sync has no SyncLinks for this object, so draw a create-line
                if (allowLine == true)*/
                if (_sync.HasPassiveSyncLinkForGameObject(hoveredObject) == false)
                      Handles.DrawDottedLine(syncOffsetPosition, targetPos, 3);
            }
        } else if (hoveringSurrounderObject == true)
        {
            Vector3 targetPos = GetObjectMidVerticalPosition(hoveredSurrounderObject.gameObject);

            if (_sync.GetSyncLinksFromComponents().Count == 0)
            {
                // Selected sync has no SyncLinks, so draw a create-line
                Handles.DrawDottedLine(syncOffsetPosition, targetPos, 3);
            }

            else if (_sync.HasSyncLinkForGameObject(hoveredSurrounderObject))
            {
                // Selected sync has a synclink for this object, so draw a delete-line
                Handles.color = Color.red;
                Handles.DrawDottedLine(syncOffsetPosition, targetPos, 3);
            }
            else
            {
                // Selected sync has no SyncLinks for this object, so draw a create-line
                Handles.DrawDottedLine(syncOffsetPosition, targetPos, 3);
            }
        }
    }
    private void CreateHandles(Transform rt)
    {
        
        _sync.handleHeight = mainHandleY + _sync.handleOffset; 
        handles.Add(CreateLinkHandle(0,rt.position, new Vector3(  mainHandleX, _sync.handleHeight, mainHandleZ),_sync.handleSize, mainDefaultColor, mainActiveColor)); 
        handles.Add( CreateLinkHandle(1,rt.position, new Vector3(  creatorHandleX, creatorHandleY + _sync.handleOffset, creatorHandleZ),_sync.handleSize, creatorDefaultColor, creatorActiveColor));
        if (hoveringObject)
            handles.Add( CreateLinkHandle(2,hoveredObject.transform.position, new Vector3(  hoverHandleX, hoverHandleY+ _sync.handleOffset, hoverHandleZ),_sync.handleSize, hoverDefaultColor, hoverActiveColor));
        if (hoveringSurrounderObject)
        { 
        //    Debug.Log("drawing at " + _designArea.wallSplitter.GetWallMiniForSurroundSyncID(hoveredSurrounderObject.GetComponent<SurroundSync>().ID).transform.position);
           handles.Add( CreateLinkHandle(2,hoveredSurrounderObject.GetComponent<SurroundSync>().transform.position, new Vector3(  hoverHandleX, hoverHandleY+ _sync.handleOffset, hoverHandleZ),_sync.handleSize, hoverDefaultColor, hoverActiveColor));
        //    handles.Add( CreateLinkHandle(2,_designArea.wallSplitter.GetWallMiniForSurroundSyncID(hoveredSurrounderObject.GetComponent<SurroundSync>().ID).transform.position, new Vector3(  hoverHandleX, hoverHandleY+ _sync.handleOffset, hoverHandleZ),_sync.handleSize, hoverDefaultColor, hoverActiveColor));

        }

        if (_sync.GetSyncLinksFromComponents().Count == 0) return;
        for (int i = 0; i < _sync.GetSyncLinksFromComponents().Count; i++) 
            existorHandles.Add( CreateLinkHandle(i+100,_sync.GetSyncLinksFromComponents()[i].linkedObject.transform.position, new Vector3(  existorHandleX, existorHandleY+ _sync.handleOffset, existorHandleZ),_sync.handleSize, existorHandleColor, existorHandleColor));
    }
     private void DrawHandles()
     {
         
         // Main Handle ------- 
      //   DrawHandle(0, true);
     
         DrawExistingLinkHandles();

         
     
         // Create Handle ------- 
             DrawHandle(1, true);

         // Hover Handle
         if (hoveringObject == true || hoveringSurrounderObject == true)
         { 
             DrawHandle(2, true); 
         }
     }
       
     #region LinkHandles
     private LinkHandle CreateLinkHandle(int id, Vector3 parentPos, Vector3 handlePos, float size , Color colDefault, Color colActive )
     {
         return new LinkHandle
         {
             ID = id,
             handleSize = size  ,
             offsetX =  handlePos.x,
             offsetY = handlePos.y,
             offsetZ = handlePos.z,
             position = new Vector3(parentPos.x+ handlePos.x, parentPos.y+handlePos.y, parentPos.z+handlePos.z),
             defaultColor = colDefault,
             activeColor = colActive
            
         }; 
     }
     private void DrawHandle(int id, bool localHandle)
     {
         Transform rt = _sync.transform;
         LinkHandle givenHandle = null;
         if (localHandle)
             givenHandle =   handles[id];
         else 
             givenHandle =   existorHandles[id-100];

     
    
         if (id == 1) 
             Handles.color =  _designArea.designMode == DesignMode.SyncLinking   == false ? handles[id].defaultColor : handles[id].activeColor;  
         else if (id == 2)
         {
             if (_sync.GetSyncLinksFromComponents().Count == 0)
                 Handles.color = handles[id].defaultColor;
             else if (_sync.HasSyncLinkForGameObject(hoveredObject) || _sync.HasSyncLinkForGameObject(hoveredSurrounderObject))
                 Handles.color = handles[id].activeColor;
             else 
                 Handles.color = handles[id].defaultColor; 
         } 
         else if (id > 99) // existorhandles
             Handles.color = existorHandleColor; 
    
         if (Event.current.type == EventType.Repaint)
             Handles.SphereHandleCap(givenHandle.ID, givenHandle.position, rt.rotation * Quaternion.LookRotation(Vector3.up), givenHandle.handleSize, EventType.Repaint );
         if (Event.current.type == EventType.Layout) 
             Handles.SphereHandleCap(givenHandle.ID, givenHandle.position, rt.rotation * Quaternion.LookRotation(Vector3.up), givenHandle.handleSize, EventType.Layout );
  
     }

     #endregion
 
    
     private void DrawLinkCreatorHandle()
     {
         // a single handle at the top of the menu that can be clicked and dragged on top of a gameobject to create a SyncLink to it
         DrawHandle(1, true);  
     }
     private void DrawExistingLinkHandles()
     {
         // one handle for each SyncLink, directly above the linked object
         foreach (LinkHandle linkHandle in existorHandles)
         {  
             DrawHandle(linkHandle.ID, false);
         }
     }
     
     
     private void DrawLinkEditorHandles()
     {
         // when a synclink is selected; draw necessary design handles on the sides / bottom of the SyncHandle
     }

     
     private void CheckForInput()
     {
        
        // if (Event.current is {isKey: true, keyCode: KeyCode.C})
      
         if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
         {
             if (_syncMaster.useHandles)
             {
                 
                 if (_designArea.designMode == DesignMode.SyncLinking) 
                 { 
                     if (hoveringObject == true)
                         ReleaseCreateOnHoveredObject(hoveredObject); 
                     else      if (hoveringSurrounderObject == true)
                         ReleaseCreateOnHoveredSurrounderObject(hoveredSurrounderObject); 
                     else 
                         DisableAllModes();
                 } 
             }
       

         }
         if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
         {
             if (_syncMaster.useHandles)
             {
                 if (_designArea.designMode == DesignMode.SyncLinking) 
                 {
                   
                 }
             }

        
           
         }
         // CreateMode -------
      
             if (_designArea.designMode == DesignMode.SyncLinking) 
             {
                 if (Event.current.type == EventType.MouseMove)
                 {
               
                     GameObject hoveredPickObject = HandleUtility.PickGameObject(Event.current.mousePosition, false);
                     if (hoveredPickObject != null)
                     {
                         if (hoveredPickObject.GetComponentInParent<Sync>() != null)
                         {
                             if (hoveredPickObject.GetComponentInParent<Sync>() != _sync)
                                 HoverSyncObject(hoveredPickObject);
                         } 
                         if (hoveredPickObject.GetComponentInParent<SurroundSync>() != null)
                             HoverSurroundSyncObject(hoveredPickObject);

                         if (hoveredPickObject.gameObject.name == "FlexFloor")
                         {
                             hoveringObject = false;
                             hoveringSurrounderObject = false;
                         }
                        
                     }
                     else
                     {
                         hoveringObject = false;
                         hoveringSurrounderObject = false;
                     } 
                 } 
             }
         
        
     }

  

     #endregion

     
     
     private void ButtonLogic()
     {
         float buttonHeight = 32;
         float buttonPadding = 15;
         float buttonY = buttonPadding;
 
         Rect GetRect(float y) => new Rect(buttonPadding, y, 120, buttonHeight);
 
         void AddBtn(string v, Action pressed)
         {
             if (GUI.Button(GetRect(buttonY), v))
                 pressed();
             buttonY += buttonHeight + buttonPadding;
         }
         Handles.BeginGUI();
        
         GUI.backgroundColor = Color.clear; 
             GUI.backgroundColor = Color.cyan; 
         AddBtn("" + _designArea.designMode, ClickButton);

       
         Handles.EndGUI();
     }
     private void ClickButton()
     {
         _designArea.ToggleSyncLinkMode();
     }
     private void ClickButtonDeleteSyncLink()
     {
          
     }

 


     #region Interaction
    
   
       
   private void HoverSyncObject(GameObject obj)
   {
       hoveredObject = obj.GetComponentInParent<Sync>().gameObject;
       hoveringObject = true;
       hoveringSurrounderObject = false;
   }
   private void HoverSurroundSyncObject (GameObject obj)
   {
   //    Debug.Log("hover parent " + obj.GetComponentInParent<SurroundSync>().gameObject.name + " with pos " + obj.GetComponentInParent<SurroundSync>().gameObject.transform.position);
       hoveredSurrounderObject = obj.GetComponentInParent<SurroundSync>().gameObject; 
       hoveringSurrounderObject = true;
       hoveringObject = false;
   }
  
   private void ReleaseCreateOnHoveredObject( GameObject givenObject)
   { 
       if (_sync.GetSyncLinksAsGameObjects().Contains(givenObject) )
           _sync.DestroySyncLinkObject(givenObject);
       else if (_sync.GetPassiveSyncLinksAsGameObjects().Contains(givenObject))
       {
           // do nothing when attempting to link onto an existing passive SyncLink
       }
       else
           _sync.CreateSyncLink(givenObject);
       
//     _designArea.ToggleSyncLinkMode();

   } 
   private void ReleaseCreateOnHoveredSurrounderObject(GameObject givenObject)
   {
       SurroundSync sSync = givenObject.GetComponent<SurroundSync>();
       if (_sync.GetSyncLinksAsGameObjects().Contains(sSync.gameObject))
           _sync.DestroySyncLinkObject(sSync.gameObject);
       else if (_sync.GetPassiveSyncLinksAsGameObjects().Contains(givenObject))
       {
           // do nothing when attempting to link onto an existing passive SyncLink
       }
       else
           _sync.CreateSyncLink(sSync.gameObject, true);
     //  _designArea.ToggleSyncLinkMode();
   }
   
    
    
    
 
    
 
    // from mouse 1 up if no hovers
    // from EnableCreateMode()
    private void DisableAllModes()
    {
        hoveringObject = false;
        SceneView.RepaintAll();
    }
   
    

    #endregion
   
     
  private void ToggleAlternateSnapLayoutMode(CustomShortcutArguments args)
  {
      _designArea.ToggleSnapMode();
  }

    private void Snap()
    {
        // Catching input events for snap activation
        // If escape is pressed then interrupts the snap mode
        if(Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Escape)
        {
            _designArea.ToggleSnapMode(true);
        }
        // If rightclick is pressed we continue to enable the snap mode
        // if(Event.current.type != EventType.MouseDown && Event.current.button != 1)
        if(_designArea._snapping == false)
        {
            return;
        }
        Tools.current = Tool.None;
        if (Selection.activeGameObject == _sync.gameObject)
        // if (Selection.activeGameObject == _mockPhysical.gameObject && Event.current.keyCode == KeyCode.B)
        {
            List<Collider> addedCols = new List<Collider>();

            foreach (var otherSync in _designArea.GetSyncs())
            {
                if (otherSync != _sync)
                {
                    var b = otherSync.gameObject.AddComponent<BoxCollider>();
                    b.center = otherSync.bounds.center;
                    b.size = otherSync.bounds.size;

                    addedCols.Add(b);
                }
            }

            List<Collider> colliders = new List<Collider>();
            //if (_sync.GetComponent<Collider>())
            {
                colliders = _sync.GetComponentsInChildren<Collider>().ToList();
                foreach(var collider in colliders)
                {
                    collider.enabled = false;
                }

            }

            var mouse = Event.current.mousePosition;
            mouse.y = Mathf.Abs(-mouse.y);
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                var normalizedExtends = _sync.bounds.extents;
                if (Vector3.Dot(normalizedExtends, hit.normal) < 0)
                    normalizedExtends = -normalizedExtends;
                _sync.transform.position = hit.point + Vector3.Project(normalizedExtends, hit.normal);
                //normalizedExtends = _mockPhysical.GetComponent<MeshRenderer>().bounds.extents;
                var offset = Vector3.zero;
                offset += offsetToDirection(_sync.transform, Vector3.up, normalizedExtends);
                offset += offsetToDirection(_sync.transform, Vector3.down, normalizedExtends);
                offset += offsetToDirection(_sync.transform, Vector3.left, normalizedExtends);
                offset += offsetToDirection(_sync.transform, Vector3.right, normalizedExtends);
                offset += offsetToDirection(_sync.transform, Vector3.forward, normalizedExtends);
                offset += offsetToDirection(_sync.transform, Vector3.back, normalizedExtends);
                offset -= _sync.bounds.center;
                _sync.transform.position += offset;
            }
            //if (_sync.GetComponent<Collider>())
            {

                foreach (var collider in colliders)
                {
                    collider.enabled = true;
                }
            }

            int maxCount = 1000;
            while (addedCols.Count > 0 && maxCount > 0)
            {
                DestroyImmediate(addedCols[0]);
                addedCols.RemoveAt(0);
                maxCount--;
            }
        }
        else
        {
            //if (_sync.GetComponent<Collider>())
            {
                var colliders = _sync.GetComponentsInChildren<Collider>().ToList();
                foreach (var collider in colliders)
                {
                    collider.enabled = true;
                }

            }
        }
        }


    Vector3 offsetToDirection(Transform transform, Vector3 direction, Vector3 extends)
    {
        //extends = transform.TransformDirection(extends);
        var projection = Vector3.Project(extends, direction);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, projection.magnitude))
        {
            return direction.normalized * (hit.distance - projection.magnitude);
        }
        return Vector3.zero;
    }

    void DrawFreeSpaceRect()
    {
        Sync t = target as Sync;
        var tr = t.transform;
        var pos = tr.position;
        var rotatedMatrix = Handles.matrix
                                * Matrix4x4.TRS
                                (
                                 tr.position,
                                    tr.rotation, Vector3.one);
        using (new Handles.DrawingScope(rotatedMatrix))
        {
            Rect rect = t.freeArea;

            Vector3 midPos = new Vector3(rect.position.x, 0, rect.position.y);
            var p1 = midPos + new Vector3(rect.size.x, 0, rect.size.y) * 0.5f;
            var p2 = midPos + new Vector3(-rect.size.x, 0, rect.size.y) * 0.5f;
            var p3 = midPos + new Vector3(-rect.size.x, 0, -rect.size.y) * 0.5f;
            var p4 = midPos + new Vector3(rect.size.x, 0, -rect.size.y) * 0.5f;

            Handles.DrawDottedLine(p1, p2, 10f);
            Handles.DrawDottedLine(p2, p3, 10f);
            Handles.DrawDottedLine(p3, p4, 10f);
            Handles.DrawDottedLine(p4, p1, 10f);

            EditorGUI.BeginChangeCheck();
            var newPos1 = Handles.FreeMoveHandle(p1, Quaternion.identity, 0.2f, Vector3.zero, Handles.SphereHandleCap);
            if(EditorGUI.EndChangeCheck())
            {
                p1 = newPos1;
                p2 = new Vector3(p2.x, 0, newPos1.z);
                p4 = new Vector3(newPos1.x, 0, p4.z);

                var newRect = new Rect((p1 + p2 + p3 + p4).flatten() * 0.25f, new Vector2(p1.x - p2.x, p1.z - p4.z));
                t.freeArea = newRect;
                EditorUtility.SetDirty(t);
                FindObjectOfType<ToolWindowSettings>().SendPipelineUpdateSignal();
            }
            EditorGUI.BeginChangeCheck();
            var newPos2 = Handles.FreeMoveHandle(p2, Quaternion.identity, 0.2f, Vector3.zero, Handles.SphereHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                p2 = newPos2;
                p1 = new Vector3(p1.x, 0, newPos2.z);
                p3 = new Vector3(newPos2.x, 0, p3.z);

                var newRect = new Rect((p1 + p2 + p3 + p4).flatten() * 0.25f, new Vector2(p1.x - p2.x, p2.z - p3.z));
                t.freeArea = newRect;
                EditorUtility.SetDirty(t);
                FindObjectOfType<ToolWindowSettings>().SendPipelineUpdateSignal(); 
            }

            EditorGUI.BeginChangeCheck();
            var newPos3 = Handles.FreeMoveHandle(p3, Quaternion.identity, 0.2f, Vector3.zero, Handles.SphereHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                p3 = newPos3;
                p4 = new Vector3(p4.x, 0, newPos3.z);
                p2 = new Vector3(newPos3.x, 0, p2.z);

                var newRect = new Rect((p1 + p2 + p3 + p4).flatten() * 0.25f, new Vector2(p1.x - p3.x, p2.z - p3.z));
                t.freeArea = newRect;
                EditorUtility.SetDirty(t);
                FindObjectOfType<ToolWindowSettings>().SendPipelineUpdateSignal(); 
            }

            EditorGUI.BeginChangeCheck();
            var newPos4 = Handles.FreeMoveHandle(p4, Quaternion.identity, 0.2f, Vector3.zero, Handles.SphereHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                p4 = newPos4;
                p3 = new Vector3(p3.x, 0, newPos4.z);
                p1 = new Vector3(newPos4.x, 0, p1.z);

                var newRect = new Rect((p1 + p2 + p3 + p4).flatten() * 0.25f, new Vector2(p4.x - p3.x, p1.z - p4.z));
                t.freeArea = newRect;
                EditorUtility.SetDirty(t);
                FindObjectOfType<ToolWindowSettings>().SendPipelineUpdateSignal();
            }

        }
    }
    void DrawBoundsBox()
    {
        //Min Bounding Box
        Sync t = target as Sync;
        var tr = t.transform;
        var pos = tr.position;

        Handles.color = Color.green;
        var rotatedMatrix = Handles.matrix
                                * Matrix4x4.TRS
                                (
                                 tr.position,
                                    tr.rotation, Vector3.one);
        using (new Handles.DrawingScope(rotatedMatrix))
        {
            m_BoundsHandle.center = t.bounds.center;
            m_BoundsHandle.size = t.bounds.size;
            EditorGUI.BeginChangeCheck();
            m_BoundsHandle.DrawHandle();
            if (EditorGUI.EndChangeCheck())
            { 
                Undo.RecordObject(t, "Changed Bounds");
                t.bounds.center = m_BoundsHandle.center;
                t.bounds.size = m_BoundsHandle.size; 
            }
        }
    }

    void DrawMinAndMaxBoxes()
    {
        //Min Bounding Box
        Sync t = target as Sync;
        var tr = t.transform;
        var pos = tr.position;

        Handles.color = Color.blue;
        var rotatedMatrix = Handles.matrix
                                * Matrix4x4.TRS
                                (tr.rotation *
                                 t.bounds.center.ScaleBy(t.minSize) +
                                 tr.position,
                                    tr.rotation, Vector3.one);
        using (new Handles.DrawingScope(rotatedMatrix))
        {
            m_BoundsHandle.center = Vector3.zero;
            m_BoundsHandle.size = (t.bounds.extents * 2.01f).ScaleBy(t.minSize);
            EditorGUI.BeginChangeCheck();
            m_BoundsHandle.DrawHandle();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(t, "Changed Min Scale");
                t.minSize = m_BoundsHandle.size.ScaleBy((t.bounds.extents * 2.01f).invert());
                EditorUtility.SetDirty(t);
            }
        }

        Handles.color = Color.red;
        rotatedMatrix = Handles.matrix
                               * Matrix4x4.TRS
                               (tr.rotation *
                                t.bounds.center.ScaleBy(t.maxSize) +
                                tr.position,
                                   tr.rotation, Vector3.one);
        using (new Handles.DrawingScope(rotatedMatrix))
        {
            m_BoundsHandle.center = Vector3.zero;
            m_BoundsHandle.size = (t.bounds.extents * 2.01f).ScaleBy(t.maxSize);
            EditorGUI.BeginChangeCheck();
            m_BoundsHandle.DrawHandle();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(t, "Changed Max Scale");
                t.maxSize = m_BoundsHandle.size.ScaleBy((t.bounds.extents * 2.01f).invert());
                EditorUtility.SetDirty(t);
            }
        }
    }

   
    
     
     
     
     #region Helpers
     private Vector2 ConvertTo2DCoordinates(Vector3 originalVector)
     {
         Vector2 returnVector = Vector2.zero; 
        
         var transform = SceneView.currentDrawingSceneView.camera.transform;
         Vector3 distanceFromCam = new Vector3(transform.position.x, transform.position.y, 0);
         Plane plane = new Plane(Vector3.forward, distanceFromCam);
         Ray ray = HandleUtility.GUIPointToWorldRay(originalVector);
         if (plane.Raycast(ray, out var enter))
         { 
             Vector3 pos = ray.GetPoint(enter);
             returnVector.x = pos.x;
             returnVector.y = pos.y;
         }
         return returnVector;
     }
   

     private bool CheckForHandleHover(LinkHandle linkHandle)
     {
         bool returnBool = false;
         // collider creation
         _sync.gameObject.AddComponent<SphereCollider>();
         SphereCollider coll = _sync.gameObject.GetComponent<SphereCollider>();  
         coll.center = new Vector3(linkHandle.offsetX, linkHandle.offsetY, linkHandle.offsetZ);  
         coll.radius = linkHandle.handleSize/2f; 

         // raycast logic
         Vector3 mousePosition = Event.current.mousePosition;
         Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition); 
         RaycastHit hit;
         if (coll.Raycast(ray, out hit, Mathf.Infinity))
             returnBool = true;
         
         DestroyImmediate(coll); 
         return returnBool;
     }

     #endregion

      
}



#endif // UNITY_EDITOR