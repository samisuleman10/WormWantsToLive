using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.XRTools.Utils;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif
using UnityEngine;
using UnityEngine.Animations;
using Random = UnityEngine.Random;


public enum LayoutMode
{
    Default,
    LayoutEdit 
}
public enum MockMode
{
    Default,
    Spawn,
    Snap
}
// Space where scans can be visualized, mocked, edited, saved and loaded. 
[SelectionBase][ExecuteInEditMode]
public class LayoutArea : MonoBehaviour
{
    [Tooltip("")]

    
    [Header("Assigns")] 
    public DesignAccess designAccess; 
    public Transform mockPhysicalsParent;
    public GameObject layoutPointPrefab;  
    public Transform flatPlane;
    public Transform overCameraRigTransform;
    [Header("Settings")]
    [Tooltip("Size of the room when you click Reset Walls, relative to the flat plane")]
    public float defaultDimensionScale = 0.4f;
    [Tooltip("Show distance labels for each wall")]
    public bool drawDistanceLabels = true; 
    [Tooltip("Font size for distance labels")]
    public int distanceLabelFontSize = 14;
    [Tooltip("Y position of distance labels")]
    public float distanceLabelHeight = 0;
    [Tooltip("Draw a transparent floor between LayoutPoints")]
    public bool drawLayoutFloor = true;
    [Tooltip("Draw a transparent floor between existing and placeable LayoutPoints")]
    public bool drawLayoutPointAddFloor = false;
    [Tooltip("Draw transparent walls between LayoutPoints")]
    public bool drawLayoutWalls = true; 
    [Tooltip("Height of drawn walls")]
    public float layoutWallsHeight = 2f;
    [Tooltip("Width of drawn lines")]
    public float layoutPointLineWidth = 0.2f;
    [Tooltip("Visual size of LayoutPoint object when adding points")]
    public float placementLayoutPointSize = 0.8f;
    public bool snapMockToFloor = true;
    public bool snapMockToMock = true;
    public bool snapMockToWall = false; // todo don't think this works yet
    public Material mockDefaultMaterial;
    public Material mockMiscMaterial;
    public Material mockTableMaterial;
    public Material mockSeatMaterial;
    public Material mockDoorMaterial;
    public Material mockWindowMaterial;
    public int infoWindowWidth;
    public int infoWindowHeight;
    public int infoWindowLeftSpacing = 5;
    public int infoWindowVerticalSpacing = 15;
    [Tooltip("Set camera to LayoutArea on clicking Reset/Load")]
    public bool focusCameraAfterInteraction; 
    public string roomLayoutsPath;
    public string roomChartsPath;
    public string recentRoomLayoutsPath;
    
    
    private Dictionary<Classification, Material> classificationMaterials;
    private List<LayoutPoint> _tempStoredLayoutPoints = new List<LayoutPoint>();  
    private List<MockPhysical> _tempStoredMocks = new List<MockPhysical>();
    private float tempMetersPerUnit;
    
  [HideInInspector]  public LayoutMode _layoutMode;
  [HideInInspector]  public MockMode _mockMode; 
  [HideInInspector] public bool _hasActiveMock;
  [HideInInspector] public MockPhysical _activeMock;
  [HideInInspector] public string activeRoomLayout = "-"; 
  [HideInInspector] public float metersPerUnit = 1;
  [HideInInspector] public bool wallsHaveJustBeenReset = false;
  [HideInInspector] public GameObject baseMock;

    public void rotateAllMocksToCenter()
    {
        var surrounder = FindObjectOfType<SurrounderModule>();
        var m =
                (surrounder.getGroundBox()[0] +
                surrounder.getGroundBox()[1] +
                surrounder.getGroundBox()[2] +
                surrounder.getGroundBox()[3]) / 4f;
        ;
        foreach (var mock in FindObjectsOfType<MockPhysical>(true))
        {
            
            mock.flipMockSizeCorrectly(m);

        }
    }

#if UNITY_EDITOR
    private ToolWindowSettings _toolWindowSettings;

    public void RefitLayoutPoints()
    { 
        // take each LP, and adjust its positioning to LA MPU settings
        if (metersPerUnit > 0)
        {
            foreach (var lp in GetLayoutPointsByTransform())
                lp.transform.localPosition = new Vector3(lp.absolutePosition.x / metersPerUnit,  
                  //  layoutPointPrefab.GetComponent<MeshFilter>().sharedMesh.bounds.extents.y *  layoutPointPrefab.transform.localScale.y/2f, 
                  0,
                    lp.absolutePosition.z / metersPerUnit);

            var localScale = flatPlane.transform.localScale; 
          float xMin = localScale.x  * transform.localScale.x * -5f+ flatPlane.transform.position.x; 
          float xMax = localScale.x * transform.localScale.x * 5f  + flatPlane.transform.position.x;
          float zMin = localScale.z * transform.localScale.z * -5f+ flatPlane.transform.position.z;
          float zMax = localScale.z * transform.localScale.z *  5f + flatPlane.transform.position.z;

          float lowestX = 99999f;
          float highestX = -99999f; 
          float lowestZ = 99999f;
          float highestZ = -99999f; 
          foreach (var lp in GetLayoutPointsByTransform())
          {
              if (lp.absolutePosition.x < lowestX)
                  lowestX = lp.transform.position.x;
              if (lp.transform.position.x > highestX)
                  highestX = lp.transform.position.x;
              if (lp.transform.position.z < lowestZ)
                  lowestZ = lp.transform.position.z;
              if (lp.transform.position.z > highestZ)
                  highestZ = lp.transform.position.z;
          }

          float planeDifX = xMax - xMin;
          float planeDifZ = zMax - zMin;
          float lpDifX = highestX - lowestX;
          float lpDifZ = highestZ - lowestZ;

          float xAddition = 0;
          float zAddition = 0;
          if (lpDifX > planeDifX  &&  lpDifZ > planeDifZ)
          {
            //  Debug.LogWarning("Layout exceeds plane size!");
            float xSurplus =  lpDifX - planeDifX;
            float zSurplus =  lpDifZ - planeDifZ;
            
            float newLowXPos = xMin - xSurplus / 2f;
            float newLowZPos = zMin - zSurplus / 2f;
            
            if (lowestX <= newLowXPos)
                xAddition = newLowXPos - lowestX;
            else 
                xAddition =  lowestX - newLowXPos;
              
            if (lowestZ <= newLowZPos)
                zAddition = newLowZPos - lowestZ;
            else 
                zAddition =  lowestZ - newLowZPos;
            
            Vector3 repositionVector = new Vector3(xAddition, 0, zAddition);
            foreach (var lp in GetLayoutPointsByTransform())
                lp.transform.position -= repositionVector;
          }
          else if (   lpDifX > planeDifX)
          {
              float xSurplus =  lpDifX - planeDifX;
              float newLowXPos = xMin - xSurplus / 2f;
            
              if (lowestX <= newLowXPos)
                  xAddition = newLowXPos - lowestX;
              else 
                  xAddition =  lowestX - newLowXPos;

              Vector3 repositionVector = new Vector3(xAddition, 0, 0);
              foreach (var lp in GetLayoutPointsByTransform())
                  lp.transform.position -= repositionVector;
          }
          else if (   lpDifZ > planeDifZ)
          {
              float zSurplus =  lpDifZ - planeDifZ;
            
              float newLowZPos = zMin - zSurplus / 2f;

              if (lowestZ <= newLowZPos)
                  zAddition = newLowZPos - lowestZ;
              else 
                  zAddition =  lowestZ - newLowZPos;
            
              Vector3 repositionVector = new Vector3(0, 0, zAddition);
              foreach (var lp in GetLayoutPointsByTransform())
                  lp.transform.position -= repositionVector;
          }
          else
          {
              float xRemnant = planeDifX - lpDifX;
              float zRemnant = planeDifZ - lpDifZ;
        
              float newLowXPos = xMin + xRemnant / 2f;
              float newLowZPos = zMin + zRemnant / 2f;

              if (lowestX <= newLowXPos)
                  xAddition = newLowXPos - lowestX;
              else 
                  xAddition =  lowestX - newLowXPos;
              
              if (lowestZ <= newLowZPos)
                  zAddition = newLowZPos - lowestZ;
              else 
                  zAddition =  lowestZ - newLowZPos;
              
              Vector3 repositionVector = new Vector3(xAddition, 0, zAddition);
              foreach (var lp in GetLayoutPointsByTransform())
                  lp.transform.position -= repositionVector;
          }
        }
    }

    
    public float GetUnitsPerXSide()
    {
        return 10 * flatPlane.localScale.x;
    }
    public float GetUnitsPerZSide()
    {
        return 10 * flatPlane.localScale.z;
    }
    public string GetActiveRoomLayout()
    {
        if (activeRoomLayout == "")
            return "-";
        return activeRoomLayout;
    }

    public void CreateRectangleShape(Vector2 shapeVector)
    {
        float unitWidth = shapeVector.x ;
        float unitHeight = shapeVector.y ;
        if (unitWidth == 0 || unitHeight == 0)
        {
            Debug.LogWarning("Please enter a value > 0 for both x and y!");
            return;
        }
        DeleteLayoutPoints();
        SpawnLayoutPointAtLocation(new Vector3 (unitWidth,0,0), 1);
        SpawnLayoutPointAtLocation(new Vector3 (0,0,0), 2);
        SpawnLayoutPointAtLocation(new Vector3 (0,0,unitHeight), 3);
        SpawnLayoutPointAtLocation(new Vector3 (unitWidth,0,unitHeight), 4);
        RefitLayoutPoints();
    }

   
    
  private void SendPipelineUpdateSignal()
  {
      if (_toolWindowSettings == null)
          _toolWindowSettings = FindObjectOfType<ToolWindowSettings>();
      _toolWindowSettings.SendPipelineUpdateSignal();
  }

    
    public void UpdateFromToolWindow()
    {
        CheckForChanges();
    }
#endif
    private void InitializeMockMaterialDictionary()
  { 
      classificationMaterials = new Dictionary<Classification, Material>();
      classificationMaterials.Add(Classification.Misc, mockMiscMaterial); 
      classificationMaterials.Add(Classification.Table, mockTableMaterial); 
      classificationMaterials.Add(Classification.Closet, mockSeatMaterial); 
      
  }

#if UNITY_EDITOR
    private void Update()
    {
   //     transform.position += Vector3.zero;
//        CheckForChanges();
      
    }

    public void ReceiveToggleSignal()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected != null)
        {
            if (selected.GetComponent<MockPhysical>() != null)
                ToggleMockMode();
            if (selected.GetComponent<LayoutArea>() != null)
                ToggleLayoutPointMode();
        } 
    }
    private void ToggleLayoutPointMode()
    {
        if (_layoutMode == LayoutMode.LayoutEdit)
            _layoutMode = LayoutMode.Default;
        else  if (_layoutMode == LayoutMode.Default)
            _layoutMode = LayoutMode.LayoutEdit;
    }
    
 
    private void ToggleMockMode()
    { 
        /*if (_mockMode == MockMode.Default)
            _mockMode = MockMode.Spawn    ;
        else  if (_mockMode == MockMode.Spawn)
            _mockMode = MockMode.Snap;
        else  if (_mockMode == MockMode.Snap)
            _mockMode = MockMode.Default;*/
        if (_mockMode == MockMode.Default)
            _mockMode = MockMode.Snap;
        else  if (_mockMode == MockMode.Snap)
            _mockMode = MockMode.Default;
   
    }
#endif
    #region OnGUISource






    #endregion


    #region ButtonSource


#if UNITY_EDITOR
    public void ResetLayout()
   {
       DeleteLayoutPoints();
       SpawnDefaultLayoutPoints();
   }
   public void ResetMocks()
   {
       DeleteMocks();
       SpawnDefaultMocks(); 
    }
#endif
    #endregion
       
    public void SpawnLayoutPointAtLocation(Vector3 location, int indexOfLinkedLayoutPoint)
    {


        GameObject layoutObj = Instantiate(layoutPointPrefab, LayoutPointParent.transform);
#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(layoutObj, "LayoutPoint Create");
#endif

        layoutObj.gameObject.name = "ManualLayoutPoint" + indexOfLinkedLayoutPoint;
        layoutObj.transform.position = new Vector3(location.x, 1, location.z);
        layoutObj.transform.SetSiblingIndex(indexOfLinkedLayoutPoint+1);
        layoutObj.GetComponent<LayoutPoint>().absolutePosition = location;
#if UNITY_EDITOR
        Undo.RegisterCompleteObjectUndo(layoutObj, "LayoutPoint Create");
#endif
    }
#if UNITY_EDITOR
    private List<Vector3> DetermineDefaultLayoutPositions()
   {
       GameObject smallerPlane = Instantiate(flatPlane.gameObject );
       smallerPlane.transform.position = flatPlane.transform.position;
       smallerPlane.transform.localScale *= defaultDimensionScale;
       List<Vector3> returnList = new List<Vector3>();
       var vertices = smallerPlane.GetComponent<MeshFilter>().sharedMesh.vertices; 
       
       returnList.Add(new Vector3(smallerPlane.transform.TransformPoint(vertices[110]).x, 0  , smallerPlane.transform.TransformPoint(vertices[110]).z ) ); 
       returnList.Add(new Vector3(smallerPlane.transform.TransformPoint(vertices[120]).x, 0  , smallerPlane.transform.TransformPoint(vertices[120]).z ) ); 
       returnList.Add(new Vector3(smallerPlane.transform.TransformPoint(vertices[10]).x, 0  , smallerPlane.transform.TransformPoint(vertices[10]).z ) ); 
       returnList.Add(new Vector3(smallerPlane.transform.TransformPoint(vertices[0]).x, 0 , smallerPlane.transform.TransformPoint(vertices[0]).z ) ); 
       

       DestroyImmediate(smallerPlane);
       return returnList;
   }
   private void SpawnDefaultLayoutPoints()
   { 
       metersPerUnit = 1;
       foreach (var position in DetermineDefaultLayoutPositions())
       {
           GameObject layoutObj = Instantiate(layoutPointPrefab, LayoutPointParent.transform);
           layoutObj.gameObject.name = "DefaultLayoutPoint" + DetermineDefaultLayoutPositions().IndexOf(position);
           layoutObj.GetComponent<LayoutPoint>().absolutePosition =  new Vector3(position.x, 0, position.z); 
           layoutObj.transform.position = new Vector3(position.x, 1, position.z); 
       }

       wallsHaveJustBeenReset = true; 
       SceneView.RepaintAll();
   }

  
   private void SpawnDefaultMocks()
   {
       if (classificationMaterials == null)
           InitializeMockMaterialDictionary();
       
      // find centerpoint of default layoutpoints
      var centerPos = Vector3.zero; 
      List<Vector3> layoutPointPositions = designAccess.GetActiveRoomLayout().GetLayoutPositions().ToList();
      foreach (var pos in layoutPointPositions)
          centerPos += pos;
      centerPos *= (1f / layoutPointPositions.Count);
 
      Vector3 wallCenter0 = (layoutPointPositions[1] + layoutPointPositions[2]) / 2f;
      Vector3 wallCenter1 = (layoutPointPositions[2] + layoutPointPositions[3]) / 2f;
      Vector3 wallCenter2 = (layoutPointPositions[3] + layoutPointPositions[0]) / 2f;
      Vector3 wallCenter3 = (layoutPointPositions[0] + layoutPointPositions[1]) / 2f;

      wallCenter0 = new Vector3(wallCenter0.x, 0, wallCenter0.z);
      wallCenter1 = new Vector3(wallCenter1.x, 0, wallCenter1.z);
      wallCenter2 = new Vector3(wallCenter2.x, 0, wallCenter2.z);
      wallCenter3 = new Vector3(wallCenter3.x, 0, wallCenter3.z);
      // TABLE
      float tableDepth = 0.8f;
      float tableHeight = 0.5f; 
      SpawnMock(centerPos + new Vector3(0,0,1f), Classification.Table, new Vector3(1.2f, tableHeight, tableDepth));
 
      // SMALLSEAT
      float seatDepth = 0.3f;
      float seatHeight = 0.2f;
      Vector3 smallSeatPos = centerPos + new Vector3(0,0,-centerPos.z/7.5f);
      SpawnMock(smallSeatPos, Classification.Closet, new Vector3(0.3f, seatHeight, seatDepth));
      // LARGEClutter
       seatDepth = 0.6f;
       seatHeight = 0.4f;
      Vector3 largeSeatPos = new Vector3(wallCenter1.x  + seatDepth, wallCenter1.y, wallCenter1.z );
      SpawnMock(largeSeatPos, Classification.Misc, new Vector3(0.4f, seatHeight, seatDepth));
     
    
   }


    public void SpawnRandomLayout(bool lShape, List<Classification> classifications)
    { 
        DeleteMocks(); 
        FindObjectOfType<RoomGenerator>().GenerateRoom();
        return; 
    }
#endif
    public Vector3 getRandomBoxSpaceInPolygon(bool stacked, Bounds bounds, Quaternion rot, IEnumerable<Bounds> extraBounds = null)
    {
        Vector3 midPoint = GetAreaCenter();
        Bounds boundsOfLayoutPoints = GeometryUtility.CalculateBounds(GetLayoutPointsAsVector3().ToArray(), Matrix4x4.identity);
        boundsOfLayoutPoints.Expand(Vector3.up * 100f);
        int maxTries = 100;
        Vector3 position = Vector3.zero;
        Vector3 raycastPos = Vector3.zero;
        do
        {
             raycastPos = new Vector3(
                Random.Range(boundsOfLayoutPoints.min.x, boundsOfLayoutPoints.max.x),
                0,
                Random.Range(boundsOfLayoutPoints.min.z, boundsOfLayoutPoints.max.z));
            maxTries--;

            RaycastHit hit;

            if (Physics.BoxCast(raycastPos + bounds.center + Vector3.up * 100f, bounds.size, Vector3.down, out hit, rot))
            {
                position = hit.point;
            }
            if(extraBounds!= null)
            {
                position = new Vector3(raycastPos.x, 0, raycastPos.z);
                foreach (var bound in extraBounds)
                {
                    Ray ray = new Ray(raycastPos + Vector3.up * 100f, Vector3.down);
                    float dis = 0;
                    var wasHit = bound.IntersectRay(ray, out dis);

                    if (wasHit)
                    {
                        position = ray.GetPoint(dis);
                        break;
                    }
                }
            }

        } while ((
        !(
        GeometryUtils.PointInPolygon(raycastPos + bounds.center + rot * new Vector3(bounds.size.x, 0, bounds.size.z), GetLayoutPointsAsVector3()) &&
        GeometryUtils.PointInPolygon(raycastPos + bounds.center + rot * new Vector3(-bounds.size.x, 0, bounds.size.z), GetLayoutPointsAsVector3()) &&
        GeometryUtils.PointInPolygon(raycastPos + bounds.center + rot * new Vector3(bounds.size.x, 0, -bounds.size.z), GetLayoutPointsAsVector3()) &&
        GeometryUtils.PointInPolygon(raycastPos + bounds.center + rot * new Vector3(-bounds.size.x, 0, -bounds.size.z), GetLayoutPointsAsVector3())
        ) 
        || 
        (!stacked && position.y > 0.01f)) 
        && 
        maxTries > 0);
//        Debug.Log(maxTries);
        if (!stacked && maxTries == 0)
            position.y = -0.1f;
        raycastPos.y = position.y;
        return raycastPos;
    }

    public (Vector3 pos, Vector3 normal) getRandomPointOnWall()
    {
        float t = Random.Range(0f, 1f);
        float currentT = 0;
        float totalDistance = getTotalLengthOfWalls();
        for (int i = 0; i < GetLayoutPointsAsVector3().Count; i++)
        {
            var p1 = GetLayoutPointsAsVector3()[i];
            var p2 = GetLayoutPointsAsVector3()[(i + 1) % GetLayoutPointsAsVector3().Count];

            float bT = currentT;
            currentT += Vector3.Distance(p1, p2) / totalDistance;

            if(t < currentT)
            {
                return (Vector3.Lerp(p1, p2, Mathf.InverseLerp(bT, currentT, t)), Quaternion.Euler(0, 90, 0) * (p2-p1).normalized);
            }


        }
        return (Vector3.zero, Vector3.zero) ;
    }

    public (Vector3 pos, Vector3 normal) getRandomBoxSpaceOnWall(bool stacked, Vector3 bounds, bool alignedToWall)
    {

        int maxTries = 10000;
        Vector3 position = Vector3.zero;
        Vector3 raycastPos = Vector3.zero;
        var fBounds = bounds;
        var p = getRandomPointOnWall();
        do
        {
            p = getRandomPointOnWall();
            var rot = Quaternion.Euler(0, Vector2.SignedAngle(p.normal.flatten(), Vector2.up), 0);
            if (alignedToWall)
                bounds = rot * fBounds;
            int mul = 1;
            if (Vector3.Dot(bounds, p.normal) < 0)
                mul = -1;
            raycastPos = p.pos + Vector3.Project(bounds * 0.5f, p.normal) * mul;
            maxTries--;
            RaycastHit hit;

            if (Physics.BoxCast(raycastPos + Vector3.up * 100f, bounds, Vector3.down, out hit, rot))
            {
                position = hit.point;
            }
        }
        while ((!GeometryUtils.PointInPolygon(position, GetLayoutPointsAsVector3()) || (!stacked && position.y > 0.01f)) && maxTries > 0);
        if (!stacked && maxTries == 0)
            position.y = -0.1f;
        raycastPos.y = position.y;
        return (raycastPos, p.normal);
    }

    float getTotalLengthOfWalls()
    {
        float length = 0;
        for(int i = 0; i < GetLayoutPointsAsVector3().Count; i++)
        {
            var p1 = GetLayoutPointsAsVector3()[i];
            var p2 = GetLayoutPointsAsVector3()[(i + 1) % GetLayoutPointsAsVector3().Count];

            length += Vector3.Distance(p1, p2);
        }
        return length;
    }
    public Vector3 GetAreaCenter()
    {
        Vector3 returnPos = Vector3.zero;
        foreach (var pos in GetLayoutPointsByTransform().Select(a => a.transform.position))
            returnPos += pos;
        returnPos = returnPos * (1f / (float)GetLayoutPointsByTransform().Count);
       
        return returnPos;
    }
#if UNITY_EDITOR
    public GameObject SpawnMock(Vector3 position, Classification classification, Vector3 scale)
   { 
        // adjust height by half y scale
        GameObject cubeObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cubeObj.transform.SetParent(mockPhysicalsParent.transform);
        cubeObj.transform.position = position;
        cubeObj.transform.localScale = scale;
        cubeObj.transform.position += new Vector3(0,scale.y/2f, 0);
        cubeObj.AddComponent<MockPhysical>(); 
        cubeObj.GetComponent<MockPhysical>().Classification = classification; 
        cubeObj.GetComponent<MockPhysical>().Initialize();
        DestroyImmediate(cubeObj.GetComponent<BoxCollider>());
        cubeObj.AddComponent<BoxCollider>();

        cubeObj.name = "MOCK" + scale.x +  classification;
        return cubeObj; 
    }


 
   public void GainFocus()
   { 
       Selection.activeObject = this.gameObject;
       if (focusCameraAfterInteraction)
           SceneView.lastActiveSceneView.FrameSelected();
   }
#endif
   private LayoutPointParent _lpParent;
    private LayoutPointParent LayoutPointParent
    {
        get
        {
            if (_lpParent == null)
                _lpParent = FindObjectOfType<LayoutPointParent>();
            return _lpParent;
        }
    }
    #region Helpers
    public List<LayoutPoint> GetLayoutPointsByTransform()
    {  
        if (LayoutPointParent.GetComponentsInChildren<LayoutPoint>(true) != null)
             return LayoutPointParent.GetComponentsInChildren<LayoutPoint>(true).ToList();
        return null;
    } 
    public List<Vector3>  GetLayoutPointsAsVector3()
    {
        List<Vector3> returnList = new List<Vector3>();
        foreach (var lp in GetLayoutPointsByTransform())
            if (lp != null)
                returnList.Add(lp.transform.position);
        return returnList;
    }//
    public List<MockPhysical> GetMocksByTransform()
    { 
        List<MockPhysical>  mocks  = new List<MockPhysical>();
        foreach (var mp in mockPhysicalsParent.GetComponentsInChildren<MockPhysical>(true))
            if (mp != null)
                mocks.Add(mp);
 
        return mocks;
    }
  //
  private void DeleteLayoutPoints()
    {
        if (GetLayoutPointsByTransform()!= null)
           foreach (var layoutPoint in GetLayoutPointsByTransform())
              DestroyImmediate(layoutPoint.gameObject);
    }

  public void DeleteMocks()
  { 
        DeactivateMockSelection();
        var mocks = GetMocksByTransform();
        if (mocks.Count != 0)
           foreach (var mock in mocks)
               DestroyImmediate(mock.gameObject);
    }
  public void ActivateMocks(bool setActive)
  { 
      var mocks = GetMocksByTransform();
      if (mocks.Count != 0)
          foreach (var mock in mocks)
              mock.gameObject.SetActive(setActive);
  }
  

    #endregion


    public Material GetMockMaterial( Classification classification)
    {
        if (classificationMaterials == null)
            InitializeMockMaterialDictionary();
        if (classificationMaterials.ContainsKey(classification) == false)
            return classificationMaterials[Classification.Misc];
        return classificationMaterials[classification];
    }
    public void DeactivateMockSelection()
    {
        if (_hasActiveMock)
        {
            _hasActiveMock = false;  
            if (_activeMock != null)
                _activeMock.DeactiveMock();
        }
    }
    public void ReconstructFromScanLayout(string layoutText)
    {
        RoomLayout roomLayout = JsonUtility.FromJson<RoomLayout>(layoutText);
        
        ReconstructRoomLayout(roomLayout); 

    }
    public string GetRoomLayout()
    {
        RoomLayout roomLayout = CreateRoomLayout();
        return JsonUtility.ToJson(roomLayout, true);
    }

    private RoomLayout CreateRoomLayout()
    {
        RoomLayout roomLayout = new RoomLayout();
        roomLayout.mockDatas = new List<MockData>();
        roomLayout.mockPhysicals = GetMocksByTransform();
        if (roomLayout.mockPhysicals.Count != 0)
            foreach (var mock in roomLayout.mockPhysicals)
                roomLayout.mockDatas.Add(mock.GetMockData());

        roomLayout.roomDimensions = new RoomDimensions();
        roomLayout.roomDimensions.layoutPointPositions = GetLayoutPointsAsVector3().ToList();
        return roomLayout;
    }
#if UNITY_EDITOR
  
    public string SaveRoomLayoutWithFileDialog()
    {
        RoomLayout roomLayout = CreateRoomLayout();
        
        var jsonString = JsonUtility.ToJson(roomLayout, true); 
        
        var path = EditorUtility.SaveFilePanel(
            "Save RoomLayout as txt",
            "",
            ".txt",
            "txt"); 
        
        string fileName = "";
        if (path != "")
        { 
            fileName = Path.GetFileName(path).Substring(0, Path.GetFileName(path).Length - 4);
            WriteFilenameIntoRecentFile( fileName); 
            File.WriteAllText(path, jsonString);
        }
        return fileName;
    }
    private void WriteFilenameIntoRecentFile(string fileName)
    {
        string recentFileText = File.ReadAllText(recentRoomLayoutsPath);
        recentFileText =  fileName +"," +  recentFileText; 
        File.WriteAllText(recentRoomLayoutsPath, recentFileText);
    }
    public void SaveToPath(string path)
    {
        RoomLayout roomLayout = CreateRoomLayout();
        var jsonString = JsonUtility.ToJson(roomLayout, true);
        if (path != "")
        {
            WriteFilenameIntoRecentFile( Path.GetFileName(path).Substring(0, Path.GetFileName(path).Length - 4));
            File.WriteAllText(path, jsonString);
        }
    }
    public string LoadRoomLayoutWithFileDialog()
    {
        var path = EditorUtility.OpenFilePanel(
            "Load RoomLayout",
            "",
            "txt" );
        string fileName = "";
        if (path != "")
        {
            fileName = Path.GetFileName(path).Substring(0, Path.GetFileName(path).Length - 4);
            WriteFilenameIntoRecentFile( fileName); 
            RoomLayout roomLayout = JsonUtility.FromJson<RoomLayout>(File.ReadAllText(path));
            ReconstructRoomLayout(roomLayout);
        }
        return fileName;

    }
    public void LoadRoomLayoutWithPath(string path)
    { 
        if (path != "")
        {
            WriteFilenameIntoRecentFile( Path.GetFileName(path).Substring(0, Path.GetFileName(path).Length - 4));
          
            RoomLayout roomLayout = JsonUtility.FromJson<RoomLayout>(File.ReadAllText(path));
            ReconstructRoomLayout(roomLayout);
        }
    }


    public void LoadRoomLayoutWithFilename(string fileName)
    { 
        if (fileName != "")
        {
            string path = roomLayoutsPath + fileName + ".txt";
            WriteFilenameIntoRecentFile( fileName); 
            RoomLayout roomLayout = JsonUtility.FromJson<RoomLayout>(File.ReadAllText(path));
            ReconstructRoomLayout(roomLayout);
        }
    }
#endif

    private float defaultRoomSize = 4;
    private void ReconstructRoomLayout( RoomLayout roomLayout )
    {
        DeleteLayoutPoints();

        if (roomLayout.GetLayoutPositions().Count == 0)
        {
            List<Vector3> newWallPoints = new List<Vector3>();
            Vector3 userPos = overCameraRigTransform.position;
            Vector3 pos1 = userPos + new Vector3(defaultRoomSize/2, 0, -defaultRoomSize/2);
            Vector3 pos2 = userPos + new Vector3(-defaultRoomSize/2, 0, -defaultRoomSize/2);
            Vector3 pos3 = userPos + new Vector3(-defaultRoomSize/2, 0, defaultRoomSize/2);
            Vector3 pos4 = userPos + new Vector3(defaultRoomSize/2, 0, defaultRoomSize/2);
            newWallPoints.Add(pos1);
            newWallPoints.Add(pos2);
            newWallPoints.Add(pos3);
            newWallPoints.Add(pos4);
            roomLayout.roomDimensions.layoutPointPositions = newWallPoints.ToList();
        }

        
        DeleteMocks();
        List<MockPhysical> mockList = new List<MockPhysical>();
        if (roomLayout.mockDatas != null)
          if (roomLayout.mockDatas.Count > 0)
            foreach (var mockData in roomLayout.mockDatas)
                mockList.Add(MockPhysical.GetMockPhysicalFromMockData(this, mockData));
        roomLayout.mockPhysicals = new List<MockPhysical>();
        roomLayout.mockPhysicals.AddRange(mockList);

        int index = 0;
        foreach (var pos in roomLayout.roomDimensions.layoutPointPositions)
        {
//            Debug.Log((" spawn at: " + pos));
            SpawnLayoutPointAtLocation(pos, index++);
        }

    }
#if UNITY_EDITOR
    private void CheckForChanges()
    {
         // create reference for current active LP's & Mocks
        List<LayoutPoint> currentLayoutPoints = GetLayoutPointsByTransform().ToList();
        List<MockPhysical> currentMocks = GetMocksByTransform().ToList();

        // check for null / empty lists
        if (_tempStoredLayoutPoints == null)
            _tempStoredLayoutPoints = currentLayoutPoints.ToList();
        if (_tempStoredLayoutPoints.Count == 0)
            _tempStoredLayoutPoints = currentLayoutPoints.ToList();
        if (_tempStoredMocks == null)
            _tempStoredMocks = currentMocks.ToList();
        if (_tempStoredMocks.Count == 0)
            _tempStoredMocks = currentMocks.ToList();
        
        // check for null elements 
        if (_tempStoredLayoutPoints.Count != 0)
            foreach (var lp in _tempStoredLayoutPoints)
                if (lp == null)
                {
                    SendPipelineUpdateSignal();
                    _tempStoredLayoutPoints = currentLayoutPoints.ToList();
                    return;
                }
        if (_tempStoredMocks.Count != 0)
            foreach (var mock in _tempStoredMocks)
                if (mock == null)
                {
                    SendPipelineUpdateSignal();
                    _tempStoredMocks = currentMocks.ToList();
                    return;
                }
        
        // check for size changes
        if (_tempStoredLayoutPoints.Count != currentLayoutPoints.Count)
        {
            SendPipelineUpdateSignal();
            _tempStoredLayoutPoints = currentLayoutPoints.ToList();
        }
        if (_tempStoredMocks.Count != currentMocks.Count)
        {
            SendPipelineUpdateSignal();
            _tempStoredMocks = currentMocks.ToList(); 
        }

        // check for transform changes 
        foreach (var lp in _tempStoredLayoutPoints)
            if (lp != null)
            {
                if (lp.transform.hasChanged)
                {
                    SendPipelineUpdateSignal();
                    lp.transform.hasChanged = false;
                }
            }
        foreach (var mock in _tempStoredMocks)
            if (mock != null)
            {
                if (mock.transform.hasChanged)
                {
                    SendPipelineUpdateSignal();
                    mock.transform.hasChanged = false;
                }
            }
             
        // check for various changes
       
        if (tempMetersPerUnit != metersPerUnit)
        {
            tempMetersPerUnit = metersPerUnit;
           // RefitLayoutPoints();
        }
        
       
    }

    public void DeleteRoomLayout(string fileName)
    {
        string path = roomLayoutsPath + fileName + ".txt";
        AssetDatabase.DeleteAsset(path);
    }
    public void DeleteRoomChart(string fileName)
    {
        string path = roomChartsPath + fileName + ".txt";
        AssetDatabase.DeleteAsset(path);
    }



    public void SpawnBaseMock()
    {
     if (baseMock != null)
         DestroyImmediate(baseMock.gameObject);
     
     baseMock =  SpawnMock(Vector3.zero, Classification.Misc, Vector3.one);
  //   baseMock =  SpawnMock(Vector3.zero, Classification.Base, Vector3.one);
     baseMock.AddComponent<BaseMock>();
     Selection.activeGameObject = baseMock;
    }
#endif
}
