#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
//using Unity.Plastic.Antlr3.Runtime.Misc;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using Object = UnityEngine.Object;
using Toggle = UnityEngine.UIElements.Toggle;


public class ControlContent 
{
    // REFERENCE
    private ToolWindow _toolWindow;
    private ToolWindowSettings _toolWindowSettings;
    private DesignArea _designArea;
    private LayoutArea _layoutArea;
    private SREditorCameraTool _editorCameraTool;
    
    // STRUCTURE
    private VisualElement _templateRoot; 
    private Label _roomHeaderLabel;
    private Label _designAreaLabel;
    private Label _layoutAreaLabel; 
    private Button _buttonResetWalls;  
    private Button _buttonResetMocks;  
    private Button _buttonSaveRoomLayout;  
    private Button _buttonSaveAsRoomLayout;  
    private Button _buttonLoadRoomLayout;  
    private Button _buttonLoadRecentRoomLayout;  
    private Button _buttonGenerateMocks; 
    private Button _buttonInitialize ;
    private Button _buttonResetDesignArea ;
   
    private Button _buttonNewSyncSetup; 
    private Button _buttonSaveSyncSetup; 
    private Button _buttonPresetMocks ;
    private Button _buttonBaseMock ;
    private Button _buttonSceneViewCameraDesign ;
    private Button _buttonSceneViewCameraLayout ;
    private Button _buttonSceneViewCameraDesignReset ;
    private Button _buttonSceneViewCameraLayoutReset ;
    private MaskField _maskFieldGeneratedClassifications;
    private Toggle _lShapeToggle; 
    private ListView _recentListViewSync;
    private VisualElement _loadLogicRoom;
    private VisualElement _recentLogicRoom;
    private VisualElement _recentListContainerRoom;
    private ListView _recentListViewRoom;
    private static VisualTreeAsset _basicListRowTemplate; 
    private Button _closeRecentRoom;
    private VisualElement _syncSetupListViewContainer;
    private ListView _syncSetupListView;

    // SycnSetups
    private VisualElement _newSetupContent;
    private TextField _syncSetupNameField;
    private Button _syncSetupRenameButton;
    private Label _syncSetupRowNameLabel;

    // STATE 
    private List<string> _recentRoomLayoutNames;
    private List<string> _syncSetupNames;
    private static VisualTreeAsset _syncSetupListRowTemplate;
    private string _lastSelectedSyncSetup;

    private void SendQueries()
    {
           _templateRoot = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>
            ( ToolWindowSettings.EditorFolderPath+ "/ControlContent.uxml").Instantiate();
        _templateRoot.style.backgroundColor =ToolWindowSettings.ContentColor; 
        
        _basicListRowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( ToolWindowSettings.EditorFolderPath + "/ListRowTemplate.uxml");
        _syncSetupListRowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( ToolWindowSettings.EditorFolderPath + "/SSListRowTemplate.uxml");
 
        _roomHeaderLabel = _templateRoot.Q<Label>("RoomHeaderLabel");  
        _designAreaLabel = _templateRoot.Q<Label>("DesignAreaLabel");  
        _layoutAreaLabel = _templateRoot.Q<Label>("LayoutAreaLabel");  
        _buttonResetWalls = _templateRoot.Q<Button>("ButtonResetWalls");  
        _buttonResetMocks = _templateRoot.Q<Button>("ButtonResetMocks");  
        _buttonSaveRoomLayout = _templateRoot.Q<Button>("ButtonSaveLayout");    
        _buttonSaveAsRoomLayout = _templateRoot.Q<Button>("ButtonSaveAsRoom");    
        _buttonLoadRoomLayout = _templateRoot.Q<Button>("ButtonLoadLayout");  
        _buttonLoadRecentRoomLayout = _templateRoot.Q<Button>("ButtonLoadRecent");   
        _buttonGenerateMocks = _templateRoot.Q<Button>("ButtonGenerate");   
        _buttonInitialize = _templateRoot.Q<Button>("ButtonInitialize");  
        _buttonResetDesignArea = _templateRoot.Q<Button>("ButtonReset");  
        _buttonNewSyncSetup = _templateRoot.Q<Button>("ButtonNew");   
        _buttonSaveSyncSetup = _templateRoot.Q<Button>("ButtonSave");   
        _maskFieldGeneratedClassifications = _templateRoot.Q<MaskField>("MaskFieldGeneratedClassifications");  
        _lShapeToggle = _templateRoot.Q<Toggle>("LShapeToggle");   
        _loadLogicRoom = _templateRoot.Q<VisualElement>("LoadLogicRoom");  
        _recentLogicRoom = _templateRoot.Q<VisualElement>("RecentLogicRoom");  
        _recentListContainerRoom = _templateRoot.Q<VisualElement>("RecentListRoom");    
        _closeRecentRoom = _templateRoot.Q<Button>("CloseRecentButtonRoom"); 
        _buttonPresetMocks = _templateRoot.Q<Button>("ButtonPresets");
        _buttonBaseMock = _templateRoot.Q<Button>("ButtonBase");
        _buttonSceneViewCameraDesign = _templateRoot.Q<Button>("ButtonCameraDesign");
        _buttonSceneViewCameraLayout = _templateRoot.Q<Button>("ButtonCameraLayout");
        _buttonSceneViewCameraDesignReset = _templateRoot.Q<Button>("ButtonCameraDesignReset");
        _buttonSceneViewCameraLayoutReset = _templateRoot.Q<Button>("ButtonCameraLayoutReset"); 
        _newSetupContent = _templateRoot.Q<VisualElement>("NewSetupContent");  
        _syncSetupNameField = _templateRoot.Q<TextField>("SyncSetupNameField");  
        _syncSetupRenameButton = _templateRoot.Q<Button>("RenameButton");
        _syncSetupListViewContainer  = _templateRoot.Q<VisualElement>("ListRoot");
    }
    
    
    public VisualElement CreateContent(ToolWindow toolWindow)
    {
        _toolWindow = toolWindow;
        SendQueries(); 
        
        _designAreaLabel.RegisterCallback<MouseDownEvent>(evt => { ClickDesignAreaLabel(evt); });
        _layoutAreaLabel.RegisterCallback<MouseDownEvent>(evt => { ClickLayoutAreaLabel(evt); });
        LayoutArea.activeRoomLayout = "";
        
        AddButtonActions();
        UpdateHeaderLabels();
        RefreshDisplayContainers();
        return _templateRoot;
    }

    private void RefreshSyncSetupList()
    {
        if (_syncSetupListViewContainer == null)
            SendQueries();
        UpdateSyncSetupsList( );
        _syncSetupListView = CreateSyncSetupListView(_syncSetupNames, ClickSyncSetupInList, HoverAmountLabel); 
        _syncSetupListViewContainer.Clear();
        _syncSetupListViewContainer.Add(_syncSetupListView);
    }

    private void UpdateSyncSetupsList()
    {  
        _syncSetupNames = new List<string>();
 
        foreach (var syncSetup in DesignArea.GetPresentSyncSetups()) 
                _syncSetupNames.Add(syncSetup.gameObject.name);
        foreach (var syncSetup in DesignArea.GetStoredSyncSetups())
            if (_syncSetupNames.Contains(syncSetup.gameObject.name) == false)
                if (syncSetup.gameObject.activeSelf)  
                    _syncSetupNames.Add(syncSetup.gameObject.name);  
    }
    private ListView CreateSyncSetupListView( List<string> stringList, Action<string, MouseDownEvent> clickAction, Action<string, MouseOverEvent> hoverAction)
    { 
        VisualElement MakeItem() => _syncSetupListRowTemplate.CloneTree();
        void BindItem(VisualElement row, int i)
        {  
            Label nameLabel =  row.Q<Label>("NameLabel");
            Label amountLabel =  row.Q<Label>("AmountLabel");
            row.RegisterCallback<MouseDownEvent>(evt => { clickAction.Invoke(stringList[i], evt); }); 
            amountLabel.tooltip = DesignArea.GetTooltipForAmountLabel(stringList[i]);
            
            bool present = false;
            bool active = false;
            if (DesignArea.IsSyncSetupPresent(stringList[i]))
            {
                present = true;
                active = DesignArea.GetPresentSyncSetupForName(stringList[i]).gameObject.activeSelf;
            }  
            nameLabel.text = stringList[i];

            if (present)
                amountLabel.text = DesignArea.GetPresentSyncSetupForName(stringList[i]).GetElementCount().ToString() ;
            else
                amountLabel.text =  DesignArea.GetStoredSyncSetupForName(stringList[i]).GetElementCount().ToString()  ; 

            if (present && active)
                nameLabel.style.color = ToolWindowSettings.SyncSetupActiveColor;
            else if (present)
                nameLabel.style.color = ToolWindowSettings.SyncSetupPresentColor;
            else 
                nameLabel.style.color = ToolWindowSettings.SyncSetupStoredColor;

            bool saved = DesignArea.IsSyncSetupSaved(stringList[i]); 

            if (saved && present)
                amountLabel.style.color = ToolWindowSettings.SyncSetupPresentColor;
            else 
                amountLabel.style.color = ToolWindowSettings.SyncSetupStoredColor;
                
            Action<string> clickDeleteAction = ClickSyncSetupRowDeleteButton;
            row.Q<Button>("DeleteButton").RegisterCallback<MouseUpEvent>(evt => { clickDeleteAction.Invoke(stringList[i]); });

            Action<string, bool, Toggle> clickToggleAction = ClickSyncSetupToggle;
            Toggle toggle =   row.Q<Toggle>("IncludeToggle");
            if (DesignArea.IsSyncSetupPresent(stringList[i]))
                toggle.SetValueWithoutNotify(true);
            toggle.RegisterValueChangedCallback(e => clickToggleAction.Invoke(stringList[i],e.newValue, toggle));
        }
        return new ListView(stringList, ToolWindowSettings.organizeRowHeight, MakeItem, BindItem)
        {
            selectionType = SelectionType.Single,
            reorderable = true,
            style =
            {
                height = ToolWindowSettings.organizeColumnHeight,
                flexShrink = 0,
                flexGrow = 1
            }
        };
    }
    private void ClickSaveSyncSetup()
    {
        DesignArea.SaveSyncSetup();
    }

    private void ClickSyncSetupToggle(string syncSetupName, bool isOn, Toggle toggle)
    {
        if (DesignArea.ToggleSyncSetup(syncSetupName, isOn) == false)
            toggle.SetValueWithoutNotify(true); // to make sure at least one syncsetup is present
        RefreshDisplayContainers();
    }

    private void ClickSyncSetupRowDeleteButton(string syncSetupName)
    {
        DesignArea.DeleteSyncSetup(syncSetupName);  
        RefreshDisplayContainers();
        _lastSelectedSyncSetup = "";
        UpdateRenameButton();
        RefreshSyncSetupList();
    }

    private void ClickSyncSetupInList(string syncSetupName, MouseDownEvent evt)
    {
        _lastSelectedSyncSetup = syncSetupName;
        if (evt.clickCount == 2)
        {
            if (DesignArea.IsSyncSetupPresent(syncSetupName))
            {
                DesignArea.SwitchToSyncSetup(syncSetupName);
            } 
        }
        else if (evt.clickCount == 1)
        { 
            if (DesignArea.IsSyncSetupPresent(syncSetupName))
            {
                Selection.activeGameObject = DesignArea.GetPresentSyncSetupForName(syncSetupName).gameObject;
            } 
        } 
        UpdateAndSelectSyncSetupNameField(syncSetupName, false);
        UpdateRenameButton(); 
    }
    private void HoverAmountLabel(string syncSetupName, MouseOverEvent evt)
    {
         
    }
    
    private void ClickRenameSyncSetupButton()
    {
        if (_lastSelectedSyncSetup != "")
            if (_syncSetupNameField.value != "")
            {
                if (DesignArea.IsSyncSetupPresent(_lastSelectedSyncSetup))
                    DesignArea.RenamePresentSyncSetup(_lastSelectedSyncSetup, _syncSetupNameField.value);
                else
                    DesignArea.RenameStoredSyncSetup(_lastSelectedSyncSetup, _syncSetupNameField.value);

                _lastSelectedSyncSetup = "";
                RefreshSyncSetupList();
                UpdateRenameButton();
                _syncSetupNameField.value = "";
            } 
    }

  
    public void UpdateAndSelectSyncSetupNameField(string syncSetupName = "", bool refreshList = true)
    { 
        if (syncSetupName == "")
            _syncSetupNameField.value = DesignArea.GetFirstActivePresentSyncSetup().gameObject.name; 
        else 
            _syncSetupNameField.value = syncSetupName;
        if (refreshList)
             RefreshSyncSetupList();
        _syncSetupNameField.SelectAll();
        _syncSetupNameField.Focus();
    }
    private void ClickNewSyncSetup()
    {
        UpdateHeaderLabels();
        _lastSelectedSyncSetup=   DesignArea.ClickNewSyncSetup(); 
    }
    private void ClickDesignAreaLabel(MouseDownEvent mouseDownEvent)
    { 
     if (mouseDownEvent.button == 0)
          EditorCameraTool.MoveCameraToPreset (SREditorCameraTool.CameraPresetNames.Design);
     if (mouseDownEvent.button == 1)
     {
         Selection.activeGameObject = DesignArea.flexFloor.gameObject;
         ToolWindowSettings.openEditContent = true;
     }

    }
    private void ClickLayoutAreaLabel(MouseDownEvent mouseDownEvent)
    { 
    if (mouseDownEvent.button == 0)
        EditorCameraTool.MoveCameraToPreset (SREditorCameraTool.CameraPresetNames.Layout);  
    if (mouseDownEvent.button == 1)
    {
        Selection.activeGameObject = LayoutArea.gameObject;
        ToolWindowSettings.openEditContent = true;
    }
    }
    
   
    private void UpdateHeaderLabels()
    {
        if (LayoutArea.activeRoomLayout == "")
            _roomHeaderLabel.text = "RoomLayout" ;
        else
            _roomHeaderLabel.text = "RoomLayout: " + LayoutArea.activeRoomLayout; 
    }


    private void UpdateRenameButton()
    {
        if (string.IsNullOrEmpty(_lastSelectedSyncSetup))
        {
            _syncSetupNameField.style.visibility = new StyleEnum<Visibility>(Visibility.Hidden);
            _syncSetupRenameButton.style.visibility = new StyleEnum<Visibility>(Visibility.Hidden);
        }
        else
        {
            if (_syncSetupRenameButton == null)
                SendQueries(); 
            _syncSetupNameField.style.visibility = new StyleEnum<Visibility>(Visibility.Visible);
            _syncSetupRenameButton.style.visibility = new StyleEnum<Visibility>(Visibility.Visible);
            if (_lastSelectedSyncSetup.Length == 1)
                _syncSetupRenameButton.text = "Rename " + _lastSelectedSyncSetup.Substring(_lastSelectedSyncSetup.Length - 1, 1);
            else if (_lastSelectedSyncSetup.Length == 2)
                _syncSetupRenameButton.text = "Rename " + _lastSelectedSyncSetup.Substring(_lastSelectedSyncSetup.Length - 2, 2);
            else if (_lastSelectedSyncSetup.Length == 3)
                _syncSetupRenameButton.text = "Rename " + _lastSelectedSyncSetup.Substring(_lastSelectedSyncSetup.Length - 3, 3);
            else if (_lastSelectedSyncSetup.Length == 4)
                _syncSetupRenameButton.text = "Rename ." + _lastSelectedSyncSetup.Substring(_lastSelectedSyncSetup.Length - 3, 3);
            else
                _syncSetupRenameButton.text = "Rename .." + _lastSelectedSyncSetup.Substring(_lastSelectedSyncSetup.Length - 3, 3);

        }
    }

    public void RefreshDisplayContainers()
    {
        RefreshSyncSetupList();
        UpdateRenameButton();
//        _loadLogicSync.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        _loadLogicRoom.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
   //     _recentLogicSync.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        _recentLogicRoom.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
    }
    


    private void ClickRecentRoomLayouts()
    {
        UpdateRecentRoomLayoutsList();
        CreateListViewForRoomLayouts();
        _loadLogicRoom.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None); 
        _recentLogicRoom.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
    }
 
 
    private void UpdateRecentRoomLayoutsList()
    {
        CleanRecentFile(LayoutArea.roomLayoutsPath,  LayoutArea.recentRoomLayoutsPath);
        string fileText = File.ReadAllText(LayoutArea.recentRoomLayoutsPath);
        _recentRoomLayoutNames = new List<string>();
        string[] names = fileText.Split(',');
        for (int i = 0; i < names.Length  ; i++)
            if (i < ToolWindowSettings.recentLayoutsAmount)
                if (names[i] != "")
                    if (_recentRoomLayoutNames.Contains(names[i]) == false)
                      _recentRoomLayoutNames.Add(names[i]); 
    }
    private void CleanRecentFile(string filePath, string recentFilePath)
    {
        string fileText = File.ReadAllText(recentFilePath);
        string[] recentNames = fileText.Split(',');
        
        string[] layoutPathNames = Directory.GetFiles(filePath, "*.txt",  SearchOption.AllDirectories);
        List<string> layoutTextFileNames = new List<string>();
        foreach (var name in layoutPathNames) 
            layoutTextFileNames.Add(name.Substring(filePath.Length, name.Length-filePath.Length-4 ));  

        List<string> singletonNames = new List<string>();
        string newFileText = "";
        foreach (var name in recentNames)
            if (name != "")
                if (layoutTextFileNames.Contains(name))
                    if (singletonNames.Contains(name) == false)
                        singletonNames.Add(name);
        foreach (var name in singletonNames)
            newFileText += name + ",";
        File.WriteAllText(recentFilePath, newFileText);
    }
   
    private void CreateListViewForRoomLayouts( )
    {
        _recentListContainerRoom.Clear();
        VisualElement MakeItem() => _basicListRowTemplate.CloneTree();
        void BindItem(VisualElement e, int i)
        {
            e.Q<Label>("NameLabel").text = _recentRoomLayoutNames[i]; 
            e.RegisterCallback<MouseUpEvent>(evt => { ClickRecentRoomLayoutRow(_recentRoomLayoutNames[i]); });
        }
        _recentListViewRoom = new ListView(_recentRoomLayoutNames, 20, MakeItem, BindItem)
        {
            selectionType = SelectionType.Single,
            reorderable = true,
            style = {height = _recentRoomLayoutNames.Count * 20,}};
        _recentListContainerRoom.Add(_recentListViewRoom);
    }
  
    private void ClickRecentRoomLayoutRow(string recentRoomLayoutName)
    {
        LayoutArea.activeRoomLayout = recentRoomLayoutName;
        string path = LayoutArea.roomLayoutsPath + LayoutArea.activeRoomLayout + ".txt";
        LayoutArea.LoadRoomLayoutWithPath(path);
        UpdateHeaderLabels();
        RefreshDisplayContainers();
    }
 
    private void ClickSaveAsRoomLayout()
    {
        LayoutArea.activeRoomLayout =   LayoutArea.SaveRoomLayoutWithFileDialog();
        UpdateHeaderLabels(); 
    }
    private void ClickSaveRoomLayout()
    {
        if (LayoutArea.activeRoomLayout != "")
        {
            string path = LayoutArea.roomLayoutsPath + LayoutArea.activeRoomLayout + ".txt";
            LayoutArea.SaveToPath(path);
        } 
        else
        {
            string path = LayoutArea.roomLayoutsPath + "AutoSave.txt";
            LayoutArea.activeRoomLayout = "Autosave";
            UpdateHeaderLabels(); 
            LayoutArea.SaveToPath(path);
        }
    }
    private void ClickLoadRoomLayout()
    { 
        LayoutArea.activeRoomLayout =  LayoutArea.LoadRoomLayoutWithFileDialog(); 
        UpdateHeaderLabels();
        LayoutArea.GainFocus();
    } 

    private void ClickToggleWalls()
    {
        DesignArea.ToggleWallPreviews();
    }

    private void ClickInitializeDesignArea()
    {  
            DesignArea.InitializeDesignArea(); 
    }
    private void ClickResetDimensions()
    {
        DesignArea.ResetDimensions();
    }
  
    
    public void InitializeContent()
    {

        _maskFieldGeneratedClassifications.choices = Enum.GetNames(typeof(Classification)).ToList();

    }
    
    #region ButtonFunctions
    private void ClickResetWalls()
    { 
        LayoutArea.activeRoomLayout = "";
        UpdateHeaderLabels();
        LayoutArea.ResetLayout();
        LayoutArea.GainFocus();
    }
    private void ClickResetMocks()
    {
        LayoutArea.activeRoomLayout = "";
        UpdateHeaderLabels();
        LayoutArea.ResetMocks();
        LayoutArea.GainFocus();
    } 
    


    private void ClickClearPrefabs()
    {
        DesignArea.ClearReferencePrefabs();
    }
    
    
    private void ClickGenerateMocks()
    {
        bool lShape = _lShapeToggle.value;
        List<Classification> selectedClassifications = new List<Classification>();
        for (int i = 0; i < Enum.GetNames(typeof(Classification)).ToList().Count; i++)
        {
            int layer = 1 << i;
            if ((_maskFieldGeneratedClassifications.value & layer) != 0)
                selectedClassifications.Add((Classification)System.Enum.Parse( typeof(Classification), Enum.GetNames(typeof(Classification)).ToList()[i] ));
        }

        if (selectedClassifications.Count == 0)
        {
         //   Debug.Log("Blaze full random mode!  L: " + lShape);//
        }
        else
            foreach (var classification in selectedClassifications)
                Debug.Log("Include: " + classification);
        LayoutArea.SpawnRandomLayout(lShape, selectedClassifications);
    }

    
    #endregion

  private void AddButtonActions()
    {
          _buttonResetWalls.clicked -= ClickResetWalls;  
        _buttonResetMocks.clicked -= ClickDeleteMocks;  
        _buttonPresetMocks.clicked -= ClickResetMocks;  
        _buttonBaseMock.clicked -= ClickBaseMock;  
        _buttonSaveRoomLayout.clicked -= ClickSaveRoomLayout;  
        _buttonSaveAsRoomLayout.clicked -= ClickSaveAsRoomLayout;  
        _buttonLoadRoomLayout.clicked -= ClickLoadRoomLayout;  
        _buttonLoadRecentRoomLayout.clicked -= ClickRecentRoomLayouts;   
        _buttonGenerateMocks.clicked -= ClickGenerateMocks;   
        _buttonInitialize.clicked -= ClickInitializeDesignArea;  
        _buttonResetDesignArea.clicked -= ClickResetDesignArea;  
        _buttonNewSyncSetup.clicked -= ClickNewSyncSetup;  
        _buttonSaveSyncSetup.clicked -= ClickSaveSyncSetup;   
        _closeRecentRoom.clicked -= RefreshDisplayContainers; 
        _buttonSceneViewCameraDesign.clicked -= ClickCameraDesignArea;
        _buttonSceneViewCameraLayout.clicked -= ClickCameraLayoutArea;
        _buttonSceneViewCameraDesignReset.clicked -= ClickCameraDesignAreaReset;
        _buttonSceneViewCameraLayoutReset.clicked -= ClickCameraLayoutAreaReset;
        _syncSetupRenameButton.clicked -= ClickRenameSyncSetupButton;
        
        _buttonResetWalls.clicked += ClickResetWalls;  
        _buttonResetMocks.clicked += ClickDeleteMocks;  
        _buttonPresetMocks.clicked += ClickResetMocks;  
        _buttonBaseMock.clicked += ClickBaseMock;  
        _buttonSaveRoomLayout.clicked += ClickSaveRoomLayout;  
        _buttonSaveAsRoomLayout.clicked += ClickSaveAsRoomLayout;
        _buttonLoadRoomLayout.clicked += ClickLoadRoomLayout;  
        _buttonLoadRecentRoomLayout.clicked += ClickRecentRoomLayouts; 
        _buttonGenerateMocks.clicked += ClickGenerateMocks;   
        _buttonInitialize.clicked += ClickInitializeDesignArea;  
        _buttonResetDesignArea.clicked += ClickResetDesignArea;   
        _buttonNewSyncSetup.clicked += ClickNewSyncSetup;  
        _buttonSaveSyncSetup.clicked += ClickSaveSyncSetup;   
        _closeRecentRoom.clicked += RefreshDisplayContainers; 
      _syncSetupRenameButton.clicked += ClickRenameSyncSetupButton;

        // Scene View Camera
        _buttonSceneViewCameraDesign.clicked -= ClickCameraDesignArea;
        _buttonSceneViewCameraDesign.clicked += ClickCameraDesignArea;
        _buttonSceneViewCameraLayout.clicked -= ClickCameraLayoutArea;
        _buttonSceneViewCameraLayout.clicked += ClickCameraLayoutArea;
        _buttonSceneViewCameraDesignReset.clicked -= ClickCameraDesignAreaReset;
        _buttonSceneViewCameraDesignReset.clicked += ClickCameraDesignAreaReset;
        _buttonSceneViewCameraLayoutReset.clicked -= ClickCameraLayoutAreaReset;
        _buttonSceneViewCameraLayoutReset.clicked += ClickCameraLayoutAreaReset;
    }

  private void ClickResetDesignArea()
  {
     DesignArea.ResetDesignArea();
  }


  private void ClickBaseMock()
  {
      _layoutArea.SpawnBaseMock();
  }

  private void ClickDeleteMocks()
  {
      _layoutArea.DeleteMocks();
  }

   


  public void LoadRoomLayoutFromOutside(string fileName)
  {
      LayoutArea.activeRoomLayout = fileName;
      LayoutArea.LoadRoomLayoutWithFilename(fileName);
      UpdateHeaderLabels();
  }

    private void ClickCameraDesignArea()
    {
        EditorCameraTool.MoveCameraToPreset (SREditorCameraTool.CameraPresetNames.Design);
    }
    private void ClickCameraLayoutArea()
    {
        EditorCameraTool.MoveCameraToPreset (SREditorCameraTool.CameraPresetNames.Layout);
    }
    private void ClickCameraDesignAreaReset()
    {
        EditorCameraTool.SaveCameraPreset (SREditorCameraTool.CameraPresetNames.Design);
    }
    private void ClickCameraLayoutAreaReset()
    {
        EditorCameraTool.SaveCameraPreset (SREditorCameraTool.CameraPresetNames.Layout);
    }

    
    private ToolWindowSettings ToolWindowSettings 
    { 
        get {
            if (_toolWindowSettings == null)
            {
                _toolWindowSettings = Object.FindObjectOfType<ToolWindowSettings>();
            }
            return _toolWindowSettings;
        } 
    }

    private DesignArea DesignArea 
    { 
        get {
            if (_designArea == null)
            {
                _designArea = Object.FindObjectOfType<DesignArea>();
            }
            return _designArea;
        } 
    }

    private LayoutArea LayoutArea
    { 
        get {
            if (_layoutArea == null)
            {
                _layoutArea = Object.FindObjectOfType<LayoutArea>();
            }
            return _layoutArea;
        } 
    }

    private SREditorCameraTool EditorCameraTool 
    { 
        get {
            if (_editorCameraTool == null)
            {
                _editorCameraTool = Object.FindObjectOfType<SREditorCameraTool>();
            }
            return _editorCameraTool;
        } 
    }
}
#endif