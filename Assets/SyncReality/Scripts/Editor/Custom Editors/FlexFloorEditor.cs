#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

[CustomEditor(typeof(FlexFloor))]
public class FlexFloorEditor : Editor
{
    private FlexFloor _flexFloor;
    private DesignArea _designArea;
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
    protected virtual void OnSceneGUI()
    {
        _flexFloor = (FlexFloor) target;
        _designArea = _flexFloor.designArea;
        if (_designArea == null)
            Debug.LogWarning("DesignArea is null for FlexFloor Editor!");
     //   DrawInfoWindow();

    }
    private Rect _infoWindowRect;
    private ToolWindowSettings _toolWindowSettings;
    private void DrawInfoWindow()
    {
        if (_toolWindowSettings == null)
            _toolWindowSettings = FindObjectOfType<ToolWindowSettings>();
        int yValue = _toolWindowSettings.iconsPaddingY + _toolWindowSettings.iconsSize + 25;
        _infoWindowRect = new Rect(10, yValue, _designArea.infoWindowWidth, _designArea.infoWindowHeight);
        GUI.Window(0, _infoWindowRect, CreateInfoWindo, "Design Area");
        //     GUI.Window(0, _infoWindowRect, CreateInfoWindo, tex);
    }
    void CreateInfoWindo(int windowID)
    { 
     //   GUI.Label(new Rect(_designArea.infoWindowLeftSpacing, 5 + _designArea.infoWindowVerticalSpacing * 1, _designArea.infoWindowWidth-5, 20), "SyncLayout: " + _designArea.GetActiveSyncLayout()); 
       
         
        GUI.DragWindow(new Rect(0, 0, 1000, 40));
    }
     
}
#endif