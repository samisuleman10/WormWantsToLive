using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.XRTools.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using static SurrounderModule;

#if UNITY_EDITOR
using static ToolWindowSettings;
#endif

// fix child objects not positioned with matrix




[ExecuteAlways]
/// <summary>
/// Processes Surroundings, which includes floor, walls and ceiling
/// </summary>
public class SurrounderModule : ModuleBase<(List<ScanVolume> scans, RoomDimensions roomDimensions, List<SurroundSync> surroundSyncs), (List<ScanVolume> scanVolumes, List<WallInfo> wallInfos)>
{
    [FormerlySerializedAs("designStorage")] public DesignAccess designAccess;
    public RoomDimensions roomDimensions;
    public GameObject groundObject; 

    /// <summary>
    /// Default tile used if no SurrounderSync can be applied
    /// </summary>
     
    public SurroundSync defaultTile;

    public bool showInfo;

    public bool showFloor, showWalls;

    public bool previewSurrounder;

    [Header("SurroundSyncs")] 
    public bool useManualList;
    public List<SurroundSync> manualList;
    [Header("EdgePieces")]
    public bool drawEdgeObjects;
    public GameObject innerEdgeObject;
    public GameObject outerEdgeObject;
    [Header("WallPiece")]
    public bool perfectFit;
    [Range(0.1f, 10f)]
    public float density = 1f;
    [Range(0.00f, 100f)]
    public float wallPieceRandomness = 1f;
    public bool takeListOrder;
    [Header("Rotation")]
    [Range(0.01f, 10f)]
    public float rotationVarianceRandomness = 1f;
    public Vector3 rotationVariance;
    public Vector3 rotationOffset;
    [Header("Scale")]
    [Range(0.01f, 10f)]
    public float scaleVarianceRandomness = 1f;
    public Vector3 scaleVariance;
    public Vector3 scaleOffset;
    [Header("Position")]
    [Range(0.01f, 10f)]
    public float positionVarianceRandomness = 1f;
    public Vector3 positionVariance;
    public Vector3 positionOffset;
 
    
    
    //List that determines wether a layout Point is an inner or an outer forceCorner
    private List<bool> outerOrInnerEdge = new List<bool>();
    private Dictionary<WallFace, int> totalDirCounts;
    private List<Vector3> layoutPoints;
    private  StoryTeller storyTeller ; 

#if UNITY_EDITOR
    /*public void ExecuteForDesignArea()
    {
        // create custom roomdimensions for designarea
        DesignArea designArea = FindObjectOfType<DesignArea>();
        RoomDimensions roomDimensions = new RoomDimensions();
        Transform rt = designArea.FlexFloorProp.transform;
        List<Vector3> pointList = new List<Vector3>();
        var vertices = designArea.FlexFloorProp.GetComponent<MeshFilter>().sharedMesh.vertices;
        pointList.Add(rt.TransformPoint(vertices[10])); 
        pointList.Add(rt.TransformPoint(vertices[120]));
        pointList.Add(rt.TransformPoint(vertices[110])); 
        pointList.Add(rt.TransformPoint(vertices[0]));
        roomDimensions.layoutPointPositions = new List<Vector3>(); 
        foreach (var point in pointList)
            roomDimensions.layoutPointPositions.Add(point);
        
        
        var tileInfos = createTileInfos(designAccess.GetActiveRoomLayout(), designAccess.GetSurroundSyncs());

        // FD:: added flexFloor check to know if a new flexFloor needs to be instantiated
        processGroundPlane(instantiate: designArea.FlexFloorProp == null); 
    }*/
  #endif  
    
    /// <summary>
    /// Processes FillerInstances for Surroundings and RoomDiemensions for other modules
    /// </summary>
    /// <param name="input">Scanned Volume Data. Is retreived from scanner by default</param>
    /// <returns>Left ScannedVoulmes and RoomDimensions</returns>
    public override (List<ScanVolume> scanVolumes, List<WallInfo> wallInfos) Execute((List<ScanVolume> scans, RoomDimensions roomDimensions, List<SurroundSync> surroundSyncs) input  )
    {
#if UNITY_EDITOR
        ToolWindowSettings.Instance.objectsToDrawInPreview = new List<ObjectInfo>();
#endif
         storyTeller = FindObjectOfType<StoryTeller>(); 
        var tileInfos = createTileInfos(designAccess.GetActiveRoomLayout(), input.surroundSyncs);
        //If hit play, Instantiate the objects
        
        if (Application.isPlaying)
        {
            processGroundPlane(instantiate: true);
        }
                #if UNITY_EDITOR
        else
        {

            

            if (ToolWindowSettings.Instance.pru == null)
            {
                if (showWalls)
                {
                    //previewTiles(tileInfos, GetSurroundSyncs()); //todo I (T) commented this out, still need it? 
                }
            }
            if (showFloor) 
                processGroundPlane();
            if (ToolWindowSettings.Instance.pru != null && previewSurrounder)
            {
                if (designAccess.GetActiveRoomLayout().GetLayoutPositions().Count != 0)
                {
                    //previewTilesEditorWindow(tileInfos, GetSurroundSyncs() );
                   //processGroundPlane(instantiate: false );
                } 
            }

        }
        #endif
        return (input.scans, tileInfos);
    }



    private void OnDrawGizmos()
    { 
        if(showInfo)
        {
            /*var tileInfos = createTileInfos(); // Tchange
            showTileInfos(tileInfos, GetSurroundSyncs());
            previewGround();*/
        }
       // previewGround();
    }

    /// <summary>
    /// Computes the minimal bounding box based on the floor points
    /// </summary>
    /// <returns>The four points of the bounding box in local space</returns>
    public List<Vector3> getGroundBox()
    {
        Vector3[] vecArr = new Vector3[4];
        var hull = new List<Vector3>();
        GeometryUtils.ConvexHull2D(designAccess.GetActiveRoomLayout().GetLayoutPositions(), hull);
        GeometryUtils.OrientedMinimumBoundingBox2D(hull, vecArr);



        var sortedList = new List<Vector3>();
        int longestI = 0; 

        var dirLength = new Dictionary<WallFace, float>
        {
            {WallFace.North, 0 },
            {WallFace.East, 0 },
            {WallFace.South, 0 },
            {WallFace.West, 0 }
        };

        var roomLayout = designAccess.GetActiveRoomLayout();
        for (int i = 0; i < roomLayout.GetLayoutPositions().Count; i++)
        {
            var curVecToFill = roomLayout.GetLayoutPositions()[(i + 1) % roomLayout.GetLayoutPositions().Count] - roomLayout.GetLayoutPositions()[i];

            var dir = checkFaceDirection(i, vecArr);

            dirLength[dir] += curVecToFill.magnitude;
        }

        var longestWall = (dirLength.OrderByDescending(l => l.Value).First().Key);
        longestI = ((int)longestWall);
        /*
        for (int i = 0; i < vecArr.Length; i++)
        {
            if(Vector3.Distance(vecArr[i], vecArr[(i+1) % vecArr.Length]) > longestDistance)
            {
                longestI = i;
            }
        }*/
        for (int i = -longestI; i < vecArr.Length - longestI; i++)
            sortedList.Add(vecArr[(i + vecArr.Length) % vecArr.Length]);

        return sortedList.Select(v => { return new Vector3(v.x, 0, v.z); }).ToList();
    }

    Vector3 getNormalForFace(WallFace face)
    {
        int f = (int)face;
        return (getGroundBox()[(f + 1) % 4] - getGroundBox()[f]).normalized;
    }


    WallFace checkFaceDirection(int i, Vector3[] vecArr)
    {

        var roomLayout = designAccess.GetActiveRoomLayout();
        var lps = roomLayout.GetLayoutPositions();
        var curVecToFill = lps[(i + 1) % lps.Count] - lps[i];


        var nextVecToFill = lps[(i + 2) % lps.Count] - lps[(i + 1) % lps.Count];
        int j = 1;
        while(getDirOfVec(nextVecToFill, vecArr) == getDirOfVec(curVecToFill, vecArr))
        {
            nextVecToFill = lps[(i + 2 + j) % lps.Count] - lps[(i + 1 + j) % lps.Count];
            j++;
        }
        var prevVecToFill = lps[(i + 0) % lps.Count] - lps[(i - 1 + lps.Count) % lps.Count];

        j = 1;
        while (getDirOfVec(prevVecToFill,vecArr) == getDirOfVec(curVecToFill, vecArr))
        {
            prevVecToFill = lps[(i + 0 - j + lps.Count) % lps.Count] - lps[(i - 1 - j + lps.Count) % lps.Count];
            j++;
        }


        var nextDir = getDirOfVec(nextVecToFill, vecArr);
        var prevDir = getDirOfVec(prevVecToFill, vecArr);


        if (nextDir == prevDir && (nextDir == WallFace.South || nextDir == WallFace.North))
        {
            return nextDir;
        }

        return getDirOfVec(curVecToFill, vecArr);
    }

    public WallFace getDirOfVec(Vector3 curVecToFill, Vector3[] vecArr)
    {

        var angle = (((int)Vector3.SignedAngle(curVecToFill.normalized, (vecArr[1] - vecArr[0]).normalized, Vector3.up)) + 360) % 360;
        var dir = (angle < 45 ? WallFace.North : angle < 135 ? WallFace.West : angle < 225 ? WallFace.South : angle < 315 ? WallFace.East : WallFace.North);
        return dir;
    }

    /// <summary>
    /// Draws the four points of the Floor boudning box as Gizmo Spheres
    /// </summary>
    void previewGround()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(getGroundBox()[0], 3f);
        Gizmos.DrawSphere(getGroundBox()[0] + (getGroundBox()[1] - getGroundBox()[0]).normalized * 5f, 3f);
    }
    public bool rotateGround = false;
    public bool drawCeiling;
    /// <summary>
    /// Processes the ground plane based on the minimal bounding box of the floor points
    /// </summary>
    /// <param name="instantiate">Wether or not the ground plane is instantiated as an object or drawn directly</param>
    public void processGroundPlane(bool instantiate = false)
    {
        var groundBox = getGroundBox();
        var center = Vector3.zero;
        foreach (var v in groundBox)
            center += v;
        center = center / groundBox.Count;
        center.y = 0;
    //    if (groundObject == null)
    //        groundObject = FindObjectOfType<FlexFloor>().gameObject;
 
        
        if (groundObject == null)
            Debug.LogWarning("Hi, I'm a weird bug. Please recompile, thanks!");
        var floorMesh = groundObject.GetComponent<MeshFilter>()?.sharedMesh; 
        var ceilingMesh = groundObject.GetComponent<MeshFilter>()?.sharedMesh; 
#if UNITY_EDITOR
        if(!instantiate)
        {
        // PREVIEWWINDOW
            int id = 0;
            Vector3 revertScale = Vector3.one;
            if(designAccess.GetFloorMeshPrefab() != null)
            {
                floorMesh = designAccess.GetFloorMeshPrefab().GetComponent<MeshFilter>().sharedMesh;
                revertScale = designAccess.GetFloorMeshPrefab().transform.localScale;
            }
            if (designAccess.GetCeilingMeshPrefab() != null)
                ceilingMesh = designAccess.GetCeilingMeshPrefab().GetComponent<MeshFilter>().sharedMesh;
            var scale = new Vector3(floorMesh.bounds.size.x, 1, floorMesh.bounds.size.z).invert().ScaleBy(new Vector3((groundBox[1] - groundBox[0]).magnitude, 1, (groundBox[3] - groundBox[0]).magnitude)).ScaleBy(revertScale.invert());
            var matrix = Matrix4x4.TRS(
                        center,
                        Quaternion.Euler(0, -Vector3.SignedAngle(groundBox[1] - groundBox[0], Vector3.right, Vector3.up) + (rotateGround ? 90 : 0), 0),
                        rotateGround ? scale.flip() : scale);

            Material matGround = groundObject.GetComponent<MeshRenderer>().sharedMaterial;
            Material matCeiling = groundObject.GetComponent<MeshRenderer>().sharedMaterial;
            if (designAccess.GetFloorMaterial1() != null)
                matGround = designAccess.GetFloorMaterial1(); 
            if (designAccess.GetCeilingMaterial1() != null)
                matCeiling = designAccess.GetCeilingMaterial1(); 
            Instance.objectsToDrawInPreview.Add(new ToolWindowSettings.ObjectInfo(id, designAccess.GetFloorMeshPrefab() != null ? designAccess.GetFloorMeshPrefab() : groundObject, matrix, null, null, WallFace.East, false, designAccess.GetFloorMeshPrefab() != null ? null : matGround));
            
            if(drawCeiling)
                Instance.objectsToDrawInPreview.Add(new ToolWindowSettings.ObjectInfo(id, designAccess.GetCeilingMeshPrefab() != null ? designAccess.GetCeilingMeshPrefab() : groundObject, matrix * Matrix4x4.Translate(Vector3.up * designAccess.GetCeilingHeight()) * Matrix4x4.Rotate(Quaternion.Euler(180,0,0)), null, null, WallFace.East, false, designAccess.GetCeilingMeshPrefab() != null ? null : matCeiling));
            
        } else
        #endif
        {
            // RUNTIME  
            GameObject spawnedGroundObject = null;
            if (designAccess.GetFloorMeshPrefab() != null)
                spawnedGroundObject = Instantiate(designAccess.GetFloorMeshPrefab());
            else
            {
                spawnedGroundObject = Instantiate(groundObject);
                if (designAccess.GetFloorMaterial1() != null)
                    spawnedGroundObject.GetComponent<MeshRenderer>().sharedMaterial = designAccess.GetFloorMaterial1();
            }
            spawnedGroundObject.transform.SetParent(groundObject.transform.GetComponentInParent<Transform>());
            
            GameObject spawnedCeilingObject = null;
            if (designAccess.GetCeilingMeshPrefab() != null)
                spawnedCeilingObject = Instantiate(designAccess.GetCeilingMeshPrefab());
            else
            {
                spawnedCeilingObject = Instantiate(groundObject);
                if (designAccess.GetCeilingMaterial1() != null)
                    spawnedCeilingObject.GetComponent<MeshRenderer>().sharedMaterial = designAccess.GetCeilingMaterial1();
            }

            spawnedCeilingObject.transform.SetParent(groundObject.transform.GetComponentInParent<Transform>());

            floorMesh = spawnedGroundObject.GetComponent<MeshFilter>().sharedMesh;
            ceilingMesh = spawnedCeilingObject.GetComponent<MeshFilter>().sharedMesh;
            
            // multiple meshes - take combined OR mesh in child object 
            // no meshes - warn people
            // previewwindow 
            
            /*MeshFilter floorMeshFilter = spawnedGroundObject.AddComponent<MeshFilter>();
            MeshFilter ceilingMeshFilter = spawnedCeilingObject.AddComponent<MeshFilter>();
            MeshRenderer floorMeshRenderer =spawnedGroundObject.AddComponent<MeshRenderer>();   
            MeshRenderer ceilingMeshRenderer =  spawnedCeilingObject.AddComponent<MeshRenderer>();*/
            
            
            
            // Add FlexFloor's Plane mesh if no mesh set for SyncSetup
            /*if (designAccess.GetFloorMesh() == null)
                floorMeshFilter.sharedMesh = groundObject.GetComponent<MeshFilter>().sharedMesh;
            else
                floorMeshFilter.sharedMesh = designAccess.GetFloorMesh();
            if (designAccess.GetCeilingMesh() == null)
                ceilingMeshFilter.sharedMesh = groundObject.GetComponent<MeshFilter>().sharedMesh;
            else
                ceilingMeshFilter.sharedMesh = designAccess.GetCeilingMesh();*/
            
         
        
            // same for material
            /*if (designAccess.GetFloorMaterial1() == null)
                floorMeshRenderer.sharedMaterial = groundObject.GetComponent<MeshRenderer>().sharedMaterial;
            else
            {
                spawnedGroundObject.GetComponent<MeshRenderer>().sharedMaterial = designAccess.GetFloorMaterial1();
                if (designAccess.GetFloorMaterial2() != null)
                {
                    Material[] matArray = new Material[2];
                    matArray[0] =  floorMeshRenderer.sharedMaterial; 
                    matArray[1] = designAccess.GetFloorMaterial2();
                    floorMeshRenderer.materials = matArray;
                } 
            }
            if (designAccess.GetCeilingMaterial1() == null)
                ceilingMeshRenderer.sharedMaterial = groundObject.GetComponent<MeshRenderer>().sharedMaterial;
            else
            {
                ceilingMeshRenderer.sharedMaterial = designAccess.GetCeilingMaterial1();
                if (designAccess.GetCeilingMaterial2() != null)
                { 
                    Material[] matArray = new Material[2];
                    matArray[0] =  ceilingMeshRenderer.sharedMaterial; 
                    matArray[1] = designAccess.GetCeilingMaterial2();
                    ceilingMeshRenderer.materials = matArray;
                } 
            }*/

         
            var o = Instantiate(designAccess.AssembleRuntimeFloor(spawnedGroundObject), center, Quaternion.Euler(0, -Vector3.SignedAngle(groundBox[1] - groundBox[0], Vector3.right, Vector3.up), 0), storyTeller.surroundSyncsParent);
            o.transform.localScale = new Vector3(floorMesh.bounds.size.x, 1, floorMesh.bounds.size.z).invert().ScaleBy(new Vector3((groundBox[1] - groundBox[0]).magnitude, 1, (groundBox[3] - groundBox[0]).magnitude));
            

            o = Instantiate(designAccess.AssembleRuntimeCeiling(spawnedCeilingObject), center + Vector3.up * designAccess.GetCeilingHeight(), Quaternion.Euler(180, -Vector3.SignedAngle(groundBox[1] - groundBox[0], Vector3.right, Vector3.up), 0), storyTeller.surroundSyncsParent);
            o.transform.localScale = new Vector3(ceilingMesh.bounds.size.x, 1, ceilingMesh.bounds.size.z).invert().ScaleBy(new Vector3((groundBox[1] - groundBox[0]).magnitude, 1, (groundBox[3] - groundBox[0]).magnitude));
            Destroy(spawnedGroundObject);
            Destroy(spawnedCeilingObject);
        }
    }

 
    /// <summary>
    /// Contains information used to spawn a WallPiece
    /// </summary>
    public struct WallInfo
    {
        public Matrix4x4 matrix; public bool isFirstTile; public bool isLastTile; public bool isInnerEdge; public WallFace dir; public Vector3 currentPos; public int faceIndex; public int totalIndex; public float j;
        public float tileBaseWidth;
        public Vector3 curVecToFill;
        public WallInfo(Matrix4x4 matrix, bool isFirstTile, bool isLastTile, bool isInnerEdge, WallFace dir, Vector3 currentPos,int faceIndex, int totalIndex, float tileBaseWidth, float j, Vector3 curVecToFill)
        {
            this.matrix = matrix;
            this.isFirstTile = isFirstTile;
            this.isLastTile = isLastTile;
            this.isInnerEdge = isInnerEdge;
            this.dir = dir;
            this.currentPos = currentPos;
            this.faceIndex = faceIndex;
            this.totalIndex = totalIndex;
            this.tileBaseWidth = tileBaseWidth;
            this.j = j;
            this.curVecToFill = curVecToFill;
        }
    }

    Dictionary<WallFace, float> totalWidthPerFace = new Dictionary<WallFace, float>();
    /// <summary>
    /// Processes all the walls based on floor points, Wall Objects to place, and rules
    /// </summary>
    public List<WallInfo> createTileInfos(RoomLayout roomLayout, List<SurroundSync> surroundSyncs) // parameter: Tchange
    {

        //Reinstantiate Count Variables
        outerOrInnerEdge = new List<bool>();
        totalDirCounts = new Dictionary<WallFace, int>()
            {
                { WallFace.North, 0},
                { WallFace.East, 0},
                { WallFace.South, 0},
                { WallFace.West, 0},
            };

       totalWidthPerFace = new Dictionary<WallFace, float>();

        for (int i = 0; i < roomLayout.GetLayoutPositions().Count; i++)
        {
            var curVecToFill = roomLayout.GetLayoutPositions()[(i + 1) % roomLayout.GetLayoutPositions().Count] - roomLayout.GetLayoutPositions()[i];

            var dir = checkFaceDirection(i, getGroundBox().ToArray());

            if(totalWidthPerFace.ContainsKey(dir))
                totalWidthPerFace[dir] += curVecToFill.magnitude;
            else
                totalWidthPerFace[dir] = curVecToFill.magnitude;
        }
        Dictionary<WallFace, float> currentWidthPerFace = new Dictionary<WallFace, float>();
        currentWidthPerFace[WallFace.North] = 0;
        currentWidthPerFace[WallFace.West] = 0;
        currentWidthPerFace[WallFace.South] = 0;
        currentWidthPerFace[WallFace.East] = 0;

        List<WallInfo> wallInfos = 
            new List<WallInfo>();

        //Helper Variable to count total wall pieces that needs to be placed
        int totalPlacementCount = 0;

        var availabeSurroundSyncs = surroundSyncs;

        int wallIndex = 0;

        //This loop computes the wallInfos for each layoutPoint to the next layoutPoint
        for (int i = 0; i < roomLayout.GetLayoutPositions().Count; i++) 
        {
            
            //Placement Vectors
            var curVecToFill = roomLayout.GetLayoutPositions()[(i + 1) % roomLayout.GetLayoutPositions().Count] - roomLayout.GetLayoutPositions()[i];
            var nextVecToFill = roomLayout.GetLayoutPositions()[(i + 2) % roomLayout.GetLayoutPositions().Count] - roomLayout.GetLayoutPositions()[(i + 1) % designAccess.GetStoredLayoutPoints().Count];
            var prevVecToFill = roomLayout.GetLayoutPositions()[(i + 0) % roomLayout.GetLayoutPositions().Count] - roomLayout.GetLayoutPositions()[(i + designAccess.GetStoredLayoutPoints().Count - 1) % designAccess.GetStoredLayoutPoints().Count];

            //Loop Helper variables
            var dir = checkFaceDirection(i, getGroundBox().ToArray());

            var firstWallPiece = filterAWallPiece(0, surroundSyncs, 0, dir, roomLayout.GetLayoutPositions()[i], Quaternion.Euler(0, 90, 0) * curVecToFill);

            float tileWidth = Mathf.Abs((firstWallPiece.surroundSync.bounds.size).x);  
            float tileOffset = Mathf.Abs((firstWallPiece.surroundSync.bounds.center).x);
            float ftileWidth = (firstWallPiece.surroundSync.bounds.size).x;
            float ftileOffset = (firstWallPiece.surroundSync.bounds.center).x;
            float additinalWithPerTile = 0; 
            float loopStepSize = Mathf.Max(perfectFit ? tileWidth + additinalWithPerTile : density, 0.05f);
            var startVal = perfectFit ? (tileWidth * 0 + additinalWithPerTile * 1f) * 0.5f : 0;

            float widthOfDrawnTiles = 0;
            List<(SurroundSync surroundSync, GameObject wallPiece, WallInfo info)> infosToDraw = new List<(SurroundSync surroundSync, GameObject wallPiece, WallInfo info)>();
            //This loop steps through the vector of one layout point to the next based on tile width
            for (float j = 0; j < curVecToFill.magnitude; j += loopStepSize) //+tileWidth * 0.5f + tileOffset
            {

                //Calculation of direction of this wall piece (North, East, South, West)
                dir = checkFaceDirection(i, getGroundBox().ToArray());

                var placementPosition = roomLayout.GetLayoutPositions()[i] +
                    curVecToFill.normalized * j;

                var wallPieceFilterResult = filterAWallPiece(wallIndex, availabeSurroundSyncs, currentWidthPerFace[dir] / totalWidthPerFace[dir], dir, placementPosition, Quaternion.Euler(0,90,0) * curVecToFill);
                wallIndex++;
                if (wallPieceFilterResult.surroundSync == null)
                    continue;
                
                    if(!wallPieceFilterResult.surroundSync.baseReplacer && availabeSurroundSyncs.Contains(wallPieceFilterResult.surroundSync))
                        availabeSurroundSyncs.Remove(wallPieceFilterResult.surroundSync);

              

                tileWidth = Mathf.Abs((wallPieceFilterResult.surroundSync.bounds.size).x);
                tileOffset = Mathf.Abs((firstWallPiece.surroundSync.bounds.center).x);

                loopStepSize = perfectFit ? tileWidth : density;

                //Determination of forceCorner properties
                bool firstTileOfVec = j - loopStepSize < 0;
                bool lastTileOfVec = j + loopStepSize > curVecToFill.magnitude;

                bool innerEdge = Vector3.SignedAngle(firstTileOfVec ? prevVecToFill : curVecToFill, lastTileOfVec ? nextVecToFill : curVecToFill, Vector3.up) < 0;

                if (firstTileOfVec)
                    outerOrInnerEdge.Add(innerEdge);

                //Noise Calculation for Transform Randomness of wall pieces
                var currentPos = roomLayout.GetLayoutPositions()[i] + 
                    curVecToFill.normalized * (j + ((wallPieceFilterResult.surroundSync.bounds.center).x))
                    ;
                var perlinValPos = (Mathf.PerlinNoise(currentPos.x * positionVarianceRandomness, currentPos.z * positionVarianceRandomness) - 0.5f) * 2f;
                var perlinValRot = (Mathf.PerlinNoise(currentPos.x * rotationVarianceRandomness, currentPos.z * rotationVarianceRandomness) - 0.5f) * 2f;
                var perlinValScale = (Mathf.PerlinNoise(currentPos.x * scaleVarianceRandomness, currentPos.z * scaleVarianceRandomness) - 0.5f) * 2f;

                //Calculation of Transform matrix for this wall piece
                var fillVecRot = Quaternion.Euler(0, Vector2.SignedAngle(new Vector2(curVecToFill.x, curVecToFill.z), Vector2.left), 0);
                var totalWallPieceRot = fillVecRot * Quaternion.Euler(rotationOffset) * Quaternion.Euler(perlinValRot * rotationVariance);
                Matrix4x4 offsetMatrix =

                    /*Matrix4x4.Translate((new Vector3(
                        0,
                        0,
                        +wallPieceFilterResult.surroundSync.bounds.center.z * 1f))) * */
                    Matrix4x4.Scale((Vector3.one + perlinValScale * scaleVariance + scaleOffset)) * //.ScaleBy(new Vector3(perfectFit ? 1f + additinalWithPerTile * 0.5f : 1, 1, 1))) *
                    Matrix4x4.Translate(new Vector3(currentPos.x, 0, currentPos.z) + perlinValPos * (fillVecRot * positionVariance) + (Vector3.one + perlinValScale * scaleVariance + scaleOffset).ScaleBy(positionOffset)) *
                    Matrix4x4.Rotate(totalWallPieceRot)
                    /*Matrix4x4.Translate(-new Vector3(
                            (wallPieceFilterResult.surroundSync.bounds.size.x + wallPieceFilterResult.surroundSync.bounds.center.x * 1f)
                        , 0, 0))*/
;
                
                
                //Increment placement variables
                int faceIndex = totalDirCounts[dir];
                totalPlacementCount++;
                totalDirCounts[dir]++;

                bool isEdge = lastTileOfVec || firstTileOfVec;

                if (currentWidthPerFace.ContainsKey(dir))
                    currentWidthPerFace[dir] += loopStepSize;
                else
                    currentWidthPerFace[dir] = loopStepSize;

                var info = new WallInfo( 
                    offsetMatrix, 
                    firstTileOfVec,
                    lastTileOfVec,
                    innerEdge,
                    dir,
                    currentPos,
                    faceIndex,
                    totalPlacementCount,
                    loopStepSize,
                    j,
                    curVecToFill);

                wallInfos.Add(info);

                if (wallPieceFilterResult.lastTile)
                    j = curVecToFill.magnitude;


                if(wallPieceFilterResult.wallPiece != null)
                {
                    infosToDraw.Add((wallPieceFilterResult.surroundSync, wallPieceFilterResult.wallPiece, info));
                    widthOfDrawnTiles += Mathf.Abs((wallPieceFilterResult.surroundSync.bounds.size).x);
                }


            }
            foreach (var wall in infosToDraw)
            {
                var center = (wall.surroundSync.bounds.center);
                var size = (wall.surroundSync.bounds.size);
                float scale = (curVecToFill.magnitude / (widthOfDrawnTiles));
                var offsetVec = (wall.info.currentPos - wall.info.matrix.rotation * Vector3.right * (-center.x * 1 + (size.x * 0.5f + center.x) * 1f)) - roomLayout.GetLayoutPositions()[i];
#if UNITY_EDITOR
                drawWallPieceDirectly(wall.wallPiece, FindObjectsOfType<WallMini>().FirstOrDefault(wm => wm.mappedSurroundSync == wall.surroundSync),

                    Matrix4x4.Translate(-(offsetVec - 1f * offsetVec * scale) * 1f) *
                    wall.info.matrix *

                    Matrix4x4.Translate((1f * new Vector3(- (Mathf.Abs(size.x) * 0.5f), 0, 0))) *
                    Matrix4x4.Translate((1f * new Vector3(0, 0,  (-(center.z) * 1 + (-Mathf.Abs(size.z)) * 0.5f)))) *
                    Matrix4x4.Translate(-(1f * new Vector3(-(wall.surroundSync.bounds.center.x), 0, (wall.surroundSync.bounds.center.z))) * 1f) *
                    Matrix4x4.Translate(wall.surroundSync.totalTranslateOffset + wall.surroundSync.translateVariance * (Mathf.PerlinNoise(wall.info.currentPos.x * 1323.2134f, wall.info.currentPos.z * 1323.2134f) - 0.5f)) *
                    Matrix4x4.Rotate(Quaternion.Euler(0, wall.surroundSync.totalRotationOffset + wall.surroundSync.rotationVaricance * (Mathf.PerlinNoise(wall.info.currentPos.x * 132123.2134f, wall.info.currentPos.z * 132435.2134f) - 0.5f), 0)) *

                    Matrix4x4.Scale(Vector3.one + wall.surroundSync.totalScaleOffset + wall.surroundSync.scaleOffset * (Mathf.PerlinNoise(wall.info.currentPos.x * 132.2134f, wall.info.currentPos.z * 132.2134f) - 0.5f)) *

                    Matrix4x4.Translate((1f * new Vector3(-wall.surroundSync.bounds.center.x, 0, wall.surroundSync.bounds.center.z)) * 1f) *

                    Matrix4x4.Rotate(Quaternion.Inverse(wall.surroundSync.model.transform.rotation)) *

                    Matrix4x4.identity

                    , scale, wall.surroundSync.face);


                       ; //;
#endif
                if (Application.isPlaying)
                    InstantiateWallPiece(wall.wallPiece,
                    Matrix4x4.Translate(-(offsetVec - 1f * offsetVec * scale) * 1f) *
                    wall.info.matrix *

                    Matrix4x4.Translate((1f * new Vector3(-(Mathf.Abs(size.x) * 0.5f), 0, 0))) *
                    Matrix4x4.Translate((1f * new Vector3(0, 0, (-(center.z) * 1 + (-Mathf.Abs(size.z)) * 0.5f)))) *
                    Matrix4x4.Translate(-(1f * new Vector3(-(wall.surroundSync.bounds.center.x), 0, (wall.surroundSync.bounds.center.z))) * 1f) *
                    Matrix4x4.Translate(wall.surroundSync.totalTranslateOffset + wall.surroundSync.translateVariance * (Mathf.PerlinNoise(wall.info.currentPos.x * 1323.2134f, wall.info.currentPos.z * 1323.2134f) - 0.5f)) *
                    Matrix4x4.Rotate(Quaternion.Euler(0, wall.surroundSync.totalRotationOffset + wall.surroundSync.rotationVaricance * (Mathf.PerlinNoise(wall.info.currentPos.x * 132123.2134f, wall.info.currentPos.z * 132435.2134f) - 0.5f), 0)) *

                    Matrix4x4.Scale(Vector3.one + wall.surroundSync.totalScaleOffset + wall.surroundSync.scaleOffset * (Mathf.PerlinNoise(wall.info.currentPos.x * 132.2134f, wall.info.currentPos.z * 132.2134f) - 0.5f)) *

                    Matrix4x4.Translate((1f * new Vector3(-wall.surroundSync.bounds.center.x, 0, wall.surroundSync.bounds.center.z)) * 1f) *

                    Matrix4x4.Rotate(Quaternion.Inverse(wall.surroundSync.model.transform.rotation)) *

                    Matrix4x4.identity

                    ,
                        scale, wall.surroundSync); ;
            }

        }


        drawBackdrops();

        if(Application.isPlaying)
        {

        }

        return wallInfos;

        
        
    }



    /// <summary>
    /// Filters out a fitting wall piece based on rules and computed wallInfo
    /// </summary>
    /// <param name="info">The previously computed wallInfos</param>
    /// <param name="currentDirCounts">Counts of directions in the placement loop</param>
    /// <param name="totalDirCounts">Counts of directions calculated in the info Processing loop</param>
    /// <param name="wallObjectsToPlace">List of possible wall tiles</param>
    /// <returns></returns>
    (GameObject wallPiece, int skips, SurroundSync surroundSync, List<SurroundSync> possibleSyncs, bool lastTile) filterAWallPiece(int totalIndex, List<SurroundSync> availableSurroundSyncs, float overridePercent, WallFace overrideDir, Vector3 placementPos, Vector3 normal)
    {

        float tilePercentage = overridePercent;
        var dir = overrideDir;  

        List<SurroundSync> filteredSurroundSyncs = new List<SurroundSync>();

        bool lastTile = false;

        var availableSurroundSyncsC = availableSurroundSyncs.Where(a => a != null).ToList();

        // Check for specific-indexed-surroundsyncs first
        foreach (SurroundSync surroundSync in availableSurroundSyncsC.OrderBy(ss => ss.mapToIndex).ThenBy(ss => ss.offset))
        {
            if (surroundSync.face == dir && !surroundSync.baseReplacer)
            {
                // Grab ANY specific-piece, if available
                float indexInFace = surroundSync.mapToIndex;  // 3 
                float faceCount = designAccess.GetWallPiecesForFace(surroundSync.face); // 10 
                float surroundSyncPercentage = (indexInFace / faceCount);  // 0.3f    0.03 difference
                float percentStep =  Mathf.Max(surroundSync.model.transform.TransformVector(surroundSync.bounds.extents).x * 1f, 0.05f) / totalWidthPerFace[dir]; //0.084

                var oneStepPercent = (1f / faceCount);

                var op = overridePercent + 1f * totalLengthOfIndex(surroundSync.mapToIndex, dir, availableSurroundSyncs) / totalWidthPerFace[dir];
                var om = surroundSyncPercentage + surroundSync.offset * 0.35f; //This is not exact
                if ((indexInFace != faceCount && 
                    surroundSyncPercentage - oneStepPercent <= op) || 
                    (indexInFace == faceCount && overridePercent + Mathf.Abs(surroundSync.model.transform.TransformVector(surroundSync.bounds.extents).x / totalWidthPerFace[dir]) * 2f >= 1f))
                {

                    if (om <= 1.1f && om >= -0.1f)
                    {
                        var mocks = FindObjectOfType<ModuleManager>().currentMocks;
                        if(Application.isPlaying)
                            foreach (var mock in mocks)
                                mock.gameObject.SetActive(true);
                        var rayPos = placementPos;
                        rayPos.y = 0.1f;

                        var tan = Quaternion.Euler(0, 90, 0) * normal;

                        if (!surroundSync.hasPlayfield || (!Physics.Raycast(rayPos, normal, 1f) && !Physics.Raycast(rayPos - tan.normalized * surroundSync.bounds.extents.x, normal, 1f) && !Physics.Raycast(rayPos - tan.normalized * surroundSync.bounds.extents.x * 0.5f, normal, 1f)))// || (mocks.Count() == 0 || mocks.Min(o => Vector3.Distance(o.transform.position, placementPos)) > 3f))
                        {
                            filteredSurroundSyncs.Add(surroundSync);
                            availableSurroundSyncs.RemoveAll(ss => ss.face == surroundSync.face && ss.mapToIndex < surroundSync.mapToIndex && !ss.baseReplacer);
                        }
                        if (Application.isPlaying)
                            foreach (var mock in mocks)
                                mock.gameObject.SetActive(false);
                    } else
                    {
                        //availableSurroundSyncs.Remove(surroundSync);
                        //Debug.Log("Removed: " + surroundSync.name);
                    }
                    if(indexInFace == faceCount)
                    {
                        lastTile = true;
                    }

                }

                   
            }
        }

        /*// check for forceCorner-cases
        if (filteredSurroundSyncs.Count == 0)
            if (isEdge)
                foreach (SurroundSync surroundSync in availableSurroundSyncs)
                    if (surroundSync.forceCorner)
                        filteredSurroundSyncs.Add(surroundSync);*/


        bool basePieceUsed = false;
        // if no specific-indexed-surroundsyncs found: take the replace-alls
        if (filteredSurroundSyncs.Count == 0)
        {
            basePieceUsed = true;
            foreach (SurroundSync surroundSync in availableSurroundSyncsC)
                if (surroundSync.baseReplacer && 
                    (surroundSync.baseReplaceFace1 && dir == WallFace.North ||
                    surroundSync.baseReplaceFace2 && dir == WallFace.East ||
                    surroundSync.baseReplaceFace3 && dir == WallFace.South ||
                    surroundSync.baseReplaceFace4 && dir == WallFace.West))
                    filteredSurroundSyncs.Add(surroundSync);
        }

        if (filteredSurroundSyncs.Count == 0)
        {
            
        }
        
        var perlinValWP = 0;

        GameObject wallPiece = null;
        SurroundSync chosenSync = null;
        int skips = 0;

        var highestPrioLowestIndex = filteredSurroundSyncs.Where(ss => ss.mapToIndex == filteredSurroundSyncs.Where(ss => ss.priority == filteredSurroundSyncs.Max(ss => ss.priority)).OrderBy(ss => ss.mapToIndex).First().mapToIndex);

        if (filteredSurroundSyncs.Count > 0)
        {
            var highestPrioList = highestPrioLowestIndex.OrderBy(ss => ss.offset).ToList();
            int index = 0;
            if (basePieceUsed)
                index = takeListOrder ? totalIndex % filteredSurroundSyncs.Count : (int)(Mathf.Clamp((perlinValWP + 1) / 2, 0f, 0.999f) * highestPrioList.Count);
//            Debug.Log(index);

            SurroundSync piece = null;

            if(basePieceUsed)
            {
                piece = filteredSurroundSyncs[index];
            }
            else
            {
                piece = highestPrioList[index];
            }
            wallPiece = piece.model;
       //     skips = piece.width - 1;
            chosenSync = piece;
        } else
        {
//            Debug.Log("No Base Tile Found");
        }
      
        if (defaultTile == null)
            Debug.LogError("[SurrounderModule](filterAWallPiece): defaultTile needs to be assigned in inspector");
        
        if (wallPiece == null)
        {
            wallPiece = defaultTile.model;
            chosenSync = defaultTile;
        }

        if (chosenSync == null)
        {
            Debug.Log("nulliyo");

        }
        

        return (wallPiece, skips, chosenSync, filteredSurroundSyncs, lastTile);
    }

    float totalLengthOfIndex(int i, WallFace face, List<SurroundSync> availableSurroundSyncs)
    {
        float res = 0;
        foreach(var ss in availableSurroundSyncs)
        {
            if (ss.baseReplacer || ss.face != face || ss.mapToIndex != i)
                continue;
            res += Mathf.Max(ss.model.transform.TransformVector(ss.bounds.extents).x * 1f, 0.05f);
        }
        return res;
    }

    public GameObject testBackdrop;
    void drawBackdrops()
    {

       var backdropPrefab = designAccess.GetCenterBackdrop();
      //  var prefab = designStorage.DesignArea.backdropper.;
        if (backdropPrefab != null && backdropPrefab.anchored && backdropPrefab.backdropAsset != null)
        {

        //var prefab = testBackdrop;
        var groundBox = getGroundBox();

        Matrix4x4 matrix =
            Matrix4x4.TRS((groundBox[0] + groundBox[1] + groundBox[2] + groundBox[3]) * 0.25f, Quaternion.Euler(0, -Vector3.SignedAngle((groundBox[3] - groundBox[2]), Vector3.forward, Vector3.up), 0), Vector3.one)
            * Matrix4x4.Translate(Vector3.up * backdropPrefab.backdropVerticalOffset)
            * Matrix4x4.Translate(Vector3.right * backdropPrefab.backdropDistance)
            * Matrix4x4.Translate(-Vector3.forward * backdropPrefab.backdropHorizontalOffset)

            * Matrix4x4.Rotate(Quaternion.Euler(0, backdropPrefab.backdropRotation + 180f, 0));

        if (Application.isPlaying)
        {
            if (backdropPrefab != null && backdropPrefab.backdropAsset != null)
                Instantiate(backdropPrefab.backdropAsset, matrix.ExtractPosition(), Quaternion.Euler(0, 90, 0) * matrix.ExtractRotation(), storyTeller.surroundSyncsParent);
        }
#if UNITY_EDITOR
            else
            {

            ToolWindowSettings.Instance.objectsToDrawInPreview.Add(
                new ToolWindowSettings.ObjectInfo(
                    -1,
                    backdropPrefab.backdropAsset,
                    matrix * Matrix4x4.Rotate(Quaternion.Euler(0, 90, 0))
                    ,
                    null,
                    null,
                    WallFace.North));

        }
#endif
        }

        var bdDict = designAccess.GetActiveBackdrops();
        foreach (var k in bdDict.Keys)
        {
            backdropPrefab = designAccess.GetActiveBackdrops()[k];
            if (!backdropPrefab.anchored)
                continue;
            if (backdropPrefab.backdropAsset == null)
                continue;
            //var prefab = testBackdrop;
            var groundBox = getGroundBox();

            Matrix4x4 matrix = 
                Matrix4x4.TRS((groundBox[(2 - (int)k + 4) % 4] + groundBox[(3 - (int)k + 4) % 4]) * 0.5f, Quaternion.Euler(0, -Vector3.SignedAngle((groundBox[(3 - (int)k + 4) % 4] - groundBox[(2 - (int)k + 4) % 4]), Vector3.forward, Vector3.up), 0), Vector3.one)
                * Matrix4x4.Translate(Vector3.up * backdropPrefab.backdropVerticalOffset)
                * Matrix4x4.Translate(Vector3.right * backdropPrefab.backdropDistance)
                * Matrix4x4.Translate(-Vector3.forward * backdropPrefab.backdropHorizontalOffset)

                * Matrix4x4.Rotate(Quaternion.Euler(0, backdropPrefab.backdropRotation + 180f, 0));
            
            if(Application.isPlaying)
            {
                if(backdropPrefab != null && backdropPrefab.backdropAsset != null)
                    Instantiate(backdropPrefab.backdropAsset, matrix.ExtractPosition(), Quaternion.Euler(0,90,0) * matrix.ExtractRotation(), storyTeller.surroundSyncsParent);
            }
#if UNITY_EDITOR
            else
            {

            ToolWindowSettings.Instance.objectsToDrawInPreview.Add(
                new ToolWindowSettings.ObjectInfo(
                    -1, 
                    backdropPrefab.backdropAsset,
                    matrix * Matrix4x4.Rotate(Quaternion.Euler(0, 90, 0))
                    ,  
                    null, 
                    null, 
                    WallFace.North));

            }

#endif
        }
    }

    /// <summary>
    /// Draws a wall piece directly for preview mode
    /// </summary>
    /// <param name="wallPiece">The wallPiece Prefab/GameObject</param>
    /// <param name="matrix">The transform matrix processed by the surrounder for this wallPiece</param>
    /// 
#if UNITY_EDITOR
        void drawWallPieceDirectly(GameObject wallPiece, WallMini surroundSync, Matrix4x4 matrix, float scale, WallFace face)
    {
        if (ToolWindowSettings.Instance.objectsToDrawInPreview == null)
            ToolWindowSettings.Instance.objectsToDrawInPreview = new List<ObjectInfo>();
        int id = ToolWindowSettings.Instance.objectsToDrawInPreview != null ? ToolWindowSettings.Instance.objectsToDrawInPreview.Count : 0;

        var addScale = wallPiece.transform.rotation * new Vector3(scale * 1, 1, 1);
        addScale = new Vector3(Mathf.Abs(addScale.x), Mathf.Abs(addScale.y), Mathf.Abs(addScale.z));
        ToolWindowSettings.Instance.objectsToDrawInPreview.Add(new ToolWindowSettings.ObjectInfo(id, wallPiece,matrix * Matrix4x4.Scale(addScale), surroundSync?.mappedSurroundSync?.transform, null, face));

        
    }
#endif


    /// <summary>
    /// Instantiats a wallPiece
    /// </summary>
    /// <param name="wallPiece">The wallPiece Prefab/GameObject</param>
    /// <param name="matrix">The transform matrix processed by the surrounder for this wallPiece</param>
    void InstantiateWallPiece(GameObject wallPiece, Matrix4x4 matrix, float additionalScale, SurroundSync surroundSync)
    {
    //    Debug.Log("INS");
        var m = matrix;
        Vector3 position = m.GetColumn(3);
        Quaternion rotation = Quaternion.LookRotation(
            m.GetColumn(2),
            m.GetColumn(1)
        );
        Vector3 scale = new Vector3(
            m.GetColumn(0).magnitude,
            m.GetColumn(1).magnitude,
            m.GetColumn(2).magnitude
        );
        GameObject o = null;
        if(surroundSync.baseReplacer)
            o = Instantiate(surroundSync.transform.GetChild(0).gameObject, position, rotation *wallPiece.transform.rotation,  storyTeller.surroundSyncsParent);
        else
        {
            //surroundSync.transform.GetChild(0).transform.localPosition = Vector3.zero;
            o = surroundSync.transform.GetChild(0).gameObject;
            o.transform.position = position;
            o.transform.rotation = rotation * wallPiece.transform.rotation;
            surroundSync.transform.parent = storyTeller.surroundSyncsParent;
        }
        o.transform.localScale = (scale.ScaleBy(((new Vector3(additionalScale, 1,1)))));
        if (Mathf.Abs( wallPiece.transform.localEulerAngles.y) == 90)
            o.transform.localScale = o.transform.localScale.flip();
    }


    /// <summary>
    /// Compute the forceCorner objects
    /// </summary>
    /// <param name="outerOrInnerEdge">List of bools that specify wether a layout point is an inner or an outer forceCorner</param>
    void computeEdgeObjects(List<bool> outerOrInnerEdge)
    {
        for (int i = 0; i < designAccess.GetStoredLayoutPoints().Count; i++)
        {
            var objToUse = outerOrInnerEdge[i] ? outerEdgeObject : innerEdgeObject;

            if (objToUse != null)
            {
                foreach (var mf in objToUse.GetComponentsInChildren<MeshFilter>())
                {
                    var mr = mf.GetComponent<MeshRenderer>();
                    if (mr != null)
                    {
                        Graphics.DrawMesh(mf.sharedMesh, Matrix4x4.Translate(designAccess.GetActiveRoomLayout().GetLayoutPositions()[i]) * mf.transform.localToWorldMatrix, mr.sharedMaterial, 0);
                    }
                }
            }

        }
    }
    
    /// <summary>
    /// Gets the surround Syncs according to Surrounder Settings and Design Space
    /// </summary>
    /// <returns>List of SurroundSyncs</returns>
    /*private List<SurroundSync> GetSurroundSyncs()
    {
        if (!useManualList) return designAccess.GetSurroundSyncs();
        if (manualList.Count == 0)
            Debug.LogWarning("Manual SurroundSync List is empty but you're trying to use it! Check the Surrounder's inspector ");
        return manualList;

    }*/

    private void OnValidate()
    {
//        FindObjectOfType<ModuleManager>().UpdatePipeline();
    }
}
