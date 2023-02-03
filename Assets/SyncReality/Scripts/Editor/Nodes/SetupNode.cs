using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;


public enum NodeType
{
    Physical,
    Ethereal,
    Traversal
}
public class SetupNode : Node
{
    public string GUID;
    public string nodeName;
    public bool startNode = false;
    public ToolbarMenu syncSetupSelector;
    public VisualElement highlightContainer;
    public IntegerField priorityField;
    public NodeType nodeType;
    public ChartGraphView parentGraph;


    public void FlipType()
    {
        if (nodeType == NodeType.Physical)
            nodeType = NodeType.Ethereal;
        else    if (nodeType == NodeType.Ethereal)
            nodeType = NodeType.Traversal;
        else    if (nodeType == NodeType.Traversal)
            nodeType = NodeType.Physical;
    }
    public override void OnSelected()
    {
        parentGraph.SelectNode(this);
    }

    public override void OnUnselected()
    {
        parentGraph.DeselectNode(this);

    }
}
#endif
