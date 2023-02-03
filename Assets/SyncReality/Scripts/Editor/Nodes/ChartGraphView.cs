#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements; 
using Button = UnityEngine.UIElements.Button; 
using Object = UnityEngine.Object;
using Toggle = UnityEngine.UIElements.Toggle;


public class ChartGraphView : GraphView
{

    private DesignArea _designArea;
    public readonly Vector2 defaultNodeSize = new Vector2(240, 200);

    private int _setupNodeCount = 0;
    private SetupNode _selectedNode;
    private MagicEdge _selectedEdge;
    public List<ExposedProperty> exposedProperties = new List<ExposedProperty>();
    public Blackboard blackboard;

    public ChartGraphView()
    {
        if (_designArea == null)
            _designArea = Object.FindObjectOfType<DesignArea>();
        _setupNodeCount = 0;
        var chartGraph = Resources.Load<StyleSheet>("ChartGraph");
        if (chartGraph != null)
        {
            styleSheets.Add(chartGraph);
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();
            BuildNodes();
            graphViewChanged += OnGraphChange;
        }

    }

    public void FlipType()
    {
        if (_selectedNode != null)
        {
            _selectedNode.FlipType();
            StyleNodeAccordingToType(_selectedNode); 
        }
    }

    public void ResetNode()
    {
        if (_selectedNode != null)
        {
            _selectedNode.nodeType = NodeType.Physical;
            StyleNodeAccordingToType(_selectedNode);
            foreach (var portElement in _selectedNode.outputContainer.Children())
            {
                Port port = portElement as Port;
                if (port != null)
                    port.DisconnectAll(); // does not work.. :(
            }
            foreach (var portElement in _selectedNode.inputContainer.Children())
            {
                Port port = portElement as Port;
                if (port != null)
                    port.DisconnectAll(); 
            }
            ReenableNodePorts(_selectedNode);
        } 
    }

    private void  ReenableNodePorts(Node node)
    {
        foreach (var portElement in node.outputContainer.Children())
        {
            Port port = portElement as Port;
            if (port != null)
                port.SetEnabled(true);
        }
        foreach (var portElement in node.inputContainer.Children())
        {
            Port port = portElement as Port;
            if (port != null)
                port.SetEnabled(true);
        }
    }
    public void SelectNode(SetupNode node)
    {
        _selectedNode = node;
    }
    public void DeselectNode(SetupNode node)
    {
        _selectedNode = null;
    }

    public void SelectEdge(MagicEdge edge)
    {
        _selectedEdge = edge;
    }
    public void DeselectEdge(MagicEdge edge)
    {
        _selectedEdge = null;
    }
    public void FlipEdge()
    {
        if (_selectedEdge != null)
        {
            _selectedEdge.FlipDirection();
            StyleNodeAccordingToEdge(_selectedEdge); 
            _selectedEdge.Unselect(this);
        }
    }

  

    private GraphViewChange  OnGraphChange(GraphViewChange graphViewChange)
    {

        if (graphViewChange.edgesToCreate != null)
        { 
            foreach (var edge in graphViewChange.edgesToCreate)
            { 
                Port input = edge.input ;
                Port output = edge.output ;
                Rect position = edge.GetPosition();
                CreateMagicEdge(input, output, position);
                edge.input.node.RefreshPorts();
                edge.input.node.RefreshExpandedState();
                edge.output.node.RefreshPorts();
                edge.output.node.RefreshExpandedState();
            }
            graphViewChange.edgesToCreate.Clear();
        }  
        
        MarkDirtyRepaint();
        return graphViewChange;
    }

    private void CreateMagicEdge(Port inputSocket, Port outputSocket, Rect position)
    {
        var tempEdge = new MagicEdge()
        {
            output = outputSocket,
            input = inputSocket,
            graphView = this
          //  candidatePosition = position.position
        };
        tempEdge.SetPosition( position);
        tempEdge?.input.Connect(tempEdge);
        tempEdge?.output.Connect(tempEdge);
        StyleNodeAccordingToEdge(tempEdge);
        this.AddElement(tempEdge);
        RemoveUsedInputPortsFromView(tempEdge.output.node);
        RemoveUsedOutputPortsFromView(tempEdge.input.node);
    }

    private void RemoveUsedOutputPortsFromView(Node node)
    {
        ReenableNodePorts(node);
        List<string> portNames = new List<string>();
        foreach (var portElement in node.inputContainer.Children())
        {
            Port port = portElement as Port;
            if (port != null)
                if (port.connected)
                    portNames.Add(port.portName);
        }
        foreach (var portElement in node.outputContainer.Children())
        {
            Port port = portElement as Port;
            if (port != null)
            {
                if (portNames.Contains(port.portName)) 
                    portElement.SetEnabled(false); 
            }
             
        }
    }

    private void RemoveUsedInputPortsFromView(Node node)
    {
        ReenableNodePorts(node);
        List<string> portNames = new List<string>();
        foreach (var portElement in node.outputContainer.Children())
        {
            Port port = portElement as Port;
            if (port != null)
                if (port.connected)
                    portNames.Add(port.portName);
        }
        foreach (var portElement in node.inputContainer.Children())
        {
            Port port = portElement as Port;
            if (port != null)
                if (portNames.Contains(port.portName))
                    portElement.SetEnabled(false);

        }
    }
  

    public void ClearBlackBoardAndExposedProperties()
    {
        exposedProperties.Clear(); 
        if (blackboard != null)
           blackboard.Clear();
    }
    public void AddPropertyToBlackBoard(ExposedProperty exposedProperty, bool loadMode = false)
    {
        var localPropertyName = exposedProperty.PropertyName;
        var localPropertyValue = exposedProperty.PropertyValue;

        while (exposedProperties.Any(x => x.PropertyName == localPropertyName))
            localPropertyName = $"{localPropertyName}(1)";
        
        
        var property = new ExposedProperty();
        property.PropertyName = localPropertyName;
        property.PropertyValue = localPropertyValue;
        exposedProperties.Add(property);
        
        var container = new VisualElement();
        var blackboardField = new BlackboardField {text = property.PropertyName, typeText = "string property"};
        container.Add(blackboardField);
        
        
        var propertyValueTextField = new TextField("Value:")
        {
            value = localPropertyValue
        };
        propertyValueTextField.RegisterValueChangedCallback(evt =>
        {
            var index = exposedProperties.FindIndex(x => x.PropertyName == property.PropertyName);
            exposedProperties[index].PropertyValue = evt.newValue;
        });
        container.Add(propertyValueTextField);

        var blackboardRow = new BlackboardRow(blackboardField, propertyValueTextField);
        container.Add(blackboardRow);

        blackboard.Add(container);
    }
   
    private void BuildNodes()
    {
        BuildSetupNodes();
    }

    private void BuildSetupNodes()
    {
        var startSyncSetup = _designArea.GetFirstActivePresentSyncSetup();
                  CreateNode(startSyncSetup.gameObject.name,true, true, false);
          foreach (var syncSetup in _designArea.GetPresentSyncSetups())
                 if (syncSetup.gameObject.name!= startSyncSetup.gameObject.name)
                    CreateNode(syncSetup.gameObject.name, true, false, false);
    }

    public void CreateNodeThroughButton(string syncSetupName)
    {
        CreateNode(syncSetupName, true, false, false);
    }
    
    
    private void CreateNode(string syncSetupName, bool assignNewGuid = true, bool firstNode= false, bool loadedFromData = false)
    {
        AddElement(CreateSetupNode(syncSetupName, assignNewGuid, firstNode, loadedFromData));
    }

    

    public SetupNode CreateSetupNode(string syncSetupName, bool assignNewGuid = true, bool firstNode= false, bool loadedFromData = false, string savedGUID = "", NodeType nodeType = NodeType.Physical)
    {
        _setupNodeCount++;
        var node = new SetupNode
        {
            title = syncSetupName,
            nodeName = syncSetupName, 
            parentGraph =  this
        };
        if (assignNewGuid)
           node.GUID = Guid.NewGuid().ToString();
        else if (loadedFromData)
            node.GUID = savedGUID;
        
        node.nodeType = nodeType;

        node.style.minWidth = 200;

        node.style.flexGrow = 1;
        node.style.flexShrink = 0;

        //    setupNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));

        /*var button = new Button(() => { AddChoicePort(setupNode); })
        {               style =                {
                height = new StyleLength(22),
                marginTop = 7,
                paddingTop = 2                },
            text = "Add Portal"            };
        setupNode.titleContainer.Add(button);*/

        node.syncSetupSelector = new ToolbarMenu { text = syncSetupName };
        node.syncSetupSelector.style.color = new Color(0, 128, 0);

        if (syncSetupName != "")
        {
            foreach (var syncSetup in _designArea.GetPresentSyncSetups())
            {
                if (syncSetup.gameObject.name == syncSetupName)
                    node.syncSetupSelector.menu.AppendAction(syncSetup.gameObject.name,
                        a => { SyncSetupSelectorAction(node, syncSetup); }, a => DropdownMenuAction.Status.Checked);
                else
                    node.syncSetupSelector.menu.AppendAction(syncSetup.gameObject.name,
                        a => { SyncSetupSelectorAction(node, syncSetup); }, a => DropdownMenuAction.Status.Normal);
            }
        }
        else // newly created node
        {
            node.syncSetupSelector.menu.AppendAction("Select SyncSetup", a => { SyncSetupSelectorAction(node, null); },
                a => DropdownMenuAction.Status.Normal);

        }

        node.titleContainer.Clear();
        node.highlightContainer = node.syncSetupSelector;
        node.titleContainer.Add(node.syncSetupSelector);
        VisualElement nodeBottomContent = new VisualElement
        {
            style =
            {
                flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
                backgroundColor = new StyleColor( new Color32(58, 58, 58, 255)) 
            }
        };
        VisualElement startToggleContainer = new VisualElement
        {
            style =
            {
                flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
                justifyContent = new StyleEnum<Justify>(Justify.FlexStart),
                paddingLeft = 2
            }
        };
        var startLabel = new Label("Priority: ");
        startLabel.style.paddingTop = 2;
        var priorityField = new IntegerField("");
        priorityField.style.height = 15;
        priorityField.style.paddingTop = 2;
        //priorityField.style.paddingTop = 2;
        node.priorityField = priorityField;
        if (firstNode)
        {
            node.priorityField.SetValueWithoutNotify(0);  
            node.highlightContainer.style.backgroundColor = new StyleColor(new Color32(99, 99, 99,255));
        }
        else 
            node.priorityField.SetValueWithoutNotify(1);  

        priorityField.RegisterValueChangedCallback(evt => ChangePriorityField(priorityField.value, node)); 

        startToggleContainer.Add(startLabel);
        startToggleContainer.Add(priorityField);
        nodeBottomContent.Add(startToggleContainer);

        node.inputContainer.style.backgroundColor = new StyleColor(ToolWindowSettings.Instance.inputContainerColor);
        node.outputContainer.style.backgroundColor = new StyleColor(ToolWindowSettings.Instance.outputContainerColor);
        

     //   if (manuallyCreated == false)

   //  Debug.Log(syncSetupName +"  assignNewGuid:"+assignNewGuid + "  firstNode:"+firstNode+ "  loadedFromData:"+loadedFromData);
     
            var idLabel = new Label();
            idLabel.text = "GUID:" + node.GUID.Substring(0, 8) + "\n" + node.nodeType;
            node.titleContainer.Add(idLabel);
        

        node.mainContainer.Add(nodeBottomContent);

        
        if (loadedFromData == false)
            node.SetPosition(new Rect(Vector2.zero + Vector2.right * 1.1f * _setupNodeCount * defaultNodeSize,
                defaultNodeSize));
        else
            node.SetPosition(new Rect(Vector2.zero, defaultNodeSize));
        UpdateNodePorts(node, syncSetupName, firstNode);
        node.RefreshExpandedState();
        node.RefreshPorts();
        StyleNodeAccordingToType(node);
        return node;
    }

    private void StyleNodeAccordingToType(SetupNode node)
    {
        if (node.nodeType == NodeType.Physical)
        {
            node.inputContainer.style.backgroundColor = new StyleColor(new Color32(111, 58, 58,255));
            node.outputContainer.style.backgroundColor = new StyleColor(new Color32(111, 58, 58,255)); 
        }
        else   if (node.nodeType == NodeType.Ethereal)
        {
            node.inputContainer.style.backgroundColor = new StyleColor(new Color32(22, 58, 199,255));
            node.outputContainer.style.backgroundColor = new StyleColor(new Color32(22, 58, 199,255)); 
        }
        else   if (node.nodeType == NodeType.Traversal)
        {
            node.inputContainer.style.backgroundColor = new StyleColor(new Color32(11, 111, 58,255));
            node.outputContainer.style.backgroundColor = new StyleColor(new Color32(11, 111, 58,255)); 
        }
        node.titleContainer.Q<Label>("").text ="GUID:" + node.GUID.Substring(0, 8) + "\n" + node.nodeType;
            
    }
    public void StyleNodeAccordingToEdge(MagicEdge selectedEdge)
    {
        if (selectedEdge.biDirectional == true)
        {
            selectedEdge.output.portColor = new Color32(133, 12, 224, 255);
            selectedEdge.input.portColor = new Color32(133, 12, 224, 255);
        }
        else
        {
            selectedEdge.output.portColor = new Color32(133, 12, 224, 255);
            selectedEdge.input.portColor = new Color32(132, 222, 231, 255);
        }
  
    }
    private void ChangePriorityField(int value, SetupNode givenNode )
    {
        
            /*foreach (var node in GetSetupNodes())
            {
                node.startToggle.SetValueWithoutNotify(false);
                node.highlightContainer.style.backgroundColor = new StyleColor(new Color32(58, 58, 58,255));
            }
            givenNode.startToggle.SetValueWithoutNotify(true);
            givenNode.highlightContainer.style.backgroundColor = new StyleColor(new Color32(99, 99, 99,255));*/
        
        
    }

    /*private void RefreshNodes()
    {
        foreach (var node in GetSetupNodes())
            RefreshNode(node);
        
    }
    private void RefreshNode(SetupNode node)
    {
        
    }*/

    private List<SetupNode> GetSetupNodes()
    {
        return nodes.ToList().Cast<SetupNode>().ToList();
    }

        private List<Edge> Edges => edges.ToList(); 
        private void UpdateNodePorts(SetupNode node, string syncSetupName, bool startNode = false)
        {
            var connectedEdges = Edges.Where(x => x.input.node ==  node || x.output.node ==  node).ToArray();
            foreach (var edge in connectedEdges)
            {
                edge.input.Clear();
                edge.output.Clear();
                RemoveElement(edge);
            }
            GenerateInputPortsFromDoors(node, syncSetupName);
            GenerateOutputPortsFromDoors(node, syncSetupName);
        }
         private void GenerateInputPortsFromDoors(SetupNode node, string syncSetupName )
        {
            node.inputContainer.Clear(); 
            var syncSetup = _designArea.GetPresentSyncSetupForName(syncSetupName);
            if (syncSetup == null)
                Debug.LogWarning("Did not find SyncSetup while creating ports: " + syncSetupName); 

            foreach (var sync in _designArea.GetSyncs(syncSetupName))
            {
                /*if (sync.classificationList.Contains( Classification.Door))
                {
                    var generatedPort = GeneratePort(node, Direction.Input);
                    generatedPort.portName = ""+sync.gameObject.name;
                    node.inputContainer.Add(generatedPort);
                }
                if (sync.classificationList.Contains( Classification.Portal))
                {
                    var generatedPort = GeneratePort(node, Direction.Input);
                    generatedPort.portName = ""+sync.gameObject.name;
                    node.inputContainer.Add(generatedPort);
                }*/
                /*if (sync.classificationList.Contains( Classification.PortalSpawn))
                {
                    var generatedPort = GeneratePort(node, Direction.Input);
                    generatedPort.portName = "Spawn_"+sync.gameObject.name;
                    node.inputContainer.Add(generatedPort);
                }*/
            } 
        }
        private void GenerateOutputPortsFromDoors(SetupNode node, string syncSetupName, bool startNode = false)
        {
            node.outputContainer.Clear(); 
            var syncSetup = _designArea.GetPresentSyncSetupForName(syncSetupName);
            if (syncSetup == null)
                Debug.LogWarning("Did not find SyncSetup while creating ports: " + syncSetupName);

            foreach (var sync in _designArea.GetSyncs(syncSetupName))
            {
                /*if (sync.classificationList.Contains( Classification.Door))
                {
                    var generatedPort = GeneratePort(node, Direction.Output);
                    generatedPort.portName = ""+sync.gameObject.name;
                    node.outputContainer.Add(generatedPort);
                }
                if (sync.classificationList.Contains( Classification.Portal))
                {
                    var generatedPort = GeneratePort(node, Direction.Output);
                    generatedPort.portName = ""+sync.gameObject.name;
                    node.outputContainer.Add(generatedPort);
                }*/
                /*if (sync.classificationList.Contains( Classification.PortalExit))
                {
                    var generatedPort = GeneratePort(node, Direction.Output);
                    generatedPort.portName = "Exit_"+sync.gameObject.name;
                    node.outputContainer.Add(generatedPort);
                }*/
            }
        }
        
        
        private void SyncSetupSelectorAction( SetupNode node, SyncSetup givenSyncSetup = null)
        {
            node.syncSetupSelector = new ToolbarMenu { text = givenSyncSetup.gameObject.name };
            node.syncSetupSelector.style.color = new Color(0,128,0);

            if (givenSyncSetup != null)
            {
                foreach (var syncSetup in _designArea.GetStoredSyncSetups())
                {
                    if (syncSetup.gameObject.name == givenSyncSetup.gameObject.name)
                        node.syncSetupSelector.menu.AppendAction(syncSetup.gameObject.name, a => { SyncSetupSelectorAction(node, syncSetup);}, a => DropdownMenuAction.Status.Checked);
                    else
                        node.syncSetupSelector.menu.AppendAction(syncSetup.gameObject.name, a => { SyncSetupSelectorAction(node, syncSetup);}, a => DropdownMenuAction.Status.Normal);
                }
            }
            else
            {
                foreach (var syncSetup in _designArea.GetStoredSyncSetups())
                    node.syncSetupSelector.menu.AppendAction(syncSetup.gameObject.name, a => { SyncSetupSelectorAction(node, syncSetup);}, a => DropdownMenuAction.Status.Normal);
            }
         
            node.titleContainer.Clear();
            node.titleContainer.Add(node.syncSetupSelector);
            node.nodeName = givenSyncSetup.gameObject.name;
            UpdateNodePorts(node, givenSyncSetup.gameObject.name);
        }
       
        
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            ports.ForEach((port) =>
            {
                if (startPort != port && startPort.node != port.node)
                    compatiblePorts.Add(port);
            });
            return compatiblePorts;
        }

        private Port GeneratePort(SetupNode node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
        {
            return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
        }


        /*
        public void AddChoicePort(SetupNode setupNode, string overriddenPortName = "")
        {
            var generatedPort = GeneratePort(setupNode, Direction.Output);
            
            var portLabel = generatedPort.contentContainer.Q<Label>("type");
            generatedPort.contentContainer.Remove(portLabel); // remove default label
            
            var outputPortCount = setupNode.outputContainer.Query("connector").ToList().Count;
            generatedPort.portName = $" Choice {outputPortCount}";
            
            var outputPortName = string.IsNullOrEmpty(overriddenPortName)
                ? $"Portal {outputPortCount + 1}"
                : overriddenPortName;
            
            var textField = new TextField()
            {
                name = string.Empty,
                value = outputPortName
            };
            textField.RegisterValueChangedCallback(evt => generatedPort.portName = evt.newValue);
            generatedPort.contentContainer.Add(new Label("  "));
            generatedPort.contentContainer.Add(textField);

            var deleteButton = new Button(() => RemovePort(setupNode, generatedPort))
            {
                text = "X"
            };
            generatedPort.contentContainer.Add(deleteButton);
            generatedPort.portName = outputPortName; 
            setupNode.outputContainer.Add(generatedPort);
            setupNode.RefreshPorts();
            setupNode.RefreshExpandedState(); 
        }
        
        
        /*
        private void RemovePort(Node node, Port socket)
        {
            var targetEdge = edges.ToList()
                .Where(x => x.output.portName == socket.portName && x.output.node == socket.node);
         //   if (!targetEdge.Any()) return;
            if (targetEdge.Any())
            {
                var edge = targetEdge.First();
                edge.input.Disconnect(edge);
                RemoveElement(targetEdge.First());
            }
             
            node.outputContainer.Remove(socket);
            node.RefreshPorts();
            node.RefreshExpandedState();
        }
        */
        
        /*private SetupNode CreateStartNode(string syncSetupName)  // obsoleted this
        { 
            var node = new SetupNode
            {
                title = "",
                GUID = Guid.NewGuid().ToString(),
                nodeName =   syncSetupName,
                startNode = true
            };

            //    node.capabilities &= ~Capabilities.Movable;
         //   node.capabilities &= ~Capabilities.Deletable;
            
            node.style.width = 240;
            //  node.styleSheets.Add(Resources.Load<StyleSheet>("Node"));

            node.syncSetupSelector = new ToolbarMenu { text = syncSetupName };
            node.syncSetupSelector.style.color = new Color(0,128,0); 
            foreach (var syncSetup in _designArea.GetPresentSyncSetups())
            {
                if (syncSetup.gameObject.name == syncSetupName)
                  node.syncSetupSelector.menu.AppendAction(syncSetup.gameObject.name, a => { SyncSetupSelectorAction(node, syncSetup);}, a => DropdownMenuAction.Status.Checked);
                else
                    node.syncSetupSelector.menu.AppendAction(syncSetup.gameObject.name, a => { SyncSetupSelectorAction(node, syncSetup);}, a => DropdownMenuAction.Status.Normal);
            }
            node.titleContainer.Clear();
            node.titleContainer.Add(node.syncSetupSelector);

            RefreshNode(node, syncSetupName, true);
                
            node.RefreshExpandedState();
            node.RefreshPorts();
            node.titleContainer.style.height = 15;
            node.SetPosition(new Rect(100, 100, 100, 150));
            return node;
        }*/
       
}
#endif
