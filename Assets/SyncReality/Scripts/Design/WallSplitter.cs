using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OVR.OpenVR;
using UnityEditor;
using UnityEngine; 

// how to handle wallpieces and their content, when wallpieces are destroyed and rebuilt repeatedly?
// one solution: create for each Face a persistent dictionary of <int, SurroundSync> 
// this dict would only reset on total reset



#if UNITY_EDITOR
// Handles the creation and updating of WallPieces for the Surrounder
[ExecuteInEditMode]
public class WallSplitter : MonoBehaviour
{
     [Header("Assigns")]
     public DesignArea designArea; 
     public GameObject wallPiecePrefab;
     public GameObject wallPiecesParent;
     public GameObject wallMiniPrefab;
     [Header("Settings")] 
     public bool spawnMiniModel;
     public float wallPieceHeight = 1f;
     public float wallPieceDepth = 0.2f;
     public float miniHeightOffset = 0.3f;
     public float miniScale = 0.2f;
     public float miniHeightAdd;
     public float miniFrontOffset;
     public float miniMeshHeight;
     public Material hiddenMiniMaterial;
     public Material baseMiniMaterial;

     private int _previousAllAmount;
     private int _previousNorthAmount;
     private int _previousEastAmount;
     private int _previousSouthAmount;
     private int _previousWestAmount;
     private float _prevWallPieceHeight;
     private float _prevWallPieceDepth;
     private Material _prevMaterial;
   //  private Dictionary<WallFace, List<WallPiece>> _wallPiecesPerFace;
     private Dictionary<int, List<WallPiece>> _wallPiecesPerWallNumber;
     
     private List<GameObject> _miniPieces; 
     
     private void Update()
     {
        if (Application.isPlaying)
            return;
          CheckForChanges();

     }
     
     public void RedrawWallPieces(int index = 0, WallFace face = WallFace.North )
     { 
#if UNITY_EDITOR 
          DestroyWallPieces();
          DestroyWallMinis();
          SpawnWallPieces();
          ConnectSurroundSyncsToWallPieces();
          DestroyExampleMinis();
          SpawnExampleMinis();
          SpawnHiddenMinis();
          
          if (index == 0) return;
          foreach (var mini in _miniPieces)
               if (mini.GetComponent<WallMini>().index == index && mini.GetComponent<WallMini>().face == face)
                    Selection.activeGameObject = mini.GetComponent<WallMini>().mappedSurroundSync.gameObject; 
#endif
     }

  


     private void DestroyWallPieces()
     {
          /*if (_wallPiecesPerFace == null) return;
          foreach (var wallPiece in from wallPieceList in _wallPiecesPerFace.Values from wallPiece in wallPieceList where wallPiece.gameObject != null select wallPiece)
               if (wallPiece != null)
                DestroyImmediate(wallPiece.gameObject);*/
          
          if (wallPiecesParent != null)
          {
               foreach (var rt in wallPiecesParent.GetComponentsInChildren<Transform>())
                    if (rt != transform && rt != wallPiecesParent.transform)
                         if (rt != null)
                              DestroyImmediate(rt.gameObject);
          }
     }
     private void DestroyWallMinis()
     { 
     
               foreach (var rt in transform.GetComponentsInChildren<WallMini>())
                    if (rt != transform  )
                         if (rt != null)
                              DestroyImmediate(rt.gameObject);
        
     }
  
     private void SpawnWallPieces()
     {
          /*_wallPiecesPerFace = new Dictionary<WallFace, List<WallPiece>>
          {
               {WallFace.North, new List<WallPiece>()}, {WallFace.East, new List<WallPiece>()},
               {WallFace.South, new List<WallPiece>()}, {WallFace.West, new List<WallPiece>()}
          };  */
          _wallPiecesPerWallNumber = new Dictionary<int, List<WallPiece>>
          {
               {1, new List<WallPiece>()}, {2, new List<WallPiece>()},
               {3, new List<WallPiece>()}, {4, new List<WallPiece>()}
          };
          var syncSetup = designArea.GetFirstActivePresentSyncSetup(); 

          
          // get designPlane starting points and face distances
          var localScale = designArea.FlexFloorProp.transform.localScale;
          var position = designArea.FlexFloorProp.transform.position;
          float xMin = localScale.x* designArea.transform.localScale.x * -5f+ position.x;
          float xMax = localScale.x * designArea.transform.localScale.x* 5f  + position.x;
          float zMin = localScale.z * designArea.transform.localScale.z* -5f+ position.z;
          float zMax = localScale.z* designArea.transform.localScale.z *  5f + position.z;
          Vector2 cornerNE = new Vector2(xMin, zMin);
          Vector2 cornerSE = new Vector2(xMin, zMax);
          Vector2 cornerSW = new Vector2(xMax, zMax);
          Vector2 cornerNW = new Vector2(xMax, zMin);
          float distanceNorth = Vector2.Distance(cornerNW, cornerNE);
          float distanceEast = Vector2.Distance(cornerNE, cornerSE);
          float distanceSouth= Vector2.Distance(cornerSE, cornerSW);
          float distanceWest = Vector2.Distance(cornerSW, cornerNW);

          // Calculate size of individual pieces
          float sizeNorth = distanceNorth / syncSetup.wallPiecesOne;
          float sizeEast  = distanceEast  / syncSetup.wallPiecesTwo ;
          float sizeSouth  = distanceSouth  / syncSetup.wallPiecesThree ;
          float sizeWest  = distanceWest  / syncSetup.wallPiecesFour ;
          
            for (int i = 0; i < syncSetup.wallPiecesOne; i++)
            {
                GameObject pieceObj = Instantiate(wallPiecePrefab );
                pieceObj.hideFlags = HideFlags.HideInHierarchy;
                pieceObj.name = "WP_NORTH_" + i;
                pieceObj.transform.localScale = new Vector3(sizeNorth,wallPieceHeight, wallPieceDepth);
                pieceObj.transform.position = new Vector3(cornerNW.x - (sizeNorth/2f) - (i*sizeNorth), wallPieceHeight/2f , cornerNW.y);
                ProcessCreatedWallPiece(pieceObj, 1, i);
            }
            for (int i = 0; i < syncSetup.wallPiecesTwo; i++)
            {
                 GameObject pieceObj = Instantiate(wallPiecePrefab);
                 pieceObj.hideFlags = HideFlags.HideInHierarchy;
                pieceObj.name = "WP_EAST_" + i; 
                pieceObj.transform.localScale = new Vector3(wallPieceDepth,wallPieceHeight, sizeEast);
                pieceObj.transform.position = new Vector3(cornerNE.x , wallPieceHeight/2f , cornerNE.y+ (sizeEast/2f) + (i*sizeEast));  
                ProcessCreatedWallPiece(pieceObj, 2, i);

            }
            for (int i = 0; i < syncSetup.wallPiecesThree; i++)
            {
                 GameObject pieceObj = Instantiate(wallPiecePrefab);
                 pieceObj.hideFlags = HideFlags.HideInHierarchy;
                pieceObj.name = "WP_SOUTH_" + i; 
                pieceObj.transform.localScale = new Vector3(sizeSouth,wallPieceHeight, wallPieceDepth);
                pieceObj.transform.position = new Vector3(cornerSE.x + (sizeSouth/2f) + (i*sizeSouth), wallPieceHeight/2f , cornerSW.y); 
                ProcessCreatedWallPiece(pieceObj, 3, i);
            }
            for (int i = 0; i < syncSetup.wallPiecesFour; i++)
            {
                 GameObject pieceObj = Instantiate(wallPiecePrefab);
                 pieceObj.hideFlags = HideFlags.HideInHierarchy;
                pieceObj.name = "WP_WEST_" + i; 
                pieceObj.transform.localScale = new Vector3(wallPieceDepth,wallPieceHeight, sizeWest);
                pieceObj.transform.position = new Vector3(cornerSW.x , wallPieceHeight/2f , cornerSW.y - (sizeWest/2f) - (i*sizeWest)); 
                ProcessCreatedWallPiece(pieceObj, 4, i);

            }
          
     }
     
     private void ProcessCreatedWallPiece(GameObject pieceObject, int number, int index)
     {
          pieceObject.GetComponent<WallPiece>().face = designArea.compass.GetWallFaceForNumber(number);
          pieceObject.GetComponent<WallPiece>().index = index;
          pieceObject.GetComponent<WallPiece>().anchoredSurroundSyncs = new List<SurroundSync>();
          pieceObject.GetComponent<MeshRenderer>().material = _prevMaterial;
          pieceObject.transform.SetParent(wallPiecesParent.transform);
          _wallPiecesPerWallNumber[number].Add(pieceObject.GetComponent<WallPiece>());
     }
     
     private void ConnectSurroundSyncsToWallPieces()
     {
          var faceSurroundSyncs = GetListForWallNumber(1);
          var syncSetup = designArea.GetFirstActivePresentSyncSetup(); 

          foreach (SurroundSync surroundSync in faceSurroundSyncs)
               if (surroundSync != null)
                    if (surroundSync.mapToIndex < syncSetup.wallPiecesOne)
                         _wallPiecesPerWallNumber[1][surroundSync.mapToIndex].anchoredSurroundSyncs.Add(surroundSync);
          faceSurroundSyncs = GetListForWallNumber(2);
          foreach (SurroundSync surroundSync in faceSurroundSyncs)
               if (surroundSync != null)
                    if (surroundSync.mapToIndex < syncSetup.wallPiecesTwo) 
                         _wallPiecesPerWallNumber[2][surroundSync.mapToIndex].anchoredSurroundSyncs.Add(surroundSync);
          faceSurroundSyncs = GetListForWallNumber(3);
          foreach (SurroundSync surroundSync in faceSurroundSyncs)
               if (surroundSync != null)
                    if (surroundSync.mapToIndex <syncSetup.wallPiecesThree) 
                         _wallPiecesPerWallNumber[3][surroundSync.mapToIndex].anchoredSurroundSyncs.Add(surroundSync);
          faceSurroundSyncs = GetListForWallNumber(4);
          foreach (SurroundSync surroundSync in faceSurroundSyncs)
               if (surroundSync != null)
                    if (surroundSync.mapToIndex < syncSetup.wallPiecesFour) 
                         _wallPiecesPerWallNumber[4][surroundSync.mapToIndex].anchoredSurroundSyncs.Add(surroundSync);
     }
     public List<SurroundSync> GetListForWallNumber(int number)
     {
          List<SurroundSync> returnList = new List<SurroundSync>();
          foreach (var surroundSync in designArea.GetSurroundSyncs())
          {
               if (surroundSync != null)
                    if (surroundSync.face == designArea.compass.GetWallFaceForNumber(number))
                         returnList.Add(surroundSync);
          } 
          return returnList; 
     }
     private void SpawnExampleMinis()
     {
          foreach (var wallPiece in GetAllWallPieces())
               if  (wallPiece.anchoredSurroundSyncs != null)
                   if (wallPiece.anchoredSurroundSyncs.Count > 0) 
                         SpawnMinisForWallPiece(wallPiece, wallPiece.anchoredSurroundSyncs.OrderBy(w => w.offset).ToList()); 
     }
    
       private void SpawnMinisForWallPiece(WallPiece wallPiece, List<SurroundSync> spawnables, bool spawnAsHidden = false  )
       { 
          float heightOffsetTracker = miniHeightOffset;
          foreach (var ssync in spawnables)
          {
               Vector3 position = wallPiece.transform.position; 
               int rotateAmount = 0;
               if (wallPiece.face == WallFace.East)
                    rotateAmount = 90;
               else   if (wallPiece.face == WallFace.South)
                    rotateAmount = 180;
               else   if (wallPiece.face == WallFace.West)
                    rotateAmount = 270;

               if (wallPiece.face == WallFace.North) // Z+
               {
                    position = new Vector3(position.x, position.y  + miniHeightAdd + heightOffsetTracker, position.z + miniFrontOffset );
                    if (spawnAsHidden)
                         position += new Vector3(0, 0, -1.5f);
                    
               }
               else if (wallPiece.face == WallFace.East) // X+
               {
                    position = new Vector3(position.x  + miniFrontOffset, position.y  + miniHeightAdd + heightOffsetTracker, position.z );
                    if (spawnAsHidden)
                         position += new Vector3(   -1.5f,0,0);
               }
               else if (wallPiece.face == WallFace.South) // Z-
               {
                    position = new Vector3(position.x, position.y  + miniHeightAdd + heightOffsetTracker, position.z  - miniFrontOffset);
                    if (spawnAsHidden)
                         position += new Vector3(0, 0, 1.5f);
               }
               else if (wallPiece.face == WallFace.West) // X-
               {
                    position = new Vector3(position.x  - miniFrontOffset, position.y  + miniHeightAdd + heightOffsetTracker, position.z );
                    if (spawnAsHidden)
                         position += new Vector3(   1.5f,0,0);
               }
               
               heightOffsetTracker += miniHeightOffset;
 
               if (ssync.bounds.extents == Vector3.zero)
                    ssync.CreateCombinedMesh();
               
               GameObject miniObj = Instantiate(wallMiniPrefab  );
            
               miniObj.transform.position = position; 
               miniObj.GetComponent<WallMini>().InsertSurroundSync( ssync);
               miniObj.GetComponent<WallMini>().meshObject.transform.position += new Vector3(0, miniMeshHeight, 0);
               if (spawnAsHidden)
                    miniObj.GetComponent<WallMini>().meshObject.GetComponent<MeshRenderer>().material = hiddenMiniMaterial; 
               if (ssync.baseReplacer)
                    miniObj.GetComponent<WallMini>().meshObject.GetComponent<MeshRenderer>().material = baseMiniMaterial;
               miniObj.name = "WallMini_SS_" + ssync.face + "_" + ssync.mapToIndex;
              
               
               GameObject childObj = (ssync.transform.GetChild(0).gameObject); 

               
               if (spawnMiniModel )
                    childObj = Instantiate(ssync.model, miniObj.transform); 
               
               
               
               
           

            
 
               float miniBoundsHeight = miniObj.GetComponent<WallMini>().meshObject.sharedMesh.bounds.size.y  ;
               float miniObjectBoundsWidth = ssync.bounds.extents.z; 
               float modelYSize = ssync.bounds.extents.y; 
               float modelScale = miniBoundsHeight * miniScale / modelYSize * 0.9f; 
               Vector3 miniObjPosition = new Vector3(0,  -miniBoundsHeight / 2.2f,0);
               miniObjPosition += new Vector3( miniObjectBoundsWidth*modelScale  - (miniObjectBoundsWidth*modelScale/8f) ,0,miniFrontOffset); 
               
           //   miniContentObj.transform.localPosition = miniObjPosition; 
          //  miniContentObj.transform.position = new Vector3(99, 0, 99);


          if (spawnMiniModel)
          {
               childObj.transform.localPosition = miniObjPosition; 
               childObj.transform.localScale = new Vector3(modelScale, modelScale, modelScale); 
          }
          else
          {
               ssync.transform.position =miniObj.transform.position + miniObjPosition ;  
               ssync.transform.Rotate(new Vector3(0,rotateAmount,0));
               ssync.transform.eulerAngles = new Vector3(
                    ssync.transform.eulerAngles.x,
                     rotateAmount,
                    ssync.transform.eulerAngles.z
               ); 
               miniObj.transform.Rotate(new Vector3(0,rotateAmount,0));
          } 
               miniObj.transform.parent = this.transform;
               _miniPieces.Add(miniObj);
               wallPiece.anchoredWallMini = miniObj;  
          }
          FindObjectOfType<ModuleManager>().UpdatePipeline();  
     }
       private void SpawnHiddenMinis()
       {
            Dictionary<int,  List<SurroundSync>> hiddenSurroundSyncs = new  Dictionary<int,  List<SurroundSync>>
            {
                 {1, new List<SurroundSync>()},
                 {2, new List<SurroundSync>()},
                 {3, new List<SurroundSync>()},
                 {4, new List<SurroundSync>()}
            };
            foreach (var ssync in designArea.GetSurroundSyncs())
            {
                 int faceSize = designArea.GetIndexAmountForWallFace(ssync.face);
                 if (ssync.mapToIndex >= faceSize)
                      hiddenSurroundSyncs[designArea.compass.GetNumberForFace(ssync.face)].Add(ssync);
            }

            for (int i = 1; i < 5; i++)
                 if (hiddenSurroundSyncs[i].Count > 0)
                 {
                      WallPiece lastPieceInLine = _wallPiecesPerWallNumber[i][_wallPiecesPerWallNumber[i].Count-1];
                      SpawnMinisForWallPiece(lastPieceInLine, hiddenSurroundSyncs[i], true); 
                 } 
       }
     private void DestroyExampleMinis()
     {
          if (_miniPieces != null)
             foreach (var mini in _miniPieces)
               if (mini != null)
                    DestroyImmediate(mini);
          _miniPieces = new List<GameObject>();
     }

 
 


     #region CalledFromDesignArea
     public void ResetToDefaultAmounts()
     {
          var syncSetup = designArea.GetFirstActivePresentSyncSetup(); 
          syncSetup.wallPiecesOne = designArea.defaultWallSegments;
          syncSetup.wallPiecesTwo = designArea.defaultWallSegments;
          syncSetup.wallPiecesThree = designArea.defaultWallSegments;
          syncSetup.wallPiecesFour = designArea.defaultWallSegments; 
          _previousAllAmount = designArea.defaultWallSegments;
          _previousNorthAmount = designArea.defaultWallSegments;
          _previousEastAmount = designArea.defaultWallSegments;
          _previousSouthAmount= designArea.defaultWallSegments;
          _previousWestAmount= designArea.defaultWallSegments;
     }
  


    
     public void InitializeWallSplitter()
     {
        

     }
     public void ResetWallSplitter()
     { 
          foreach (var rt in GetComponentsInChildren<Transform>())
               if (rt != this.transform && rt != wallPiecesParent.transform)
                    if (rt != null)
                         DestroyImmediate(rt.gameObject);
          // Initialize and Redraw are called from DesignArea, no need to do so here
     }
     #endregion
     
     
     #region HelperFunctions
     
     private void CheckForChanges()
     {
          bool registerChange = false;
          var syncSetup = designArea.GetFirstActivePresentSyncSetup(); 

          if (_prevMaterial != designArea.wallPieceMaterial)
          {
               registerChange = true;
               _prevMaterial = designArea.wallPieceMaterial;
          }

          if (_prevWallPieceHeight != wallPieceHeight)
          {
               registerChange = true;
               _prevWallPieceHeight = wallPieceHeight;
          }
          if (_prevWallPieceDepth != wallPieceDepth)
          {
               registerChange = true;
               _prevWallPieceDepth = wallPieceDepth;
          } 
          else if (_previousNorthAmount != syncSetup.wallPiecesOne)
          {
               _previousNorthAmount = syncSetup.wallPiecesOne;
               registerChange = true;
          }
          else if (_previousEastAmount != syncSetup.wallPiecesTwo)
          {
               _previousEastAmount = syncSetup.wallPiecesTwo;
               registerChange = true;
          }
          else if (_previousSouthAmount != syncSetup.wallPiecesThree)
          {
               _previousSouthAmount = syncSetup.wallPiecesThree;
               registerChange = true;
          }
          else if (_previousWestAmount != syncSetup.wallPiecesFour)
          {
               _previousWestAmount = syncSetup.wallPiecesFour;
               registerChange = true;
          }

          if (registerChange)
               designArea.WallPieceAmountChanged();
     }
     #endregion


     private List<WallPiece> GetAllWallPieces()
     {
          if (_wallPiecesPerWallNumber == null)
               RedrawWallPieces();
          return _wallPiecesPerWallNumber.Values.SelectMany(list => list).ToList();
     }
     private List<WallMini> GetAllWallMinis()
     { 
          return transform.GetComponentsInChildren<WallMini>().ToList();
     }
     public void DeleteSurroundSync(SurroundSync surroundSync, bool directlyDeleted = false)
     {
          SyncMaster _syncMaster = FindObjectOfType<SyncMaster>();
          if (_syncMaster != null)
          {
               _syncMaster.DeleteSyncLinksForSurroundSync(surroundSync);
               if (directlyDeleted == false)
               {
                    GameObject toDelete = null;
                    foreach (var ssync in designArea.GetSurroundSyncs())
                         if (surroundSync == ssync)
                              toDelete = ssync.gameObject; 
                    if (toDelete == null)
                         Debug.LogWarning("Trying to delete SurroundSync - did not find gameobject for " + surroundSync.ID);
                    DestroyImmediate(toDelete);
               } 
               designArea.InitializeDesignArea();
          } 
     }
   
 
     /*public void SelectWallMiniForWallPiece(WallPiece wallPiece)
     {
#if UNITY_EDITOR
          Selection.activeObject = wallPiece.anchoredWallMini;
          #endif
     }

     public void DeleteSurroundSyncForWallPiece(WallPiece wallPiece)
     {
          /*SurroundSync deleteSync = null;
          foreach (var ss in wallSaver.storedSurroundSyncs.Where(ss => ss.face == wallPiece.face && ss.mapToIndex == wallPiece.index))
               deleteSync = ss;
          if (deleteSync != null)
               wallSaver.storedSurroundSyncs.Remove(deleteSync);
          RedrawWallPieces();#1#
     }*/

     public WallMini GetWallMiniForSurroundSyncID(string ssID)
     {
          foreach (var wallMini in GetAllWallMinis())
               if (wallMini != null)
                    if (wallMini.GetComponent<WallMini>().mappedSurroundSync != null)
                           if (wallMini.GetComponent<WallMini>().mappedSurroundSync.ID == ssID)
                                 return wallMini;
          Debug.LogWarning("Did not find a SurroundSync for SSync: " + ssID);
          return null;
     }

     public WallPiece FindNearestWallPiece(Vector3 transformPosition)
     {
          WallPiece nearestWallPiece = null;
          float smallestDistance = Mathf.Infinity;
          foreach (var wallPiece in GetAllWallPieces())
               if (Vector3.Distance(wallPiece.transform.position, transformPosition) < smallestDistance)
               {
                    smallestDistance = Vector3.Distance(wallPiece.transform.position, transformPosition);
                    nearestWallPiece = wallPiece;
               }
       
                    
          if (nearestWallPiece == null)
               Debug.LogWarning("Returning a null for nearest wallpiece - this is a problem!");
          return nearestWallPiece;
     }


     public void CheckForWallMiniChanges()
     {
          if (_miniPieces != null)
               foreach (var wallMiniObj in _miniPieces) 
                    if (wallMiniObj != null)
                         wallMiniObj.GetComponent<WallMini>().CheckForChanges(); 
     }

    
}
#endif