#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Syncreality.Playfield;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using Object = UnityEngine.Object;
using Random = System.Random;
using Slider = UnityEngine.UIElements.Slider;
using Toggle = UnityEngine.UIElements.Toggle;


public class EditContent  
{
    // OUTSIDE REFERENCE
    private ToolWindow _toolWindow;
    private DesignArea _designArea;
    private LayoutArea _layoutArea;
    private ToolWindowSettings _toolWindowSettings;
    private SyncMaster _syncMaster;
        // STRUCTURE
    private VisualElement _templateRoot;
    private VisualElement _gameObjectRoot;
    private VisualElement _syncRoot;
    private VisualElement _surroundSyncRoot;
    private VisualElement _playfieldRoot;
    private VisualElement _mockPhysicalRoot;
    private VisualElement _compassRoot;
    private VisualElement _wallNumberRoot;
    private VisualElement _layoutAreaRoot; 
    private VisualElement _designAreaRoot;
    private VisualElement _wallPieceRoot;
    private VisualElement _ceilingRoot;
    private VisualElement _messMakerRoot;
    private VisualElement _storyTellerRoot;
    private VisualElement _noneRoot;
    private static VisualTreeAsset _basicListRowTemplate;

    // STRUCTURE - SYNC
    private VisualElement _generalSyncContent; 
    private VisualElement _syncLinkContent; 
    private Label _nameLabelSync;
    private Label _limitBySizeLabel;
    private Label _freeAreaLabel;
    private EnumField _classificationSelector;
    private EnumField _qualitySelector;
    private BoundsField _boundsField;
    private Button _recombineMeshesButton;
    private Button _playFieldButton;
    private Toggle _forceSizeToggle;
    private Toggle _freeAreaToggle;
    private VisualElement _sizesContainer; 
    private VisualElement _freeAreaContainer; 
    private Vector3Field _minSizeVector3Field;
    private Vector3Field _maxSizeVector3Field;
    private RectField _freeAreaRectField;
    private VisualElement _syncIcon;
    private Button _mockifyButton;
    private VisualElement _classificationContainer;
    private MaskField _classificationMaskField;



    // STRUCTURE - SYNCLINKS (SYNC/SURROUNDSYNC)
    private VisualElement _syncLinkListViewRoot;
    private ListView _syncLinkListView;
    private static VisualTreeAsset _syncLinkRowTemplate;
    private static List<SyncLink> _syncLinkList = new List<SyncLink>();
    private Label _syncLinkNameLabel1;
    private Label _syncLinkNameLabel2;
    private VisualElement _syncLinkIcon;
    private Button _syncLinkCloseButton;
    private FloatField _minDistFloat;
    private FloatField _prefDistFloat;
    private FloatField _maxDistFloat; 
    private Toggle _onTopToggle;
    private Toggle _onBottomToggle;
    private Toggle _limitRotateToggle;
    private Toggle _outerRotateToggle;
    private FloatField _minAngleFloat;
    private FloatField _maxAngleFloat;
    private VisualElement _regularSyncLinkContent;
    private VisualElement _virtualSyncLinkContent;
    private Label _virtualInfoLabel;
    private Vector3Field _syncLinkAnchorVector;
    private Vector3Field _syncLinkOffsetVector;
    private Toggle _syncLinkVirtualScaleToggle;
    private Toggle _syncLinkEveryClutterToggle;
    private VisualElement _syncLinkEveryClutterContainer;
    private Button _syncLinkInferButton;
    private Button _syncLinkRepositionButton;
    
    // STRUCTURE - SURROUNDSYNC
    private VisualElement _syncLinkListViewRootSS;
    private VisualElement _generalContentSS; 
    private VisualElement _nonBasePieceContentSS; 
    private VisualElement _basePieceContentSS; 
    private VisualElement _syncLinkContentSS; 
    private Label _nameLabelSS;
    private Toggle _basePieceToggleSS;
    private Toggle _requireFreeSpaceToggleSS;
    private EnumField _faceSelectorSS;
    private IntegerField _mappedIndexFieldSS;
    private IntegerField _orderFieldSS;
    private IntegerField _priorityFieldSS;
    private IntegerField _repeatAmountFieldSS;  
    private Button  _initializeButtonSS;
    private Toggle _basePieceToggleRandomize;
    private Toggle _basePieceToggleFace1;
    private Toggle _basePieceToggleFace2;
    private Toggle _basePieceToggleFace3;
    private Toggle _basePieceToggleFace4;
    private Label _basePieceLabelFace1;
    private Label _basePieceLabelFace2;
    private Label _basePieceLabelFace3;
    private Label _basePieceLabelFace4;
    private Button _recombineMeshButtonSS;
    private Button _playFieldButtonSS;
    private VisualElement _surroundSyncIcon;


    // STRUCTURE - MOCKPHYSICAL
    private Label _nameLabelMock;
    private EnumField _classificationSelectorMock;
    
    // STRUCTURE - PLAYFIELD
    private Label _nameLabelPlayfield;
    private Button _parentButtonPlayfield;
    private Button _focusButtonPlayfield;
    private Button _deleteButtonPlayfield;
  
    // STRUCTURE - COMPASS
    private Label _headLabelCompass;
    private Label _infoLabelCompass; 
    private Button _redrawCompass;
    private Button _reverseButton;
    private Button _hideButton;
    private Button _clearBDSpawns;
    private Label _centerLabel; 
    private Label _wallLabel1; 
    private Label _wallLabel2; 
    private Label _wallLabel3; 
    private Label _wallLabel4; 
    private EnumField _northPropertySelector;


    // STRUCTURE - WALLNUMBER
    private Label _headLabelWallNumber;
    private Label _infoLabelWallNumber;
    private Button _northButton;
    private Toggle _backdropToggle;
    private VisualElement _backdropContent;
    private FloatField _backdropDistance;
    private FloatField _backdropRotation;
    private FloatField _backdropHorizontalOffset;
    private FloatField _backdropVerticalOffset;
    private Button _backdropInferButton;
    private Button _backdropSaveButton;
    private Button _backdropDeleteButton;
    private Button _backdropSpawnButton;
    private ObjectField _backdropAssetPicker;
    private Label _labelWallNumberDistance;
    private Label _labelWallNumberRotation;
    private Label _labelWallNumberHorizontal;
    private Label _labelWallNumberVertical;

    // STRUCTURE - DESIGNAREA
    private Label _designAreaHeadLabel;
    private Label _floorProxyLabel;
    private Label _ceilingProxyLabel; 
    private SliderInt _sliderWallPiecesAll;
    private SliderInt _sliderWallPiecesOne;
    private SliderInt _sliderWallPiecesTwo;
    private SliderInt _sliderWallPiecesThree;
    private SliderInt _sliderWallPiecesFour;
    private Label _sliderLabelOne;
    private Label _sliderLabelTwo;
    private Label _sliderLabelThree;
    private Label _sliderLabelFour;
    private VisualElement _designAreaListViewContainer;
    private ListView _designAreaListView;
    private Dictionary<string, GameObject> _designAreaElements;
    private Toggle _toggleSC;
    private Toggle _toggleSS;
    private Toggle _toggleSL;
    private Toggle _toggleMM; 
    private MaskField _excludeMaskField;
    private ObjectField _floorMeshPicker;
    private ObjectField _floorMaterialPicker1;
    private ObjectField _floorMaterialPicker2;
    private FloatField _ceilingHeightFloatField;
    private ObjectField _ceilingMeshPicker;
    private ObjectField _ceilingMaterialPicker1;
    private ObjectField _ceilingMaterialPicker2;
   

    // STRUCTURE - LAYOUTAREA
    private Label _layoutAreaLabel;
    private Label _createRectangleLabel;
    private Vector2Field _createShapeVector2;
    private FloatField _metersPerUnitFloat;
    
    // STRUCTURE - MESSMAKER
    private VisualElement _listViewContainerMessMaker; 
    private VisualElement _editSectionMessMaker     ; 
    private ListView _listViewMessMaker; 
    private List<MessModule> _messModules;
    private TextField _nameFieldMessMaker;
    private ObjectField _objectFieldMessMaker;
    private EnumField _surfacePickerMessMaker;
    private IntegerField _amountFieldMessMaker;
    private Vector3Field _scaleVarianceMessMaker;
    private FloatField _rotationVarianceMessMaker;
    private FloatField _shiftAmountMessMaker;
    private FloatField _shiftVarianceMessMaker;
    private FloatField _gravityStrengthMessMaker;
    private Button _addMessModuleButton;
    private Button _sceneAnchorButton;
    private Button _messDeleteButton;
    
    
    // STRUCTURE - STORYTELLER
    private Toggle _executeOnStartToggle;
    private Toggle _passthroughToggle;
    private Label _storyObjectLabel;
    private Button _spawnButton;
    private Button _deconstructSpawnsButton;
    private Button _switchToSetupTwoButton;
    private Button _addSetupTwoButton;
    private VisualElement _pipelineFinishEvent;


    
    // STRUCTURE - NONE
    private Label _syncTotalLabel;
    private Label _syncQualityLabel;
    private Label _syncClassificationLabel;
    private Label _mockTotalLabel;
    private Label _mockClassificationLabel;

    // STATE
    private GameObject _selectedGameObject;
    private SelectionMode _selectionMode;
    private SyncLink _activeSyncLink;
    private string _selectedSurroundSyncID;

    // Properties
    private SyncMaster SyncMaster
    {
        get
        {
            if (_syncMaster == null)
                _syncMaster = Object.FindObjectOfType<SyncMaster>();
            return _syncMaster;
        }
    }
    private ToolWindowSettings ToolWindowSettings
    {
        get
        {
            if (_toolWindowSettings == null)
                _toolWindowSettings = Object.FindObjectOfType<ToolWindowSettings>();
            return _toolWindowSettings;
        }
    }
    private DesignArea DesignArea
    {
        get
        {
            if (_designArea == null)
                _designArea = Object.FindObjectOfType<DesignArea>();
            return _designArea;
        }
    }
    private LayoutArea LayoutArea
    {
        get
        {
            if (_layoutArea == null)
                _layoutArea = Object.FindObjectOfType<LayoutArea>();
            return _layoutArea;
        }
    }

    public VisualElement CreateContent(ToolWindow toolWindow)
        {  
            _toolWindow = toolWindow;
            // SyncMaster = Object.FindObjectOfType<SyncMaster>();
            // ToolWindowSettings = Object.FindObjectOfType<ToolWindowSettings>();
            // DesignArea = Object.FindObjectOfType<DesignArea>();
            // LayoutArea = Object.FindObjectOfType<LayoutArea>();
            _templateRoot = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>
                ( ToolWindowSettings.EditorFolderPath+ "/EditContent.uxml").Instantiate();
            _templateRoot.style.backgroundColor =ToolWindowSettings.ContentColor;
            return _templateRoot;
        }
        
        public void QueryTheTemplate()
        {
            _gameObjectRoot = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( ToolWindowSettings.EditorFolderPath+ "/GameObjectContent.uxml").Instantiate();
            _syncRoot = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( ToolWindowSettings.EditorFolderPath+ "/SyncContent.uxml").Instantiate();
            _surroundSyncRoot = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( ToolWindowSettings.EditorFolderPath+ "/SurroundSyncContent.uxml").Instantiate();
            _playfieldRoot = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( ToolWindowSettings.EditorFolderPath+ "/PlayfieldContent.uxml").Instantiate();
            _designAreaRoot = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( ToolWindowSettings.EditorFolderPath+ "/DesignAreaContent.uxml").Instantiate();
            _wallPieceRoot = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( ToolWindowSettings.EditorFolderPath+ "/WallPieceContent.uxml").Instantiate();
            _ceilingRoot = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( ToolWindowSettings.EditorFolderPath+ "/CeilingContent.uxml").Instantiate();
            _mockPhysicalRoot = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( ToolWindowSettings.EditorFolderPath+ "/MockPhysicalContent.uxml").Instantiate();
            _compassRoot = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( ToolWindowSettings.EditorFolderPath+ "/CompassContent.uxml").Instantiate();
            _wallNumberRoot = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( ToolWindowSettings.EditorFolderPath+ "/WallNumberContent.uxml").Instantiate();
            _layoutAreaRoot = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( ToolWindowSettings.EditorFolderPath+ "/LayoutAreaContent.uxml").Instantiate();
            _messMakerRoot = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( ToolWindowSettings.EditorFolderPath+ "/MessMakerContent.uxml").Instantiate();
            _storyTellerRoot = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( ToolWindowSettings.EditorFolderPath+ "/StoryTellerContent.uxml").Instantiate();
            _noneRoot = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( ToolWindowSettings.EditorFolderPath+ "/NoneContent.uxml").Instantiate();
            
            _basicListRowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ToolWindowSettings.EditorFolderPath+ "/ListRowTemplate.uxml");  
        }

        public void UpdateContent(SelectionMode selectionMode, GameObject selectedGameobject)
        {
//            Debug.Log("sel ID: " + Selection.instanceIDs.Length);
            if (_templateRoot != null)
            {
                _templateRoot.Clear();
                _selectionMode = selectionMode;
                 _selectedGameObject = selectedGameobject;

                switch (_selectionMode)
                {
                    case SelectionMode.None:
                        _templateRoot.Add(_noneRoot);
                        LoadNoneContent();
                        break;
                    case SelectionMode.GameObject: // use this exclusively for game-logic gameobjects? (eg check for passive synclinks)
                        _templateRoot.Add(_gameObjectRoot);
                        LoadGameObjectContent();
                        break;
                    case SelectionMode.DesignArea: 
                        _templateRoot.Add(_designAreaRoot);
                        LoadDesignAreaContent();
                        break;
                    case SelectionMode.WallPiece: // remove? cannot be selected anymore
                        _templateRoot.Add(_wallPieceRoot);
                        LoadEditWallPieceContent();
                        break;
                    case SelectionMode.Ceiling: // TBD
                        _templateRoot.Add(_ceilingRoot);
                        LoadEditCeilingContent();
                        break;
                    case SelectionMode.Sync: 
                        _templateRoot.Add(_syncRoot); 
                        LoadSyncContent(); 
                        break;  
                    case SelectionMode.SurroundSync: // == surroundsync
                        _templateRoot.Add(_surroundSyncRoot);
                         LoadSurroundSyncContent();
                        break;
                    case SelectionMode.Playfield: // to do
                        _templateRoot.Add(_playfieldRoot);
                        LoadPlayfieldContent();
                        break;
                    case SelectionMode.MockPhysical:  // in progress
                        _templateRoot.Add(_mockPhysicalRoot);
                        LoadMockPhysicalContent();
                        break;
                    case SelectionMode.Compass:  
                        _templateRoot.Add(_compassRoot);
                        LoadCompassContent();
                        break;
                    case SelectionMode.WallNumber:  // in progress
                        _templateRoot.Add(_wallNumberRoot); 
                    //    InitializeWallNumberContent();  //
                        LoadWallNumberContent(); 
                        break;
                    case SelectionMode.LayoutArea:  
                        _templateRoot.Add(_layoutAreaRoot);  
                        LoadLayoutAreaContent(); 
                        break;
                    case SelectionMode.MessMaker:  
                        _templateRoot.Add(_messMakerRoot);  
                        LoadMessMakerContent(); 
                        break;
                    case SelectionMode.StoryTeller:  
                        _templateRoot.Add(_storyTellerRoot);  
                        LoadStoryTellerContent(); 
                        break;
                }
            }

        }

        private void LoadStoryTellerContent()
        { 
            StoryTeller storyTeller = Object.FindObjectOfType<StoryTeller>();
            _switchToSetupTwoButton =   _storyTellerRoot.Q<Button>("ButtonSwitchTwo");
            _addSetupTwoButton =   _storyTellerRoot.Q<Button>("ButtonSpawn");
            _spawnButton =   _storyTellerRoot.Q<Button>("ButtonDeconstruct"); 
            _executeOnStartToggle = _storyTellerRoot.Q<Toggle>("OnStartToggle"); 
            _passthroughToggle = _storyTellerRoot.Q<Toggle>("PassthroughToggle"); 
            _storyObjectLabel = _storyTellerRoot.Q<Label>("StoryObjectLabel"); 
            _deconstructSpawnsButton =   _storyTellerRoot.Q<Button>("ButtonDeconstruct");
            _pipelineFinishEvent =   _storyTellerRoot.Q<VisualElement>("PipelineFinishEvent");
    
            SerializedObject so = new SerializedObject(storyTeller);
            _executeOnStartToggle.bindingPath = nameof( storyTeller.executeOnStart);
            _passthroughToggle.bindingPath = nameof( storyTeller.disablePassthroughAfterScanning);
            _executeOnStartToggle.Bind(so);
            _passthroughToggle.Bind(so);
            _spawnButton.clicked -= ClickSpawn;
            _switchToSetupTwoButton.clicked -= ClickSwitchToSetupTwo;
            _addSetupTwoButton.clicked -= ClickAddSetupTwo;
            _deconstructSpawnsButton.clicked -= ClickDeconstructSpawns;
            _spawnButton.clicked += ClickSpawn;
            _switchToSetupTwoButton.clicked += ClickSwitchToSetupTwo;
            _addSetupTwoButton.clicked += ClickAddSetupTwo;
            _deconstructSpawnsButton.clicked += ClickDeconstructSpawns;

            int storyObjectsSync = 0;
            int storyObjectsSurroundSync = 0;
            int storyObjectsBackdrop = 0;
            int storyObjectsMessModule = 0;

        

            foreach (var sync in DesignArea.GetSyncs())
                if (sync != null)
                    if (sync.GetComponent<StoryObject>() != null)
                        storyObjectsSync++;
            foreach (var sync in DesignArea.GetSurroundSyncs())
                if (sync != null)
                    if (sync.GetComponent<StoryObject>() != null)
                        storyObjectsSurroundSync++;

            if (DesignArea.backdropper.GetCenterBackdrop().anchored)
                foreach (var obj in DesignArea.backdropper.GetCenterBackdrop().backdropAsset.GetComponentsInChildren<StoryObject>())
                    if (obj != null)
                        storyObjectsBackdrop++;
            if (DesignArea.GetBackdropForWallFace(WallFace.North).anchored)
                foreach (var obj in DesignArea.GetBackdropForWallFace(WallFace.North).backdropAsset.GetComponentsInChildren<StoryObject>())
                    if (obj != null)
                        storyObjectsBackdrop++;
            if (DesignArea.GetBackdropForWallFace(WallFace.East).anchored)
                foreach (var obj in DesignArea.GetBackdropForWallFace(WallFace.East).backdropAsset.GetComponentsInChildren<StoryObject>())
                    if (obj != null)
                        storyObjectsBackdrop++;
            if (DesignArea.GetBackdropForWallFace(WallFace.South).anchored)
                foreach (var obj in DesignArea.GetBackdropForWallFace(WallFace.South).backdropAsset.GetComponentsInChildren<StoryObject>())
                    if (obj != null)
                        storyObjectsBackdrop++;
            if (DesignArea.GetBackdropForWallFace(WallFace.West).anchored)
                foreach (var obj in DesignArea.GetBackdropForWallFace(WallFace.West).backdropAsset.GetComponentsInChildren<StoryObject>())
                    if (obj != null)
                        storyObjectsBackdrop++;
            foreach (var messModule in DesignArea.GetMessModulesFromAnchors())
            foreach (var obj in messModule.prefab.transform.GetComponentsInChildren<StoryObject>())
                if (obj != null)
                    storyObjectsMessModule++;
            
            string str = "StoryObjects: 0 in ";
            if (storyObjectsSync == 0)
                str += "Syncs, ";
            if (storyObjectsSurroundSync == 0)
                str += "SurroundSyncs, ";
            if (storyObjectsBackdrop == 0)
                str += "Backdrops, ";
            if (storyObjectsMessModule == 0)
                str += "MessModules ";
            if (storyObjectsSync != 0)
                str += "\n" + storyObjectsSync + " in Syncs";
            if (storyObjectsSurroundSync != 0)
                str += "\n" + storyObjectsSurroundSync + " in SurroundSyncs";
            if (storyObjectsBackdrop != 0)
                str += "\n" + storyObjectsBackdrop + " in Backdrops";
            if (storyObjectsMessModule != 0)
                str += "\n" + storyObjectsMessModule + " in MessModules";

            _storyObjectLabel.text = str;
   
            _pipelineFinishEvent.Clear();
            _pipelineFinishEvent.Add( IMGUIEventField());
            VisualElement IMGUIEventField( )
            {
                return new IMGUIContainer(() =>
                {
                    so.Update();
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(so.FindProperty(nameof(storyTeller.onPipelineFinished)));
                    if (EditorGUI.EndChangeCheck())
                        so.ApplyModifiedProperties();
                });
            } 
        }

        private void ClickSpawn()
        {
            Object.FindObjectOfType<ModuleManager>().Execute();

        }


        private void ClickSwitchToSetupTwo()
        {
         //   Object.FindObjectOfType<StoryTeller>().SwitchSceneSetup(2); 
        }
        private void ClickAddSetupTwo()
        {
         //   Object.FindObjectOfType<StoryTeller>().AddSceneSetup(2); 
        } 
        private void ClickDeconstructSpawns()
        {
            Object.FindObjectOfType<StoryTeller>().DeconstructSpawnedEnvironment();
        }

        private void LoadLayoutAreaContent()
        {
            LayoutArea layoutArea = _selectedGameObject.GetComponentInChildren<LayoutArea>(); 
            if (layoutArea == null)
                Debug.LogWarning("LayoutArea could not be accessed from selection for loading UI!");
            
            _layoutAreaLabel = _layoutAreaRoot.Q<Label>("LayoutAreaLabel");
            _createRectangleLabel = _layoutAreaRoot.Q<Label>("CreateRectangleLabel");
            _createShapeVector2 = _layoutAreaRoot.Q<Vector2Field>("CreateShapeVector2");
            _metersPerUnitFloat = _layoutAreaRoot.Q<FloatField>("MetersPerUnitFloat");

            SerializedObject so = new SerializedObject(layoutArea);
            _metersPerUnitFloat.bindingPath = nameof( layoutArea.metersPerUnit);
            _metersPerUnitFloat.Bind(so);
            
            //todo make _metersPerUnitFloat responsive again
            //  _metersPerUnitFloat.RegisterValueChangedCallback((_) => layoutArea.RefitLayoutPoints()); 

            _createRectangleLabel.RegisterCallback<MouseDownEvent>(evt =>
            {
                layoutArea.CreateRectangleShape(_createShapeVector2.value);
            });
            if (layoutArea.wallsHaveJustBeenReset == true)
            {
                layoutArea.wallsHaveJustBeenReset = false;
                float xDistance = Mathf.Abs(Vector3.Distance(layoutArea.GetLayoutPointsAsVector3()[0], layoutArea.GetLayoutPointsAsVector3()[1]) * layoutArea.metersPerUnit);
                float zDistance = Mathf.Abs(Vector3.Distance(layoutArea.GetLayoutPointsAsVector3()[1], layoutArea.GetLayoutPointsAsVector3()[2]) * layoutArea.metersPerUnit);
                _createShapeVector2.value = new Vector2(xDistance, zDistance);
            }
        }
       

      
 
        private void LoadDesignAreaContent()
        { 
            _designAreaHeadLabel =   _designAreaRoot.Q<Label>("HeadLabel"); 
            _floorProxyLabel =   _designAreaRoot.Q<Label>("FloorProxyLabel"); 
            _ceilingProxyLabel =   _designAreaRoot.Q<Label>("CeilingProxyLabel"); 
            _sliderWallPiecesAll =   _designAreaRoot.Q<SliderInt>("SliderAll"); 
            _sliderWallPiecesOne =   _designAreaRoot.Q<SliderInt>("SliderOne"); 
            _sliderWallPiecesTwo =   _designAreaRoot.Q<SliderInt>("SliderTwo"); 
            _sliderWallPiecesThree =   _designAreaRoot.Q<SliderInt>("SliderThree"); 
            _sliderWallPiecesFour =   _designAreaRoot.Q<SliderInt>("SliderFour");
            _sliderLabelOne =   _designAreaRoot.Q<Label>("LabelOne");
            _sliderLabelTwo  =   _designAreaRoot.Q<Label>("LabelTwo");
            _sliderLabelThree  =   _designAreaRoot.Q<Label>("LabelThree");
            _sliderLabelFour  =   _designAreaRoot.Q<Label>("LabelFour");
            _designAreaListViewContainer  =   _designAreaRoot.Q<VisualElement>("ListRoot");
            _toggleSC  =   _designAreaRoot.Q<Toggle>("ToggleSC");
            _toggleSS  =   _designAreaRoot.Q<Toggle>("ToggleSS");
            _toggleSL  =   _designAreaRoot.Q<Toggle>("ToggleSL");
            _toggleMM  =   _designAreaRoot.Q<Toggle>("ToggleMM"); 
            _excludeMaskField  =   _designAreaRoot.Q<MaskField>("ExcludeMaskField"); 
            _floorMeshPicker  =   _designAreaRoot.Q<ObjectField>("FloorMeshPicker");
            _floorMaterialPicker1  =   _designAreaRoot.Q<ObjectField>("FloorMaterialPicker1");
            _floorMaterialPicker2  =   _designAreaRoot.Q<ObjectField>("FloorMaterialPicker2");
            _ceilingHeightFloatField  =   _designAreaRoot.Q<FloatField>("CeilingHeightFloatField"); 
            _ceilingMeshPicker  =   _designAreaRoot.Q<ObjectField>("CeilingMeshPicker"); 
            _ceilingMaterialPicker1  =   _designAreaRoot.Q<ObjectField>("CeilingMaterialPicker1"); 
            _ceilingMaterialPicker2  =   _designAreaRoot.Q<ObjectField>("CeilingMaterialPicker2"); 
    

            SyncSetup syncSetup = DesignArea.GetFirstActivePresentSyncSetup();

            _designAreaHeadLabel.text = syncSetup.gameObject.name;
            _floorProxyLabel.text = "FloorProxy has " + syncSetup.GetFloorComponentCount() + " Components, " +
                                    syncSetup.GetFloorChildCount() + "  children";
            _ceilingProxyLabel.text = "CeilingProxy has " + syncSetup.GetCeilingComponentCount() + " Components, " +
                                      syncSetup.GetCeilingChildCount() + "  children";
            string spaceOrNot = "";
        
                if (DesignArea.compass.GetWallFaceForNumber(1) == WallFace.East ||
                    DesignArea.compass.GetWallFaceForNumber(1) == WallFace.West)
                    spaceOrNot += "   ";
                else spaceOrNot = "";
                _sliderLabelOne.text = "# "+ DesignArea.compass.GetWallFaceForNumber(1) + spaceOrNot;
                
                if (DesignArea.compass.GetWallFaceForNumber(2) == WallFace.East ||
                    DesignArea.compass.GetWallFaceForNumber(2) == WallFace.West)
                    spaceOrNot += "   ";
                else spaceOrNot = "";
            _sliderLabelTwo.text = "# "+ DesignArea.compass.GetWallFaceForNumber(2)+ spaceOrNot;
            
            if (DesignArea.compass.GetWallFaceForNumber(3) == WallFace.East ||
                DesignArea.compass.GetWallFaceForNumber(3) == WallFace.West)
                spaceOrNot += "   ";
            else spaceOrNot = "";
            _sliderLabelThree.text = "# " + DesignArea.compass.GetWallFaceForNumber(3) + spaceOrNot;
            
            if (DesignArea.compass.GetWallFaceForNumber(4) == WallFace.East ||
                DesignArea.compass.GetWallFaceForNumber(4) == WallFace.West)
                spaceOrNot += "   ";
            else spaceOrNot = "";
            _sliderLabelFour.text = "# "+ DesignArea.compass.GetWallFaceForNumber(4)+ spaceOrNot;

            SerializedObject so = new SerializedObject(syncSetup); 
            
            _sliderWallPiecesOne.bindingPath = nameof( syncSetup.wallPiecesOne);
            _sliderWallPiecesTwo.bindingPath = nameof( syncSetup.wallPiecesTwo);
            _sliderWallPiecesThree.bindingPath = nameof( syncSetup.wallPiecesThree);
            _sliderWallPiecesFour.bindingPath = nameof( syncSetup.wallPiecesFour);
  
            _sliderWallPiecesAll.Bind(so);
            _sliderWallPiecesOne.Bind(so);
            _sliderWallPiecesTwo.Bind(so);
            _sliderWallPiecesThree.Bind(so);
            _sliderWallPiecesFour.Bind(so); 
            
            _floorMeshPicker.allowSceneObjects = false;
            _floorMaterialPicker1.allowSceneObjects = false;
            _floorMaterialPicker2.allowSceneObjects = false;
            _ceilingMeshPicker.allowSceneObjects = false;
            _ceilingMaterialPicker1.allowSceneObjects = false;
            _ceilingMaterialPicker2.allowSceneObjects = false;
 
            
            _floorMeshPicker.objectType = typeof(GameObject);
            _floorMaterialPicker1.objectType = typeof(Material);
            _floorMaterialPicker2.objectType = typeof(Material);
            _ceilingMeshPicker.objectType = typeof(GameObject);
            _ceilingMaterialPicker1.objectType = typeof(Material);
            _ceilingMaterialPicker2.objectType = typeof(Material);
           
            _floorMeshPicker.bindingPath = nameof(syncSetup.floorMeshPrefab);
            _floorMaterialPicker1.bindingPath = nameof(syncSetup.floorMaterial1);
            _floorMaterialPicker2.bindingPath = nameof(syncSetup.floorMaterial2);
            _ceilingHeightFloatField.bindingPath = nameof( syncSetup.ceilingHeight);
            _ceilingMeshPicker.bindingPath = nameof(syncSetup.ceilingMeshPrefab);
            _ceilingMaterialPicker1.bindingPath = nameof( syncSetup.ceilingMaterial1);
            _ceilingMaterialPicker2.bindingPath = nameof( syncSetup.ceilingMaterial2);
     
            _floorMeshPicker.Bind(so);
            _floorMaterialPicker1.Bind(so);
            _floorMaterialPicker2.Bind(so);
            _ceilingHeightFloatField.Bind(so); 
            _ceilingMaterialPicker1.Bind(so); 
            _ceilingMeshPicker.Bind(so); 
            _ceilingMaterialPicker2.Bind(so); 
            _ceilingHeightFloatField.RegisterValueChangedCallback((e) => { ToolWindowSettings.SendPipelineUpdateSignal(); });
            _floorMeshPicker.RegisterValueChangedCallback((e) => { ToolWindowSettings.SendPipelineUpdateSignal(); });
            _ceilingMeshPicker.RegisterValueChangedCallback((e) => { ToolWindowSettings.SendPipelineUpdateSignal(); });
            _ceilingMaterialPicker1.RegisterValueChangedCallback((e) => { ToolWindowSettings.SendPipelineUpdateSignal(); });
            _ceilingMaterialPicker2.RegisterValueChangedCallback((e) => { ToolWindowSettings.SendPipelineUpdateSignal(); });
            _floorMaterialPicker1.RegisterValueChangedCallback((e) => { ToolWindowSettings.SendPipelineUpdateSignal(); });
            _floorMaterialPicker2.RegisterValueChangedCallback((e) => { ToolWindowSettings.SendPipelineUpdateSignal(); });
            
            SerializedObject daSo = new SerializedObject(DesignArea); 
            
            List<string> classificationNames = Enum.GetNames(typeof(Classification)).ToList();
            _excludeMaskField.choices = classificationNames;
            _excludeMaskField.bindingPath = nameof(DesignArea.classificationFilter);
            _excludeMaskField.Bind(daSo);
            _excludeMaskField.RegisterValueChangedCallback((e) =>
            { 
                UpdateDesignAreaElementNames(DesignArea );
                _designAreaListView = CreateDesignAreaListView(_designAreaElements.Keys.ToList(), ClickDesignAreaElement); 
                _designAreaListViewContainer.Clear();
                _designAreaListViewContainer.Add(_designAreaListView);
            });
          
            _toggleSC.RegisterValueChangedCallback((_) =>
            {
                  UpdateDesignAreaElementNames(DesignArea );
                _designAreaListView = CreateDesignAreaListView(_designAreaElements.Keys.ToList(), ClickDesignAreaElement); 
                _designAreaListViewContainer.Clear();
                _designAreaListViewContainer.Add(_designAreaListView); 
            });
            _toggleSS.RegisterValueChangedCallback((_) =>
            {
                  UpdateDesignAreaElementNames(DesignArea );
                _designAreaListView = CreateDesignAreaListView(_designAreaElements.Keys.ToList(), ClickDesignAreaElement); 
                _designAreaListViewContainer.Clear();
                _designAreaListViewContainer.Add(_designAreaListView); 
            });
            _toggleSL.RegisterValueChangedCallback((_) =>
            {
                  UpdateDesignAreaElementNames(DesignArea );
                _designAreaListView = CreateDesignAreaListView(_designAreaElements.Keys.ToList(), ClickDesignAreaElement); 
                _designAreaListViewContainer.Clear();
                _designAreaListViewContainer.Add(_designAreaListView); 
            });
            _toggleMM.RegisterValueChangedCallback((_) =>
            {
                  UpdateDesignAreaElementNames(DesignArea );
                _designAreaListView = CreateDesignAreaListView(_designAreaElements.Keys.ToList(), ClickDesignAreaElement); 
                _designAreaListViewContainer.Clear();
                _designAreaListViewContainer.Add(_designAreaListView); 
            });
            
             UpdateDesignAreaElementNames(DesignArea );
            _designAreaListView = CreateDesignAreaListView(_designAreaElements.Keys.ToList(), ClickDesignAreaElement); 
            _designAreaListViewContainer.Clear();
            _designAreaListViewContainer.Add(_designAreaListView); 
        }
          
        private void UpdateDesignAreaElementNames(DesignArea designArea)
        {
            _designAreaElements = new Dictionary<string, GameObject>();
            
            if (_toggleSC.value)
                foreach (var sync in designArea.GetSyncs())
                    if (_designAreaElements.ContainsKey(sync.gameObject.name ) == false)
                        if (designArea.classificationFilterList.Count == 0 || designArea.classificationFilterList.Contains(sync.classification) == false)
                            if (sync.hasPlayfield)
                                _designAreaElements.Add("PF " +sync.gameObject.name, sync.gameObject);
                            else
                                _designAreaElements.Add(sync.gameObject.name, sync.gameObject); 
            if (_toggleSS.value)
                foreach (var ssync in designArea.GetSurroundSyncs())
                    if (_designAreaElements.ContainsKey(ssync.gameObject.name ) == false)
                        if (ssync.hasPlayfield)
                             _designAreaElements.Add("PF " + ssync.gameObject.name, ssync.gameObject);
                        else
                            _designAreaElements.Add(ssync.gameObject.name, ssync.gameObject);

            if (_toggleSL.value)
                foreach (var syncLink in designArea.syncMaster.GetAllSyncLinks())
                    if (_designAreaElements.ContainsKey(syncLink.ID ) == false)
                        if (designArea.classificationFilterList.Count == 0 || designArea.classificationFilterList.Contains(syncLink.parentSync.classification)  == false)
                            _designAreaElements.Add(syncLink.ID, syncLink.parentSync.gameObject);
            if (_toggleMM.value)
                foreach (var messModule in designArea.GetMessModulesFromAnchors())
                    if (_designAreaElements.ContainsKey(messModule.moduleID ) == false)
                       if (designArea.messMaker != null)
                           _designAreaElements.Add(messModule.moduleID, designArea.messMaker.gameObject); 
             
        }
        private ListView CreateDesignAreaListView(  List<string> elementsList, Action<string, MouseUpEvent> clickAction)
        { 
            VisualElement MakeItem() => _basicListRowTemplate.CloneTree();
            void BindItem(VisualElement e, int i)
            { 
                e.Q<Label>("NameLabel").text = elementsList[i]; 
                e.RegisterCallback<MouseUpEvent>(evt => { clickAction.Invoke(elementsList[i], evt); });
            }
            return new ListView(elementsList, 24, MakeItem, BindItem)
            {
                selectionType = SelectionType.Single,
                reorderable = true,
                style =
                { 
                    height = elementsList.Count * 24,
                }
            };
        }
        private void ClickDesignAreaElement(string elementName, MouseUpEvent evt)
        {
            Selection.activeGameObject = _designAreaElements[elementName];
            if (_designAreaElements[elementName] != DesignArea.messMaker.gameObject)
               SceneView.FrameLastActiveSceneView();   
        }
        private void LoadMessMakerContent()
    {
        MessMaker messMaker = _selectedGameObject.GetComponentInChildren<MessMaker>(); 
        if (messMaker == null)
            Debug.LogWarning("MessMaker could not be accessed from selection for loading UI!");

        _listViewContainerMessMaker =   _messMakerRoot.Q<VisualElement>("ListContainer"); 
        _nameFieldMessMaker =   _messMakerRoot.Q<TextField>("NameField"); 
        _editSectionMessMaker =   _messMakerRoot.Q<VisualElement>("EditSection"); 
        _objectFieldMessMaker =   _messMakerRoot.Q<ObjectField>("ObjectField"); 
        _surfacePickerMessMaker =   _messMakerRoot.Q<EnumField>("SurfaceField"); 
        _amountFieldMessMaker =   _messMakerRoot.Q<IntegerField>("AmountField"); 
        _scaleVarianceMessMaker =   _messMakerRoot.Q<Vector3Field>("ScaleVariance"); 
        _rotationVarianceMessMaker =   _messMakerRoot.Q<FloatField>("RotationVariance"); 
        _shiftAmountMessMaker =   _messMakerRoot.Q<FloatField>("ShiftAmount"); 
        _shiftVarianceMessMaker =   _messMakerRoot.Q<FloatField>("ShiftVariance"); 
        _gravityStrengthMessMaker =   _messMakerRoot.Q<FloatField>("GravityStrength");
        _addMessModuleButton =   _messMakerRoot.Q<Button>("AddModuleButton");
        _sceneAnchorButton =   _messMakerRoot.Q<Button>("ButtonSceneAnchor");
        _messDeleteButton =   _messMakerRoot.Q<Button>("ButtonDelete");

        _messModules =  UpdateMessModuleList( );
        _listViewMessMaker =   CreateMessModuleListView(  _messModules, ClickMessModule );
        _listViewContainerMessMaker.Clear();
        _listViewContainerMessMaker.Add(_listViewMessMaker);
        _editSectionMessMaker.style.visibility = new StyleEnum<Visibility>(Visibility.Hidden);

        _messDeleteButton.clicked -= ClickDeleteModuleButton;
        _messDeleteButton.clicked += ClickDeleteModuleButton;
        _addMessModuleButton.clicked -= ClickAddModuleButton;
        _addMessModuleButton.clicked += ClickAddModuleButton;
    
      //  Debug.Log("loading messmaker ui");

      
        
    }

        private void ClickDeleteModuleButton()
        {
            DesignArea.DeleteMessAnchorsForMessModule(_lastClickedMessModule);
            DesignArea.messMaker.DeleteMessModule(_lastClickedMessModule);
            LoadMessMakerContent(); 
        }


        private void ClickAddModuleButton()
    {
        DesignArea.messMaker.CreateMessModule();
      //  Debug.Log("lclick mm  button");

     //   EditorUtility.FocusProjectWindow(); 
        LoadMessMakerContent();
    //    UpdateMessMakerContentForMessModule(messModule);
    }

    private void UpdateMessMakerContentForMessModule(MessModule messModule)
    {
        _editSectionMessMaker.style.visibility = new StyleEnum<Visibility>(Visibility.Visible);
        SerializedObject so = new SerializedObject(messModule); 
            
        _objectFieldMessMaker.objectType = typeof(GameObject);
        _objectFieldMessMaker.allowSceneObjects = false;
        _nameFieldMessMaker.bindingPath = nameof(messModule.moduleID);    
        _objectFieldMessMaker.bindingPath = nameof(messModule.prefab);   
        _surfacePickerMessMaker.bindingPath = nameof(messModule.surfaceType);   
        _amountFieldMessMaker.bindingPath = nameof(messModule.spawnAmount);   
        _scaleVarianceMessMaker.bindingPath = nameof(messModule.scaleVariance);   
        _rotationVarianceMessMaker.bindingPath = nameof(messModule.rotationVariance);   
        _shiftAmountMessMaker.bindingPath = nameof(messModule.shiftAmount);
        _shiftVarianceMessMaker.bindingPath = nameof(messModule.shiftVariance);   
        _gravityStrengthMessMaker.bindingPath = nameof(messModule.gravityStrength);    
        _nameFieldMessMaker.Bind(so);
        _objectFieldMessMaker.Bind(so);
        _surfacePickerMessMaker.Bind(so);
        _amountFieldMessMaker.Bind(so);
        _scaleVarianceMessMaker.Bind(so);
        _rotationVarianceMessMaker.Bind(so);
        _shiftAmountMessMaker.Bind(so);
        _shiftVarianceMessMaker.Bind(so);
        _gravityStrengthMessMaker.Bind(so);
        _nameFieldMessMaker.RegisterValueChangedCallback((x) => _toolWindowSettings.SendPipelineUpdateSignal());
        _objectFieldMessMaker.RegisterValueChangedCallback((_) => _toolWindowSettings.SendPipelineUpdateSignal());
        _surfacePickerMessMaker.RegisterValueChangedCallback((_) => _toolWindowSettings.SendPipelineUpdateSignal());
        _amountFieldMessMaker.RegisterValueChangedCallback((_) => _toolWindowSettings.SendPipelineUpdateSignal());
        _scaleVarianceMessMaker.RegisterValueChangedCallback((_) => _toolWindowSettings.SendPipelineUpdateSignal());
        _rotationVarianceMessMaker.RegisterValueChangedCallback((_) => _toolWindowSettings.SendPipelineUpdateSignal());
        _shiftAmountMessMaker.RegisterValueChangedCallback((_) => _toolWindowSettings.SendPipelineUpdateSignal());
        _shiftVarianceMessMaker.RegisterValueChangedCallback((_) => _toolWindowSettings.SendPipelineUpdateSignal());
        _gravityStrengthMessMaker.RegisterValueChangedCallback(x => _toolWindowSettings.SendPipelineUpdateSignal());

        _sceneAnchorButton.clicked -= ClickRemoveAnchorButton;
        _sceneAnchorButton.clicked -= ClickCreateAnchorButton;
        
        _sceneAnchorButton.RegisterValueChangedCallback((_) => _toolWindowSettings.SendPipelineUpdateSignal());

        
        if (DesignArea.GetMessModulesFromAnchors().Contains(messModule))
        {
            _sceneAnchorButton.text = "Remove Anchor";
            _sceneAnchorButton.clicked+= ClickRemoveAnchorButton;
        }
        else
        {
            _sceneAnchorButton.text = "Create Anchor";
            _sceneAnchorButton.clicked += ClickCreateAnchorButton;
        }
    }
    
    

    private void ClickRemoveAnchorButton()
    { 
        if (_lastClickedMessModule == null)
        {
            Debug.LogWarning("Tried to anchor a MessModule, but no recent selection was found!");
        }
        else
        {
            _sceneAnchorButton.text = "Anchor to Scene";
            _sceneAnchorButton.clicked -= ClickRemoveAnchorButton;
            _sceneAnchorButton.clicked += ClickCreateAnchorButton;  
            DesignArea.DestroyMessModuleAnchor(_lastClickedMessModule);
        } 

    }
    private void ClickCreateAnchorButton( )
    {
        if (_lastClickedMessModule == null)
        {
            Debug.LogWarning("Tried to anchor a MessModule, but no recent MessModule selection was found!");
        }
        else
        {
            DesignArea.CreateMessModuleAnchor(_lastClickedMessModule);
            _sceneAnchorButton.text = "Clear Anchor";
            _sceneAnchorButton.clicked -= ClickCreateAnchorButton;
            _sceneAnchorButton.clicked += ClickRemoveAnchorButton; 
        }
    }
    private List<MessModule> UpdateMessModuleList()
    {
        List<MessModule> returnList = new List<MessModule>();
        if ( AssetDatabase.IsValidFolder("Assets/Resources/MessModules"  ) == false )
            AssetDatabase.CreateFolder("Assets/Resources", "MessModules");
        string[] allPaths = Directory.GetFiles("Assets/Resources/MessModules", "*.asset",  SearchOption.AllDirectories);
        foreach (string path in allPaths)
        {
            string cleanedPath = path.Replace("\\", "/");
            returnList.Add((MessModule)AssetDatabase.LoadAssetAtPath(cleanedPath, typeof(MessModule)));
        }
        return returnList;
    }
    private ListView CreateMessModuleListView( List<MessModule> messList, Action<MessModule, MouseUpEvent> clickAction)
    {
        SyncSetup syncSetup = Object.FindObjectOfType<DesignArea>().GetFirstActivePresentSyncSetup();
        VisualElement MakeItem() => _basicListRowTemplate.CloneTree();
        void BindItem(VisualElement e, int i)
        { 
            e.Q<Label>("NameLabel").text = messList[i].moduleID;
            if (syncSetup.HasAnchorForMessModule(messList[i]))
                e.Q<Label>("NameLabel").style.color = Color.green;
            else
                e.Q<Label>("NameLabel").style.color = Color.white;
            e.RegisterCallback<MouseUpEvent>(evt => { clickAction.Invoke(messList[i], evt); });
        }
        return new ListView(messList, 24, MakeItem, BindItem)
        {
            selectionType = SelectionType.Single,
            reorderable = true,
            style =
            { 
                height = messList.Count * 24,
            }
        };
    }

    private MessModule _lastClickedMessModule;
    private void ClickMessModule(MessModule messModule, MouseUpEvent evt)
    {
        if (evt.button == 0)  // leftclick
        {
            _lastClickedMessModule = messModule;
            UpdateMessMakerContentForMessModule(messModule);
        }
        else if (evt.button == 1) // rightclick
        {
            
        } 
    }

   

    private bool deleteClickAllowed = true; // ugly fix for bug where delete button triggers twice

        private void LoadCompassContent()
        {
            Compass compass = Selection.activeGameObject.GetComponent<Compass>();
            if (compass == null)
                return;
            _headLabelCompass = _compassRoot.Q<Label>("HeadLabel");
            _infoLabelCompass = _compassRoot.Q<Label>("InfoLabel");
            _redrawCompass = _compassRoot.Q<Button>("ButtonRedraw");
            _clearBDSpawns = _compassRoot.Q<Button>("ButtonClearSpawns");
            _reverseButton = _compassRoot.Q<Button>("ButtonReverse");
            _hideButton = _compassRoot.Q<Button>("ButtonHide");
            _centerLabel = _compassRoot.Q<Label>("LabelCenter");
            _wallLabel1 = _compassRoot.Q<Label>("LabelWall1");
            _wallLabel2 = _compassRoot.Q<Label>("LabelWall2");
            _wallLabel3 = _compassRoot.Q<Label>("LabelWall3");
            _wallLabel4 = _compassRoot.Q<Label>("LabelWall4");
            
            _backdropToggle = _compassRoot.Q<Toggle>("AnchorBackdropToggle");
            _backdropContent = _compassRoot.Q<VisualElement>("BackdropContent");
            _backdropDistance = _compassRoot.Q<FloatField>("DistanceFloat");
            _backdropRotation = _compassRoot.Q<FloatField>("RotationFloat");
            _backdropHorizontalOffset = _compassRoot.Q<FloatField>("HorizontalOffsetFloat");
            _backdropVerticalOffset = _compassRoot.Q<FloatField>("VerticalOffsetFloat");
            _backdropSpawnButton = _compassRoot.Q<Button>("RepositionButton");
            _backdropDeleteButton = _compassRoot.Q<Button>("DeleteButton");
            _backdropInferButton = _compassRoot.Q<Button>("InferButton");
            _backdropSaveButton = _compassRoot.Q<Button>("SaveButton");
            _backdropAssetPicker = _compassRoot.Q<ObjectField>("ObjectField");
            _labelWallNumberDistance = _compassRoot.Q<Label>("LabelDistance");
            _labelWallNumberRotation = _compassRoot.Q<Label>("LabelRotation");
            _labelWallNumberHorizontal = _compassRoot.Q<Label>("LabelHorizontal");
            _labelWallNumberVertical = _compassRoot.Q<Label>("LabelVertical");
            _northPropertySelector = _compassRoot.Q<EnumField>("NorthPropertyField");

 

            _hideButton.style.visibility = new StyleEnum<Visibility>(Visibility.Visible);
            _reverseButton.clicked -= ClickReverseOrder;
            _reverseButton.clicked += ClickReverseOrder;
            _hideButton.clicked -= ClickHideCompassButton;
            _hideButton.clicked += ClickHideCompassButton;
            _redrawCompass.clicked -= ClickRedrawCompass;
            _redrawCompass.clicked += ClickRedrawCompass;
            _clearBDSpawns.clicked -= DeleteBackdropSpawns;
            _clearBDSpawns.clicked += DeleteBackdropSpawns;

            string centerStr = "Center";
            if (compass.backdropper.GetCenterBackdrop().anchored)
                centerStr += " HAS Backdrop";
            else
                centerStr += " has NO Backdrop"; 
            string wallstr1 = "" + compass.GetWallFaceForNumber(1);
            if (DesignArea.GetBackdropForWallFace(compass.GetWallFaceForNumber(1)).anchored)
                wallstr1 += " HAS Backdrop";
            else
                wallstr1 += " has NO Backdrop";  
            string wallstr2 = "" + compass.GetWallFaceForNumber(2);
            if (DesignArea.GetBackdropForWallFace(compass.GetWallFaceForNumber(2)).anchored)
                wallstr2 += " HAS Backdrop";
            else
                wallstr2 += " has NO Backdrop"; 
            string wallstr3 = "" + compass.GetWallFaceForNumber(3);
            if (DesignArea.GetBackdropForWallFace(compass.GetWallFaceForNumber(3)).anchored)
                wallstr3 += " HAS Backdrop";
            else
                wallstr3 += " has NO Backdrop"; 
            string wallstr4 = "" + compass.GetWallFaceForNumber(4);
            if (DesignArea.GetBackdropForWallFace(compass.GetWallFaceForNumber(4)).anchored)
                wallstr4 += " HAS Backdrop";
            else
                wallstr4 += " has NO Backdrop"; 
          
            _centerLabel.text = centerStr;
            _wallLabel1.text = wallstr1;
            _wallLabel2.text = wallstr2;
            _wallLabel3.text = wallstr3;
            _wallLabel4.text = wallstr4;
            _wallLabel1.RegisterCallback<MouseDownEvent>(evt => { Selection.activeGameObject = compass._wallNumber1.gameObject; });
            _wallLabel2.RegisterCallback<MouseDownEvent>(evt => { Selection.activeGameObject = compass._wallNumber2.gameObject; });
            _wallLabel3.RegisterCallback<MouseDownEvent>(evt => { Selection.activeGameObject = compass._wallNumber3.gameObject; });
            _wallLabel4.RegisterCallback<MouseDownEvent>(evt => { Selection.activeGameObject = compass._wallNumber4.gameObject; });
            
            
            Backdrop backdrop =  compass.backdropper.GetCenterBackdrop(); 

            _backdropContent.style.display = _backdropToggle.value  ?  new StyleEnum<DisplayStyle>(DisplayStyle.Flex) : new StyleEnum<DisplayStyle>(DisplayStyle.None);
            _backdropToggle.RegisterCallback<ChangeEvent<bool>>(evt =>
            { 
                _backdropContent.style.display = evt.newValue ? new StyleEnum<DisplayStyle>(DisplayStyle.Flex) : new StyleEnum<DisplayStyle>(DisplayStyle.None);
            });
            _backdropAssetPicker.objectType = typeof(GameObject);
            _backdropAssetPicker.allowSceneObjects = false;
            
            _backdropToggle.value = backdrop.anchored;
            _backdropAssetPicker.value = backdrop.backdropAsset;
            
            _labelWallNumberDistance.text =  "("+ backdrop.backdropDistance + ")";
            _labelWallNumberRotation.text=  "("+backdrop.backdropRotation+ ")";
            _labelWallNumberHorizontal.text=  "("+backdrop.backdropHorizontalOffset+ ")";
            _labelWallNumberVertical .text= "("+ backdrop.backdropVerticalOffset+ ")";
            _backdropDistance.value = backdrop.backdropDistance;
            _backdropRotation.value = backdrop.backdropRotation;
            _backdropHorizontalOffset.value = backdrop.backdropHorizontalOffset;
            _backdropVerticalOffset.value = backdrop.backdropVerticalOffset;
            
            _backdropToggle.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                Compass _compass = Selection.activeGameObject.GetComponent<Compass>(); 
                if (_compass == null) 
                    return;  
                Backdrop _backdrop =  _compass.backdropper.GetCenterBackdrop();
                _backdrop.anchored = evt.newValue;
                ToolWindowSettings.Instance.SendPipelineUpdateSignal();  
            });
            _backdropAssetPicker.RegisterCallback<ChangeEvent<Object>>(evt =>
            {
                Compass _compass = Selection.activeGameObject.GetComponent<Compass>(); 
                if (_compass == null) 
                    return;  
                Backdrop _backdrop =  _compass.backdropper.GetCenterBackdrop();
                if (_backdropAssetPicker.value != null)
                {
                    _backdrop.backdropAsset = _backdropAssetPicker.value as GameObject;
                    _backdrop.assetReferencePath =  PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(_backdrop.backdropAsset);
                } 
                ToolWindowSettings.Instance.SendPipelineUpdateSignal();
            });
            _backdropInferButton.clicked -= InferBackdropValuesCenter;
            _backdropInferButton.clicked += InferBackdropValuesCenter;
            _backdropSpawnButton.clicked -= SpawnBackdropCenter;
            _backdropSpawnButton.clicked += SpawnBackdropCenter;       
            _backdropDeleteButton.clicked -= DeleteBackdropSpawns;
            _backdropDeleteButton.clicked += DeleteBackdropSpawns;    
            _backdropSaveButton.clicked -= SaveBackdropCenter;
            _backdropSaveButton.clicked += SaveBackdropCenter;

            SerializedObject so = new SerializedObject(compass);
            _northPropertySelector.bindingPath = nameof(compass.northProperty);
            _northPropertySelector.Bind(so);
            
        }
 private void InferBackdropValuesCenter()
        {  
            Backdrop backdrop = DesignArea.backdropper.GetCenterBackdrop();
            Vector3 zeroPos = DesignArea.flexFloor.transform.position;

            if (backdrop.backdropSpawnedAsset == null)
                Debug.Log("Center Backdrop has no spawn to infer from!");
            else
            {
                Transform spawnRT = backdrop.backdropSpawnedAsset.transform;
                _backdropRotation.value = spawnRT.eulerAngles.y  ;
                _backdropVerticalOffset.value = spawnRT.localPosition.y - zeroPos.y;
                _backdropHorizontalOffset.value = - spawnRT.localPosition.x    ;// x-axis 
                _backdropDistance.value = -spawnRT.localPosition.z; // z-axis
            }

        }
  
 private void SpawnBackdropCenter()
 {
     Compass _compass = Selection.activeGameObject.GetComponent<Compass>(); 
     if (_compass == null) 
         return;   
     
     if (_backdropAssetPicker.value == null)
         Debug.Log("Please Select a Prefab Asset!");
     else
         _compass.backdropper.SpawnCenterBackdrop(_backdropDistance.value,  _backdropRotation.value, _backdropHorizontalOffset.value, _backdropVerticalOffset.value );
 }
 private void SaveBackdropCenter()
 {
     Compass _compass = Selection.activeGameObject.GetComponent<Compass>(); 
     if (_compass == null) 
         return;  
     Backdrop _backdrop =  _compass.backdropper.GetCenterBackdrop(); 

     _compass.backdropper.SaveCenterBackdropValues( _backdropDistance.value, _backdropRotation.value, _backdropHorizontalOffset.value, _backdropVerticalOffset.value);
     _labelWallNumberDistance.text =  "("+ _backdrop.backdropDistance + ")";
     _labelWallNumberRotation.text=  "("+_backdrop.backdropRotation+ ")";
     _labelWallNumberHorizontal.text=  "("+_backdrop.backdropHorizontalOffset+ ")";
     _labelWallNumberVertical .text= "("+ _backdrop.backdropVerticalOffset+ ")";
     ToolWindowSettings.Instance.SendPipelineUpdateSignal();
 }
        private void ClickRedrawCompass()
        {
            Compass compass = Selection.activeGameObject.GetComponent<Compass>();
            compass.RedrawCompass();

        }

        private void ClickHideCompassButton()
        {
            DesignArea.compass.HideCompass();
            if (_hideButton != null)
                _hideButton.style.visibility = new StyleEnum<Visibility>(Visibility.Hidden);
        }

        private void ClickReverseOrder()
        {
            DesignArea.compass.reverseFaceOrder = !DesignArea.compass.reverseFaceOrder;
            DesignArea.compass.RedrawCompass();
            LoadCompassContent();
        } 
       
        private void LoadWallNumberContent()
        {
            WallNumber wallNumber = _selectedGameObject.GetComponentInChildren<WallNumber>();
            if (wallNumber == null)
                return;
            _headLabelWallNumber = _wallNumberRoot.Q<Label>("HeadLabel");
         //   _infoLabelWallNumber = _wallNumberRoot.Q<Label>("InfoLabel");
            _northButton = _wallNumberRoot.Q<Button>("NorthButton");
            _backdropToggle = _wallNumberRoot.Q<Toggle>("AnchorBackdropToggle");
            _backdropContent = _wallNumberRoot.Q<VisualElement>("BackdropContent");
            _backdropDistance = _wallNumberRoot.Q<FloatField>("DistanceFloat");
            _backdropRotation = _wallNumberRoot.Q<FloatField>("RotationFloat");
            _backdropHorizontalOffset = _wallNumberRoot.Q<FloatField>("HorizontalOffsetFloat");
            _backdropVerticalOffset = _wallNumberRoot.Q<FloatField>("VerticalOffsetFloat");
            _backdropSpawnButton = _wallNumberRoot.Q<Button>("RepositionButton");
            _backdropDeleteButton = _wallNumberRoot.Q<Button>("DeleteButton");
            _backdropInferButton = _wallNumberRoot.Q<Button>("InferButton");
            _backdropSaveButton = _wallNumberRoot.Q<Button>("SaveButton");
            _backdropAssetPicker = _wallNumberRoot.Q<ObjectField>("ObjectField");
            _labelWallNumberDistance = _wallNumberRoot.Q<Label>("LabelDistance");
            _labelWallNumberRotation = _wallNumberRoot.Q<Label>("LabelRotation");
            _labelWallNumberHorizontal = _wallNumberRoot.Q<Label>("LabelHorizontal");
            _labelWallNumberVertical = _wallNumberRoot.Q<Label>("LabelVertical");
 
            WallFace wallFace = DesignArea.compass.GetWallFaceForNumber(wallNumber.GetWallNumberInt());
            Compass compass = wallNumber.compass;
            Backdrop backdrop =  DesignArea.GetBackdropForWallFace(wallFace); 
            _headLabelWallNumber.text ="WallFace: " + wallFace; 
            _headLabelWallNumber.RegisterCallback<MouseDownEvent>(evt => { Selection.activeGameObject = compass.gameObject; });

        //    _infoLabelWallNumber.text = "WallFace: " + wallFace; 

            _northButton.clicked -= MakeNorth;
            _northButton.clicked += MakeNorth;

            _northButton.style.visibility = wallFace == WallFace.North ? new StyleEnum<Visibility>(Visibility.Hidden) : new StyleEnum<Visibility>(Visibility.Visible);

            
            _backdropContent.style.display = _backdropToggle.value  ?  new StyleEnum<DisplayStyle>(DisplayStyle.Flex) : new StyleEnum<DisplayStyle>(DisplayStyle.None);
            _backdropToggle.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
              //  ToolWindowSettings.SendPipelineUpdateSignal();
              _backdropContent.style.display = evt.newValue ? new StyleEnum<DisplayStyle>(DisplayStyle.Flex) : new StyleEnum<DisplayStyle>(DisplayStyle.None);
              compass.UnderlineAnchoredWallNumbers();
            });
            _backdropAssetPicker.objectType = typeof(GameObject);
            _backdropAssetPicker.allowSceneObjects = false;

            _backdropToggle.value = backdrop.anchored;
            _backdropAssetPicker.value = backdrop.backdropAsset;
            
            _labelWallNumberDistance.text =  "("+ backdrop.backdropDistance + ")";
            _labelWallNumberRotation.text=  "("+backdrop.backdropRotation+ ")";
            _labelWallNumberHorizontal.text=  "("+backdrop.backdropHorizontalOffset+ ")";
            _labelWallNumberVertical .text= "("+ backdrop.backdropVerticalOffset+ ")";
            _backdropDistance.value = backdrop.backdropDistance;
            _backdropRotation.value = backdrop.backdropRotation;
            _backdropHorizontalOffset.value = backdrop.backdropHorizontalOffset;
            _backdropVerticalOffset.value = backdrop.backdropVerticalOffset;
            
            _backdropToggle.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                WallNumber wallNumber = _selectedGameObject.GetComponentInChildren<WallNumber>(); 
                if (wallNumber == null) 
                    return;
                WallFace wallFace = DesignArea.compass.GetWallFaceForNumber(wallNumber.GetWallNumberInt());
                Compass compass = wallNumber.compass;
                Backdrop backdrop =  DesignArea.GetBackdropForWallFace(wallFace);
                backdrop.anchored = evt.newValue;
                ToolWindowSettings.Instance.SendPipelineUpdateSignal(); 

            });
            _backdropAssetPicker.RegisterCallback<ChangeEvent<Object>>(evt =>
            {
                WallNumber wallNumber = _selectedGameObject.GetComponentInChildren<WallNumber>(); 
                if (wallNumber == null) 
                    return;
                WallFace wallFace = DesignArea.compass.GetWallFaceForNumber(wallNumber.GetWallNumberInt());
                Compass compass = wallNumber.compass;
                Backdrop backdrop =  DesignArea.GetBackdropForWallFace(wallFace);
                if (_backdropAssetPicker.value != null)
                {
                    backdrop.backdropAsset = _backdropAssetPicker.value as GameObject;
                    backdrop.assetReferencePath =  PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(backdrop.backdropAsset);
                } 
                ToolWindowSettings.Instance.SendPipelineUpdateSignal();
            });
          //  _backdropAssetPicker.RegisterValueChangedCallback(OnTestToggleChanged);
            _backdropInferButton.clicked -= InferBackdropValues;
            _backdropInferButton.clicked += InferBackdropValues;
            _backdropSpawnButton.clicked -= SpawnBackdrop;
            _backdropSpawnButton.clicked += SpawnBackdrop;       
            _backdropDeleteButton.clicked -= DeleteBackdropSpawns;
            _backdropDeleteButton.clicked += DeleteBackdropSpawns;    
            _backdropSaveButton.clicked -= SaveBackdrop;
            _backdropSaveButton.clicked += SaveBackdrop;
 
        }
        private void OnTestToggleChanged(ChangeEvent<Object> evt)
        { 
           
        }
        private void SaveBackdrop()
        {
            WallNumber wallNumber = _selectedGameObject.GetComponentInChildren<WallNumber>();
            if (wallNumber == null)
            {
                Debug.Log("WallNumber is null, oops!");
                return;
            }
            WallFace wallFace = DesignArea.compass.GetWallFaceForNumber(wallNumber.GetWallNumberInt());
            Backdrop backdrop = DesignArea.GetBackdropForWallFace(wallFace);

            wallNumber.compass.backdropper.SaveBackdropValues(wallFace, _backdropDistance.value, _backdropRotation.value, _backdropHorizontalOffset.value, _backdropVerticalOffset.value);
            _labelWallNumberDistance.text =  "("+ backdrop.backdropDistance + ")";
            _labelWallNumberRotation.text=  "("+backdrop.backdropRotation+ ")";
            _labelWallNumberHorizontal.text=  "("+backdrop.backdropHorizontalOffset+ ")";
            _labelWallNumberVertical .text= "("+ backdrop.backdropVerticalOffset+ ")";
            ToolWindowSettings.Instance.SendPipelineUpdateSignal();
        }


        private void SpawnBackdrop()
        {
            WallNumber wallNumber = _selectedGameObject.GetComponentInChildren<WallNumber>();
            WallFace wallFace = DesignArea.compass.GetWallFaceForNumber(wallNumber.GetWallNumberInt());

            if (_backdropAssetPicker.value == null)
                Debug.Log("Please Select a Prefab Asset!");
            else
                wallNumber.compass.backdropper.SpawnBackdrop(wallFace, wallNumber, _backdropDistance.value,  _backdropRotation.value, _backdropHorizontalOffset.value, _backdropVerticalOffset.value );
        }
        private void DeleteBackdropSpawns()
        {
            if (_selectedGameObject.GetComponentInChildren<WallNumber>() != null)
                 _selectedGameObject.GetComponentInChildren<WallNumber>().compass.backdropper.DeleteBackdropSpawns();
            else if (_selectedGameObject.GetComponentInChildren<Compass>() != null)
                _selectedGameObject.GetComponentInChildren<Compass>().backdropper.DeleteBackdropSpawns();
        }
        private void InferBackdropValues()
        {
            WallNumber wallNumber = _selectedGameObject.GetComponentInChildren<WallNumber>();
            WallFace wallFace = DesignArea.compass.GetWallFaceForNumber(wallNumber.GetWallNumberInt());
            Backdrop backdrop = DesignArea.GetBackdropForWallFace(wallFace);
            if (backdrop.backdropSpawnedAsset == null)
                Debug.Log("Backdrop on WallNumber " + wallNumber.GetWallNumberInt() + " has no spawn to infer from!");
            else
            {
                Transform spawnRT = backdrop.backdropSpawnedAsset.transform;
                _backdropRotation.value = spawnRT.eulerAngles.y - ((int)wallFace * 90);
                _backdropVerticalOffset.value = spawnRT.localPosition.y - wallNumber.transform.localPosition.y;
                int wallNr = wallNumber.GetWallNumberInt();
                Vector3 adjustedPos = new Vector3(spawnRT.localPosition.x, wallNumber.transform.localPosition.y, spawnRT.localPosition.z);

                if (wallNr == 1)
                {
                    _backdropHorizontalOffset.value = spawnRT.localPosition.x  * -1 - wallNumber.transform.localPosition.x ;
                    adjustedPos += new Vector3(_backdropHorizontalOffset.value, 0, 0);
                }
                else if (wallNr == 2)
                {
                    _backdropHorizontalOffset.value = spawnRT.localPosition.z  ; 
                    adjustedPos -= new Vector3(0, 0, _backdropHorizontalOffset.value ); 
                }
                else if (wallNr == 3)
                { 
                    _backdropHorizontalOffset.value = spawnRT.localPosition.x  ;
                    adjustedPos -= new Vector3(_backdropHorizontalOffset.value, 0, 0);

                }
                else if (wallNr == 4)
                { 
                    _backdropHorizontalOffset.value = spawnRT.localPosition.z  * -1 -   wallNumber.transform.localPosition.z;
                    adjustedPos += new Vector3(0, 0, _backdropHorizontalOffset.value); 

                }
                _backdropDistance.value = Vector3.Distance(wallNumber.transform.localPosition, adjustedPos);

            }

        }
       
   
        private void MakeNorth()
        {
            WallNumber wallNumber = _selectedGameObject.GetComponentInChildren<WallNumber>();
            DesignArea.compass.MakeNorth(wallNumber.GetWallNumberInt());
            ToolWindowSettings.SendPipelineUpdateSignal();
        }


        private void LoadSyncContent()
        { 
            Sync sync = _selectedGameObject.GetComponentInChildren<Sync>();
            _generalSyncContent = _syncRoot.Q<VisualElement>("GeneralContent");
            _syncLinkContent = _syncRoot.Q<VisualElement>("SyncLinkContent");
            _nameLabelSync = _syncRoot.Q<Label>("NameLabel");
            _limitBySizeLabel = _syncRoot.Q<Label>("ForceSizeLabel");
            _freeAreaLabel = _syncRoot.Q<Label>("FreeAreaLabel");
            _syncLinkIcon = _syncRoot.Q<VisualElement>("Icon");
            _syncIcon = _syncRoot.Q<VisualElement>("SyncIcon");
 
            _qualitySelector = _syncRoot.Q<EnumField>("QualitySelector");
            _boundsField = _syncRoot.Q<BoundsField>("BoundsField"); 
            _recombineMeshesButton = _syncRoot.Q<Button>("RecombineMeshesButton");
            _playFieldButton = _syncRoot.Q<Button>("PlayFieldButton");
            _mockifyButton = _syncRoot.Q<Button>("CreateMockButton");
            _forceSizeToggle = _syncRoot.Q<Toggle>("ForceSizeToggle");
            _freeAreaToggle = _syncRoot.Q<Toggle>("FreeAreaToggle");
            _sizesContainer = _syncRoot.Q<VisualElement>("SizesCombo");
            _freeAreaContainer = _syncRoot.Q<VisualElement>("FreeAreaContainer");
            _minSizeVector3Field = _syncRoot.Q<Vector3Field>("MinSizeVector3Field");
            _maxSizeVector3Field = _syncRoot.Q<Vector3Field>("MaxSizeVector3Field");  
            _freeAreaRectField = _syncRoot.Q<RectField>("FreeAreaRectField");   

            _syncLinkRowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(_toolWindowSettings.EditorFolderPath + "/SyncLinkRowTemplate.uxml");
            _syncLinkListViewRoot = _syncRoot.Q<VisualElement>("SyncLinkListRoot");
            _syncLinkNameLabel1 = _syncRoot.Q<Label>("LinkNameLabel1");
            _syncLinkNameLabel2 = _syncRoot.Q<Label>("LinkNameLabel2");
            _syncLinkCloseButton = _syncRoot.Q<Button>("SyncLinkCloseButton");
            _minDistFloat = _syncRoot.Q<FloatField>("MinDistFloat");
            _prefDistFloat = _syncRoot.Q<FloatField>("PrefDistFloat");
            _maxDistFloat = _syncRoot.Q<FloatField>("MaxDistFloat");
            _onTopToggle = _syncRoot.Q<Toggle>("OnTopToggle");
            _onBottomToggle = _syncRoot.Q<Toggle>("OnBottomToggle");
            _limitRotateToggle = _syncRoot.Q<Toggle>("LimitRotateToggle");
            _outerRotateToggle = _syncRoot.Q<Toggle>("OuterRotateToggle");
            _minAngleFloat = _syncRoot.Q<FloatField>("MinAngleFloat");
            _maxAngleFloat = _syncRoot.Q<FloatField>("MaxAngleFloat");
            
            _regularSyncLinkContent = _syncRoot.Q<VisualElement>("RegularContent");
            _virtualSyncLinkContent = _syncRoot.Q<VisualElement>("VirtualContent");
            _virtualInfoLabel = _syncRoot.Q<Label>("VirtualInfoLabel");
            _syncLinkAnchorVector = _syncRoot.Q<Vector3Field>("AnchorVector3Field");
            _syncLinkOffsetVector = _syncRoot.Q<Vector3Field>("OffsetVector3Field");
            _syncLinkVirtualScaleToggle = _syncRoot.Q<Toggle>("ScaleToggle");
            _syncLinkEveryClutterToggle = _syncRoot.Q<Toggle>("EveryClutterToggle");
            _syncLinkEveryClutterContainer = _syncRoot.Q<VisualElement>("ClutterToggleCombo");
            _syncLinkInferButton = _syncRoot.Q<Button>("InferButton");
            _syncLinkRepositionButton = _syncRoot.Q<Button>("RepositionButton");
            _classificationMaskField = _syncRoot.Q<MaskField>("ClassificationMask");
            _classificationContainer = _syncRoot.Q<VisualElement>("ClassificationCombo");


             
            SerializedObject so = new SerializedObject(sync);

            _classificationContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            if (sync.quality == SyncQuality.Virtual)
                _classificationContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);

            _generalSyncContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            _syncLinkContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            _nameLabelSync.text = _selectedGameObject.name;
        
      
            _qualitySelector.bindingPath = nameof(sync.quality);
            _qualitySelector.Bind(so);
   
        _syncIcon.RegisterCallback<MouseDownEvent>(evt => { ClickDeleteSyncIcon(_selectedGameObject.GetComponentInChildren<Sync>()); UpdateContent(SelectionMode.None, null); SceneView.RepaintAll();});
  
        _boundsField.bindingPath =  nameof(sync.bounds);     
        _boundsField.Bind(so);
        RoundBounds(sync);
        _boundsField.RegisterCallback<ChangeEvent<Vector3>>(evt => { SceneView.RepaintAll(); RoundBounds(_selectedGameObject.GetComponentInChildren<Sync>()); });  
        _recombineMeshesButton.clicked -= ClickRecombineMeshesButton;
        _recombineMeshesButton.clicked += ClickRecombineMeshesButton;
        _forceSizeToggle.bindingPath = nameof(sync.useSizes);
        _forceSizeToggle.Bind(so);
        if (sync.quality == SyncQuality.Virtual)
        {
            _forceSizeToggle.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            _freeAreaToggle.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            _limitBySizeLabel.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            _sizesContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            _freeAreaContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            _freeAreaLabel.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        }
        else
        {
            _forceSizeToggle.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            _freeAreaToggle.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            _limitBySizeLabel.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex); 
            _sizesContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            _freeAreaContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            _freeAreaLabel.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        }
        if (_freeAreaToggle.value == false)
            _freeAreaContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        if (_forceSizeToggle.value == false)
            _sizesContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            
            
        /*    
         _classificationSelector = _syncRoot.Q<EnumField>("ClassificationSelector");
         _classificationSelector.RegisterCallback<ChangeEvent<Enum>>(evt =>
        { 
            _toolWindowSettings.SendPipelineUpdateSignal();
        });
        _classificationSelector.bindingPath = nameof(sync.classification);   
        _classificationSelector.Bind(so);
        _classificationSelector.RegisterCallback<ChangeEvent<float>>(evt => { DesignArea.RegisterChangeFromUI(); SceneView.RepaintAll(); });*/

        _qualitySelector.RegisterCallback<ChangeEvent<Enum>>(evt =>
            {
                if (_qualitySelector.value.ToString() == SyncQuality.Virtual.ToString())
                {
                    _limitBySizeLabel.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                    _freeAreaLabel.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                    _forceSizeToggle.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                    _freeAreaToggle.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                    _sizesContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                    _freeAreaContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                    _classificationContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);  
                    SceneView.RepaintAll();
                }
                else
                { 
                    _classificationContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex); 
                    _limitBySizeLabel.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                    _freeAreaLabel.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                    _forceSizeToggle.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                    _freeAreaToggle.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                    if (_forceSizeToggle.value == true)
                         _sizesContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);      
                    if (_freeAreaToggle.value == true)
                        _freeAreaContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                    SceneView.RepaintAll();
                }
                _toolWindowSettings.SendPipelineUpdateSignal();
            });
            _forceSizeToggle.RegisterCallback<ChangeEvent<bool>>(evt => { _sizesContainer.style.display = evt.newValue ?  new StyleEnum<DisplayStyle>(DisplayStyle.Flex) : new StyleEnum<DisplayStyle>(DisplayStyle.None); });
            _freeAreaToggle.RegisterCallback<ChangeEvent<bool>>(evt => { _freeAreaContainer.style.display = evt.newValue ?  new StyleEnum<DisplayStyle>(DisplayStyle.Flex) : new StyleEnum<DisplayStyle>(DisplayStyle.None); });
            
            _freeAreaToggle.bindingPath = nameof(sync.hasFreeArea);  
            _freeAreaToggle.Bind(so);  
            _minSizeVector3Field.bindingPath = nameof(sync.minSize);  
            _minSizeVector3Field.Bind(so);  
            _maxSizeVector3Field.bindingPath = nameof(sync.maxSize); 
            _maxSizeVector3Field.Bind(so);  
            _minSizeVector3Field.RegisterCallback<ChangeEvent<Vector3>>(evt => { SceneView.RepaintAll(); }); 
            _maxSizeVector3Field.RegisterCallback<ChangeEvent<Vector3>>(evt => { SceneView.RepaintAll(); });
    
            _freeAreaRectField.bindingPath = nameof(sync.freeArea);  
            _freeAreaRectField.Bind(so);   
            _freeAreaRectField.RegisterCallback<ChangeEvent<Vector3>>(evt => { SceneView.RepaintAll(); });

            
            _nameLabelSync.RegisterCallback<MouseDownEvent>(evt => {  SceneView.FrameLastActiveSceneView(); }); 

            
            _syncLinkCloseButton.clicked += () =>
            {
                _generalSyncContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                _syncLinkContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            };
            
            UpdateSyncLinkListForSync(sync); 
            CreateListViewForSync(sync);
            
            
            _qualitySelector.RegisterCallback<ChangeEvent<float>>(evt => { DesignArea.RegisterChangeFromUI();SceneView.RepaintAll(); });
            _boundsField.RegisterCallback<ChangeEvent<float>>(evt => { DesignArea.RegisterChangeFromUI(); SceneView.RepaintAll();});
//            _forceSizeToggle.RegisterCallback<ChangeEvent<bool>>(evt => { DesignArea.RegisterChangeFromUI();SceneView.RepaintAll(); });
            _sizesContainer.RegisterCallback<ChangeEvent<bool>>(evt => { DesignArea.RegisterChangeFromUI();SceneView.RepaintAll(); });
            _minSizeVector3Field.RegisterCallback<ChangeEvent<bool>>(evt => { DesignArea.RegisterChangeFromUI(); SceneView.RepaintAll();});
            _maxSizeVector3Field.RegisterCallback<ChangeEvent<bool>>(evt => { DesignArea.RegisterChangeFromUI();SceneView.RepaintAll(); });   
            _syncLinkAnchorVector.RegisterValueChangedCallback((_) => { ToolWindowSettings.SendPipelineUpdateSignal(); }); 
 
 
            _minDistFloat.RegisterCallback<ChangeEvent<float>>(evt => { DesignArea.RegisterChangeFromUI(); SceneView.RepaintAll(); });
            _prefDistFloat.RegisterCallback<ChangeEvent<float>>(evt => { DesignArea.RegisterChangeFromUI();SceneView.RepaintAll(); });
            _maxDistFloat.RegisterCallback<ChangeEvent<float>>(evt => { DesignArea.RegisterChangeFromUI(); SceneView.RepaintAll();});
            _onTopToggle.RegisterCallback<ChangeEvent<bool>>(evt => { DesignArea.RegisterChangeFromUI();SceneView.RepaintAll(); });
            _onBottomToggle.RegisterCallback<ChangeEvent<bool>>(evt => { DesignArea.RegisterChangeFromUI();SceneView.RepaintAll(); });
            _limitRotateToggle.RegisterCallback<ChangeEvent<bool>>(evt => { DesignArea.RegisterChangeFromUI(); SceneView.RepaintAll();});
            _outerRotateToggle.RegisterCallback<ChangeEvent<bool>>(evt => { DesignArea.RegisterChangeFromUI();SceneView.RepaintAll(); }); 
            _minAngleFloat.RegisterCallback<ChangeEvent<float>>(evt => { DesignArea.RegisterChangeFromUI(); SceneView.RepaintAll();});
            _maxAngleFloat.RegisterCallback<ChangeEvent<float>>(evt => { DesignArea.RegisterChangeFromUI();SceneView.RepaintAll(); });
         
            justMockified = false; 
            _mockifyButton.text = "Create Mock Physical";
            _mockifyButton.clicked -= Mockify;
            _mockifyButton.clicked += Mockify;

            _playFieldButton.clicked -= ClickAddPlayfield;
            _playFieldButton.clicked -= ClickExistingPlayfield;
            if (sync.hasPlayfield == false)
            {
                _playFieldButton.text = "Add Playfield";
                _playFieldButton.clicked += ClickAddPlayfield;
            }
            else
            {
                _playFieldButton.text = "Select Playfield";
                _playFieldButton.clicked += ClickExistingPlayfield;
            } 
            List<string> classificationNames = Enum.GetNames(typeof(Classification)).ToList();

            _classificationMaskField.choices = classificationNames;
            _classificationMaskField.bindingPath = nameof(sync.classification);
            _classificationMaskField.Bind(so);
           _classificationMaskField.RegisterValueChangedCallback((e) =>
           { 
               _toolWindowSettings.SendPipelineUpdateSignal();
           });

        
        
        }

        private void ChangeClassificationMaskValue(int val)
        {
            Sync sync = _selectedGameObject.GetComponentInChildren<Sync>();
            if (sync == null)
                return;
            sync.classification = (Classification)val;
        }
        
        private bool justMockified = false;
        private GameObject mockifiedMock;

        private void Mockify()
        {
            _mockifyButton.text = "Focus Camera on Mock";
            Sync sync = _selectedGameObject.GetComponentInChildren<Sync>(); 

            if (justMockified)
            {
                justMockified = false; 
                Selection.activeGameObject = mockifiedMock; 
                SceneView.FrameLastActiveSceneView(); 
            }
            else
            {
                if (sync.classificationList.Count == 0)
                mockifiedMock    = LayoutArea.SpawnMock(LayoutArea.transform.position,Classification.Misc, sync.bounds.size);
                else
                    mockifiedMock    = LayoutArea.SpawnMock(LayoutArea.transform.position, sync.classificationList[0], sync.bounds.size);

                justMockified = true; 
            } 
        }

        private void ClickAddPlayfield()
        { 
            _playFieldButton.text = "PF";
            Selection.activeGameObject.GetComponent<Sync>().AddPlayfieldToSync();
            LoadSyncContent();
        }
        private void ClickExistingPlayfield( )
        {
            GameObject pfObj = Selection.activeGameObject.GetComponent<Sync>().GetComponentInChildren<PlayfieldInteraction>().gameObject;
            Selection.activeGameObject = pfObj;
        }
        private void RoundBounds(Sync sync)
        { 
            sync.bounds.center = new Vector3( Mathf.Round(sync.bounds.center.x * 100f) / 100f,
                Mathf.Round(sync.bounds.center.y * 100f) / 100f,
                Mathf.Round(sync.bounds.center.z * 100f) / 100f);
            sync.bounds.extents = new Vector3( Mathf.Round(sync.bounds.extents.x * 100f) / 100f,
                Mathf.Round(sync.bounds.extents.y * 100f) / 100f,
                Mathf.Round(sync.bounds.extents.z * 100f) / 100f); 
        }
        private void RoundBounds(SyncLink syncLink)
        { 
            syncLink.anchorVector  = new Vector3( Mathf.Round(syncLink.anchorVector.x * 100f) / 100f,
                Mathf.Round(syncLink.anchorVector.y * 100f) / 100f,
                Mathf.Round(syncLink.anchorVector.z * 100f) / 100f);
        
        }
        private void ClickDeleteIcon(SyncLink syncLink, bool fromSurroundSync = false)
        {
         
            syncLink.parentSync.DestroySyncLink(syncLink);
            SceneView.RepaintAll();
        }
        private void ClickDeleteSyncIcon(Sync sync)
        {
            SyncMaster.DeleteSync(sync);
        }
        
        private void ClickSyncLinkFromSync(SyncLink syncLink)
        { 
            _activeSyncLink = syncLink;
            SerializedObject soLink = new SerializedObject(_activeSyncLink);

            _syncLinkNameLabel1.text = syncLink.parentSync.gameObject.name+ ",";
            _syncLinkNameLabel2.text = ""+ syncLink.linkedObject.name;
            _syncLinkNameLabel1.RegisterCallback<MouseDownEvent>(evt => { ClickSyncLinkLabel1(evt, _activeSyncLink); }); 
            _syncLinkNameLabel2.RegisterCallback<MouseDownEvent>(evt => { ClickSyncLinkLabel2(evt, _activeSyncLink); }); 

            _generalSyncContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            _syncLinkContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
   
            _syncLinkIcon.RegisterCallback<MouseDownEvent>(evt => { ClickDeleteIcon(_activeSyncLink); }); 
 
      
            _minDistFloat.bindingPath = nameof(syncLink.minDistance); 
            _prefDistFloat.bindingPath = nameof(syncLink.preferredDistance);
            _maxDistFloat.bindingPath = nameof(syncLink.maxDistance);
            _onTopToggle.bindingPath = nameof(syncLink.topOf);
            _onBottomToggle.bindingPath = nameof(syncLink.bottomOf);
            _limitRotateToggle.bindingPath = nameof(syncLink.rotationConstraint);
            _outerRotateToggle.bindingPath = nameof(syncLink.rotationOutside); 
            _minAngleFloat.bindingPath = nameof(syncLink.minAngle);
            _maxAngleFloat.bindingPath = nameof(syncLink.maxAngle); 
            
            _minDistFloat.Bind(soLink); 
            _prefDistFloat.Bind(soLink);
            _maxDistFloat.Bind(soLink);
            _onTopToggle.Bind(soLink);
            _onBottomToggle.Bind(soLink);
            _limitRotateToggle.Bind(soLink);
            _outerRotateToggle.Bind(soLink); 
            _minAngleFloat.Bind(soLink);
            _maxAngleFloat.Bind(soLink);
            
            Vector3 syncLinkLineStartPos= syncLink.parentSync.transform.position ;
            Vector3 syncLinkLineEndPos=  syncLink.linkedObject.transform.position; 
        //    syncLinkLineStartPos += new Vector3(0, syncLink.parentSync.handleHeight + 1.5f, 0); 
         //   syncLinkLineEndPos += new Vector3(0, syncLink.parentSync.handleHeight + 1.5f, 0);
            Debug.DrawLine(syncLinkLineStartPos, syncLinkLineEndPos, Color.yellow, 0.5f);
            
            if (SyncLinkHasVirtualSync(syncLink))
            {
                _regularSyncLinkContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                _virtualSyncLinkContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
   
                _syncLinkAnchorVector.bindingPath = nameof(syncLink.anchorVector);
                _syncLinkOffsetVector.bindingPath = nameof(syncLink.offsetVector);
                _syncLinkVirtualScaleToggle.bindingPath = nameof(syncLink.scaleWithSync);
                _syncLinkEveryClutterToggle.bindingPath = nameof(syncLink.onEveryClutter);
                _syncLinkAnchorVector.Bind(soLink);
                RoundBounds(   syncLink); 
                _syncLinkOffsetVector.Bind(soLink);
                _syncLinkVirtualScaleToggle.Bind(soLink);  
                _syncLinkEveryClutterToggle.Bind(soLink);  
                _syncLinkInferButton.clicked -=ClickSyncInferButton;
                _syncLinkInferButton.clicked +=ClickSyncInferButton;
                _syncLinkRepositionButton.clicked -= ClickSyncRepositionButton;
                _syncLinkRepositionButton.clicked += ClickSyncRepositionButton;
            //    _syncLinkInferButton.clicked += () =>ClickSyncInferButton(syncLink);
            _syncLinkEveryClutterContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);

                string infoStr = "";
                if (SyncLinkHasVirtualParentSync(syncLink) && SyncLinkHasVirtualLinkedSync(syncLink))
                {
                    infoStr += "Virtual Primary & Virtual Secondary";
                }
                else if (SyncLinkHasVirtualParentSync(syncLink))
                {
                    if (syncLink.linkedObject.GetComponent<Sync>()!= null)
                        if (syncLink.linkedObject.GetComponent<Sync>().quality == SyncQuality.Clutter)
                            _syncLinkEveryClutterContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex); 
                    infoStr += "Virtual SyncLink";
                }
                else if (SyncLinkHasVirtualLinkedSync(syncLink))
                { 
                        if (syncLink.parentSync.quality == SyncQuality.Clutter)
                            _syncLinkEveryClutterContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex); 
                    infoStr += "Virtual Secondary";
                }
                _virtualInfoLabel.text = infoStr;
            } 
            else
            {
                _regularSyncLinkContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                _virtualSyncLinkContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            }

        }

       

        private void ClickSyncInferButton()
        { 
            // offset is calculated for the anchor (centerpoint of virtual). so to always keep it 10cm from edge, the anchor is on the edge
            Transform parentTransform;
            Vector3 virtualPos; 
            Bounds originBounds;
            if (SyncLinkHasVirtualParentSync(_activeSyncLink) && SyncLinkHasVirtualLinkedSync(_activeSyncLink))
            {
                virtualPos = _activeSyncLink.parentSync.transform.position;
                parentTransform = _activeSyncLink.linkedObject.transform;
                originBounds = _activeSyncLink.linkedObject.GetComponent<Sync>().bounds;
            }
            else if (SyncLinkHasVirtualParentSync(_activeSyncLink))
            {
                if (_activeSyncLink.linkedObject.GetComponent<Sync>() != null)
                {
                    virtualPos = _activeSyncLink.parentSync.transform.position;
                    parentTransform = _activeSyncLink.linkedObject.transform;
                    originBounds = _activeSyncLink.linkedObject.GetComponent<Sync>().bounds;
                }
                else // linked to SurroundSync
                {
                    SurroundSync ssync = _activeSyncLink.linkedObject.GetComponent<SurroundSync>();
               //     Transform ssyncMiniTransform = DesignArea.wallSplitter.GetWallMiniForSurroundSyncID(ssync.ID).miniObject.transform;
                    virtualPos =  _activeSyncLink.parentSync.transform.position;
                    parentTransform = ssync.transform;
                    originBounds = new Bounds(Vector3.zero, Vector3.one * 2);
                }
            }
            else // SyncLinkONLYHasVirtualLinkedSync = true
            { 
                virtualPos = _activeSyncLink.linkedObject.transform.position;
                parentTransform = _activeSyncLink.parentSync.transform;
                originBounds = _activeSyncLink.parentSync.GetComponent<Sync>().bounds;
            }

            _activeSyncLink.anchorVector = parentTransform.InverseTransformPoint(virtualPos).ScaleBy(originBounds.max.invert()).ScaleBy(parentTransform.localScale);
            _syncLinkAnchorVector.value = _activeSyncLink.anchorVector; 
            _toolWindowSettings.SendPipelineUpdateSignal();
        }
        private void ClickSyncRepositionButton()
        {
            Transform parentSyncTransform;
            Vector3 anchorVector = _activeSyncLink.anchorVector;
            Bounds parentSyncBounds;
            Vector3 offset = _syncLinkOffsetVector.value;
            Transform virtualObjectToBeMoved;
            if (SyncLinkHasVirtualParentSync(_activeSyncLink) && SyncLinkHasVirtualLinkedSync(_activeSyncLink))
            {
                parentSyncTransform = _activeSyncLink.linkedObject.transform;
                parentSyncBounds = _activeSyncLink.linkedObject.GetComponent<Sync>().bounds;
                virtualObjectToBeMoved = _activeSyncLink.parentSync.transform;
                
            }
            else if (SyncLinkHasVirtualParentSync(_activeSyncLink))
            {
                if (_activeSyncLink.linkedObject.GetComponent<Sync>() != null)
                {
                    parentSyncTransform = _activeSyncLink.linkedObject.transform;
                    parentSyncBounds = _activeSyncLink.linkedObject.GetComponent<Sync>().bounds;
                    virtualObjectToBeMoved = _activeSyncLink.parentSync.transform;
                }
                else // linked to SurroundSync
                {
                    SurroundSync ssync = _activeSyncLink.linkedObject.GetComponent<SurroundSync>();
                    virtualObjectToBeMoved = _activeSyncLink.parentSync.transform;  
                    parentSyncTransform =  _activeSyncLink.parentSync.transform;  
                    parentSyncBounds = ssync.bounds;
                }
            }
            else // SyncLinkONLYHasVirtualLinkedSync = true
            {
                parentSyncTransform = _activeSyncLink.parentSync.transform; 
                parentSyncBounds = _activeSyncLink.parentSync.bounds;
                virtualObjectToBeMoved = _activeSyncLink.linkedObject.transform;
            }
            virtualObjectToBeMoved.position = anchorVector.ScaleBy(parentSyncBounds.max) + offset + parentSyncTransform.position;
        }
        private void ClickSyncLinkLabel1(MouseDownEvent mouseDownEvent, SyncLink activeSyncLink)
        {
            Selection.activeGameObject = activeSyncLink.parentSync.gameObject;
            _generalSyncContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            _syncLinkContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
           
            if (mouseDownEvent.button == 1)
                SceneView.FrameLastActiveSceneView();
        }
        private void ClickSyncLinkLabel2(MouseDownEvent mouseDownEvent, SyncLink activeSyncLink)
        {
            Selection.activeGameObject = activeSyncLink.linkedObject;
            _generalSyncContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            _syncLinkContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            
            if (mouseDownEvent.button == 1)
                SceneView.FrameLastActiveSceneView(); 
        }

        private void CreateListViewForSync(Sync sync)
        {
            _syncLinkListViewRoot.Clear();
            VisualElement MakeItem() => _syncLinkRowTemplate.CloneTree();

            void BindItem(VisualElement e, int i)
            {
                e.Q<Label>("LinkedName").text = _syncLinkList[i].ID;
                if (_syncLinkList[i].parentSync.quality == SyncQuality.Virtual)
                    e.Q<Label>("LinkedName").style.color = Color.green;
                else if (_syncLinkList[i].parentSync == sync)
                    e.Q<Label>("LinkedName").style.color = Color.yellow;
                e.RegisterCallback<MouseUpEvent>(evt => { ClickSyncLinkFromSync(_syncLinkList[i]); });
            }

            _syncLinkListView = new ListView(_syncLinkList, 35, MakeItem, BindItem)
            {
                selectionType = SelectionType.Single,
                reorderable = true,
                style =
                {
                    height = _syncLinkList.Count * 40,
                }
            };
            _syncLinkListViewRoot.Add(_syncLinkListView);
        }

        private void UpdateSyncLinkListForSync(Sync sync)
        {
            _syncLinkList = new List<SyncLink>();
            foreach (var syncLink in sync.GetSyncLinksFromComponents())
                _syncLinkList.Add(syncLink);
            foreach (var syncLink in sync.GetPassiveSyncLinks())
                _syncLinkList.Add(syncLink);
        }
       
        private void ClickRecombineMeshesButton()
        {
            _selectedGameObject.GetComponentInChildren<Sync>().CreateCombinedMesh();
            RoundBounds(_selectedGameObject.GetComponentInChildren<Sync>());
            SceneView.RepaintAll();
        }


        private SurroundSync surroundSync;
    private void LoadSurroundSyncContent()
        {
            deleteClickAllowed = true;
            if (_selectedGameObject != null)
               if (_selectedGameObject.GetComponent<SurroundSync>() != null)
                  surroundSync = _selectedGameObject.GetComponent<SurroundSync>();
            if (surroundSync == null)
                Debug.LogWarning("No SurroundSync or WallMini was selected while its UI is loading!");
            
                
            _selectedSurroundSyncID = surroundSync.ID;
            _generalContentSS = _surroundSyncRoot.Q<VisualElement>("GeneralContent");
            _syncLinkContentSS = _surroundSyncRoot.Q<VisualElement>("SyncLinkContent");
            _nonBasePieceContentSS = _surroundSyncRoot.Q<VisualElement>("NonBasePieceContent");
            _basePieceContentSS = _surroundSyncRoot.Q<VisualElement>("BasePieceContent");
            _syncLinkListViewRootSS = _surroundSyncRoot.Q<VisualElement>("SyncLinkListRoot");
            _nameLabelSS = _surroundSyncRoot.Q<Label>("NameLabel"); 
            _syncLinkIcon = _surroundSyncRoot.Q<VisualElement>("Icon");
            _basePieceToggleSS = _surroundSyncRoot.Q<Toggle>("BasePieceToggle");
            _requireFreeSpaceToggleSS = _surroundSyncRoot.Q<Toggle>("RequireFreeSpaceToggle");
            _basePieceToggleRandomize= _surroundSyncRoot.Q<Toggle>("ToggleRandomize");
            _basePieceToggleFace1= _surroundSyncRoot.Q<Toggle>("ToggleFace1");
            _basePieceToggleFace2 = _surroundSyncRoot.Q<Toggle>("ToggleFace2");
            _basePieceToggleFace3 = _surroundSyncRoot.Q<Toggle>("ToggleFace3");
            _basePieceToggleFace4 = _surroundSyncRoot.Q<Toggle>("ToggleFace4");
            _basePieceLabelFace1 = _surroundSyncRoot.Q<Label>("LabelFace1");
            _basePieceLabelFace2 = _surroundSyncRoot.Q<Label>("LabelFace2");
            _basePieceLabelFace3 = _surroundSyncRoot.Q<Label>("LabelFace3");
            _basePieceLabelFace4 = _surroundSyncRoot.Q<Label>("LabelFace4");
            _faceSelectorSS = new EnumField();
            _faceSelectorSS = _surroundSyncRoot.Q<EnumField>("FaceSelector");
            _mappedIndexFieldSS = new IntegerField();
            _orderFieldSS = new IntegerField();
            _priorityFieldSS = new IntegerField();
            _mappedIndexFieldSS = _surroundSyncRoot.Q<IntegerField>("MappedIndexInt");
            _orderFieldSS = _surroundSyncRoot.Q<IntegerField>("OrderInt");
            _priorityFieldSS = _surroundSyncRoot.Q<IntegerField>("PriorityInt");
            _repeatAmountFieldSS = _surroundSyncRoot.Q<IntegerField>("RepeatAmountInt"); 
            _initializeButtonSS = new Button();
            _initializeButtonSS = _surroundSyncRoot.Q<Button>("InitializeButton");
            _recombineMeshButtonSS = _surroundSyncRoot.Q<Button>("RecombineButton");
            _playFieldButtonSS = _surroundSyncRoot.Q<Button>("PlayFieldButton");
            _surroundSyncIcon = _surroundSyncRoot.Q<VisualElement>("SyncIcon");


            _syncLinkRowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(  _toolWindowSettings.EditorFolderPath +"/SyncLinkRowTemplate.uxml");
            _syncLinkListViewRootSS = _surroundSyncRoot.Q<VisualElement>("SyncLinkListRoot");
            _syncLinkNameLabel1 = _surroundSyncRoot.Q<Label>("LinkNameLabel1");
            _syncLinkNameLabel2 = _surroundSyncRoot.Q<Label>("LinkNameLabel2");
            _syncLinkCloseButton = _surroundSyncRoot.Q<Button>("SyncLinkCloseButton");
            _minDistFloat = _surroundSyncRoot.Q<FloatField>("MinDistFloat");
            _prefDistFloat = _surroundSyncRoot.Q<FloatField>("PrefDistFloat");
            _maxDistFloat = _surroundSyncRoot.Q<FloatField>("MaxDistFloat");
            _onTopToggle = _surroundSyncRoot.Q<Toggle>("OnTopToggle");
            _onBottomToggle = _surroundSyncRoot.Q<Toggle>("OnBottomToggle");
            _limitRotateToggle = _surroundSyncRoot.Q<Toggle>("LimitRotateToggle");
            _outerRotateToggle = _surroundSyncRoot.Q<Toggle>("OuterRotateToggle");
            _minAngleFloat = _surroundSyncRoot.Q<FloatField>("MinAngleFloat");
            _maxAngleFloat = _surroundSyncRoot.Q<FloatField>("MaxAngleFloat"); 
            
            _regularSyncLinkContent = _surroundSyncRoot.Q<VisualElement>("RegularContent");
            _virtualSyncLinkContent = _surroundSyncRoot.Q<VisualElement>("VirtualContent");
            _virtualInfoLabel = _surroundSyncRoot.Q<Label>("VirtualInfoLabel"); 
            _syncLinkAnchorVector = _surroundSyncRoot.Q<Vector3Field>("AnchorVector3Field");
            _syncLinkOffsetVector = _surroundSyncRoot.Q<Vector3Field>("OffsetVector3Field");
            _syncLinkVirtualScaleToggle = _surroundSyncRoot.Q<Toggle>("ScaleToggle");
            _syncLinkInferButton = _surroundSyncRoot.Q<Button>("InferButton");
            _syncLinkRepositionButton = _surroundSyncRoot.Q<Button>("RepositionButton");

            if (surroundSync == null)
                return;
            SerializedObject so = new SerializedObject(surroundSync);
            
            _generalContentSS.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            _syncLinkContentSS.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            _nameLabelSS.text = surroundSync.name; 
            _nameLabelSS.RegisterCallback<MouseDownEvent>(evt => { if (surroundSync != null) Selection.activeGameObject = surroundSync.gameObject; });
            _basePieceToggleSS.bindingPath =  nameof(surroundSync.baseReplacer);
            _requireFreeSpaceToggleSS.bindingPath =  nameof(surroundSync.hasPlayfield);
            _basePieceToggleRandomize.bindingPath = nameof(surroundSync.baseReplaceRandomize);
            _basePieceToggleFace1.bindingPath = nameof(surroundSync.baseReplaceFace1);
            _basePieceToggleFace2.bindingPath = nameof(surroundSync.baseReplaceFace2);
            _basePieceToggleFace3.bindingPath = nameof(surroundSync.baseReplaceFace3);
            _basePieceToggleFace4.bindingPath = nameof(surroundSync.baseReplaceFace4);
            _basePieceToggleSS.Bind(so);
            _requireFreeSpaceToggleSS.Bind(so);
            _basePieceToggleRandomize.Bind(so);
            _basePieceToggleFace1.Bind(so);
            _basePieceToggleFace2.Bind(so);
            _basePieceToggleFace3.Bind(so);
            _basePieceToggleFace4.Bind(so);
            _basePieceLabelFace1.text =   WallFace.North.ToString();
            _basePieceLabelFace2.text =   WallFace.East.ToString() + "   ";
            _basePieceLabelFace3.text =   WallFace.South.ToString();
            _basePieceLabelFace4.text =   WallFace.West.ToString() + " ";
            
            _basePieceToggleFace1.RegisterValueChangedCallback((_) => { ToolWindowSettings.SendPipelineUpdateSignal(); });
            _basePieceToggleFace2.RegisterValueChangedCallback((_) => { ToolWindowSettings.SendPipelineUpdateSignal(); });
            _basePieceToggleFace3.RegisterValueChangedCallback((_) => { ToolWindowSettings.SendPipelineUpdateSignal(); });
            _basePieceToggleFace4.RegisterValueChangedCallback((_) => { ToolWindowSettings.SendPipelineUpdateSignal(); });
            _syncLinkAnchorVector.RegisterValueChangedCallback((_) => { ToolWindowSettings.SendPipelineUpdateSignal(); });

            
            _nameLabelSS.RegisterCallback<MouseDownEvent>(evt => {  SceneView.FrameLastActiveSceneView(); }); 


            _surroundSyncIcon.RegisterCallback<MouseDownEvent>(evt => { ClickDeleteSurroundSync(); UpdateContent(SelectionMode.None, null); SceneView.RepaintAll();});

         //   _nonBasePieceContentSS.style.visibility = _basePieceToggleSS.value  ? new StyleEnum<Visibility>(Visibility.Hidden) : new StyleEnum<Visibility>(Visibility.Visible);
         _basePieceContentSS.style.display = _basePieceToggleSS.value  ?  new StyleEnum<DisplayStyle>(DisplayStyle.Flex) : new StyleEnum<DisplayStyle>(DisplayStyle.None);
            _nonBasePieceContentSS.style.display = _basePieceToggleSS.value  ?  new StyleEnum<DisplayStyle>(DisplayStyle.None) : new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
         
            _basePieceToggleSS.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                ToolWindowSettings.SendPipelineUpdateSignal();
              _nonBasePieceContentSS.style.display = evt.newValue ? new StyleEnum<DisplayStyle>(DisplayStyle.None) : new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
           //     _nonBasePieceContentSS.style.visibility = evt.newValue ? new StyleEnum<Visibility>(Visibility.Hidden) : new StyleEnum<Visibility>(Visibility.Visible);
                 _basePieceContentSS.style.display = evt.newValue ? new StyleEnum<DisplayStyle>(DisplayStyle.Flex) : new StyleEnum<DisplayStyle>(DisplayStyle.None);
           //     _basePieceContentSS.style.visibility = evt.newValue ? new StyleEnum<Visibility>(Visibility.Visible) : new StyleEnum<Visibility>(Visibility.Hidden);
            }); 
            
          
            _faceSelectorSS.bindingPath =  nameof(surroundSync.face);
            _faceSelectorSS.Bind(so);    
            
             
            _mappedIndexFieldSS.bindingPath = nameof(surroundSync.mapToIndex);  
            _mappedIndexFieldSS.Bind(so);   
            _orderFieldSS.bindingPath = nameof(surroundSync.offset);  
            _orderFieldSS.Bind(so);    
            _priorityFieldSS.bindingPath = nameof(surroundSync.priority);  
            _priorityFieldSS.Bind(so);   
            _repeatAmountFieldSS.bindingPath = nameof(surroundSync.repeatForAmount);
            _repeatAmountFieldSS.Bind(so);
            
            _mappedIndexFieldSS.RegisterValueChangedCallback((_) => { ToolWindowSettings.SendPipelineUpdateSignal(); });
            _orderFieldSS.RegisterValueChangedCallback((_) => { ToolWindowSettings.SendPipelineUpdateSignal(); });
            _priorityFieldSS.RegisterValueChangedCallback((_) => { ToolWindowSettings.SendPipelineUpdateSignal(); });
            _repeatAmountFieldSS.RegisterValueChangedCallback((_) => { ToolWindowSettings.SendPipelineUpdateSignal(); });
            _requireFreeSpaceToggleSS.RegisterValueChangedCallback((_) => { ToolWindowSettings.SendPipelineUpdateSignal(); });

        
            _syncLinkCloseButton.clicked += () =>
            {
                _generalContentSS.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                _syncLinkContentSS.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            };
 
            _initializeButtonSS.clicked -= ClickInitializeButton; 
            _initializeButtonSS.clicked += ClickInitializeButton;
 
            _syncLinkCloseButton.clicked += () =>
            {
                _generalContentSS.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                _syncLinkContentSS.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            };

            UpdateSyncLinkListForSurroundSync(surroundSync.ID);
            CreateListViewForSurroundSync();
            
            _basePieceToggleSS.RegisterCallback<ChangeEvent<float>>(evt => { DesignArea.RegisterChangeFromUI(); SceneView.RepaintAll(); }); 
            _faceSelectorSS.RegisterCallback<ChangeEvent<Enum>>(evt => { DesignArea.RegisterChangeFromUI();SceneView.RepaintAll(); }); 
            _mappedIndexFieldSS.RegisterCallback<ChangeEvent<int>>(evt => { DesignArea.RegisterChangeFromUI(); SceneView.RepaintAll();});
            _repeatAmountFieldSS.RegisterCallback<ChangeEvent<int>>(evt => { DesignArea.RegisterChangeFromUI();SceneView.RepaintAll(); });
  
            _minDistFloat.RegisterCallback<ChangeEvent<float>>(evt => { DesignArea.RegisterChangeFromUI(); SceneView.RepaintAll(); });
            _prefDistFloat.RegisterCallback<ChangeEvent<float>>(evt => { DesignArea.RegisterChangeFromUI();SceneView.RepaintAll(); });
            _maxDistFloat.RegisterCallback<ChangeEvent<float>>(evt => { DesignArea.RegisterChangeFromUI(); SceneView.RepaintAll();});
            _onTopToggle.RegisterCallback<ChangeEvent<bool>>(evt => { DesignArea.RegisterChangeFromUI();SceneView.RepaintAll(); });
            _onBottomToggle.RegisterCallback<ChangeEvent<bool>>(evt => { DesignArea.RegisterChangeFromUI();SceneView.RepaintAll(); });
            _limitRotateToggle.RegisterCallback<ChangeEvent<bool>>(evt => { DesignArea.RegisterChangeFromUI(); SceneView.RepaintAll();});
            _outerRotateToggle.RegisterCallback<ChangeEvent<bool>>(evt => { DesignArea.RegisterChangeFromUI();SceneView.RepaintAll(); }); 
            _minAngleFloat.RegisterCallback<ChangeEvent<float>>(evt => { DesignArea.RegisterChangeFromUI(); SceneView.RepaintAll();});
            _maxAngleFloat.RegisterCallback<ChangeEvent<float>>(evt => { DesignArea.RegisterChangeFromUI();SceneView.RepaintAll(); });

          
            _recombineMeshButtonSS.clicked -= ClickRecombineSSBounds;
            _recombineMeshButtonSS.clicked += ClickRecombineSSBounds;
            _playFieldButtonSS.clicked -= ClickAddPlayfieldSS;
            _playFieldButtonSS.clicked -= ClickExistingPlayfieldSS;
            if (surroundSync.hasPlayfield == false)
            {
                _playFieldButtonSS.text = "Add Playfield";
                _playFieldButtonSS.clicked += ClickAddPlayfieldSS;
            }
            else
            {
                _playFieldButtonSS.text = "Select Playfield";
                _playFieldButtonSS.clicked += ClickExistingPlayfieldSS;
            } 
            
            
        }

    private void ClickRecombineSSBounds()
    {
        if (surroundSync != null)
           surroundSync.CreateCombinedMesh();
        else
            Debug.LogWarning("Selected SurroundSync not found!");
    }

    private void ClickExistingPlayfieldSS()
    {
        GameObject pfObj = null;
       if (_selectedGameObject.GetComponent<SurroundSync>() != null)
            pfObj = _selectedGameObject.GetComponentInChildren<PlayfieldInteraction>().gameObject;
        if (pfObj == null)
            Debug.LogWarning("No Playfield was selected while Playfield UI is loading!"); 
        if (pfObj != null)
           Selection.activeGameObject = pfObj;
    }

    private void ClickAddPlayfieldSS()
    { 
       if (_selectedGameObject.GetComponent<SurroundSync>() != null)
            surroundSync = _selectedGameObject.GetComponent<SurroundSync>();
        if (surroundSync == null)
            Debug.LogWarning("No SurroundSync or WallMini was selected while Playfield UI is loading!");
        _playFieldButtonSS.text = "PF";
        surroundSync.AddPlayfieldToSurroundSync();
        LoadSurroundSyncContent();
    }
      
        private void ClickDeleteSurroundSync()
        {
            if (deleteClickAllowed)
            {
                deleteClickAllowed = false; 
                SurroundSync surroundSync = _selectedGameObject.GetComponent<SurroundSync>() ;
                Object.FindObjectOfType<DesignArea>().wallSplitter.DeleteSurroundSync(surroundSync);
                UpdateContent(SelectionMode.None, null);
            }
        }
        private void ClickInitializeButton()
        { 
            DesignArea.ReloadAndReselectWallMini(_selectedSurroundSyncID);
        }

        private void CreateListViewForSurroundSync()
        { 
            _syncLinkListViewRootSS.Clear();
            VisualElement MakeItem() => _syncLinkRowTemplate.CloneTree(); 
            void BindItem(VisualElement e, int i)
            { 
                e.Q<Label>("LinkedName").text = _syncLinkList[i].parentSync.name;
                e.Q<Label>("LinkedName").style.color = Color.magenta;
                e.RegisterCallback<MouseUpEvent>(evt => { ClickSyncLinkFromSurroundSync(_syncLinkList[i]); }); 
            } 
            _syncLinkListView = new ListView(_syncLinkList, 35, MakeItem, BindItem)
            {
                selectionType = SelectionType.Single,
                reorderable = true,
                style = {
                    height = _syncLinkList.Count * 40, 
                }};    
            _syncLinkListViewRootSS.Add(_syncLinkListView);
        }
        private void UpdateSyncLinkListForSurroundSync(string ssID)
        {
            _syncLinkList = Object.FindObjectOfType<SyncMaster>().GetPassiveSyncLinksForSurroundSync(SyncMaster.GetSurroundSyncByID(ssID));
        }
        private void ClickSyncLinkFromSurroundSync(SyncLink syncLink)
        { 
            SerializedObject soLink = new SerializedObject(syncLink);
            _activeSyncLink = syncLink;

            _generalContentSS.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            _syncLinkContentSS.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            _regularSyncLinkContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            _virtualSyncLinkContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
 
            _minDistFloat.bindingPath = nameof(syncLink.minDistance); 
            _prefDistFloat.bindingPath = nameof(syncLink.preferredDistance);
            _maxDistFloat.bindingPath = nameof(syncLink.maxDistance);
            _onTopToggle.bindingPath = nameof(syncLink.topOf);
            _onBottomToggle.bindingPath = nameof(syncLink.bottomOf);
            _limitRotateToggle.bindingPath = nameof(syncLink.rotationConstraint);
            _outerRotateToggle.bindingPath = nameof(syncLink.rotationOutside); 
            _minAngleFloat.bindingPath = nameof(syncLink.minAngle);
            _maxAngleFloat.bindingPath = nameof(syncLink.maxAngle);
            
            _minDistFloat.Bind(soLink); 
            _prefDistFloat.Bind(soLink);
            _maxDistFloat.Bind(soLink);
            _onTopToggle.Bind(soLink);
            _onBottomToggle.Bind(soLink);
            _limitRotateToggle.Bind(soLink);
            _outerRotateToggle.Bind(soLink); 
            _minAngleFloat.Bind(soLink);
            _maxAngleFloat.Bind(soLink); 
            _syncLinkIcon.RegisterCallback<MouseUpEvent>(evt => { ClickDeleteIcon(_activeSyncLink, true); }); 
            _syncLinkNameLabel1.text = syncLink.parentSync.gameObject.name+ ",";
            _syncLinkNameLabel2.text = ""+ syncLink.linkedObject.name;
            _syncLinkNameLabel1.RegisterCallback<MouseDownEvent>(evt => { ClickSyncLinkLabel1(evt, _activeSyncLink); }); 
            _syncLinkNameLabel2.RegisterCallback<MouseDownEvent>(evt => { ClickSyncLinkLabel2(evt, _activeSyncLink); }); 
            
            Vector3 syncLinkLineStartPos= syncLink.parentSync.transform.position ;
            Vector3 syncLinkLineEndPos=  syncLink.linkedObject.transform.position; 
            Debug.DrawLine(syncLinkLineStartPos, syncLinkLineEndPos, Color.yellow, 0.5f);

            
            if (SyncLinkHasVirtualSync(syncLink))
            {
                _regularSyncLinkContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                _virtualSyncLinkContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                _syncLinkAnchorVector.bindingPath = nameof(syncLink.anchorVector);
                _syncLinkOffsetVector.bindingPath = nameof(syncLink.offsetVector);
                _syncLinkVirtualScaleToggle.bindingPath = nameof(syncLink.scaleWithSync);
                _syncLinkAnchorVector.Bind(soLink);
                RoundBounds(   syncLink);
                _syncLinkOffsetVector.Bind(soLink);
                _syncLinkVirtualScaleToggle.Bind(soLink); 
                _syncLinkInferButton.clicked -=ClickSyncInferButton;
                _syncLinkInferButton.clicked +=ClickSyncInferButton;
                _syncLinkRepositionButton.clicked -= ClickSyncRepositionButton;
                _syncLinkRepositionButton.clicked += ClickSyncRepositionButton;
                string infoStr = "";
                if (SyncLinkHasVirtualParentSync(syncLink) && SyncLinkHasVirtualLinkedSync(syncLink))
                    infoStr += "Virtual Primary & Virtual Secondary";
                else       if (SyncLinkHasVirtualParentSync(syncLink) )
                    infoStr += "Virtual Primary";
                else       if (SyncLinkHasVirtualLinkedSync(syncLink) )
                    infoStr += "Virtual Secondary";
                _virtualInfoLabel.text = infoStr;
            }
            else
            {
                _regularSyncLinkContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                _virtualSyncLinkContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            }

        }

        private void ClickSurroundSyncInferButton()
        {
            Debug.Log("todo: infer data"); // todo
        }


        private bool SyncLinkHasVirtualSync(SyncLink syncLink)
        {
            if (syncLink.parentSync.quality == SyncQuality.Virtual)
                return true;
            if (syncLink.linkedObject.GetComponent<Sync>() != null)
                if (syncLink.linkedObject.GetComponent<Sync>().quality == SyncQuality.Virtual)
                    return true;
            return false;
        }
        private bool SyncLinkHasVirtualParentSync(SyncLink syncLink)
        {
            if (syncLink.parentSync.quality == SyncQuality.Virtual)
                return true;
            return false;
        }
        private bool SyncLinkHasVirtualLinkedSync(SyncLink syncLink)
        {
            if (syncLink.linkedObject.GetComponent<Sync>() != null)
                if (syncLink.linkedObject.GetComponent<Sync>().quality == SyncQuality.Virtual)
                    return true;
            return false;
        }
        private void LoadMockPhysicalContent()
        {
            MockPhysical mock = _selectedGameObject.GetComponentInChildren<MockPhysical>(); 
            _nameLabelMock =   _mockPhysicalRoot.Q<Label>("NameLabel");
            _classificationSelectorMock =  _mockPhysicalRoot.Q<EnumField>("ClassificationSelector"); 

            _nameLabelMock.text = "MockPhysical: " + mock.gameObject.name;
            _classificationSelectorMock.Init(Classification.Closet);
            SerializedObject so = new SerializedObject(mock);
            _classificationSelectorMock.Bind(so);

            
        }
        private void LoadPlayfieldContent()
        {
            PlayfieldInteraction playfieldInteraction = Selection.activeGameObject.GetComponent<PlayfieldInteraction>();
            _nameLabelPlayfield =   _playfieldRoot.Q<Label>("NameLabel");
            _parentButtonPlayfield =   _playfieldRoot.Q<Button>("ParentButton");
            _focusButtonPlayfield =   _playfieldRoot.Q<Button>("FocusButton");
            _deleteButtonPlayfield =   _playfieldRoot.Q<Button>("DeleteButton");
            Sync parentSync = playfieldInteraction.GetComponentInParent<Sync>();
            SurroundSync parentSurroundSync = null;
            if (parentSync == null)
                parentSurroundSync  = playfieldInteraction.GetComponentInParent<SurroundSync>();
            
            if (parentSync != null)
                _nameLabelPlayfield.text = "Playfield for " + parentSync.gameObject.name;
            else    if (parentSurroundSync != null)
                _nameLabelPlayfield.text = "Playfield for " + parentSurroundSync.gameObject.name;

            _parentButtonPlayfield.clicked -= ClickParentPlayfield;
            _focusButtonPlayfield.clicked -= ClickFocusPlayfield;
            _deleteButtonPlayfield.clicked -= ClickDeletePlayfield;
            _parentButtonPlayfield.clicked += ClickParentPlayfield;
            _focusButtonPlayfield.clicked += ClickFocusPlayfield;
            _deleteButtonPlayfield.clicked += ClickDeletePlayfield;

        }

        private void ClickParentPlayfield()
        {
            PlayfieldInteraction playfieldInteraction = Selection.activeGameObject.GetComponent<PlayfieldInteraction>();
            Sync parentSync = playfieldInteraction.GetComponentInParent<Sync>();
            SurroundSync parentSurroundSync = null;
            if (parentSync != null)
            {
                Selection.activeGameObject = parentSync.gameObject;
            }
            else
            {
                parentSurroundSync  = playfieldInteraction.GetComponentInParent<SurroundSync>();
                if (parentSurroundSync != null)
                    Selection.activeGameObject = parentSurroundSync.gameObject;
                else
                    Debug.LogWarning("Could not find a parent for this Playfield! Is it parented to a Sync or SurroundSync?");
            }
        }

        private void ClickFocusPlayfield()
        {
            SceneView.FrameLastActiveSceneView();
        }

        private void ClickDeletePlayfield()
        {
            Sync parentSync = Selection.activeGameObject.GetComponentInParent<Sync>() ;
            if (parentSync != null)
            {
                parentSync.RemovePlayfieldFromSync();
                Selection.activeGameObject = parentSync.gameObject;
            }
            else
            {
                if (_selectedGameObject.GetComponent<WallMini>() != null)
                    surroundSync = _selectedGameObject.GetComponent<WallMini>().mappedSurroundSync;
                else if (_selectedGameObject.GetComponent<SurroundSync>() != null)
                    surroundSync = _selectedGameObject.GetComponent<SurroundSync>();
                if (surroundSync != null)
                {
                    surroundSync.RemovePlayfieldFromSurroundSync();
                    Selection.activeGameObject = _selectedGameObject;
                }
                else
                    Debug.LogWarning("No Sync or SurroundSync was found for Playfield UI!");
                
            }
      
        }

        private void LoadNoneContent()
        { 
            _syncTotalLabel = _noneRoot.Q<Label>("LabelSyncsTotal");
            _syncQualityLabel = _noneRoot.Q<Label>("LabelSyncsQuality");
            _syncClassificationLabel = _noneRoot.Q<Label>("LabelSyncsClassification");
            _mockTotalLabel = _noneRoot.Q<Label>("LabelMocksTotal");
            _mockClassificationLabel = _noneRoot.Q<Label>("LabelMocksClassification");

            _syncTotalLabel.RegisterCallback<MouseUpEvent>(evt => { ClickNoneSyncLabel(); }); 
            _syncQualityLabel.RegisterCallback<MouseUpEvent>(evt => { ClickNoneSyncLabel();}); 
            _syncClassificationLabel.RegisterCallback<MouseUpEvent>(evt => { ClickNoneSyncLabel();}); 
            _mockTotalLabel.RegisterCallback<MouseUpEvent>(evt => { ClickNoneMockLabel();}); 
            _mockClassificationLabel.RegisterCallback<MouseUpEvent>(evt => { ClickNoneMockLabel();});

            UpdateNoneLabels();
          
        }

        private void UpdateNoneLabels()
        {
            if (DesignArea == null)
                return;
            List<Sync> activeSyncs = DesignArea.GetSyncs();
            int heroAmount = 0;
            int normalAmount = 0;
            int clutterAmount = 0;
            int virtualAmount = 0;
            int syncClassMisc = 0;
            /*int syncClassWall = 0;
            int syncClassFloor = 0;
            int syncClassCeiling = 0;
            int syncClassDoor = 0;
            int syncClassWindow = 0;*/
            int syncClassTable = 0;
            int syncClassSeat = 0;
            int syncClassSofa = 0;
            foreach (var sync in activeSyncs)
            {
                if (sync.quality == SyncQuality.Hero)
                    heroAmount++;
                else if (sync.quality == SyncQuality.Normal)
                    normalAmount++;
                else if (sync.quality == SyncQuality.Clutter)
                    clutterAmount++;
                else if (sync.quality == SyncQuality.Clutter)
                    virtualAmount++;
                
                if (sync.classification == Classification.Misc)
                    syncClassMisc++; 
                /*else   if (sync.classification == Classification.Wall)
                    syncClassWall++;
                else   if (sync.classification == Classification.Floor)
                    syncClassFloor++;
                else   if (sync.classification == Classification.Ceiling)
                    syncClassCeiling++;
                else   if (sync.classification == Classification.Door)
                    syncClassDoor++;
                else   if (sync.classification == Classification.Window)
                    syncClassWindow++;*/
                else   if (sync.classification == Classification.Table)
                    syncClassTable++;
                else   if (sync.classification == Classification.Closet)
                    syncClassSeat++;         
                else   if (sync.classification == Classification.Sofa)
                    syncClassSofa++;
            }

            _syncTotalLabel.text = "Syncs in DesignArea: "  + activeSyncs.Count;
            _syncQualityLabel.text = heroAmount + " Hero,  " + normalAmount + " Normal,  " + clutterAmount + " Clutter,  " + virtualAmount +" Virtual";
            string sclstr = "";
         //   if (syncClassMisc != 0)
                sclstr += syncClassMisc + " Misc, ";
            /*if (syncClassWall != 0)
                sclstr += syncClassWall + " Wall, ";
            if (syncClassFloor != 0)
                sclstr += syncClassFloor + " Floor, ";
            if (syncClassCeiling != 0)
                sclstr += syncClassCeiling + " Ceiling, ";
            if (syncClassDoor != 0)
                sclstr += syncClassDoor + " Door, ";
            if (syncClassWindow != 0)
                sclstr += syncClassWindow + " Window, ";*/
          //  if (syncClassTable != 0)
                sclstr += syncClassTable + " Table, ";
         //   if (syncClassSeat != 0)
                sclstr += syncClassSeat + " Seat, ";
          //  if (syncClassSofa != 0)
                sclstr += syncClassSofa + " Sofa";
            _syncClassificationLabel.text = sclstr;
             
            int mockClassMisc = 0; 
            int mockClassTable = 0;
            int mockClassSeat = 0;
            int mockClassSofa = 0;

            List<MockPhysical> mocks = Object.FindObjectOfType<LayoutArea>().GetMocksByTransform();
            foreach (var mock in mocks)
            {
                if (mock.Classification == Classification.Misc)
                    mockClassMisc++;  
                else   if (mock.Classification == Classification.Table)
                    mockClassTable++;
                else   if (mock.Classification == Classification.Closet)
                    mockClassSeat++;    
                else   if (mock.Classification == Classification.Sofa)
                    mockClassSofa++;
            }
            _mockTotalLabel.text =  "Mocks in LayoutArea: " + mocks.Count; 
            sclstr = "";
        //    if (mockClassMisc != 0)
                sclstr += mockClassMisc + " Misc, "; 
                sclstr += mockClassTable + " Table, ";
       //     if (mockClassSeat != 0)
                sclstr += mockClassSeat + " Seat, ";
        //    if (mockClassSofa != 0)
                sclstr += mockClassSofa + " Sofa";
            _mockClassificationLabel.text = sclstr;
        }


        private void ClickNoneSyncLabel()
        {
            Selection.activeGameObject = DesignArea.gameObject;
            SceneView.FrameLastActiveSceneView();
        } 
        private void ClickNoneMockLabel()
        {
            Selection.activeGameObject = LayoutArea.gameObject;
            SceneView.FrameLastActiveSceneView();
        }
       
    
        private void LoadEditWallPieceContent()
        {
         

        }
        private void LoadEditCeilingContent()
        {
           

        }
      
        private void LoadGameObjectContent()
        {
           
           
        }
    

 
        
     
    }
#endif