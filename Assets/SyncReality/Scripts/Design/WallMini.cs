using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode][SelectionBase]
public class WallMini : MonoBehaviour
{
    public MeshFilter meshObject;
    public GameObject miniObject;
    [HideInInspector] public WallFace face;
    [HideInInspector] public int index;
    public SurroundSync mappedSurroundSync;

    private bool _prevBasePiece = false;
    private int _prevIndex = 0;
    private int _prevRepeatIndex = 0;
    private WallFace _prevFace;
    

   
    public void InsertSurroundSync(SurroundSync surroundSync)
    {
        mappedSurroundSync = surroundSync;
        _prevBasePiece = mappedSurroundSync.baseReplacer;
        _prevIndex = mappedSurroundSync.mapToIndex;
        _prevRepeatIndex = mappedSurroundSync.repeatForAmount;
        _prevFace = mappedSurroundSync.face;
        face = surroundSync.face;
        index = surroundSync.mapToIndex;
    }

   

    public void CheckForChanges()
    {
        if (mappedSurroundSync != null)
        {
            if (_prevBasePiece != false)
            {
                if (_prevBasePiece != mappedSurroundSync.baseReplacer || _prevIndex != mappedSurroundSync.mapToIndex || _prevFace != mappedSurroundSync.face || _prevRepeatIndex != mappedSurroundSync.repeatForAmount)
                { 
                    _prevBasePiece = mappedSurroundSync.baseReplacer;
                    _prevIndex = mappedSurroundSync.mapToIndex;
                    _prevFace = mappedSurroundSync.face;
                    _prevRepeatIndex = mappedSurroundSync.repeatForAmount;
                    face = mappedSurroundSync.face;
                    index = mappedSurroundSync.mapToIndex; 
                    FindObjectOfType<WallSplitter>().RedrawWallPieces(index, face);
                }
            }
      
        }
    }
}
#endif
