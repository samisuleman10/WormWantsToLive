using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.IMGUI.Controls;
using UnityEditor;

using UnityEditor.PackageManager;

[CustomEditor(typeof(SurroundSync))]
public class SurroundSyncEditor : Editor 
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
            ((SurroundSync)target).CreateCombinedMesh();

        }


    }

    void OnSceneGUI()
    {
        DrawBoundsBox();
    }

    BoxBoundsHandle m_BoundsHandle = new BoxBoundsHandle();
    void DrawBoundsBox()
    {
        //Min Bounding Box
        SurroundSync t = target as SurroundSync;
        var tr = t.gameObject.transform;
        var pos = tr.position;

        Handles.color = Color.green;
        var rotatedMatrix = Handles.matrix
                                * Matrix4x4.TRS
                                (
                                 tr.position,
                                    tr.rotation, Vector3.one);
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
                t.bounds.center = m_BoundsHandle.center;
                t.bounds.size = new Vector3(Mathf.Max(m_BoundsHandle.size.x, 0.05f), Mathf.Max(m_BoundsHandle.size.y, 0.05f), Mathf.Max(m_BoundsHandle.size.z, 0.05f));
            }
        }
    }

}
#endif