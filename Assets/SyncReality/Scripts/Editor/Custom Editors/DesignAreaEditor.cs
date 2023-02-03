#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(DesignArea))]
public class DesignAreaEditor : Editor 
{
     
     private DesignArea _designArea; 
    

     
     
     protected void OnSceneGUI()
     {
          _designArea = (DesignArea) target;
         
          
        //  InputLogic();
     }
   
  

     private void ToggleBoundsEditMode(bool enabled)
     {
          // show some handles
     }
     
   
     private void InputLogic()
     {
          Vector2 mouseLocation = Vector2.zero;

          if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
          {
          }
          if (Event.current.type == EventType.MouseMove  )
          {
          }
     }
     
     public override void OnInspectorGUI()
     {
          
          DesignArea designArea = (DesignArea) target;
      
          /*if ( GUILayout.Button("Initialize Design Area"))
               designArea.InitializeDesignArea();
          if ( GUILayout.Button("Reset Design Area"))
               designArea.ResetDesignArea();*/
          //    if ( GUILayout.Button("Query Statuses"))
          //          designArea.QueryStatuses();
          DrawDefaultInspector();
          if ( GUILayout.Button("Reset Dimensions"))
               designArea.ResetDimensions();
         
     }
     
     /*private void ButtonLogic()
     {
          float btnHeight = 50;
          float btnPadding = 10;
          float btnY = btnPadding;
          Rect GetRect(float y) => new Rect(btnPadding, y, 100, 50);
          void AddButton(string v, Action pressed)
          {
               if (GUI.Button(GetRect(btnY), v))
                    pressed();
               btnY += (btnHeight + btnPadding);
          }
          Handles.BeginGUI();
          GUI.backgroundColor = Color.cyan;
          //  AddButton(buttonText, () => ToggleBoundsEditMode(_designArea.boundsEditMode)); 
          Handles.EndGUI();
     }*/
}
#endif