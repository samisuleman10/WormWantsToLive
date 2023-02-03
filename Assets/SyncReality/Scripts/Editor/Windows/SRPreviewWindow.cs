#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ToolWindowSettings;

public class SRPreviewWindow : CustomEditorWindow
    { 
        public static SRPreviewWindow Instance;
        SurrounderModule surrounder;
        private ToolWindowSettings _toolWindowSettings;
        PreviewRenderUtility previewRenderUtility;
        ModuleManager _moduleManager;
        private LayoutArea _layoutArea;
        private WallSplitter _wallSplitter;
        private DesignAccess _designAccess;
        private Compass _compass;
        private const string m_keyboardProfileName = "SyncrealityPreviewWindow";
        
        private ModuleManager ModuleManager
        {
            get
            {
                if (_moduleManager == null)
                    _moduleManager = FindObjectOfType<ModuleManager>();

                return _moduleManager;
            }
        }

        private LayoutArea LayoutArea
        {
            get 
            { 
                if(_layoutArea == null)
                    _layoutArea = FindObjectOfType<LayoutArea>();

                return _layoutArea; 
            }
        }

        private Compass Compass
        {
            get
            {
                if (_compass == null)
                    _compass = FindObjectOfType<Compass>();

                return _compass;
            }
        }
        private DesignAccess DesignAccess
        {
            get
            {
                if (_designAccess == null)
                    _designAccess = FindObjectOfType<DesignAccess>();

                return _designAccess;
            }
        }

        bool focusMode = false;
        bool drawFaceLabels = false;
        public static bool drawCeiling = false;
        
        [MenuItem("Syncreality/Preview Window")]
        [Shortcut("Syncreality/OpenPreviewWindow", KeyCode.P, UnityEditor.ShortcutManagement.ShortcutModifiers.Shift)]
        public static void ShowWindow()
        {
            Instance = GetWindow<SRPreviewWindow>();
            Instance.autoRepaintOnSceneChange = true;
        }

        public void OnEnable()
        { 
            EditorSceneManager.activeSceneChangedInEditMode += (a,b) => CleanMeUp();
            Instance = this;
            _toolWindowSettings = FindObjectOfType<ToolWindowSettings>();
            Instance.autoRepaintOnSceneChange = true;
            wantsMouseEnterLeaveWindow = true; 
        }
        

        private void OnDidOpenScene()
        {
            OnEnable();
        }
         

        public void Initialize()
        {
            if (Instance != null && _toolWindowSettings != null)
            { 
                  // Initializing Exclusive keyboard profile
            ToolShortcutManager.InitKeyboardProfile(m_keyboardProfileName);
            // Key event subscriptions
            ToolShortcutManager.StartListening(OnWasdqPressed, "SRPreviewWindow", KeyCode.W);
            ToolShortcutManager.StartListening(OnWasdqPressed, "SRPreviewWindow", KeyCode.A);
            ToolShortcutManager.StartListening(OnWasdqPressed, "SRPreviewWindow", KeyCode.S);
            ToolShortcutManager.StartListening(OnWasdqPressed, "SRPreviewWindow", KeyCode.D);
            ToolShortcutManager.StartListening(OnWasdqPressed, "SRPreviewWindow", KeyCode.Q);

            Instance.autoRepaintOnSceneChange = true; 
            
           _toolWindowSettings.pru = new PreviewRenderUtility(true);
           previewRenderUtility = _toolWindowSettings.pru;
            previewRenderUtility.camera.clearFlags = CameraClearFlags.Skybox;
            //previewRenderUtility.camera.transform.position = LayoutArea.flatPlane.transform.position + new Vector3(0, 10, -10);
            surrounder = FindObjectOfType<SurrounderModule>();
            previewRenderUtility.lights[0].type = LightType.Directional;
            previewRenderUtility.lights[0].intensity = 2;
            previewRenderUtility.lights[0].transform.forward = new Vector3(0, -1, 0);
            previewRenderUtility.ambientColor = new Color(0.5f,0.5f,0.5f);
        
        previewRenderUtility.camera.transform.position = cameraPos;
        previewRenderUtility.camera.orthographic = false;
        previewRenderUtility.cameraFieldOfView = 60;
        previewRenderUtility.camera.farClipPlane = 1000f;
        previewRenderUtility.camera.nearClipPlane = 0.01f;
        previewRenderUtility.camera.transform.rotation =cameraRot; 

        previewRenderUtility.camera.pixelRect = new Rect(0, 0, position.width, position.height);
        previewRenderUtility.camera.aspect = position.width/ position.height;

        previewRenderUtility.camera.cameraType = CameraType.SceneView;

        previewRenderUtility.camera.ResetWorldToCameraMatrix();

        ResetCameraPosition();
            }
            else
            {
                EditorGUILayout.LabelField("No ToolWindowSettings object detected! " );
                EditorGUILayout.LabelField("Please verify that Tool Scene Objects are present, " );
                EditorGUILayout.LabelField("Then reopen the window or recompile! ");
            }

        }

        private List<Vector3> _northWallPositions;
    ObjectInfo selectedObject = null;
    void OnGUI()
    {
        if (Instance == null)
            Instance = this;
        if (_toolWindowSettings == null)
            _toolWindowSettings = FindObjectOfType<ToolWindowSettings>();  //todo this draws many calls when scene objects not present. Fix & Improve
            if (Instance != null && _toolWindowSettings != null)
            {
                if (Application.isPlaying)
                {
                    ToolShortcutManager.RevertKeyboardProfileToUnityDefault();
                    return;
                }

                if (Event.current.type == EventType.MouseEnterWindow)
                {
                    ToolShortcutManager.SelectKeyboardProfile(m_keyboardProfileName);
                }
                if (Event.current.type == EventType.MouseLeaveWindow)
                {
                    ToolShortcutManager.RevertKeyboardProfileToUnityDefault();
                }      

                if (previewRenderUtility == null || surrounder == null)
                    Initialize();

                if (!hasFocus && (Event.current.keyCode == (KeyCode.LeftControl) || Event.current.keyCode == (KeyCode.RightControl)))
                    ToolShortcutManager.RevertKeyboardProfileToUnityDefault();

                var cam = previewRenderUtility.camera;
                previewRenderUtility.camera.pixelRect = new Rect(0, 0, position.width, position.height);
                previewRenderUtility.camera.aspect = position.width / position.height;
                cam.cameraType = CameraType.SceneView; 
                
                CameraControl();

                DrawObjects();
                if(drawFaceLabels)
                    DrawWallLabels();
                selectedObject = InteractWithObjects(cam); 
            
///               ModuleManager.surrounderModule.previewSurrounder = GUILayout.Toggle(ModuleManager.surrounderModule.previewSurrounder, "Draw Surrounder");
                bool previousFocusMode = focusMode;
                focusMode = GUILayout.Toggle(focusMode, "Focus Camera");
                if (previousFocusMode != focusMode && focusMode) ResetCameraPosition();
                
                drawFaceLabels = GUILayout.Toggle(drawFaceLabels, "Face Labels");

                if (DesignAccess.GetCeilingMeshPrefab() != null)
                {
                    var valBef = surrounder.drawCeiling;
                    surrounder.drawCeiling = GUILayout.Toggle(surrounder.drawCeiling, "Draw Ceiling Mesh");
                    if (valBef != surrounder.drawCeiling)
                        FindObjectOfType<ModuleManager>().Execute();
                }
        
//
            Repaint();
            }
            else
            {
                EditorGUILayout.LabelField("No ToolWindowSettings object detected! " );
                EditorGUILayout.LabelField("Please verify that Tool Scene Objects are present, " );
                EditorGUILayout.LabelField("Then reopen the window or recompile! ");
            }
              
        }

      private void DrawWallLabels()
    {
        var surrounder = FindObjectOfType<SurrounderModule>();
        var posN = worldToPRUPoint((surrounder.getGroundBox()[2] + surrounder.getGroundBox()[3]) * 0.5f + Vector3.up * 15f);
        Rect rectN = new Rect(posN, new Vector2(160, 50));
        var posE = worldToPRUPoint((surrounder.getGroundBox()[1] + surrounder.getGroundBox()[2]) * 0.5f + Vector3.up * 15f);
        Rect rectE = new Rect(posE, new Vector2(160, 50));
        var posS = worldToPRUPoint((surrounder.getGroundBox()[0] + surrounder.getGroundBox()[1]) * 0.5f + Vector3.up * 15f);
        Rect rectS = new Rect(posS, new Vector2(160, 50));
        var posW = worldToPRUPoint((surrounder.getGroundBox()[3] + surrounder.getGroundBox()[0]) * 0.5f + Vector3.up * 15f);
        Rect rectW = new Rect(posW, new Vector2(160, 50));

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 9,
            fontStyle = FontStyle.Bold
        };
        labelStyle.normal.textColor = Color.black;
        EditorGUI.LabelField(rectN, "N", labelStyle);
        EditorGUI.LabelField(rectE, "E", labelStyle);
        EditorGUI.LabelField(rectS, "S", labelStyle);
        EditorGUI.LabelField(rectW, "W", labelStyle);
    }

        private void DrawObjects( )
        {
            var rect = new Rect(0, 0, position.width, position.height);
            previewRenderUtility.BeginPreview(rect, GUIStyle.none);
            if (ToolWindowSettings.Instance.objectsToDrawInPreview == null)
                if (Application.isPlaying == false)
                    ModuleManager.Execute(true );
            
            _northWallPositions = new List<Vector3>();
            foreach (var obj in ToolWindowSettings.Instance.objectsToDrawInPreview)
            {
                if (Event.current.type == EventType.Repaint)
                    drawObject(obj, obj.transform, obj.objectToDraw, obj.includeChildren, obj.overrideMaterial);
                if (obj.objectToFocusOnLeftClick == null)     // is a surroundsync 
                    if (obj.face == WallFace.North) 
                        _northWallPositions.Add(obj.transform.ExtractPosition()); 

            }

        //    Debug.Log("north wall count: " + _northWallPositions.Count);
        //    Debug.Log("objects  count: " + ToolWindowSettings.Instance.objectsToDrawInPreview.Count);
            if (_northWallPositions.Count > 0)
                foreach (var pos in _northWallPositions)
                {
              //      Vector3 highPos   = pos + new Vector3(0, 4, 0);
              //      var transformMatrix = Matrix4x4.TRS(highPos, Quaternion.identity, new Vector3(3,3,3));
              //       previewRenderUtility.DrawMesh(Compass.previewCompassMesh.sharedMesh, transformMatrix, Compass.previewCompassMaterial, 0);

               
                } 

           
            previewRenderUtility.camera.Render( );
            var tex = previewRenderUtility.EndPreview();
            EditorGUI.DrawPreviewTexture(rect, tex);

            // Temporary debug
            // Debug.Log ("Current Camera pos: " + previewRenderUtility.camera.transform.position );
            // Debug.Log ("Current Camera rot: " + previewRenderUtility.camera.transform.rotation );
        }

    private void drawObject(ObjectInfo objInfo, Matrix4x4 matrix, GameObject obj, bool includeChildren = true, Material overrideMaterial = null)
    {
        if (obj != null)
        {
            foreach(var mf in includeChildren ? obj.GetComponentsInChildren<MeshFilter>() : new[] {obj.GetComponent<MeshFilter>()})
            {
                if(mf.sharedMesh == null)
                    continue;
            
                var transformMatrix =  Matrix4x4.TRS(obj.transform.InverseTransformPoint(mf.transform.position).ScaleBy(obj.transform.localScale), mf.transform.localRotation, includeChildren ? mf.transform.lossyScale : Vector3.one);
                if (!obj.name.Contains("Floor") && !obj.GetComponent<Sync>())
                    transformMatrix = Matrix4x4.Translate((new Vector3(-obj.transform.position.x, -obj.transform.position.y, -obj.transform.position.z))) * mf.transform.localToWorldMatrix;
            
                for(int i = 0; i < mf.sharedMesh.subMeshCount; i++)
                {
                    var mat = overrideMaterial;


                    if (mat == null && mf.GetComponent<MeshRenderer>())
                    {
                        mat = mf.GetComponent<MeshRenderer>()?.sharedMaterials[i >= mf.GetComponent<MeshRenderer>().sharedMaterials.Count() ? 0 : i];
                    }
                    if(mat == null && mf.GetComponent<SkinnedMeshRenderer>())
                    {
                        mat = mf.GetComponent<SkinnedMeshRenderer>()?.sharedMaterials[i >= mf.GetComponent<SkinnedMeshRenderer>()?.sharedMaterials.Count() ? 0 : i];
                    }
                    if(mf.gameObject.activeSelf && ((mf.GetComponent<MeshRenderer>() && mf.GetComponent<MeshRenderer>().enabled) || (mf.GetComponent<SkinnedMeshRenderer>() && mf.GetComponent<SkinnedMeshRenderer>().enabled)))
                        previewRenderUtility.DrawMesh(mf.sharedMesh, matrix * transformMatrix * (selectedObject == objInfo ? Matrix4x4.Scale(1.025f * Vector3.one) : Matrix4x4.identity), mat, i);
                }
            }
        } 

    }
    private ObjectInfo InteractWithObjects(Camera cam)
        {
            // Offset mouse position to make handle interaction correct
            var newMousePos = Event.current.mousePosition;
            newMousePos.y *= -1;
            newMousePos.y += position.height;


            ObjectInfo objToUse = null;
            bool foundOne = false;
            float minDistance = float.MaxValue;
            foreach (var obj in ToolWindowSettings.Instance.objectsToDrawInPreview)
            {
                if (obj.objectToDraw == null || obj.objectToFocusOnLeftClick == null)
                    continue;



            float distance = 0;
            var cm = obj.objectToDraw.combinedMesh();
            var scale = obj.transform.ExtractScale().ScaleBy(cm.size);
            Bounds box = new Bounds(obj.transform.ExtractPosition() + ((cm.extents.y) * Vector3.up) * obj.transform.ExtractScale().y - (cm.extents.y - cm.center.y) * Vector3.up * obj.transform.ExtractScale().y, scale);

            
            if (obj.objectToDraw.name.Contains("Wall"))
            {
                var boxPos = box.center - Vector3.up * (cm.center.y * 1f - cm.size.y + cm.extents.y);
                var screenPos = worldToPRUPoint(boxPos);
                //EditorGUI.LabelField(new Rect(screenPos, new Vector2(100, 10)), obj.objectToDraw.name);
                box.center = boxPos;
            }

                if (box.IntersectRay(cam.ScreenPointToRay(newMousePos), out distance))
                {
                    if(distance < minDistance)
                    {
                        minDistance = distance;
                        objToUse = obj;
                        foundOne = true;
                    }
                }
            }
            if(foundOne)
            {
            if (objToUse.objectToDraw == null)
                return null;
            var obj = objToUse;
                    HoverObject(obj);
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && obj.objectToFocusOnLeftClick != null) 
                    ClickObject(obj.objectToFocusOnLeftClick.gameObject); 
                if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && obj.objectToFocusOnRightClick != null) 
                     ClickObject(obj.objectToFocusOnRightClick.gameObject);  
            }
        return objToUse;
        }
        private void HoverObject(ObjectInfo obj)
        {
            var screenPos = worldToPRUPoint(obj.transform.ExtractPosition());
            int offset = 20;
            int panelHeight = 60;
            Rect panelRect = new Rect(offset, position.height - (panelHeight + offset), position.width - offset * 2, panelHeight);
            Rect labelRect = new Rect(offset+5f, position.height  - panelHeight   -panelHeight/3.5f, 150f, panelHeight-5f);
            EditorGUI.DrawRect(panelRect, _toolWindowSettings.HoverPanelColor);

            GUIStyle labelStyle  = new GUIStyle(GUI.skin.label)
            { 
                fontSize = 15,
                fontStyle = FontStyle.Bold
            };
            labelStyle.normal.textColor = Color.black;
            
            string labelString = _toolWindowSettings.GetDebugInfoForMeshDrawInfo(obj);
            
            
            EditorGUI.LabelField(labelRect, labelString, labelStyle); 
        }
        private void ClickObject(GameObject obj)
        {
          _toolWindowSettings.ClickObjectInPreviewWindow(obj);
        }

        private Vector2 worldToPRUPoint(Vector3 worldPos)
        {

            var cam = previewRenderUtility.camera;
            var screenPos = cam.WorldToScreenPoint(worldPos);
            screenPos.y *= -1;
            screenPos.y += position.height;
            return screenPos;
        }

        Quaternion focusRotation = Quaternion.identity;
        float focusDistance = 10;

        Vector3 cameraPos = Vector3.zero;
        Quaternion cameraRot = Quaternion.identity;

        void CameraControl()
        {
            var cam = previewRenderUtility.camera;

            if(focusMode)
            {
                focusDistance = Mathf.Max(focusDistance, 10f);
                var targetPos = Vector3.zero;
                var layoutPoints = DesignAccess.GetStoredLayoutPoints();
                targetPos = LayoutArea.GetAreaCenter();
                cam.transform.position = targetPos + focusRotation * new Vector3(0, 0, -focusDistance);
                cam.transform.LookAt(targetPos, Vector3.up);
            }

            Event e = Event.current;
            switch (e.type)
            {
                case EventType.KeyDown:
                {
                    if (Event.current.keyCode == (KeyCode.U))
                    {
                        Debug.Log("camera.transform.position: " + previewRenderUtility.camera.transform.position + "\n" + "camera.transform.rotation: " + previewRenderUtility.camera.transform.rotation);
                    }
                    // if (Event.current.keyCode == (KeyCode.A)) 
                    //     cam.transform.Translate(-Vector3.right * 1); 
                    // if (Event.current.keyCode == (KeyCode.D)) 
                    //     cam.transform.Translate(Vector3.right * 1); 
                    // if (Event.current.keyCode == (KeyCode.W)) 
                    //     cam.transform.Translate(Vector3.forward * 1); 
                    // if (Event.current.keyCode == (KeyCode.S)) 
                    //     cam.transform.Translate(-Vector3.forward * 1); 
                    // if (Event.current.keyCode == (KeyCode.Q)) 
                    //     previewRenderUtility.lights[0].transform.Rotate(5, 0, 0); 
                    Repaint();
                    break;
                }
                case EventType.MouseDrag:
                {
                    if (e.button == 1)
                    {
                        if(!focusMode)
                        {
                            cam.transform.Rotate(new Vector3(e.delta.y, 0, 0) * 0.2f, Space.Self);
                            cam.transform.Rotate(new Vector3(0, e.delta.x, 0) * 0.2f, Space.World);
                        } else
                        {
                            var rot = focusRotation * new Vector3(e.delta.y, 0, 0);
                            focusRotation = Quaternion.Euler(rot.x, e.delta.x, rot.z) * focusRotation * Quaternion.Euler(0, 0, 0);
                        }
                    }
                    if (e.button == 2 || e.button == 0)
                    {
                        if(!focusMode)
                        {
                            cam.transform.Translate(Vector3.up * e.delta.y * 0.2f);
                            cam.transform.Translate(-Vector3.right * e.delta.x * 0.2f);
                        } else
                        {
                            var rot = focusRotation * new Vector3(e.delta.y, 0, 0);
                            focusRotation = Quaternion.Euler(rot.x, e.delta.x, rot.z) * focusRotation * Quaternion.Euler(0, 0, 0);
                        }
                    }
                    Repaint();
                    break;
                }
                case EventType.ScrollWheel:
                {
                    if (!focusMode)
                        cam.transform.Translate(-Vector3.forward * e.delta.y);
                    else
                        focusDistance *= (1f + e.delta.y * 0.1f);

                    Repaint();
                    break;
                }
            }
        cameraPos = cam.transform.position;
        cameraRot = cam.transform.rotation;
        }

        /// <summary>
        /// Hard Reset Preview Camera Position
        /// </summary>
        private void ResetCameraPosition ()
        {
          Vector3 center = LayoutArea.GetAreaCenter();
        
          previewRenderUtility.camera.transform.position = center + new Vector3(0.0f, 53.6f, -29.9f);
          previewRenderUtility.camera.transform.rotation = new Quaternion (0.5f, 0.0f, 0.0f, 0.9f);
          
          cameraPos = previewRenderUtility.camera.transform.position;
          cameraRot = previewRenderUtility.camera.transform.rotation;
          focusRotation = cameraRot;
          focusDistance = previewRenderUtility.camera.transform.position.y;
        }
        void OnFocus ()
        {
            ToolShortcutManager.SelectKeyboardProfile(m_keyboardProfileName);
        }
        void OnLostFocus ()
        {
            ToolShortcutManager.RevertKeyboardProfileToUnityDefault();
        }
        void OnValidate ()
        {
            ToolShortcutManager.RevertKeyboardProfileToUnityDefault();
        }


        ////////////// START Clutching keys for WASD movement //////////////
        /// Refere to ToolShortcutManager for more information on keybindings
        private List <KeyCode> m_pressedClutchedKeys = new List<KeyCode> ();
        private EditorCoroutine m_clutchedKeys = null;
        /// <summary>
        /// Clutching keys for WASD movement
        /// </summary>
        /// <param name="args"></param>
        private void OnWasdqPressed (CustomShortcutArguments args)
        {
            // Keypress collection
            if (args.stage == ShortcutStage.Begin)
            {
                m_pressedClutchedKeys.Add (args.keyCode);
            }
            else
            {
                m_pressedClutchedKeys.Remove (args.keyCode);
            }
            // Coroutine management
            if (m_clutchedKeys != null && m_pressedClutchedKeys.Count == 0)
            {
                EditorCoroutineUtility.StopCoroutine (m_clutchedKeys);
                m_clutchedKeys = null;
            }
            else if (m_clutchedKeys == null && m_pressedClutchedKeys.Count > 0)
            {
                m_clutchedKeys = EditorCoroutineUtility.StartCoroutine (ClutchKeysCoroutine(), this);
            }
        }
        /// <summary>
        /// Coroutine to keep track of all keys pressed at the same time
        /// </summary>
        /// <returns></returns>
        IEnumerator ClutchKeysCoroutine ()
        {
            while (true)
            {

            yield return null;
            if (m_pressedClutchedKeys.Contains (KeyCode.A)) 
                    previewRenderUtility.camera.transform.Translate(-Vector3.right * 0.2f); 
                if (m_pressedClutchedKeys.Contains (KeyCode.D)) 
                    previewRenderUtility.camera.transform.Translate(Vector3.right * 0.2f); 
                if (m_pressedClutchedKeys.Contains (KeyCode.W)) 
                    previewRenderUtility.camera.transform.Translate(Vector3.forward * 0.2f); 
                if (m_pressedClutchedKeys.Contains (KeyCode.S)) 
                    previewRenderUtility.camera.transform.Translate(-Vector3.forward * 0.2f); 
                if (m_pressedClutchedKeys.Contains (KeyCode.Q)) 
                    previewRenderUtility.lights[0].transform.Rotate(2, 0, 0);
        }
        }

        private void OnDisable()
        {
            CleanMeUp();
        }
        private void OnDestroy()
        {
            CleanMeUp(); 
        }
        private void CleanMeUp()
        { 
            ToolShortcutManager.RevertKeyboardProfileToUnityDefault();
            if(previewRenderUtility != null)
                previewRenderUtility.Cleanup();  
        }
       
         
    }
#endif