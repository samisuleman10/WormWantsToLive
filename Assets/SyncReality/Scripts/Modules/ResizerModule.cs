using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Adapts Playfields to Clutter and Processes SmartAssets
/// </summary>
public class ResizerModule : ModuleBase<List<GameObject>, List<GameObject>>
{

    /// <summary>
    /// Adapts Playfields to Clutter and Processes SmartAssets
    /// </summary>
    /// <param name="input">All SyncInstances</param>
    /// <returns>Adapted SyncInstances</returns>
    public override List<GameObject> Execute(List<GameObject> input)
    {
        return input;
    }
}
