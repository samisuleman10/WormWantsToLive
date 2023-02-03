#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SurrounderModule))]
public class SurrounderEditor : Editor
{
    // obsoleted by previewwindow & layoutarea
    private void OnSceneGUI()
    {
        /*var surrounder = (SurrounderModule)target;
        if (surrounder == null) return;
        for (int i = 0; i < surrounder.designStorage.GetLayoutPoints().Count; i++)
        {
            var point = surrounder.designStorage.GetLayoutPoints()[i];
            var color = new Color(1, 0.8f, 0.4f, 1);
            Handles.color = color;
            surrounder.designStorage.GetLayoutPoints()[i] = Handles.PositionHandle(point, Quaternion.identity);
            if (surrounder.designStorage.GetLayoutPoints()[i] != point && !Application.isPlaying)
            {
                UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
                UnityEditor.SceneView.RepaintAll();
            }
        }*/
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }


}
#endif
