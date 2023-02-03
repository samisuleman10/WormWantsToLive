using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FingerTapActionPointCreateBase : FTAEditMesh
{
    
    [Header("Create Mesh Config")]
    public LineRenderer LineMaker;
    public Material CreatedMeshMaterial;

    // Adjustment for the top point of object bellow a newly created object, so to extend its bottom
    public float TopBottomObjAdjust = 0.025f;

    protected Dictionary<GameObject, int> _lineRendererPositions = new Dictionary<GameObject, int>();
    protected bool _isHoldingMarker = false;

    protected float _heightDiffToConsiderFloorPoint = 0.02f;
    protected bool _createMeshUntilFloor = false;

    protected string _baseObjName = "BaseObj";

    private Transform _markerOldParent;
    private GameObject _markerHolding;

    private int _currentLineRendererHoldPosition = -1;


    protected static List<GameObject> CreatedVolumes = new List<GameObject>();

    public sealed override void OnConfirm(ScannedObjectsClassificationType objectTypes = ScannedObjectsClassificationType.NonSelected, bool creationDelay = true)
    {
        if (_IsCurrentlySelecting)
        {
            base.OnConfirm(creationDelay: creationDelay);
            ClearPoints();
            return;
        }
        OnConfirmMeshCreate(objectTypes, creationDelay);
    }

    public sealed override void OnCancel(GameObject target, GameObject cursorPosition)
    {
        OnCancelMeshCreate(target, cursorPosition);
        ClearPoints();
        base.OnCancel(target, cursorPosition);
    }

    public sealed override void OnTap(GameObject target, GameObject cursor)
    {
        //is interacting and edit is still runing, so go to edit
        if (_IsCurrentlySelecting)
        {
            base.OnTap(target, cursor);
            return;
        }

        OnTapMeshCreate(target, cursor);
    }



    protected virtual void OnCancelMeshCreate(GameObject target, GameObject cursor)
    { }

    protected virtual void OnConfirmMeshCreate(ScannedObjectsClassificationType objectTypes = ScannedObjectsClassificationType.NonSelected, bool creationDelay = true)
    {
        if (!_isWaitingCreatingDelay && pointMarkers.Count > 2)
        {
            if (creationDelay)
                StartCoroutine(WaitCreationDelay());
            CreateMeshFromPoints(pointMarkers, _createMeshUntilFloor);
            ClearPoints();
        }
    }

    protected virtual void OnTapMeshCreate(GameObject target, GameObject cursor)
    {
        //is carring a marker
        if (_isHoldingMarker)
        {
            _markerHolding.transform.SetParent(_markerOldParent, true);
            _markerOldParent = null;
            _markerHolding = null;
            _isHoldingMarker = false;
            return;
        }

        //Tapped on a marker
        if (_currentPreSelectMarker != null)
        {
            _markerOldParent = _currentPreSelectMarker.transform.parent;
            _markerHolding = _currentPreSelectMarker;
            _markerHolding.transform.SetParent(cursor.transform, true);
            _markerHolding.transform.position = cursor.transform.position;

            if (!_lineRendererPositions.TryGetValue(_currentPreSelectMarker, out _currentLineRendererHoldPosition))
                _currentLineRendererHoldPosition = -1;

            _isHoldingMarker = true;
            return;
        }

        // Havent just created an object
        if (!_isWaitingCreatingDelay)
        {
            //FTAEditMesh.ChangeModuleStatus(false);
            CreateNewMarker(cursor.transform.position, cursor);
            return;
        }
    }

    private void OnEnable()
    {
        SetLineRenderer();
    }


    public override void Update()
    {
        if(_isHoldingMarker && _currentLineRendererHoldPosition != -1)
        {
            LineMaker.SetPosition(_currentLineRendererHoldPosition, _markerHolding.transform.position);
        }
        if(_IsCurrentlySelecting)
            base.Update();
    }



    protected override void ClearPoints()
    {
        base.ClearPoints();

        _lineRendererPositions.Clear();
        LineMaker.positionCount = 0;
        _currentLineRendererHoldPosition = -1;
        _isHoldingMarker = false;
    }

    private void SetLineRenderer()
    {
        if(LineMaker == null)
        {
            var liner = GetComponent<LineRenderer>();
            if (liner == null)
            {
                liner = this.gameObject.AddComponent<LineRenderer>();
                liner.positionCount = 0;
                liner.startWidth = 0.01f;
                liner.endWidth = 0.01f;
                liner.loop = true;
            }
            LineMaker = liner;
        }
    }


    protected override GameObject CreateNewMarker(Vector3 position, bool waitCreation = true)
    {
        if (_IsCurrentlySelecting)
        {
            return base.CreateNewMarker(position,  waitCreation);
        }


        var newMarker = base.CreateNewMarker(position,  waitCreation);
        if(newMarker == null)
            return null;

        
        LineMaker.positionCount = pointMarkers.Count;
        LineMaker.SetPosition((pointMarkers.Count - 1), newMarker.transform.position);

        //Get the object that will collide. 
        var collider = newMarker.GetComponent<Collider>();
        if (collider == null)
            collider = newMarker.GetComponentInChildren<Collider>();

        _lineRendererPositions.Add(collider == null ? newMarker : collider.gameObject, (pointMarkers.Count - 1));

        return newMarker;
    }

    public override void OnUndo()
    {
        if (CreatedVolumes.Count > 0)
        {
            var toRemove = CreatedVolumes[CreatedVolumes.Count - 1];
            CreatedVolumes.RemoveAt(CreatedVolumes.Count - 1);
            GameObject.Destroy(toRemove);
        }
    }

    protected override void RemoveMarker(int position)
    {
        if (position < 0 || position >= LineMaker.positionCount)
            return;

        for (int i = position; i < LineMaker.positionCount-1; i++)
        {
            LineMaker.SetPosition(i, LineMaker.GetPosition(i + 1));
        }

        LineMaker.positionCount--;
        var markerToRemove = pointMarkers[position];

        _lineRendererPositions.Remove(markerToRemove);

        base.RemoveMarker(position);
    }

    public virtual GameObject CreateMeshFromPoints(List<GameObject> meshPoints, bool alwaysGoToFloor = false)
    {
        //TODO needs to sort order of vertex for clockwise
        //

        var RootSpam = RoomSelector.GetCurrentSelectedRoom();
        if (RootSpam == null)
            return null;


        if (meshPoints.Count < 3)
            return null;

        GameObject newMeshObj = new GameObject(_baseObjName);

        //Save GameObject to Undo List
        CreatedVolumes.Add(newMeshObj);

        newMeshObj.transform.SetParent(RootSpam.transform, true);


        var newMeshFilter = newMeshObj.AddComponent<MeshFilter>();
        var newMeshRenderer = newMeshObj.AddComponent<MeshRenderer>();


        List<Vector3> pointList = meshPoints.ConvertAll<Vector3>(x => x.transform.position);
        int pointcount = pointList.Count;
        List<Vector3> pointsOnFloor = new List<Vector3>();

        float bottomHeight = 0f;
        RaycastHit hit;


        float _lowestHeight = float.MaxValue;
        float _highestHeight = float.MinValue;

        List<Vector3> _highPoints = new List<Vector3>();
        List<Vector3> _lowPoints = new List<Vector3>();
        
        //Finds highest and lowest point
        foreach (var point in pointList)
        {
            if (point.y > _highestHeight)
                _highestHeight = point.y;
            if (point.y < _lowestHeight)
                _lowestHeight = point.y;
        }

        //Find points close to highest and lowest point
        foreach (var point in pointList)
        {
            if (point.y > (_highestHeight - _heightDiffToConsiderFloorPoint))
            {
                _highPoints.Add(point);
            }
            
            if (point.y < (_lowestHeight + _heightDiffToConsiderFloorPoint))
            {
                _lowPoints.Add(point);
            }
        }

        #region Mesh by surface points
        //if (_lowPoints.Count   == _highPoints.Count && _lowPoints.Count > 2)
        //{ 
            //check for objects above ground
            if (!alwaysGoToFloor)
                foreach (var point in pointList)
                {
                    if (Physics.Raycast(point, Vector3.down, out hit, point.y))
                    {
                        bottomHeight = hit.collider.bounds.max.y - TopBottomObjAdjust;
                        break;
                    }
                }

            //Adds floor points
            foreach (var point in pointList)
            {
                pointsOnFloor.Add(new Vector3(point.x, bottomHeight, point.z));
            }
            pointList.AddRange(pointsOnFloor);

           
        //}
        #endregion
        
        //region Mesh by Top bigger
        //region Mesh by Low bigger
        //region Mesh by Top and low equal

        //Get midpoint
        Vector3 pointSum = Vector3.zero;
        foreach (var point in pointList)
        {
            pointSum += point;  
        }
        var midPoint = pointSum/pointList.Count;

        //Starts Mesh
        Mesh newMesh = new Mesh();
        newMesh.SetVertices(pointList);

        //Starts triangules
        List<int> triangules = new List<int>();

        #region upper face


        Vector3 faceDirection, faceMidPoint;
        float faceToCenterDiff;



        //Upper face
        for(int i = 0; i < (pointcount - 2); i++)
        {
            //calculate face side
            faceDirection = Vector3.Cross(pointList[i+1] - pointList[0], pointList[i+2] - pointList[0]);
            faceMidPoint = new Vector3(midPoint.x, pointList[0].y, midPoint.z);
            //B-A
            faceToCenterDiff = Vector3.Dot(faceDirection, midPoint - faceMidPoint);

            if (faceToCenterDiff <= 0)
            {
                triangules.Add(0);
                triangules.Add(i + 1);
                triangules.Add(i + 2);
            }
            else
            {
                triangules.Add(i + 2);
                triangules.Add(i + 1);
                triangules.Add(0);
            }
        }
#endregion Upper Face

        #region Side faces
        ///
        //Side Faces
        for (int i = 0; (i < (pointcount -1)) && (i < (pointList.Count-1)) && ((i+pointcount) < pointList.Count); i++)
        {
            faceDirection = Vector3.Cross(pointList[i+1] - pointList[i], pointList[i+pointcount] - pointList[i]);
            faceMidPoint = (pointList[i] + pointList[i + 1] + pointList[i + pointcount] + pointList[i + pointcount + 1]) / 4;
            faceToCenterDiff = Vector3.Dot(faceDirection, midPoint - faceMidPoint);
            if (faceToCenterDiff <= 0)
            {
                triangules.Add(i);
                triangules.Add(i + 1);
                triangules.Add(i + pointcount);
                triangules.Add(i + pointcount);
                triangules.Add(i + 1);
                triangules.Add(i + pointcount + 1);
            }
            else
            {
                triangules.Add(i + pointcount + 1);
                triangules.Add(i + 1);
                triangules.Add(i + pointcount);
                triangules.Add(i + pointcount);
                triangules.Add(i + 1);
                triangules.Add(i);
            }
        }

        //Last side face (has to go back)

        faceDirection = Vector3.Cross(pointList[pointcount - 1] - pointList[0], pointList[pointcount] - pointList[0]);
        faceMidPoint = (pointList[0] + pointList[pointcount-1] + pointList[pointcount] + pointList[pointList.Count - 1]) / 4;
        faceToCenterDiff = Vector3.Dot(faceDirection, midPoint - faceMidPoint);
        if (faceToCenterDiff <= 0)
        {
            triangules.Add(0);
            triangules.Add(pointcount - 1);
            triangules.Add(pointcount);
            triangules.Add(pointcount);
            triangules.Add(pointcount - 1);
            triangules.Add(pointList.Count - 1);
        }
        else
        {
            triangules.Add(pointList.Count - 1);
            triangules.Add(pointcount - 1);
            triangules.Add(pointcount);
            triangules.Add(pointcount);
            triangules.Add(pointcount - 1);
            triangules.Add(0);
        }
        #endregion


        faceDirection = Vector3.Cross(pointList[pointcount + 1] - pointList[pointcount], pointList[pointcount+2] - pointList[pointcount]);
        faceMidPoint = new Vector3(midPoint.x, pointList[pointcount].y, midPoint.z);
        faceToCenterDiff = Vector3.Dot(faceDirection, midPoint - faceMidPoint);

        //Down face 
        for (int i = pointcount; i < (pointList.Count - 2); i++)
        {
            if (faceToCenterDiff <= 0)
            {
                triangules.Add(pointcount);
                triangules.Add(i + 1);
                triangules.Add(i + 2);
            }
            else
            {
                triangules.Add(i + 2);
                triangules.Add(i + 1);
                triangules.Add(pointcount);
            }
        }


        newMesh.SetTriangles(triangules, 0, true);

        //End Mesh

        newMeshRenderer.material = CreatedMeshMaterial;
        newMeshFilter.mesh = newMesh;
        

        newMeshObj.AddComponent<MeshCollider>();

        return newMeshObj;
    }

}
