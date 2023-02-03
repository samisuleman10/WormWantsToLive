#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SetupNodeData 
{
    public string NodeGUID;
    public string nodeText;
    public Vector2 Position;
    public int priorityValue;
    public NodeType nodeType;
}
#endif