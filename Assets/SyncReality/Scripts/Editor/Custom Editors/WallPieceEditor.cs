#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WallPiece))]
public class WallPieceEditor : Editor
{
  
    public override void OnInspectorGUI()
    {
        
        // wallpieces are not currently selectable anyway..
        /*
        WallPiece wallPiece = (WallPiece) target;
       

        if (wallPiece.anchoredWallMini == null)
        {
     
            DrawDefaultInspector();
            if ( GUILayout.Button("Create SurroundSync"))
                FindObjectOfType<SyncMaster>().CreateSurroundSyncForWallPiece(wallPiece, wallPiece.creationModel);
        }
        else
        { 
            if ( GUILayout.Button("Select SurroundSync"))
                FindObjectOfType<WallSplitter>().SelectWallMiniForWallPiece(wallPiece);
            if ( GUILayout.Button("Delete SurroundSync"))
                FindObjectOfType<WallSplitter>().DeleteSurroundSyncForWallPiece(wallPiece);
        }
        */
        
        
   
       
        
         
    }
}
#endif