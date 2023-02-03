#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class MagicEdge : Edge
{

    public ChartGraphView graphView;

    public bool biDirectional = false;

    public MagicEdge()
    {
        styleSheets.Add(Resources.Load<StyleSheet>("Edge")); 
    }
    public override void OnSelected()
    {
        graphView.SelectEdge(this);
    }
    public override void OnUnselected()
    {
        graphView.DeselectEdge(this);
    }
    public void FlipDirection()
    {
        biDirectional = !biDirectional;
    }
}
#endif