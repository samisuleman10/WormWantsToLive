#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

[CustomEditor(typeof(WallMini))]
public class WallMiniEditor : Editor
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
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (target != null && GUILayout.Button("Set Bounds to Mesh Size"))
        {
            ((WallMini)target).mappedSurroundSync.CreateCombinedMesh();

        }

      //  ((WallMini)target).mappedSurroundSync.bounds = EditorGUILayout.BoundsField(((WallMini)target).mappedSurroundSync.bounds);

    }

    void OnSceneGUI()
    {
        DrawBoundsBox();
        if (((WallMini) target).mappedSurroundSync.gameObject != null)
             if (((WallMini) target).mappedSurroundSync != null)
                  Selection.activeGameObject = ((WallMini) target).mappedSurroundSync.gameObject;
    }

    BoxBoundsHandle m_BoundsHandle = new BoxBoundsHandle();
    void DrawBoundsBox()
    {
        /*//Min Bounding Box
        var wallMini = (target as WallMini);
        SurroundSync t = wallMini.mappedSurroundSync;
        var tr = wallMini.transform;
        var pos = tr.position;
        Handles.matrix = Matrix4x4.identity;
        Handles.color = Color.green;
        if (tr.GetChild(1) != null)
            if (tr.GetChild(1).GetComponent<SurroundSync>() != null)
            {
                var rotatedMatrix = Handles.matrix
                                    * Matrix4x4.TRS
                                    (
                                        tr.GetChild(1).position,
                                        tr.GetChild(1).rotation, tr.GetChild(1).lossyScale);
                using (new Handles.DrawingScope(rotatedMatrix))
                {
                    m_BoundsHandle.center = t.bounds.center;
                    m_BoundsHandle.size = t.bounds.size;
                    EditorGUI.BeginChangeCheck();
                    m_BoundsHandle.DrawHandle();
                    if (EditorGUI.EndChangeCheck())
                    {
                        FindObjectOfType<ModuleManager>().UpdatePipeline();
                        Undo.RecordObject(t, "Changed Bounds");
                    }
                    t.bounds.center = m_BoundsHandle.center;
                    t.bounds.size = m_BoundsHandle.size;
                }
            }*/
     
    }
}
#endif