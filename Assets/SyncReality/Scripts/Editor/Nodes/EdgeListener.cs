
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class EdgeListener : IEdgeConnectorListener
{
    public void OnDropOutsidePort(Edge edge, Vector2 position)
    {
        Debug.Log("ondropoutside");

    }

    public void OnDrop(GraphView graphView, Edge edge)
    {
        Debug.Log("ondrop");
    }
}
#endif