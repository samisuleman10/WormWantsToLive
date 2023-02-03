using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NodeLinkData
{
    public string BaseNodeGUID;
    public string PortName;
    public string TargetNodeGUID;
    public string TargetPortName;
    public bool biDirectional = true;
}
