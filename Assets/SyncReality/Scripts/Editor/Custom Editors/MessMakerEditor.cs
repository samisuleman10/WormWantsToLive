using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


    
#if UNITY_EDITOR


[CustomEditor(typeof(MessMaker))]
public class MessMakerEditor : Editor
{
    private Tool _lastUsedTool = Tool.None;

    
    private void OnEnable()
    { 
        _lastUsedTool = Tools.current;
        Tools.current = Tool.None; 
    }
    private void OnDisable()
    {
        Tools.current = _lastUsedTool;
    }
}
    

#endif