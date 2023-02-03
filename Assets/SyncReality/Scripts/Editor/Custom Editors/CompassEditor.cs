using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(Compass))]
public class CompassEditor : Editor
{
    private Compass _compass;
    private DesignArea _designArea;
    private Tool lastTool = Tool.None;

    private void OnEnable()
    { 
        lastTool = Tools.current;
        Tools.current = Tool.None; 
        if (_compass == null)
            _compass = (Compass) target; 
        _compass.Highlight();
        
    }
    private void OnDisable()
    {
        Tools.current = lastTool;
        if (_compass == null)
            _compass = (Compass) target; 
        _compass.Delight();
    }

    private void OnSceneGUI()
    {
        if (_compass == null)
            _compass = (Compass) target;  
   // Handles.DrawWireCube(_compass._wallNumber1.transform.position, new Vector3(2, 4, 2));
   Backdrop backdrop = _compass.designArea.backdropper.GetCenterBackdrop();
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