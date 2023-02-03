using UnityEngine;
using System.Collections;
using UnityEditor;
//TODO: switch to ui_toolkit
namespace utils.GUI
{
#if UNITY_EDITOR
    [CustomEditor(typeof(GUI_MenuManager<System.Enum>))]
    public class GUI_MenuManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            GUI_MenuManager<System.Enum> myScript = (GUI_MenuManager<System.Enum>)target;
            if (GUILayout.Button("Set Exit Point"))
            {
                myScript.SetExitPoint();
            }
        }
    }
#endif
}

