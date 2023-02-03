using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(StoryTeller))]
public class StoryTellerEditor : Editor
{
    
    
    public override void OnInspectorGUI()
    {
        StoryTeller storyTeller = (StoryTeller) target;
        DrawDefaultInspector();
        /*if (GUILayout.Button("Load Layout One"))
        {
            storyTeller.LoadLayoutOne();
        }
        if (GUILayout.Button("Load Layout Two"))
        {
            storyTeller.LoadLayoutTwo();

        }*/
    }
}

#endif
