#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Syncreality.Playfield;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static TMPro.EditorUtilities.TMP_EditorCoroutine;
using Object = UnityEngine.Object;


public enum SelectionMode
    {
        None, 
        GameObject, // REMOVE ?
        DesignArea,
        WallPiece,
        SurroundSync,
        Ceiling,
        Sync,
        Playfield,
        MockPhysical,
        LayoutArea,
        Compass,
        WallNumber, 
        MessMaker ,
        StoryTeller
    }
    
    // Responsible for Filler editing and management
 
    public class ToolWindow : EditorWindow
    {

        private ModuleManager _moduleManager;

        private ModuleManager ModuleManager
        {
            get
            {
                if (_moduleManager == null)
                    _moduleManager = FindObjectOfType<ModuleManager>();
                return _moduleManager;
            }
        }
        // REFERENCE
        private DesignArea _designArea;
        private LayoutArea _layoutArea;
        private StoryTeller _storyTeller;
        private DesignAccess _designAccess;
        private ToolWindowSettings _toolWindowSettings; 
        private EditContent _editContent = new EditContent();
        private ControlContent _controlContent = new ControlContent();
        private MatchContent _matchContent = new MatchContent();
        public static ToolWindow Instance;
        
        // STRUCTURE
        private VisualElement _baseRoot;
        private VisualElement _headerRoot;
        private VisualElement _contentRoot;
        private List<VisualElement> _headerContainers = new List<VisualElement>();
        private List<VisualElement> _contentContainers = new List<VisualElement>();
        
        // STATE 
        private VisualElement _selectedHeader;
        private SelectionMode _selectionMode;
        private GameObject _selectedGameobject;
        private GameObject _lastSelectedGameobject;
        private int _lastSelectedIndex;
        private bool _doneBuilding = false;
      // private bool _inAlternateMode; // disused for now; just toggle individual parts specifically (currently: only LayoutArea's LayoutEditMode)
        
        // VARIABLES
        private List<string> forbiddenNames;

    public DesignArea DesignAreaProp
    {
        get
        {
            if (_designArea == null)
                _designArea = FindObjectOfType<DesignArea>();
            return _designArea;
        }
    }

    public LayoutArea LayoutAreaProp
    {
        get
        {
            if (_layoutArea == null)
                _layoutArea = FindObjectOfType<LayoutArea>();
            return _layoutArea;
        }
    }
    public StoryTeller StoryTellerProp
    {
        get
        {
            if (_storyTeller == null)
                _storyTeller = FindObjectOfType<StoryTeller>();
            return _storyTeller;
        }
    }
    public ToolWindowSettings ToolWindowSettingsProp
    {
        get
        {
            if (_toolWindowSettings == null)
                _toolWindowSettings = FindObjectOfType<ToolWindowSettings>();
            return _toolWindowSettings;
        }
    }

    private void UpdateForbiddenNames()
        {
            forbiddenNames = new List<string>
            {
                "Main Camera",  
                "MockPhysicals", 
                "Main Camera",   
                "MockPhysicalParent", 
                "SurroundSyncCreatorDetector",
                "WallSplitter",
                "SkyFiller",
                "WallSplitter", 
                "SyncMaster",
                "ToolWindowSettings", 
                "SyncCreation",
                "CreationPlane", 
                "Pipeline",
                "Tools",
                "Scanner",
                "ToolWindowSettings",
                "FlatPlane", 
                "FloorProxy",
                "CeilingProxy"
            };
        }
        
        [MenuItem("Syncreality/Tool Window")]
        [Shortcut("Syncreality/OpenToolWindow", KeyCode.T, UnityEditor.ShortcutManagement.ShortcutModifiers.Shift)]
        public static void ShowWindow()
        {
          //  Debug.Log("ShowWindow"); 
            ToolWindow toolwnd = GetWindow<ToolWindow>();
            toolwnd.titleContent = new GUIContent("Tool Window");
            if (toolwnd.ToolWindowSettingsProp != null)
            {
                toolwnd.minSize = toolwnd.ToolWindowSettingsProp.MinSize;
                toolwnd.maxSize = toolwnd.ToolWindowSettingsProp.MaxSize;
            } 
            toolwnd.autoRepaintOnSceneChange = true; 
        }
        public static void ShowWindow(CustomShortcutArguments args)
        {
            ShowWindow();
        }
         
        private void OnScene(SceneView sceneview)
        {
            #if UNITY_EDITOR
            if (DesignAreaProp == null)
                return; 
            
            int topIconCount = 0;
            Rect compassWidgetRect = new Rect(45, ToolWindowSettingsProp.iconsPaddingY, ToolWindowSettingsProp.iconsSize, ToolWindowSettingsProp.iconsSize);
            Texture icon = _toolWindowSettings.compassWidgetImage;
            if (_toolWindowSettings.useGreyIcons)
                icon = _toolWindowSettings.compassWidgetImageGrey;
            GUIContent compassContent = new GUIContent {image = icon};
            compassContent.tooltip = "Compass";
            topIconCount++;
            Rect messWidgetRect  = new Rect(40 + ToolWindowSettingsProp.iconsPaddingX * topIconCount, ToolWindowSettingsProp.iconsPaddingY, ToolWindowSettingsProp.iconsSize, ToolWindowSettingsProp.iconsSize);
            icon = _toolWindowSettings.messWidgetImage;
            if (_toolWindowSettings.useGreyIcons)
                icon = _toolWindowSettings.messWidgetImageGrey;
            GUIContent messContent = new GUIContent {image = icon};
            messContent.tooltip = "MessMaker";
            topIconCount++; 

            Rect storyTellerWidgetRect  = new Rect(40 + ToolWindowSettingsProp.iconsPaddingX * topIconCount, ToolWindowSettingsProp.iconsPaddingY, ToolWindowSettingsProp.iconsSize, ToolWindowSettingsProp.iconsSize);
            icon = _toolWindowSettings.storyTellerWidgetImage;
            if (_toolWindowSettings.useGreyIcons)
                icon = _toolWindowSettings.storyTellerWidgetImageGrey;
            GUIContent storyTellerContent = new GUIContent {image = icon};
            storyTellerContent.tooltip = "StoryTeller";
            topIconCount++; 
        //    Rect storyTellerSyncSetupRect  = new Rect(ToolWindowSettingsProp.iconsPaddingX * iconCount + ToolWindowSettingsProp.iconsPaddingX/2, ToolWindowSettingsProp.iconsPaddingY +ToolWindowSettingsProp.iconsSize/3.8f, ToolWindowSettingsProp.iconsSize, ToolWindowSettingsProp.iconsSize);

        Rect syncSetupWidgetRect  = new Rect(35 + ToolWindowSettingsProp.iconsPaddingX * topIconCount, ToolWindowSettingsProp.iconsPaddingY +5, ToolWindowSettingsProp.iconsSize *0.875f, ToolWindowSettingsProp.iconsSize *0.75f);
        icon = _toolWindowSettings.syncSetupWidgetImage; 
        GUIContent syncSetupContent = new GUIContent {image = icon};
        syncSetupContent.tooltip = "SyncSetup Settings";
         
        GUI.Label(new Rect(10, 40, 10, 40), GUI.tooltip);

        int botIconCount = 0;

        Rect syncLinkWidgetRect  = new Rect( 46 , ToolWindowSettingsProp.iconsPaddingY + ToolWindowSettingsProp.iconsSize + 5, ToolWindowSettingsProp.iconsSize *0.75f, ToolWindowSettingsProp.iconsSize*0.75f);
        if (DesignAreaProp.designMode == DesignMode.SyncLinking)
               icon = _toolWindowSettings.syncLinkWidgetImageOn;
        else
            icon = _toolWindowSettings.syncLinkWidgetImageOff;  
        GUIContent syncLinkContent = new GUIContent {image = icon};
        syncLinkContent.tooltip = "SyncLinking: G";
        botIconCount++;
        Rect snapWidgetRect  = new Rect( 40 + ToolWindowSettingsProp.iconsPaddingX * botIconCount + 4, ToolWindowSettingsProp.iconsPaddingY + ToolWindowSettingsProp.iconsSize + 4, ToolWindowSettingsProp.iconsSize *0.75f, ToolWindowSettingsProp.iconsSize*0.75f);
        if (DesignAreaProp._snapping == true)
            icon = _toolWindowSettings.snapWidgetImageOn;
        else
            icon = _toolWindowSettings.snapWidgetImageOff;  
        GUIContent snapContent = new GUIContent {image = icon};
        snapContent.tooltip = "Snap: C";  // ToolShortcutManager.GetListenedKeycode(ToolShortcutManager.SnapOrLayoutMode(null));
        
            Handles.BeginGUI();
            if (GUI.Button(compassWidgetRect, compassContent, new GUIStyle()))
                DesignAreaProp.ClickCompassWidget();
            if (GUI.Button(messWidgetRect, messContent, new GUIStyle()))
                DesignAreaProp.ClickMessWidget();
            if (GUI.Button(storyTellerWidgetRect, storyTellerContent, new GUIStyle()))
                StoryTellerProp.ClickStoryTellerWidget();
            if (GUI.Button(syncSetupWidgetRect, syncSetupContent, new GUIStyle()))
                DesignAreaProp.ClickSyncSetupWidget();

            if (_selectionMode == SelectionMode.Sync)
            {
                if (GUI.Button(syncLinkWidgetRect, syncLinkContent, new GUIStyle()))
                    DesignAreaProp.ToggleSyncLinkMode();
                if (GUI.Button(snapWidgetRect, snapContent, new GUIStyle()))
                    DesignAreaProp.ToggleSnapMode();
            }
             
        //    GUI.Label(storyTellerSyncSetupRect,StoryTellerProp.activeSyncSetup.ToString() );
            Handles.EndGUI();
            #endif
        }
        
        public void OnEnable()
        {
            if (ToolWindowSettingsProp != null)
            { 
                SceneView.duringSceneGui -= OnScene; 
                SceneView.duringSceneGui += OnScene; 
                EditorSceneManager.sceneOpened -= ReEnableToolWindowOnSceneChange;
                EditorSceneManager.sceneOpened += ReEnableToolWindowOnSceneChange;
                EditorApplication.playModeStateChanged -= ReEnableToolWindowOnPlayChange;
                EditorApplication.playModeStateChanged += ReEnableToolWindowOnPlayChange;
                EnableToolWindow();
            }
            else
            rootVisualElement.Add(new Label("No ToolWindowSettings object detected!\n" +
                                            " Please verify that Tool Scene Objects are present!\n" +
                                            "If this message persists, click Window - Layouts - Default")); 
          
        }

        public void OnDisable()
        {
            SceneView.duringSceneGui -= OnScene;  
            ToolShortcutManager.StopListening(ToggleSynclinkMode, context: "SceneView", KeyCode.G);
            ToolShortcutManager.StopListening(ShowWindow, KeyCode.LeftShift, KeyCode.LeftShift, KeyCode.T);
        }

        private void ReEnableToolWindowOnPlayChange(PlayModeStateChange playModeState)
        {
            if (playModeState == PlayModeStateChange.EnteredEditMode || playModeState == PlayModeStateChange.ExitingPlayMode) 
                    EnableToolWindowAfterPlayChange();
        }

        private void ReEnableToolWindowOnSceneChange(Scene scene, OpenSceneMode mode)
        {
       //     Debug.Log("ReEnableToolWindow"); 
            EnableToolWindowAfterSceneChange();
        }
       
        private void EnableToolWindow()
        {
            rootVisualElement.Clear();
            InitializePrimarySetup();
            
            InitializeSecondarySetup();

            InitializeFinalSetup();
        }
        private void EnableToolWindowAfterSceneChange()
        {
            InitializePrimarySetup();
            
            InitializeFinalSetup();
        }
        private void EnableToolWindowAfterPlayChange()
        {
            InitializePrimarySetup();
            
            InitializeFinalSetup();
        }

        private void InitializePrimarySetup()
        {
            ToolShortcutManager.StartListening(ToggleSynclinkMode, context: "SceneView", KeyCode.G);
            ToolShortcutManager.StartListening(ShowWindow, KeyCode.LeftShift, KeyCode.T);
            ToolShortcutManager.StartListening(ToggleAlternateLayoutMode, context: "SceneView", KeyCode.C);
            Selection.activeGameObject = null;
            _selectionMode = SelectionMode.None;
            Instance = this; 
            _doneBuilding = false;
            if (ToolWindowSettingsProp != null)
                ToolWindowSettingsProp.toolWindowBuilt = true;
            _designAccess = FindObjectOfType<DesignAccess>(); 
        }
        private void InitializeSecondarySetup()
        {
            CreateRoots();
            BuildUI(); 
        }
        private void InitializeFinalSetup()
        {
            UpdateSelectionMode();
            UpdateEditContent();
            _doneBuilding = true; 
           //     SceneView.duringSceneGui += view => { CheckForKeyInput(); };
        }



#if UNITY_EDITOR
    private void Update()
        {
            if (ToolWindowSettingsProp != null)
            {
                if (_doneBuilding == false)
                {
                    EnableToolWindow();
                    return;
                }
                if (ToolWindowSettingsProp.openEditContent)
                {
                    ToolWindowSettingsProp.openEditContent = false;
                    ClickHeader(0);
                }
                if (ToolWindowSettingsProp.forceUpdateControlForRoomLayout)
                {
                    ToolWindowSettingsProp.forceUpdateControlForRoomLayout = false;
                    _controlContent.LoadRoomLayoutFromOutside(ToolWindowSettingsProp.forceUpdateControlContentName);
                }
                if (ToolWindowSettingsProp.forceUpdateContentSections)
                {
                    ToolWindowSettingsProp.forceUpdateContentSections = false;
                    if (_selectedGameobject != null)
                        UpdateEditContent();
                    _controlContent.RefreshDisplayContainers();
                }
                if (ToolWindowSettingsProp.forceSelectSyncSetupNameField)
                {
                    ToolWindowSettingsProp.forceSelectSyncSetupNameField = false;
                    ForceSelectSyncSetupNameField();
                }
            
                if (_designAccess == null)
                    _designAccess = FindObjectOfType<DesignAccess>();
                // if (DesignArea == null)
                //     DesignArea = FindObjectOfType<DesignArea>();
                // if (LayoutArea == null)
                //     LayoutArea = FindObjectOfType<LayoutArea>();
            

                if (DesignAreaProp != null)
                    DesignAreaProp.UpdateFromToolWindow();
                else
                    Debug.LogError("[ToolWindow](Update): Design Area is null or could not be found");

                if (LayoutAreaProp != null)
                    LayoutAreaProp.UpdateFromToolWindow();
                else
                    Debug.LogError("[ToolWindow](Update): LayoutArea is null or could not be found");
                if (ToolWindowSettingsProp.forcePipelineUpdate)
                { 
                    ToolWindowSettingsProp.forcePipelineUpdate = false;
                    _designAccess.UpdateRoomLayoutStorage(); 
                    ModuleManager.UpdatePipeline();
                }
            } 
        }

   


#endif
     //   static void OnHierarchyChanged() { }

   
        private void OnSelectionChange()
        {
            if (Selection.transforms.Length > 0 )
            { 
                if (_doneBuilding)
                {
                    UpdateSelectionMode();
                    UpdateEditContent(_toolWindowSettings.autoSelectEdit);
                } 
            }
            else if (ToolWindowSettingsProp.selectNoneUI)
            {
                _selectionMode = SelectionMode.None;
               UpdateEditContent();
            }
        }
        
        EditorCoroutine syncCreationCoroutine;
        void OnHierarchyChange()
        {
            if (_doneBuilding && Selection.transforms.Length > 0 )
            {
                //    UpdateSelectionMode();
                //    UpdateEditContent(); 
                _controlContent.RefreshDisplayContainers();
                if (_selectionMode == SelectionMode.GameObject )  
                    if (_selectedGameobject != null)
                        if (forbiddenNames.Contains(_selectedGameobject.name) == false) 
                          //      _designArea.syncMaster.CheckForAutoCreation(_selectedGameobject);  
                            syncCreationCoroutine  = EditorCoroutineUtility.StartCoroutineOwnerless(AttemptSyncCreationAfterDelay());  
                          //    StartCoroutine(AttemptSyncCreationAfterDelay());  
                          
            } 
        } 
        private IEnumerator AttemptSyncCreationAfterDelay()
        {
            // Coroutine because otherwise there would be random duplicate-creations
            yield return new EditorWaitForSeconds(0.05f); 
            if (_selectedGameobject != null)
                CheckForSyncCreation(_selectedGameobject);

        }
        private void CheckForSyncCreation(GameObject activeGameObject)
        {
         //   EditorCoroutineUtility.StopCoroutine(syncCreationCoroutine);
            DesignAreaProp.SyncMasterProp.CheckForAutoCreation(activeGameObject);
        }

        private void OnFocus()
        {
            if (_doneBuilding)
            {
                SceneView.duringSceneGui -= this.OnSceneGUI;
                SceneView.duringSceneGui += this.OnSceneGUI;
          //      UpdateSelectionMode();
          //      UpdateEditContent(); 
            }
        }

        private void OnLostFocus()
        {
            UpdateSelectionMode();
//            UpdateEditContent(); 
        }

        void OnDestroy() {
            // When the window is destroyed, remove the delegate
            // so that it will no longer do any drawing.
            SceneView.duringSceneGui -= this.OnSceneGUI;
        }

      
        

        /// <summary>
        /// Triggered from ShortcutManager after subscribing in OnEnable() method
        /// </summary>
        /// <param name="args"></param>
        private void ToggleSynclinkMode(CustomShortcutArguments args)
        {
            DesignAreaProp.ToggleSyncLinkMode();
        }

        /// <summary>
        /// Toggle Layout mode for addings layout points
        /// </summary>
        /// <param name="args"></param>
        private void ToggleAlternateLayoutMode(CustomShortcutArguments args)
        { 
            LayoutAreaProp.ReceiveToggleSignal();
        }
        
     
        // INPUT OPTION 2
        // Triggers <when scene view is in focus>, for keys and mouse (does not work for dragging assets into Scene, though)
        private void CheckForKeyInput()
        {
            // TODO:: is this a placeholder?
                if(Event.current.type == EventType.KeyDown  ) 
                    Debug.Log("Scene Focus: KeyDown");
                if(Event.current.type == EventType.KeyUp  ) 
                    Debug.Log("Scene Focus: KeyUp");
                if(Event.current.type == EventType.MouseDown  ) 
                    Debug.Log("Scene Focus: MouseDown");
                if(Event.current.type == EventType.MouseUp  ) 
                    Debug.Log("Scene Focus: MouseUp");
                if (Event.current is {isKey: true, keyCode: KeyCode.A})
                    Debug.Log("Scene Focus: KeyA");  
           
        }
        // INPUT OPTION 3
        // Triggers <when ToolWindow is in focus>, for keys (and mouse? worked earlier)  (add a delay here, it triggers quickly)
        public void OnGUI()
        {
            /*// OnGUI is the only function that can implement the "Immediate Mode" GUI (IMGUI) system for rendering and handling GUI events
            if (!ToolWindowSettingsProp.useWindowFocusInput) return;
            // TODO:: is this a placeholder?
            if (Event.current is {isKey: true, keyCode: KeyCode.Alpha0})
            {
                    
            }
            if (Event.current.type == EventType.MouseDown)
            {
                //    Debug.Log("Window Focus: MOUSE DOWN");
            }
            if (Event.current.type == EventType.MouseUp)
            {
                //    Debug.Log("Window Focus: MOUSEUP");
            }*/
        }
    
   #region Updating & Selection
      
        
        void UpdateSelectionMode()
        {
            UpdateForbiddenNames();

            if (Selection.activeGameObject == null)
            {
           //   _selectionMode = SelectionMode.None;  
            }
            else
            {
                if (forbiddenNames.Contains(Selection.activeGameObject.name))
                {
                    // do nothing
                }
                else
                { 
                    DesignAreaProp.ToggleSyncLinkMode(true);
                    DesignAreaProp.ToggleSnapMode(true);
                    if (Selection.activeGameObject != null)
                        _selectedGameobject = Selection.activeGameObject;
                    if (_selectedGameobject == null)
                        _selectionMode = SelectionMode.None; 
                    else   if (_selectedGameobject.GetComponent<Sync>() != null)
                        _selectionMode = SelectionMode.Sync;
                    else   if (_selectedGameobject.GetComponent<FlexFloor>() != null)
                        _selectionMode = SelectionMode.DesignArea    ;
                    else   if (_selectedGameobject.GetComponent<DesignArea>() != null)
                        _selectionMode = SelectionMode.DesignArea    ;
                    else   if (_selectedGameobject.GetComponent<SyncSetup>() != null)
                        _selectionMode = SelectionMode.DesignArea    ;
                    else   if (_selectedGameobject.GetComponent<WallPiece>() != null)
                        _selectionMode = SelectionMode.WallPiece; 
                    else   if (_selectedGameobject.GetComponent<SurroundSync>() != null)
                        _selectionMode = SelectionMode.SurroundSync;
                    else   if (_selectedGameobject.GetComponent<MockPhysical>() != null)
                        _selectionMode = SelectionMode.MockPhysical;
                    else   if (_selectedGameobject.GetComponent<PlayfieldInteraction>() != null)
                        _selectionMode = SelectionMode.Playfield;
                    else   if (_selectedGameobject.GetComponent<Compass>() != null)
                        _selectionMode = SelectionMode.Compass; 
                    else   if (_selectedGameobject.GetComponent<WallNumber>() != null)
                        _selectionMode = SelectionMode.WallNumber;
                    else   if (_selectedGameobject.GetComponent<LayoutArea>() != null)
                        _selectionMode = SelectionMode.LayoutArea;  
                    else   if (_selectedGameobject.GetComponent<MessMaker>() != null)
                        _selectionMode = SelectionMode.MessMaker; 
                    else   if (_selectedGameobject.GetComponent<StoryTeller>() != null)
                        _selectionMode = SelectionMode.StoryTeller;
                    else
                        _selectionMode = SelectionMode.GameObject; 
                }
             
            }
     

        } 
        void UpdateEditContent(bool autoSelectEdit = false)
        { 
            if (_selectionMode == SelectionMode.None || _selectionMode == SelectionMode.GameObject )
            {
                // skip showing headers
            }   
            else if (ToolWindowSettingsProp.skipAutoSelectEditForAreas == true && (_selectionMode == SelectionMode.DesignArea ||   _selectionMode == SelectionMode.LayoutArea)) 
            {
                // skip showing headers
            }  
            else if (autoSelectEdit)
                ClickHeader(0);
            
            _editContent.UpdateContent(_selectionMode, _selectedGameobject); 
        }
        private void ForceSelectSyncSetupNameField()
        {
            _controlContent.UpdateAndSelectSyncSetupNameField();
        }
        #endregion
    
   
        #region Construction
        
        void CreateRoots()
        {
            _baseRoot = rootVisualElement.Q<VisualElement>("EditorLocalRoot");
            if (_baseRoot == null) {
                _baseRoot = new VisualElement() {style = {
                        flexGrow = 1,
                        flexBasis = new StyleLength(new Length(100, LengthUnit.Percent))
                    }};
                rootVisualElement.Add(_baseRoot); }
            else
                _baseRoot.Clear();
            _headerRoot = new VisualElement() {style = {
                    height = ToolWindowSettingsProp.HeaderHeight,
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row)
                }};
            _contentRoot = new VisualElement() {style = {
                    flexGrow = 1,
                    height = Length.Percent(84),
                    backgroundColor = ToolWindowSettingsProp.ContentColor
                }};
        }
        
        private void BuildUI()
        {
            BuildHeaderSection();
            BuildContentSection();
            _baseRoot.Add(_headerRoot); 
            _baseRoot.Add(_contentRoot);
            ClickHeader( ToolWindowSettingsProp.lastKnownIndex); 
        }
        
        private void BuildHeaderSection()
        { 
            for (int i = 0; i < 2; i++)
                _headerContainers.Add(CreateHeader(i));
            foreach (var element in _headerContainers)
                element.RegisterCallback<MouseDownEvent>(evt => {
                        ClickHeader(_headerContainers.IndexOf(element));
                });
            foreach (var element in _headerContainers)
                element.RegisterCallback<MouseOverEvent>(evt => {
                        if (_selectedHeader != element)
                            element.style.backgroundColor = ToolWindowSettingsProp.HoverColor;
                });
            foreach (var element in _headerContainers)
                element.RegisterCallback<MouseOutEvent>(evt => {
                        if (_selectedHeader != element)
                            element.style.backgroundColor = ToolWindowSettingsProp.HeaderColor;
                });
            foreach (var element in _headerContainers)
                PopulateHeader(element, _headerContainers.IndexOf(element));
            foreach (var element in _headerContainers)
                _headerRoot.Add(element);
        }
        
        private void BuildContentSection()
        {
            for (int i = 0; i < 3; i++)
                _contentContainers.Add(new VisualElement() {style = {
                            flexGrow = 1, 
                            display = DisplayStyle.None
                }});
            _contentContainers[0].Add(_editContent.CreateContent(this));
          _contentContainers[1].Add(_controlContent.CreateContent(this));
          _contentContainers[2].Add(_matchContent.CreateContent(this));
          _editContent.QueryTheTemplate();
          _controlContent.InitializeContent();
       //   _matchContent.InitializeContent();
          foreach (var element in _contentContainers)
                _contentRoot.Add(element);
        }
        
        #endregion
        
        #region Headers


        private int clickedHeaderIndex = 1;

    private void ClickHeader(int index)
    { 
            if (index <0  )
            Debug.Log("CLICK 0 or smaller!" + index);
            if (index > 2 == false)  
            {
                clickedHeaderIndex = index;
                ToolWindowSettingsProp.lastKnownIndex = index; 
                _selectedHeader = _headerContainers[index];
                _headerContainers[0].style.borderBottomColor = ToolWindowSettingsProp.HeaderBorderColor;
                _headerContainers[1].style.borderBottomColor = ToolWindowSettingsProp.HeaderBorderColor;
               // _selectedHeader.style.borderBottomColor = _toolWindowSettings.HeaderSelectedColor;
                ToolWindowSettingsProp.lastKnownIndex = index;
                foreach (VisualElement headerContainer in _headerContainers)
                    headerContainer.style.backgroundColor = ToolWindowSettingsProp.HeaderColor;
                if (_headerContainers != null || _headerContainers.Count < 1 )
                    _headerContainers[index].style.backgroundColor = ToolWindowSettingsProp.HeaderSelectedColor;
                foreach (var element in _contentContainers)
                    element.style.display = DisplayStyle.None;
                if (_contentContainers != null || _contentContainers.Count < 1 )
                   _contentContainers[index].style.display = DisplayStyle.Flex; 
                if (index == 1)
                    _controlContent.RefreshDisplayContainers();
                if (index == 2)
                    _matchContent.UpdateContent();
            }
      
        }
        
        private void PopulateHeader(VisualElement header, int headerIndex)
         {
             string str = "";
             if (headerIndex == 0)
                 str += "Edit";
             else if (headerIndex == 1)
                 str += "Control";
             else if (headerIndex == 2)
                 str += "Chart";
             var titleLabel = new Label(str) {style = {
                 fontSize = 14, 
                 unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold),
                 color = new StyleColor(new Color(0.82f,0.82f,0.82f)), 
             }};
            header.Add(titleLabel);
        }
        
        VisualElement CreateHeader(int index)
        {
            VisualElement header = new VisualElement() {style = {
                    flexGrow = 1, 
                    width = Length.Percent(33), 
                    borderBottomWidth = 2f,
                    borderBottomColor = ToolWindowSettingsProp.HeaderBorderColor,
                    alignItems = new StyleEnum<Align>(Align.Center),
                    justifyContent = new StyleEnum<Justify>(Justify.Center),
                    borderLeftWidth = 2f,
                    borderLeftColor = ToolWindowSettingsProp.HeaderBorderColor,
                    backgroundColor = ToolWindowSettingsProp.HeaderColor
                }};
            if (index == 0)
                header.style.borderLeftWidth = 0;
            return header;
        }
        #endregion
        
     
      
         
       
 
       
        void OnSceneGUI(SceneView sceneView)
        {
            //    InputLogic();
            //     DrawSceneButtons();
        }

        private void InputLogic()
        {
       
            if(Event.current.type == EventType.MouseMove  )
            {
                GameObject nearestObject = HandleUtility.PickGameObject(Event.current.mousePosition, true);
                if (nearestObject != null)
                    Debug.Log("Hover near: " + nearestObject.name);
            }
        }

        private void DrawSceneButtons()
        {
            Handles.BeginGUI();
            GUILayout.BeginVertical();
            GUILayout.BeginArea(new Rect(20,20,50, 500));
            if (GUILayout.Button("1", GUILayout.MinHeight(50)))
                Debug.Log("1");
            if (GUILayout.Button("2", GUILayout.MinHeight(50)))
                Debug.Log("2");
            if (GUILayout.Button("3", GUILayout.MinHeight(50)))
                Debug.Log("3");
            GUILayout.EndArea();
            GUILayout.EndVertical();
            Handles.EndGUI();   
        }
        
        
      
        
    }

[InitializeOnLoad]
public static class PlayStateNotifier
{
          
    static PlayStateNotifier()
    {
        EditorApplication.playModeStateChanged += ModeChanged;
    }
      
    static void ModeChanged(PlayModeStateChange playModeState)
    {
        if (playModeState == PlayModeStateChange.EnteredEditMode)
        {
          //  Debug.Log("entered edit mode");
            Object.FindObjectsOfType<MockPhysical>(true).ToList().ForEach(m => m.gameObject.SetActive(true));
        }

    }
}
#endif