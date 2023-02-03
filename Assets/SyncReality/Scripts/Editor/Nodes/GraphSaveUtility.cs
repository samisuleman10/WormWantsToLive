

#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OVR.OpenVR;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphSaveUtility
{
    private ChartGraphView _graphView;
    
    private List<Edge> Edges => _graphView.edges.ToList();
    private List<SetupNode> Nodes => _graphView.nodes.ToList().Cast<SetupNode>().ToList();

    private SpaceChartContainer _containerCache;
    
    public static GraphSaveUtility GetInstance(ChartGraphView graphView)
    {
        return new GraphSaveUtility
        {
            _graphView = graphView
        };
    }

    private bool SaveNodes(SpaceChartContainer container)
    {

        var connectedPorts = Edges.Where(x => x.input.node != null).ToArray();

        for (int i = 0; i < connectedPorts.Length; i++)
        {
            var outputNode = connectedPorts[i].output.node as SetupNode;
            var inputNode = connectedPorts[i].input.node as SetupNode;
            
            container.nodeLinks.Add(new NodeLinkData
            {
                BaseNodeGUID = outputNode.GUID,
                PortName = connectedPorts[i].output.portName,
                TargetPortName = connectedPorts[i].input.portName,
                TargetNodeGUID = inputNode.GUID
            });
        }

        //foreach (var node in Nodes.Where(node=>!node.entryPoint))
        foreach (var node in Nodes)
        {
            container.setupNodeData.Add(new SetupNodeData
            {
                NodeGUID = node.GUID,
                nodeText = node.nodeName,
                Position = node.GetPosition().position,
                nodeType = node.nodeType,
                priorityValue = node.priorityField.value
            });
        }

        return true;
    }
    private void SaveEdges(SpaceChartContainer container)
    {
        foreach (var edge in Edges.Where(x => x.input.node != null))
        {
            MagicEdge magicEdge = edge as MagicEdge;
            foreach (var nodeLinkData in container.nodeLinks)
                if (nodeLinkData.PortName == magicEdge.output.portName && nodeLinkData.TargetPortName == magicEdge.input.portName)
                    nodeLinkData.biDirectional = magicEdge.biDirectional;
        }
    }
    
    public void SaveGraph(string fileName)
    {
      //  if (!Edges.Any()) return; // don't save if there are no edges (= links)

      var container = ScriptableObject.CreateInstance<SpaceChartContainer>();
      SaveNodes(container);
      SaveEdges(container);
      SaveExposedProperties(container);
        AssetDatabase.CreateAsset(container, $"Assets/Resources/SpaceCharts/{fileName}.asset");
        AssetDatabase.SaveAssets();
    }

   

    private void SaveExposedProperties(SpaceChartContainer container)
    {
        container.exposedProperties.Clear();
        container.exposedProperties.AddRange(_graphView.exposedProperties);
    }
    public void LoadGraph(string fileName)
    {
        _containerCache = Resources.Load<SpaceChartContainer>( "SpaceCharts/" + fileName);
        if (_containerCache == null)
        {
            EditorUtility.DisplayDialog("File Not Found", "Target Space Chart does not exist!", "OK");
            return;
        }
        ClearGraph();
        CreateNodes();
        ConnectNodes();
        CreateExposedProperties();
    }

    private void CreateExposedProperties()
    {
        
        _graphView.ClearBlackBoardAndExposedProperties();
        foreach (var exposedProperty in _containerCache.exposedProperties)
        {
            _graphView.AddPropertyToBlackBoard(exposedProperty);
        }
    }

    private void ClearGraph()
    {
//        Nodes.Find(x => x.startNode).GUID = _containerCache.nodeLinks[0].BaseNodeGUID;
        foreach (var node in Nodes)
        {
         //   if (node.entryPoint) continue;
            Edges.Where(x => x.input.node == node).ToList()
                .ForEach(edge => _graphView.RemoveElement(edge));
            _graphView.RemoveElement(node);
        }
    }
    
    private void CreateNodes()
    {
        foreach (var nodeData in _containerCache.setupNodeData)
        {
            var tempNode = _graphView.CreateSetupNode(nodeData.nodeText, false, false,true, nodeData.NodeGUID, nodeData.nodeType);
            tempNode.SetPosition(new Rect(
                nodeData.Position, 
                _graphView.defaultNodeSize));
            
            var idLabel = new Label();
            idLabel.text = tempNode.GUID.Substring(0,8);
            tempNode.titleContainer.Add(idLabel); 
            tempNode.priorityField.SetValueWithoutNotify(nodeData.priorityValue);
            _graphView.AddElement(tempNode);

       //     var nodePorts = _containerCache.nodeLinks.Where(x => x.BaseNodeGUID == nodeData.NodeGUID).ToList();
       //     nodePorts.ForEach(x => _graphView.AddChoicePort(tempNode, x.PortName));
        }
    }
    
    private void ConnectNodes()
    {
        for (var i = 0; i < Nodes.Count; i++)
        {
            var k = i; 
            var connections = _containerCache.nodeLinks.Where(x => x.BaseNodeGUID == Nodes[k].GUID).ToList();
            for (var j = 0; j < connections.Count(); j++)
            {
                var targetNodeGUID = connections[j].TargetNodeGUID; 
                var targetNode = Nodes.First(x => x.GUID == targetNodeGUID);
                var originNode = Nodes[i]; 
                Port outputPort = Nodes[i].outputContainer[j].Q<Port>();
                Port inputPort = (Port)targetNode.inputContainer[0];
                Label inputLabel = targetNode.inputContainer.Q<Label>("type") ;
        
            //    Debug.Log(inputLabel.text+" zereoinput " + targetNode.inputContainer[0].ToString() );

                for (int l = 0; l < targetNode.inputContainer.childCount; l++) 
                    if (targetNode.inputContainer[l].Q<Label>("type").text == connections[j].TargetPortName)
                        inputPort = (Port)targetNode.inputContainer[l]; 

                for (int l = 0; l < originNode.outputContainer.childCount; l++)
                    if (originNode.outputContainer[l].Q<Label>("type").text == connections[j].PortName)
                        outputPort = (Port)  originNode.outputContainer[l];       
                
                
                LinkNodesTogether(outputPort,  inputPort);

            //    targetNode.SetPosition(new Rect(_containerCache.setupNodeData.First(x => x.NodeGUID == targetNodeGUID).Position, _graphView.defaultNodeSize));
            }
        }
    }
    
    private void LinkNodesTogether(Port outputSocket, Port inputSocket)
    {
        var tempEdge = new MagicEdge()
        {
            output = outputSocket,
            input = inputSocket,
            graphView =  _graphView
        };
        tempEdge?.input.Connect(tempEdge);
        tempEdge?.output.Connect(tempEdge);
        foreach (var nodeLinkData in _containerCache.nodeLinks)
            if (nodeLinkData.PortName == outputSocket.portName && nodeLinkData.TargetPortName == inputSocket.portName)
                tempEdge.biDirectional = nodeLinkData.biDirectional;
        _graphView.AddElement(tempEdge);
        _graphView.StyleNodeAccordingToEdge(tempEdge);
    }

}
#endif