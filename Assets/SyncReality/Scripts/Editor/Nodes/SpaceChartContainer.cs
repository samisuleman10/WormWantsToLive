
#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpaceChartContainer : ScriptableObject
{
    public List<SetupNodeData> setupNodeData = new List<SetupNodeData>();
    public List<NodeLinkData> nodeLinks = new List<NodeLinkData>();
    public List<ExposedProperty> exposedProperties = new List<ExposedProperty>();
}
#endif