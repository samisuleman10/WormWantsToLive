using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(WallNumber))]
public class WallNumberEditor : Editor
{
    private WallNumber _wallNumber;
    private DesignArea _designArea;
    private Tool lastTool = Tool.None;

    private void OnEnable()
    { 
        lastTool = Tools.current;
        Tools.current = Tool.None;

        if (_wallNumber == null)
            _wallNumber = (WallNumber) target;
        _wallNumber.SelectWallNumber();
    }
    private void OnDisable()
    {
        Tools.current = lastTool;
        if (_wallNumber == null)
            _wallNumber = (WallNumber) target;
        _wallNumber.DelightWallNumber();
    }

    private void OnSceneGUI()
    {
        if (_wallNumber == null)
           _wallNumber = (WallNumber) target;
        if (_designArea == null)
            _designArea = FindObjectOfType<DesignArea>();
        Backdrop backdrop = _designArea.GetBackdropForWallFace(_wallNumber.compass.GetWallFaceForNumber(_wallNumber.GetWallNumberInt()));
        if (backdrop != null)
            if (backdrop.anchored)
                if (backdrop.backdropSpawnedAsset != null)
                {
                    var position = backdrop.backdropSpawnedAsset.transform.position;
                    EditorGUI.BeginChangeCheck();
                    Vector3 originalPos = Handles.PositionHandle(position, Quaternion.identity);

                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RegisterCompleteObjectUndo(backdrop.backdropSpawnedAsset.transform, "Move Backdrop Spawn"); 
                        backdrop.backdropSpawnedAsset.transform.position = originalPos;
                    }
                    if (backdrop.backdropSpawnedAsset.transform.position != position && !Application.isPlaying)
                        SceneView.RepaintAll();
                }
    }
}
#endif