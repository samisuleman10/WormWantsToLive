#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine; 
using Unity.EditorCoroutines.Editor;

[CustomEditor(typeof(LayoutArea))] 
public class LayoutAreaEditor : Editor
{
    private LayoutArea _layoutArea;
    private Tool _lastUsedTool = Tool.None;
    private List<LayoutPoint> layoutPoints;
    private Vector3 lineOffset;
    private DesignAccess _designAccess;

    // Caching system
    private bool takingInput = true;
    private Vector2 _cachedMousePositionFromGui;
    private Ray _cachedRayFromGui;
    private float _totalWallDistance;
    
    private DesignArea _designArea;
    private Rect _infoWindowRect;
    private ToolWindowSettings _toolWindowSettings;
    private int _lineCount;
    
    private void OnEnable()
    {
        ToolShortcutManager.StartListening(OnDeletePressedInputLogic, context: "SceneView", KeyCode.B);
        // Disables transform handle so as to only show those for LayoutPoints
        _lastUsedTool = Tools.current;
        Tools.current = Tool.None;
        _designAccess = FindObjectOfType<DesignAccess>();
    }

    private void OnDisable()
    {
        Tools.current = _lastUsedTool;
    }
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Rotate Mocks to Room Center"))
        {
            ((LayoutArea)target).rotateAllMocksToCenter();

        }
        DrawDefaultInspector();


    }


    
    protected void OnSceneGUI()
    {

        // Store mouse position and GUIPointToWordRay result
        if(Event.current.type.Equals(EventType.KeyDown))
        {
            _cachedMousePositionFromGui = Event.current.mousePosition;
            _cachedRayFromGui = HandleUtility.GUIPointToWorldRay(this._cachedMousePositionFromGui);
        }

        _layoutArea = (LayoutArea) target;
        if (_designAccess == null)
            _designAccess = FindObjectOfType<DesignAccess>();
      
        layoutPoints = new List<LayoutPoint>(); 
        foreach (var lp in _designAccess.GetStoredLayoutPoints())
            if (lp != null)
                layoutPoints.Add(lp);
        if (layoutPoints.Count != 0)
        {
            lineOffset = new Vector3(0,
                (layoutPoints[0].GetComponent<MeshFilter>().sharedMesh.bounds.size.y *
                 layoutPoints[0].transform.localScale.y) / 2f, 0);

            if (_layoutArea._layoutMode == LayoutMode.LayoutEdit)
                if (GetMouseLocationOnFlatPlane() != Vector3.zero == true)
                    DrawFloatingLayoutPoint();
            if (_layoutArea._layoutMode != LayoutMode.LayoutEdit)
               DrawLayoutPointHandles();
            
            if (_layoutArea.drawDistanceLabels)     
                DrawDistanceLabels();
            DrawInfoWindow();

            DrawLayoutPointLines();
         if(_layoutArea.drawLayoutFloor)
            DrawLayoutPointRect();
            if (_layoutArea.drawLayoutWalls)
                DrawLayoutPointHeightRects();
            InputLogic();
            //ButtonLogic();
       
        }
    
    }

    
    private void DrawDistanceLabels()
    {
       // one for each line: distance between its two points
       _totalWallDistance = 0;
       GUIStyle style = new GUIStyle();
       style.fontSize = _layoutArea.distanceLabelFontSize;
       for (int i = 0; i < layoutPoints.Count; i++)
       {
           Handles.color = Color.cyan;
           if (i != layoutPoints.Count - 1)
           {
               Vector3 labelPos = layoutPoints[i].transform.position + (layoutPoints[i+1].transform.position  - layoutPoints[i].transform.position) / 2f;
               labelPos += new Vector3(0, _layoutArea.distanceLabelHeight, 0);
               float distanceMeters =
                   Mathf.Abs(Vector3.Distance(layoutPoints[i + 1].transform.position, layoutPoints[i].transform.position)) * _layoutArea.metersPerUnit;
               Handles.Label(labelPos, distanceMeters.ToString("F1") +"m", style);
               _totalWallDistance += distanceMeters;
           }
           else
           {
               Vector3 labelPos = layoutPoints[i].transform.position + (layoutPoints[0].transform.position  - layoutPoints[i].transform.position) / 2f;
               labelPos += new Vector3(0, _layoutArea.distanceLabelHeight, 0);
               float distanceMeters =
                   Mathf.Abs(Vector3.Distance(layoutPoints[0].transform.position, layoutPoints[i].transform.position)) * _layoutArea.metersPerUnit;
               Handles.Label(labelPos, distanceMeters.ToString("F1") +"m", style);   
               _totalWallDistance += distanceMeters;
           }
            
       }
       
    }


    private void DrawInfoWindow()
    {
        if (_toolWindowSettings == null)
            _toolWindowSettings = FindObjectOfType<ToolWindowSettings>();
        if (_designArea == null)
            _designArea = FindObjectOfType<DesignArea>();
        int yValue = _toolWindowSettings.iconsPaddingY + _toolWindowSettings.iconsSize + 25; 
        _infoWindowRect = new Rect(10, yValue, _layoutArea.infoWindowWidth, _layoutArea.infoWindowHeight);
       GUI.Window(0, _infoWindowRect, CreateInfoWindo, "Layout Area");
   //     GUI.Window(0, _infoWindowRect, CreateInfoWindo, tex);
    }

    
    void CreateInfoWindo(int windowID)
    {
        _lineCount = 0;
        DrawLinedLabel( "Plane Size: " + _layoutArea.GetUnitsPerXSide() + "x" + _layoutArea.GetUnitsPerZSide() ); 
        DrawLinedLabel( "Plane Adjusted Size (X): " + _layoutArea.GetUnitsPerXSide() * _layoutArea.metersPerUnit +" Units"); 
        DrawLinedLabel( "Plane Surface Area: " + (_layoutArea.GetUnitsPerXSide() * _layoutArea.metersPerUnit)  * (_layoutArea.GetUnitsPerZSide() * _layoutArea.metersPerUnit) +" sq m"  ); 
        DrawLinedLabel( "Meters / Unit: " + _layoutArea.metersPerUnit ); 
        DrawLinedLabel( "RoomLayout: " + _layoutArea.GetActiveRoomLayout()); 
        DrawLinedLabel( "LayoutPoints: " + _layoutArea.GetLayoutPointsByTransform().Count ); 
        DrawLinedLabel( "Total Wall Distance: " + _totalWallDistance.ToString("F1") + " m" ); 
     //   if (GUI.Button(new Rect(_layoutArea.infoWindowLeftSpacing, 5 + _layoutArea.infoWindowVerticalSpacing * 3 + 5, 140, 20), "Refit LayoutPoints"))
        
           
    //    GUI.DragWindow(new Rect(0, 0, 1000, 40)); does not work for this setup
    }
 
    
    private void DrawLinedLabel(string labelText)
    {
        _lineCount++;
        GUI.Label(new Rect(_layoutArea.infoWindowLeftSpacing, 5 + _layoutArea.infoWindowVerticalSpacing * _lineCount, _layoutArea.infoWindowWidth-5, 20), labelText );
    }

    private void DrawFloatingLayoutPoint()
    {
        Handles.color = new Color(.3f, 0.5f, 0.9f, 0.35f);
        if (Event.current.type == EventType.Repaint)
            Handles.CylinderHandleCap(4444,GetMouseLocationOnFlatPlane(), Quaternion.LookRotation(Vector3.up), _layoutArea.placementLayoutPointSize, EventType.Repaint);
        if (Event.current.type == EventType.Layout)
            Handles.CylinderHandleCap(4444, GetMouseLocationOnFlatPlane(), Quaternion.LookRotation(Vector3.up), _layoutArea.placementLayoutPointSize, EventType.Layout);
    }


    private void DrawLayoutPointHandles()
    { 
        foreach (var layoutPoint in layoutPoints)
        {
            var position = layoutPoint.transform.position;
            EditorGUI.BeginChangeCheck();
            Vector3 originalPos = Handles.PositionHandle(position, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(layoutPoint.transform, "Move LayoutPoint"); 
                layoutPoint.transform.position = originalPos;
            }
            if (layoutPoint.transform.position != position && !Application.isPlaying)
                SceneView.RepaintAll();
        }
    }
    private void DrawLayoutPointLines()
    { 
        for (int i = 0; i < layoutPoints.Count; i++)
        {
            Handles.color = Color.cyan;
            if (i != layoutPoints.Count - 1) 
                Handles.DrawLine(layoutPoints[i].transform.position - lineOffset, layoutPoints[i+1].transform.position - lineOffset, _layoutArea.layoutPointLineWidth); 
            else 
                Handles.DrawLine(layoutPoints[i].transform.position - lineOffset, layoutPoints[0].transform.position - lineOffset, _layoutArea.layoutPointLineWidth); 
        }

        if (_layoutArea._layoutMode == LayoutMode.LayoutEdit && GetMouseLocationOnFlatPlane() != Vector3.zero)
        { 
            Handles.DrawLine(GetLayoutLineNearestToMouse().layoutPoint1.transform.position - lineOffset,  GetMouseLocationOnFlatPlane() , _layoutArea.layoutPointLineWidth * 0.75f);
            Handles.DrawLine(GetLayoutLineNearestToMouse().layoutPoint2.transform.position - lineOffset,  GetMouseLocationOnFlatPlane() , _layoutArea.layoutPointLineWidth * 0.75f);
       
            SceneView.RepaintAll();
        }
    }

    private (LayoutPoint layoutPoint1, LayoutPoint layoutPoint2) GetLayoutLineNearestToMouse()
    {
        LayoutPoint returnLayoutPoint1 = null;
        LayoutPoint returnLayoutPoint2 = null;
        float minDist = Mathf.Infinity;
        for (int i = 0; i < layoutPoints.Count; i++)
        {
            var firstLayoutPoint = layoutPoints[i];
            var secondLayoutPoint = layoutPoints[(i + 1) % layoutPoints.Count];
            var distanceBetween = HandleUtility.DistancePointLine(GetMouseLocationOnFlatPlane(), firstLayoutPoint.transform.position, secondLayoutPoint.transform.position);


            if (distanceBetween < minDist)
            {
                minDist = distanceBetween;
                returnLayoutPoint1 = firstLayoutPoint;
                returnLayoutPoint2 = secondLayoutPoint;
            }
            if (distanceBetween == minDist && Vector2.Angle(
                (secondLayoutPoint.transform.position - firstLayoutPoint.transform.position).flatten(), 
                (firstLayoutPoint.transform.position - GetMouseLocationOnFlatPlane()).flatten()) > 45)
            {
                returnLayoutPoint1 = firstLayoutPoint;
                returnLayoutPoint2 = secondLayoutPoint;
            } 
        }

        return (returnLayoutPoint1, returnLayoutPoint2);
    }

    private LayoutPoint GetLayoutPointNearestToMouse(bool useStoredRay = false)
    {
        LayoutPoint returnLayoutPoint1 = null;
        float minDist = Mathf.Infinity;
        for (int i = 0; i < layoutPoints.Count; i++)
        {
            var firstLayoutPoint = layoutPoints[i];
            var distanceBetween = Vector3.Distance(GetMouseLocationOnFlatPlane(useStoredRay), firstLayoutPoint.transform.position);


            if (distanceBetween < minDist)
            {
                minDist = distanceBetween;
                returnLayoutPoint1 = firstLayoutPoint;
            }
        } 
        return returnLayoutPoint1;
    }


    private void DrawLayoutPointRect()
    {
        // create list of points
        List<Vector3> polyPoints = new List<Vector3>();

        Handles.color = new Color(.5f, 0.5f, 0.9f, 0.35f);
  
        foreach (var layoutPoint in layoutPoints)
            polyPoints.Add(layoutPoint.transform.position - lineOffset);

       
        if (_layoutArea._layoutMode == LayoutMode.LayoutEdit && GetMouseLocationOnFlatPlane() != Vector3.zero)
            if (_layoutArea.drawLayoutPointAddFloor)
               polyPoints.Insert( layoutPoints.IndexOf(GetLayoutLineNearestToMouse().layoutPoint1)+1 , GetMouseLocationOnFlatPlane()  );

        Handles.DrawAAConvexPolygon(polyPoints.ToArray());
    }

    private void DrawLayoutPointHeightRects()
    {
        List<Vector3> polyPoints = new List<Vector3>();

        Handles.color = new Color(.6f, 0.4f, 0.8f, 0.25f);

        foreach (var layoutPoint in layoutPoints)
        {
            
            polyPoints = new List<Vector3>();
            if (layoutPoints.IndexOf(layoutPoint) < layoutPoints.Count-1)
            {
                polyPoints.Add(layoutPoint.transform.position  -  lineOffset);
                polyPoints.Add(layoutPoints[(layoutPoints.IndexOf(layoutPoint)+1)].transform.position  -  lineOffset); 
                polyPoints.Add(layoutPoints[(layoutPoints.IndexOf(layoutPoint)+1)].transform.position+ new Vector3(0, _layoutArea.layoutWallsHeight , 0));
                polyPoints.Add(layoutPoint.transform.position   + new Vector3(0, _layoutArea.layoutWallsHeight , 0));
                Handles.DrawAAConvexPolygon(polyPoints.ToArray());
            }
            else if (layoutPoints.IndexOf(layoutPoint) == layoutPoints.Count - 1)
            {
                polyPoints.Add(layoutPoint.transform.position  -  lineOffset);
                polyPoints.Add(layoutPoints[0].transform.position  -  lineOffset); 
                polyPoints.Add(layoutPoints[0].transform.position  + new Vector3(0, _layoutArea.layoutWallsHeight, 0));
                polyPoints.Add(layoutPoint.transform.position + new Vector3(0, _layoutArea.layoutWallsHeight, 0));
                Handles.DrawAAConvexPolygon(polyPoints.ToArray());
            }
        } 
    }
    
    private void InputLogic()
    {
        if (takingInput == true)
        { 
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0) 
                ClickInScene( );
            if (_layoutArea._layoutMode == LayoutMode.LayoutEdit)
            {
                if (Event.current is {isKey: true, keyCode: KeyCode.Escape})
                {
                    _layoutArea.ReceiveToggleSignal();
                    Event.current.Use();
                }
                // if (Event.current is {isKey: true, keyCode: KeyCode.X})
                // {
                //    Undo.DestroyObjectImmediate(GetLayoutPointNearestToMouse().gameObject);
                    
                //     Event.current.Use();
                //     EditorCoroutineUtility.StartCoroutine(   InputCooldownRoutine(), this);
                // }

            }
        } 
        // if (_layoutArea._layoutMode == LayoutMode.LayoutEdit)
        //     if (Event.current is {isKey: true, keyCode: KeyCode.X})
        //         Event.current.Use();  
    }

    private void OnDeletePressedInputLogic(CustomShortcutArguments args)
    {
        
        if (takingInput == true && _layoutArea._layoutMode == LayoutMode.LayoutEdit)
        {
            Undo.DestroyObjectImmediate(GetLayoutPointNearestToMouse(true).gameObject);
                    
            Event.current.Use();
            EditorCoroutineUtility.StartCoroutine(   InputCooldownRoutine(), this);
        }
        if (_layoutArea._layoutMode == LayoutMode.LayoutEdit)
        {
            Event.current.Use(); 
        }                
    }

    private IEnumerator InputCooldownRoutine()
    {
        takingInput = false;
        yield return new EditorWaitForSeconds(0.4f);
        takingInput = true;
    }
    
    
    private void ClickInScene( )
    {
        if (_layoutArea._layoutMode == LayoutMode.LayoutEdit && GetMouseLocationOnFlatPlane() != Vector3.zero)
        {
            Vector3 clickedPoint = GetMouseLocationOnFlatPlane();
             _layoutArea.SpawnLayoutPointAtLocation(new Vector3 (clickedPoint.x, 0, clickedPoint.z), layoutPoints.IndexOf(GetLayoutLineNearestToMouse().layoutPoint1));
        }
    }

    private void ButtonLogic()
    {
        float buttonHeight = 32;
        float buttonPadding = 15;
        float buttonY = buttonPadding;
 
        Rect GetRect(float y) => new Rect(buttonPadding, y, 120, buttonHeight);
 
        void AddBtn(string v, Action pressed)
        {
            if (GUI.Button(GetRect(buttonY), v))
                pressed();
            buttonY += buttonHeight + buttonPadding;
        }
        Handles.BeginGUI();
        
        GUI.backgroundColor = Color.cyan;
        string str = "";
        if (_layoutArea._layoutMode == LayoutMode.LayoutEdit)
            str = "Add LPs";
        else
            str = "Move LPs";
        AddBtn(str, ClickButtonLayoutEditMode);

        if (_layoutArea._layoutMode == LayoutMode.LayoutEdit)
        {
            GUI.backgroundColor = Color.clear;
            AddBtn("X: Delete LP", ClickButtonDeleteLayoutPoint);
        }
        
        Handles.EndGUI();
    }
    private void SpawnMockFromButton()
    {
        GameObject obj = SpawnMock(_layoutArea.GetAreaCenter(), Classification.Misc, new Vector3(2, 2, 2));
        Selection.activeGameObject = obj;
    }
    private GameObject SpawnMock(Vector3 position, Classification classification, Vector3 scale)
    {
        // adjust height by half y scale
        GameObject cubeObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cubeObj.transform.SetParent(_layoutArea.mockPhysicalsParent.transform);
        cubeObj.transform.position = position;
        cubeObj.transform.localScale = scale;
        cubeObj.transform.position += new Vector3(0,scale.y/2f, 0);
        cubeObj.AddComponent<MockPhysical>(); 
        cubeObj.GetComponent<MockPhysical>().Classification = classification; 
        cubeObj.GetComponent<MockPhysical>().Initialize(); 
        cubeObj.name = "MOCK" + scale.x +  classification;
        return cubeObj;
    }
    private void ClickButtonLayoutEditMode()
    {
     _layoutArea.ReceiveToggleSignal();
    }
    private void ClickButtonDeleteLayoutPoint()
    {
         // no code here, the button is more of a text indicator.. makes sense
    }
 
    
    #region CoordinateFunctions
  
    
    private Vector2 GetMouseCoordinatesIn2D()
    {
        Vector3 returnVector = Vector3.zero;
        Vector3 mousePosition = Event.current.mousePosition;
        /*mousePosition.y = SceneView.currentDrawingSceneView.camera.pixelHeight - mousePosition.y;
        mousePosition = SceneView.currentDrawingSceneView.camera.ScreenToWorldPoint(mousePosition);
        mousePosition.y = -mousePosition.y;*/
        var transform = SceneView.currentDrawingSceneView.camera.transform;
        
        Vector3 distanceFromCam = new Vector3(transform.position.x, transform.position.y, 0);
        Plane plane = new Plane(Vector3.forward, distanceFromCam);
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
        if (plane.Raycast(ray, out var enter))
        { 
            Vector3 pos = ray.GetPoint(enter);
            returnVector.x = pos.x;
            returnVector.y = pos.y;
        }
        return returnVector;
    }
    
    private Vector3 GetMouseLocationOnFlatPlane(bool useStoredRay = false)
    { 
        Vector3 returnVector = Vector3.zero;

        Ray ray;
        if (useStoredRay)
            ray = _cachedRayFromGui;
        else
            ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        RaycastHit hit;
        if (_layoutArea.flatPlane.GetComponent<MeshCollider>().Raycast(ray, out hit, Mathf.Infinity))
            returnVector = hit.point;

        return returnVector;
    }

    #endregion



}
#endif