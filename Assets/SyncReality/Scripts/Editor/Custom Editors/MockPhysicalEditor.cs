#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MockPhysical))]  [CanEditMultipleObjects]
public class MockPhysicalEditor : Editor
{
   private MockPhysical _mockPhysical; 
   private LayoutArea _layoutArea; 
    private List<LayoutPoint> layoutPoints;
    private Vector3 lineOffset;
    private DesignAccess _designAccess;
    
    private MockPhysical _hoveredMock;
  //  private bool _hoveringMock; 
  

    private void OnEnable()
    { 
        if (_designAccess == null)
            _designAccess = FindObjectOfType<DesignAccess>();
        if (_layoutArea == null)
            _layoutArea = FindObjectOfType<LayoutArea>();
     
    }
    private void OnDisable()
    { 
       
    }
 
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }
    protected void OnSceneGUI()
    { 
        _mockPhysical = (MockPhysical) target; 
        if (_designAccess == null)
            _designAccess = FindObjectOfType<DesignAccess>();
      
        layoutPoints = new List<LayoutPoint>();
        foreach (var lp in _designAccess.GetStoredLayoutPoints()) 
            layoutPoints.Add(lp);
        if (layoutPoints.Count != 0)
        {
            lineOffset = new Vector3(0,
                (layoutPoints[0].GetComponent<MeshFilter>().sharedMesh.bounds.size.y *
                 layoutPoints[0].transform.localScale.y) / 2f, 0);

         
     //       DrawLayoutPointHandles();
            DrawLayoutPointLines();
      //      DrawLayoutPointRect();  
            if (_layoutArea.drawLayoutWalls)
                DrawLayoutPointHeightRects();
            InputLogic();
         //   ButtonLogic();
       //     if (_layoutArea._hasActiveMock)
              MockLogic();
              if (_layoutArea._mockMode == MockMode.Snap)
                  Tools.current = Tool.None;
              //     Tools.current = _layoutArea._mockMode == MockMode.Snap ? Tool.None : lastTool;

        }
    }

    Vector3 offsetToDirection(Transform transform, Vector3 direction, Vector3 extends)
    {
        //extends = transform.TransformDirection(extends);
        var projection = Vector3.Project(extends, direction);
        RaycastHit hit;
        if(Physics.Raycast(transform.position, direction, out hit, projection.magnitude))
        {
            return direction.normalized * (hit.distance - projection.magnitude);
        }
        return Vector3.zero;
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
            layoutPoint.transform.position = Handles.PositionHandle(position, Quaternion.identity);
            if (layoutPoint.transform.position != position && !Application.isPlaying)
                SceneView.RepaintAll();
        }
    }
    private void DrawLayoutPointLines()
    { 
        for (int i = 0; i < layoutPoints.Count; i++)
        {
            Handles.color = Color.cyan;
            if ( i != layoutPoints.Count-1)
                Handles.DrawLine(layoutPoints[i].transform.position - lineOffset, layoutPoints[i+1].transform.position - lineOffset, _layoutArea.layoutPointLineWidth);
            else
                Handles.DrawLine(layoutPoints[i].transform.position - lineOffset, layoutPoints[0].transform.position - lineOffset, _layoutArea.layoutPointLineWidth);
        }
 
       
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
    private LayoutPoint GetLayoutPointNearestToMouse()
    {
        LayoutPoint returnLayoutPoint = null;
        float minDist = Mathf.Infinity;
        foreach (var layoutPoint in layoutPoints)
        { 
     
            var distanceBetween = Vector3.Distance(GetMouseLocationOnFlatPlane(), layoutPoint.transform.position);

            if (distanceBetween < minDist)
            {
                minDist = distanceBetween;
                returnLayoutPoint = layoutPoint;
            }
        } 
        return returnLayoutPoint;
    }
    private void DrawLayoutPointRect()
    { 
        List<Vector3> polyPoints = new List<Vector3>();

        Handles.color = new Color(.5f, 0.5f, 0.9f, 0.35f);
  
        foreach (var layoutPoint in layoutPoints)
            polyPoints.Add(layoutPoint.transform.position - lineOffset); 
       
        if (_layoutArea._layoutMode == LayoutMode.LayoutEdit && GetMouseLocationOnFlatPlane() != Vector3.zero)
            if (_layoutArea.drawLayoutPointAddFloor)
               polyPoints.Insert( layoutPoints.IndexOf(GetLayoutPointNearestToMouse())+1 , GetMouseLocationOnFlatPlane()  );

   //     Handles.DrawAAConvexPolygon(polyPoints.ToArray());
    }
    
    private bool takingInput = true;
    private void InputLogic()
    {
        if (takingInput == true)
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0) 
                ClickInScene( );
            /*if (_layoutArea._layoutMode == LayoutMode.LayoutEdit)
            {
                if (Event.current is {isKey: true, keyCode: KeyCode.Escape})
                {
                    _layoutArea.ToggleModes();
                    Event.current.Use();
                }
                if (Event.current is {isKey: true, keyCode: KeyCode.X})
                {
                    DestroyImmediate(GetLayoutPointNearestToMouse().gameObject);
                    Event.current.Use();
                    EditorCoroutineUtility.StartCoroutine(   InputCooldownRoutine(), this);
                }
            }*/
            else // don't run in layoutmode that's for lp's
            {
               
            }

            /*if (Event.current.type == EventType.MouseMove)
            {
                if (_hoveringMock   )
                {
                    if (CheckForMockHover(_hoveredMock.GetComponent<MockPhysical>()) == false)
                        UnhoverMocks();
                }
                else
                { 
                    foreach (var mock in _layoutArea.GetMocksByTransform()) // todo better persistent data
                        if (CheckForMockHover(mock))
                        {
                            HoverMock(mock);
                            return;
                        }
                } 
            }*/
        } 
       
        
    }

    private void ActivateMock(MockPhysical mock)
    { 
        _layoutArea._hasActiveMock = true;
        _layoutArea._activeMock = mock ;
        _mockPhysical = mock;
        _layoutArea._activeMock.ActivateMock(); // cosmetic 
        UnhoverMocks();

    }
 
    private void HoverMock(MockPhysical mock)
    { 
        if (_layoutArea._hasActiveMock)
            if (_layoutArea._activeMock == mock)
                return;

            
        UnhoverMocks();
        _hoveredMock = mock;
        mock.ResetColor(true);
     //   _hoveringMock = true;
    }
    private void UnhoverMocks(bool forceAll = false)
    { 
     //   _hoveringMock = false;
       
            foreach (var mock in _layoutArea.GetMocksByTransform())
            {
                if (_layoutArea._hasActiveMock)
                {
                    if (_layoutArea._activeMock != mock) 
                        mock.ResetColor();
                }
                else
                    mock.ResetColor();
            }
    }
    private bool CheckForMockHover(MockPhysical mock)
    {
        bool returnBool = false;
        Vector3 mousePosition = Event.current.mousePosition;
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition); 
        RaycastHit hit;
        if (mock.GetComponent<BoxCollider>().Raycast(ray, out hit, Mathf.Infinity))
            returnBool = true; 
        return returnBool;
    }
    
    
    private void ButtonLogic()
    {
        float buttonHeight = 32;
        float buttonPadding = 185;
        float buttonY = buttonPadding;
 
        Rect GetRect(float y) => new Rect(buttonPadding, y, 80, buttonHeight);
        
        
 
        void AddBtn(string v, Action pressed)
        {
            if (GUI.Button(GetRect(buttonY), v))
                pressed();
            buttonY += buttonHeight + buttonPadding;
        }
        Handles.BeginGUI();
         
            GUI.backgroundColor = Color.cyan;
            string buttonstr = "" + _layoutArea._mockMode;
            if (_layoutArea._mockMode == MockMode.Spawn)
                buttonstr += ": R-click";
        AddBtn(buttonstr, ClickSceneButton); 
      
        Handles.EndGUI();
    }

    private void ClickSceneButton()
    {
        _layoutArea.ReceiveToggleSignal();
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
    
    private void MockLogic()
    {
        if (Selection.activeGameObject == _mockPhysical.gameObject)
        { 
            if (_layoutArea._mockMode == MockMode.Snap)
            {
                SnapMock();

            }
            else if (_layoutArea._mockMode == MockMode.Spawn)
                if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
                {
                    Event.current.Use();

                    Selection.activeGameObject = SpawnMock(GetMouseLocationOnFlatPlane(), Classification.Misc, new Vector3(2, 2, 2));
                }
        } 

    }

    private void SnapMock()
    { 

     
            RunWallSnap(_layoutArea._activeMock);
            if (_layoutArea.snapMockToFloor)
            {
                RunFloorSnap(_layoutArea._activeMock);
            }
            if  (_layoutArea.snapMockToMock)
            {
                RunMockSnap(_layoutArea._activeMock);
            }
     
    }

    private void RunWallSnap(MockPhysical mock)
    {


          if (Selection.activeGameObject == _mockPhysical.gameObject  )
       // if (Selection.activeGameObject == _mockPhysical.gameObject && Event.current.keyCode == KeyCode.B)
        {
            var ps = _layoutArea.GetLayoutPointsByTransform();
            List<Collider> wallCols = new List<Collider>();
            if(_layoutArea.snapMockToWall)
            {
                for (int i = 0; i < ps.Count; i++)
                {
                    var p1 = ps[i];
                    var p2 = ps[(i + 1) % ps.Count];

                    var bc = p1.gameObject.GetComponent<BoxCollider>() ? p1.gameObject.GetComponent<BoxCollider>() : p1.gameObject.AddComponent<BoxCollider>();

                    bc.size = new Vector3(Vector3.Distance(p1.transform.position, p2.transform.position), 10, 0.1f);

                    bc.center = new Vector3(Vector3.Distance(p1.transform.position, p2.transform.position) * 0.5f, 5, 0.05f);

                    p1.transform.rotation = Quaternion.Euler(0, Vector2.SignedAngle((p2.transform.position - p1.transform.position).flatten(), Vector2.up) - 90, 0);

                    wallCols.Add(bc);

                }
            }

            _mockPhysical.GetComponent<Collider>().enabled = false;
            var mouse = Event.current.mousePosition;
            mouse.y = Mathf.Abs(-mouse.y); 
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit))
            {
                var normalizedExtends = _mockPhysical.GetComponent<MeshRenderer>().bounds.extents;
                if (Vector3.Dot(normalizedExtends, hit.normal) < 0)
                    normalizedExtends = -normalizedExtends;
                _mockPhysical.transform.position = hit.point + Vector3.Project(normalizedExtends, hit.normal);
                //normalizedExtends = _mockPhysical.GetComponent<MeshRenderer>().bounds.extents;
                var offset = Vector3.zero;
                offset += offsetToDirection(_mockPhysical.transform, Vector3.up, normalizedExtends);
                offset += offsetToDirection(_mockPhysical.transform, Vector3.down, normalizedExtends);
                offset += offsetToDirection(_mockPhysical.transform, Vector3.left, normalizedExtends);
                offset += offsetToDirection(_mockPhysical.transform, Vector3.right, normalizedExtends);
                offset += offsetToDirection(_mockPhysical.transform, Vector3.forward, normalizedExtends);
                offset += offsetToDirection(_mockPhysical.transform, Vector3.back, normalizedExtends);
                _mockPhysical.transform.position += offset;
            }
            _mockPhysical.GetComponent<Collider>().enabled = true;

            int maxCount = 1000;
            while(wallCols.Count > 0 && maxCount > 0)
            {
                DestroyImmediate(wallCols[0]);
                wallCols.RemoveAt(0);
                maxCount--;
            }

            if (Event.current.type == EventType.MouseDown)
            {
               // Selection.activeGameObject = null; //Tchange
            }
        } else
        {
            _mockPhysical.GetComponent<Collider>().enabled = true;
        }
    }
    private void RunFloorSnap(MockPhysical mock)
    {
        
    }
    private void RunMockSnap(MockPhysical mock)
    {
        
    }
    
    
    
    

    private IEnumerator InputCooldownRoutine()
    {
        takingInput = false;
        yield return new EditorWaitForSeconds(0.4f);
        takingInput = true;
    }
    
    private void ClickInScene( )
    {
        /*if (_hoveringMock)
        {
            if (_layoutArea._hasActiveMock)
            {
                if (_layoutArea._activeMock == _hoveredMock.GetComponent<MockPhysical>()) 
                    Event.current.Use(); 
                else 
                    ActivateMock(_hoveredMock.GetComponent<MockPhysical>()); 
            }
            else 
                ActivateMock(_hoveredMock.GetComponent<MockPhysical>()); 
        }
        else if (_hoveringMock == false) 
        { 
            if (_layoutArea._hasActiveMock)
            { 
                //     DeactivateMock(); disable because snap? for now
            }
        }*/
    }
    private Vector3 GetMouseLocationOnFlatPlane()
    { 
        Vector3 returnVector = Vector3.zero;
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition); 
        RaycastHit hit;
        if (_layoutArea.flatPlane.GetComponent<MeshCollider>().Raycast(ray, out hit, Mathf.Infinity))
            returnVector = hit.point;
 
        return returnVector;
    }
  
  
   

    /*
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
        
        GUI.backgroundColor = Color.clear;
        if (_layoutArea._layoutMode == LayoutMode.LayoutEdit)
            GUI.backgroundColor = Color.magenta; 
        AddBtn("C: Edit Mode", ClickButtonLayoutEditMode);

        if (_layoutArea._layoutMode == LayoutMode.LayoutEdit)
        {
            GUI.backgroundColor = Color.clear;
            AddBtn("X: Delete LP", ClickButtonDeleteLayoutPoint);
        }

        Handles.EndGUI();
    }

    private void ClickButtonLayoutEditMode()
    {
     _layoutArea.ToggleModes();
    }
    private void ClickButtonDeleteLayoutPoint()
    {
         // no code here, the button is more of a text indicator.. makes sense
    }
    */
 
    
    /*
    
  
    
    private Vector2 GetMouseCoordinatesIn2D()
    {
        Vector3 returnVector = Vector3.zero;
        Vector3 mousePosition = Event.current.mousePosition; 
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
    private Vector2 ConvertInto2DCoordinates(Vector3 position)
    {
        Vector3 returnVector = Vector3.zero; 
        var transform = SceneView.currentDrawingSceneView.camera.transform; 
        Vector3 distanceFromCam = new Vector3(transform.position.x, transform.position.y, 0);
        Plane plane = new Plane(Vector3.forward, distanceFromCam);
        Ray ray = HandleUtility.GUIPointToWorldRay(position);
        if (plane.Raycast(ray, out var enter))
        { 
            Vector3 pos = ray.GetPoint(enter);
            returnVector.x = pos.x;
            returnVector.y = pos.y;
        }
        return returnVector;
    }
    */
 
}
#endif