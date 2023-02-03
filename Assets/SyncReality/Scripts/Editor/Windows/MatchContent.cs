#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using Codice.Client.Common;
using JetBrains.Annotations;
//using Unity.Plastic.Antlr3.Runtime.Misc;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Action = System.Action;
using Object = UnityEngine.Object;

 
public class MatchContent 
{
    // REFERENCE
    private ToolWindow _toolWindow;
    private ToolWindowSettings _toolWindowSettings;
    private DesignArea _designArea;
    private LayoutArea _layoutArea;
    
    // STRUCTURE - BASE
    private VisualElement _templateRoot;
    private VisualElement _chartRoot; 
    private ChartGraphView _graphView;
    private Button _debugButton;
    private Button _rebuildButton;
    private Button _createButton;
    private Toggle _minimapToggle;
    private Toggle _blackboardToggle;

    private string _fileName = "SpaceChart";
    private MiniMap _miniMap;
    private Blackboard _blackboard;

    public VisualElement CreateContent(ToolWindow toolWindow)
    {
        _toolWindow = toolWindow;
        _toolWindowSettings = Object.FindObjectOfType<ToolWindowSettings>();
        _designArea = Object.FindObjectOfType<DesignArea>();
        _layoutArea = Object.FindObjectOfType<LayoutArea>();
        _templateRoot = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>
            ( _toolWindowSettings.EditorFolderPath+ "/MatchContent.uxml").Instantiate();
        _templateRoot.style.backgroundColor =_toolWindowSettings.ContentColor;
        _templateRoot.style.flexGrow = 1;
        _templateRoot.style.flexShrink = 0;
        UpdateContent();
        return _templateRoot;
    }
    public void UpdateContent()
    {
        _chartRoot = _templateRoot.Q<VisualElement>("ChartRoot");
        _debugButton = _templateRoot.Q<Button>("DebugButton");
        _rebuildButton = _templateRoot.Q<Button>("RebuildButton");
        _createButton = _templateRoot.Q<Button>("CreateButton");
        _minimapToggle = _templateRoot.Q<Toggle>("MinimapToggle");
        _blackboardToggle = _templateRoot.Q<Toggle>("BlackboardToggle");

        _rebuildButton.clicked -= ClickRebuildButton;
        _rebuildButton.clicked += ClickRebuildButton;
        _createButton.clicked -= ClickCreateButton;
        _createButton.clicked += ClickCreateButton;
        _debugButton.clicked -= ClickDebugButton;
        _debugButton.clicked += ClickDebugButton;
        if (_graphView == null)
           ConstructGraphView();
        if (_minimapToggle.value == true)
            if (_miniMap == null)
                  GenerateMiniMap(); 
        if (_blackboardToggle.value == true)
            if (_blackboard == null)
                GenerateBlackBoard();
        _minimapToggle.RegisterCallback<ChangeEvent<bool>>(evt => {ToggleMiniMap(evt.newValue);});
        _blackboardToggle.RegisterCallback<ChangeEvent<bool>>(evt => {ToggleBlackboard(evt.newValue);});
     if (_hasToolbar == false)
        GenerateToolbar();


    }
    private void GenerateBlackBoard()
    {
        _blackboard = new Blackboard(_graphView);
        _blackboard.Add(new BlackboardSection {title = "Exposed Variables"});
        
        _blackboard.addItemRequested = _blackboard =>
        {
            _graphView.AddPropertyToBlackBoard(new ExposedProperty(), false);
        };
        _blackboard.editTextRequested = (_blackboard, element, newValue) =>
        {
            var oldPropertyName = ((BlackboardField) element).text;
            if (_graphView.exposedProperties.Any(x => x.PropertyName == newValue))
            {
                EditorUtility.DisplayDialog("Error", "Property name already exists!",
                    "OK");
                return;
            }

            var targetIndex = _graphView.exposedProperties.FindIndex(x => x.PropertyName == oldPropertyName);
            _graphView.exposedProperties[targetIndex].PropertyName = newValue;
            ((BlackboardField) element).text = newValue;
        }; 
        _blackboard.SetPosition(new Rect(10,30,200,240));
        _graphView.blackboard = _blackboard; 
        _graphView.Add(_blackboard);
    }
   
    

    private void ClickRebuildButton()
    {
        ConstructGraphView();
        UpdateContent();
    }

    private void ClickCreateButton()
    {
        _graphView.CreateNodeThroughButton(_designArea.GetFirstActivePresentSyncSetup().gameObject.name); 
    }
    private void ClickDebugButton()
    {
        GenerateBlackBoard();
    }
    private void ToggleMiniMap(bool evtNewValue)
    {
        if (evtNewValue == true)
        {
            GenerateMiniMap();
        }
        else
        {
            if (_miniMap != null) 
                _graphView.Remove(_miniMap);
        }
    } 
    private void ToggleBlackboard(bool evtNewValue)
    {
        if (evtNewValue == true)
        {
            GenerateBlackBoard();
        }
        else
        {
            if (_blackboard != null) 
                _graphView.Remove(_blackboard);
        }
    }
 
    private void GenerateMiniMap()
    {
        _miniMap = new MiniMap{anchored = true};
        _miniMap.SetPosition(new Rect(10,10,_toolWindowSettings.miniMapWidth,_toolWindowSettings.miniMapHeight));
        _graphView.Add(_miniMap);
    }

    private void ConstructGraphView()
    {
    //     Debug.Log(("construct graphview"));
        _hasToolbar = false;
        _graphView = new ChartGraphView
        {
            name = "Chart Graph"
        };
        _graphView.StretchToParentSize();

        /*TwoPaneSplitView splitView = new TwoPaneSplitView();
         
        VisualElement inspectorView = new VisualElement();
        VisualElement leftPanel = new VisualElement();
        leftPanel.style.flexGrow = 1;
        VisualElement rightPanel = new VisualElement();
        rightPanel.style.flexGrow = 1;
    //    inspectorView.style.width = new StyleLength(100);
    leftPanel.Add(inspectorView);  
    rightPanel.Add(_graphView);  
        splitView.Add(leftPanel);
        splitView.Add(rightPanel);*/
        _chartRoot.Clear();
        _chartRoot.Add(_graphView);
    }

    private bool _hasToolbar;
    private void GenerateToolbar()
    {
         
        var toolbar = new Toolbar();

        var fileNameTextField = new TextField("");
        fileNameTextField.SetValueWithoutNotify(_fileName);
        fileNameTextField.MarkDirtyRepaint();
        fileNameTextField.RegisterValueChangedCallback(evt => _fileName = evt.newValue);
        
        toolbar.Add(new Button(() => _graphView.ResetNode()) {text = "Reset Node"});
        
        toolbar.Add(new Button(() => _graphView.FlipType()) {text = "Flip Node"});
        
        toolbar.Add(new Button(() => _graphView.FlipEdge()) {text = "Flip Edge"});
        
        toolbar.Add(new Button(() => RequestDataOperation(true)) {text = "Save Chart"});

        toolbar.Add(new Button(() => RequestDataOperation(false)) {text = "Load Chart"});
        
        toolbar.Add(fileNameTextField);
        _hasToolbar = true;
        _chartRoot.Add(toolbar);
    }
    
    private void RequestDataOperation(bool save)
    {
        if (!string.IsNullOrEmpty(_fileName))
        {
            var saveUtility = GraphSaveUtility.GetInstance(_graphView);
            if (save)
                saveUtility.SaveGraph(_fileName);
            else
                saveUtility.LoadGraph(_fileName);
        }
        else
        {
            EditorUtility.DisplayDialog("Invalid File Name", "Please Enter a valid filename", "OK");
        }
    }
    
    
    
    
    /*
         private VisualElement _columnRoot;
    private VisualElement _columnBottom;
    private VisualElement _chartRoot;
    
    // STRUCTURE - COLUMNS
    private static VisualTreeAsset _basicListRowTemplate;
    private VisualElement _columnSL; 
    private VisualElement _columnRL; 
    private VisualElement _columnRC; 
    private VisualElement _columnSC;  
    private VisualElement _listViewContainerSL; 
    private VisualElement _listViewContainerRL; 
    private VisualElement _listViewContainerRC; 
    private VisualElement _listViewContainerSC;  
    private ListView _listViewSL; 
    private ListView _listViewRL; 
    private ListView _listViewRC; 
    private ListView _listViewSC;

    // STRUCTURE - BOTTOMPANEL
    private Button _deleteButton;

    // STRUCTURE - ROOMCHART
    
    private List<string> _syncLayoutNames;
    private List<string> _roomLayoutNames;
    private List<string> _roomChartNames;
    private List<string> _sceneNames;

     
     
     public VisualElement CreateContent(ToolWindow toolWindow)
    {
        _toolWindow = toolWindow;
        _toolWindowSettings = Object.FindObjectOfType<ToolWindowSettings>();
        _designArea = Object.FindObjectOfType<DesignArea>();
        _layoutArea = Object.FindObjectOfType<LayoutArea>();
        _templateRoot = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>
            ( _toolWindowSettings.EditorFolderPath+ "/MatchContent.uxml").Instantiate();
        _templateRoot.style.backgroundColor =_toolWindowSettings.ContentColor; 
        _basicListRowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/MatchListRowTemplate.uxml");
        _basicListRowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ToolWindowSettings.Instance.EditorFolderPath+ "/ListRowTemplate.uxml"); 

        _columnRoot = _templateRoot.Q<VisualElement>("ColumnRoot");  
        _columnBottom = _templateRoot.Q<VisualElement>("ColumnBottom");  
        _chartRoot = _templateRoot.Q<VisualElement>("ChartRoot");  
        _columnSL = _templateRoot.Q<VisualElement>("ColumnSL");  
        _columnRL = _templateRoot.Q<VisualElement>("ColumnRL");  
        _columnRC = _templateRoot.Q<VisualElement>("ColumnRC");  
        _columnSC = _templateRoot.Q<VisualElement>("ColumnSC");  
        _listViewContainerSL = _columnSL.Q<VisualElement>("ListContainer");  
        _listViewContainerRL = _columnRL.Q<VisualElement>("ListContainer");  
        _listViewContainerRC = _columnRC.Q<VisualElement>("ListContainer");  
        _listViewContainerSC = _columnSC.Q<VisualElement>("ListContainer");
        _deleteButton = _templateRoot.Q<Button>("DeleteButton");

        _deleteButton.clicked += ClickDeleteButton;
        _deleteButton.style.visibility = new StyleEnum<Visibility>(Visibility.Hidden);
        
        ResetDisplayContainers();
        BuildColumns();
        return _templateRoot;
    }


    public void BuildColumns()
    {
        BuildSyncLayoutColumn();
        BuildRoomLayoutColumn();
        BuildRoomChartColumn();
        BuildSceneColumn();
    }
    private void BuildSyncLayoutColumn()
    {
      //  _syncLayoutNames =  UpdateList(_designArea.syncLayoutsPath );
        _listViewSL =   CreateListView(  _syncLayoutNames, ClickSyncLayout );
        _listViewContainerSL.Clear();
        _listViewContainerSL.Add(_listViewSL);
    } 
    private void BuildRoomLayoutColumn()
    {
        _roomLayoutNames =  UpdateList(_layoutArea.roomLayoutsPath );
        _listViewRL =   CreateListView(  _roomLayoutNames, ClickRoomLayout );
        _listViewContainerRL.Clear();
        _listViewContainerRL.Add(_listViewRL);
    }

    private void BuildRoomChartColumn()
    {
        _roomChartNames =  UpdateList(_layoutArea.roomChartsPath );
        _listViewRC =   CreateListView(  _roomChartNames, ClickRoomChart );
        _listViewContainerRC.Clear();
        _listViewContainerRC.Add(_listViewRC);
    }

    private void BuildSceneColumn()
    {
        _sceneNames =   UpdateSceneList();
        _listViewSC =  CreateListView(_sceneNames, ClickScene);
        _listViewContainerSC.Clear();
        _listViewContainerSC.Add(_listViewSC);  }
    private void ClickSyncLayout(string fileName, MouseUpEvent evt)
    {
        if (evt.button == 0)  // leftclick
        {
            LeftClickAnyListElement(fileName,0);
            _toolWindowSettings.RegisterSyncLayoutSelection(fileName);
        }
        else if (evt.button == 1) // rightclick
        {
            DeleteFromRightClick(0, fileName);
        }
        else if (evt.button == 2) // middleclick
        {
            _columnBottom.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        }
    }
    private void ClickRoomLayout(string fileName, MouseUpEvent evt)
    {
        if (evt.button == 0)  // leftclick
        {
            LeftClickAnyListElement(fileName,1);
            _toolWindowSettings.RegisterRoomLayoutSelection(fileName);
        }
        else if (evt.button == 1) // rightclick
        {
            DeleteFromRightClick(1, fileName);
        }
        else if (evt.button == 2) // middleclick
        {
            _columnBottom.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        }
 
    }
    private void ClickRoomChart(string fileName, MouseUpEvent evt)
    {
        if (evt.button == 0)  // leftclick
        {
            LeftClickAnyListElement(fileName,2);
            //todo if roomchart already selected  LoadRoomChartContent();
        }
        else if (evt.button == 1) // rightclick
        {
            DeleteFromRightClick(2, fileName);
        }
        else if (evt.button == 2) // middleclick
        {
            _columnBottom.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        }
     
    }
    private void ClickScene(string fileName, MouseUpEvent evt)
    {
        if (evt.button == 0)  // leftclick
        {
            LeftClickAnyListElement(fileName,3); 
        }
        else if (evt.button == 1) // rightclick
        {
            DeleteFromRightClick(3, fileName);
        }
        else if (evt.button == 2) // middleclick
        {
            _columnBottom.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex); 
        }
        
    }

    private int _latestColumnNumber;
    private void LeftClickAnyListElement(string name, int columnNumber)
    {
        _latestColumnNumber = columnNumber;
        _deleteButton.style.visibility = new StyleEnum<Visibility>(Visibility.Visible);
        if (columnNumber != 0)
            _listViewSL.ClearSelection();
        if (columnNumber != 1)
            _listViewRL.ClearSelection();
        if (columnNumber != 2)
            _listViewRC.ClearSelection();
        if (columnNumber != 3)
            _listViewSC.ClearSelection();
         
    }
    private List<string> UpdateList(string filePath)
    {
        List<string> returnList = new List<string>(); 
        string[] pathNames = Directory.GetFiles(filePath, "*.txt",  SearchOption.AllDirectories); 
        foreach (var name in pathNames) 
            returnList.Add(name.Substring(filePath.Length, name.Length-filePath.Length-4 ));
        return returnList;
    }
    private List<string> UpdateSceneList()
    {
        //ADDSCENE
        List<string> returnList = new List<string>(); 
        returnList.Add("hi");
        returnList.Add("how");
        returnList.Add("are");
        returnList.Add("you");
        returnList.Add("today");
        returnList.Add("not");
        returnList.Add("that");
        returnList.Add("I");
        returnList.Add("really");
        returnList.Add("care");
        returnList.Add("that");
        returnList.Add("much");
        returnList.Add(":)");
        returnList.Add("just");
        returnList.Add("kidding");
        returnList.Add("of");
        returnList.Add("course");
        return returnList;
    }
   
    private ListView CreateListView( List<string> stringList, Action<string, MouseUpEvent> clickAction)
    { 
        VisualElement MakeItem() => _basicListRowTemplate.CloneTree();
        void BindItem(VisualElement e, int i)
        { 
            e.Q<Label>("NameLabel").text = stringList[i]; 
            e.RegisterCallback<MouseUpEvent>(evt => { clickAction.Invoke(stringList[i], evt); });
        }
        return new ListView(stringList, _toolWindowSettings.organizeRowHeight, MakeItem, BindItem)
        {
            selectionType = SelectionType.Single,
            reorderable = true,
            style =
            {
                height = _toolWindowSettings.organizeColumnHeight,
                flexShrink = 0,
                flexGrow = 1
            }
        };
    }

    private void LoadRoomChartContent()
    {
        _columnRoot.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None); 
        _chartRoot.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        
        //todo node content? 
    }
    private void ResetDisplayContainers()
    {
        _columnRoot.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex); 
        _columnBottom.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        _chartRoot.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
    }
    public void InitializeContent()
    {
    }

    private void DeleteFromRightClick(int column, string fileName)
    {
        if (column == 0)
            _designArea.DeleteSyncLayout(fileName);
        else if (column == 1)
            _layoutArea.DeleteRoomLayout(fileName);
        else if (column == 2)
            _layoutArea.DeleteRoomChart(fileName);
        else if (column == 3)
        {
            //ADDSCENE
        }
            
        BuildColumns();
    }
    private void ClickDeleteButton()
    {
         // find column with _latestColumnNumber and then delete fileName
         // alternate: check rightmouseclick with callback evt? 
    }*/

   
}
#endif