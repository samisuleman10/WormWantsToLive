using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FTAPointCreateWalls : FingerTapActionPointCreateBase
{
    public float WallTickness = 0.2f;
    public float WallHeight = 2.5f;

    public float EditDistance = 2f;

    private Vector3? _lastPoint;
    private Vector3? _firstPoint;


    private Stack<Vector3> _usedPoints = new Stack<Vector3>();

    private Stack<GameObject> _currentCreatedWalls = new Stack<GameObject>();

    
    public override void OnUndo()
    {
        if (_currentCreatedWalls.Count > 0)
        {
            var boxToRemove = _currentCreatedWalls.Pop();
            
            _usedPoints.Pop();
            var lastUsedpoint = _usedPoints.Peek();
            if (pointMarkers.Count > 0)
                pointMarkers[pointMarkers.Count - 1].transform.position = new Vector3(lastUsedpoint.x, WallHeight, lastUsedpoint.z);

            Destroy(boxToRemove);
        }
        else base.OnUndo();
    }

    protected override void OnCancelMeshCreate(GameObject target, GameObject cursorPosition)
    {
        foreach(var wall in _currentCreatedWalls)
        {
            Destroy(wall);
        }
        _currentCreatedWalls.Clear();
        _usedPoints.Clear();

        base.OnCancelMeshCreate(target, cursorPosition);
    }

    protected override void OnTapMeshCreate(GameObject target, GameObject cursor)
    {
        _usedPoints.Push(cursor.transform.position);

        pointMarkers = pointMarkers.Where(p => p != null).ToList();

        if (!_lastPoint.HasValue || pointMarkers.Count == 0)
        {
            FTCursorsEventManager.ChangeCursorRayDistance(EditDistance);

            Vector3 floorPoint = new Vector3(cursor.transform.position.x, 0, cursor.transform.position.z);
            CreateNewMarker(floorPoint,  false);

            Vector3 heightPoint = new Vector3(cursor.transform.position.x, WallHeight, cursor.transform.position.z);
            CreateNewMarker(heightPoint,  false);

            _lastPoint = heightPoint;
            _firstPoint = heightPoint;
        }
        else
        {
            //Removes first floor point, so next meshes follow same logic
            if (pointMarkers.Count > 1)
                base.RemoveMarker(0);

            Vector3 newHeightPoint = new Vector3(cursor.transform.position.x, WallHeight, cursor.transform.position.z);
            _lastPoint = newHeightPoint;
            CreateNewMarker(newHeightPoint,  false);

            Vector3 newFloorPoint = new Vector3(cursor.transform.position.x, 0, cursor.transform.position.z);
            CreateNewMarker(newFloorPoint,  false);

            Vector3 heightLineVector = pointMarkers[1].transform.position - pointMarkers[0].transform.position;
            Vector3 floorLineVector = pointMarkers[2].transform.position - pointMarkers[0].transform.position;

            Vector3 directionNormal = Vector3.Cross(heightLineVector, floorLineVector).normalized;

            Vector3 newPoint1, newPoint2;
            newPoint1 = pointMarkers[1].transform.position + (directionNormal * WallTickness);

            var magOld = Camera.main.transform.position - pointMarkers[1].transform.position;
            var magNew = Camera.main.transform.position - newPoint1;

            //new points are more distant than old ones
            if (magNew.sqrMagnitude > magOld.sqrMagnitude)
                newPoint2 = pointMarkers[0].transform.position + (directionNormal * WallTickness);
            else
            {
                directionNormal = Vector3.Cross(floorLineVector, heightLineVector).normalized;
                newPoint1 = pointMarkers[1].transform.position + (directionNormal * WallTickness);
                newPoint2 = pointMarkers[0].transform.position + (directionNormal * WallTickness);
            }

            RemoveMarker(2);

            CreateNewMarker(newPoint1,  false);
            CreateNewMarker(newPoint2,  false);

            //for test. removes last line point, which should be floor
            //RemoveMarker(2);

            //confirm also clears markers
            var baseConfigForFloor = base._createMeshUntilFloor;

            base._createMeshUntilFloor = true;
            base.OnConfirmMeshCreate(creationDelay: false);


            //back to oiriginal config for floor
            base._createMeshUntilFloor = baseConfigForFloor;

            CreateNewMarker(newHeightPoint,  false);
        }
    }

    protected override void OnConfirmMeshCreate(ScannedObjectsClassificationType objectTypes = ScannedObjectsClassificationType.NonSelected, bool creationDelay = true)
    {
        var RootSpam = RoomSelector.GetCurrentSelectedRoom();
        if (RootSpam == null)
            return;


        if (_currentCreatedWalls.Count > 0)
        {
            if (_firstPoint.HasValue && _lastPoint.HasValue && _firstPoint.Value != _lastPoint.Value)
            {
                ConnectWallPoints(_lastPoint.Value, _firstPoint.Value, null);
            }

            GameObject newWallObj = new GameObject("WallBase");
            newWallObj.transform.SetParent(RootSpam.transform, true);

            foreach (var wall in _currentCreatedWalls)
                wall.transform.SetParent(newWallObj.transform, true);

            var wallComp = newWallObj.AddComponent<ScannedTypeGameObject>();

            var reverseList = _usedPoints.ToList();
            
            wallComp.ScannedPoints = new Stack<Vector3>(reverseList);
            _usedPoints.Clear();

            wallComp.ObjectClassification = ScannedObjectsClassificationType.Walls;

            //newWallObj

            foreach (var wall in _currentCreatedWalls)
                CreatedVolumes.Remove(wall);

            _currentCreatedWalls.Clear();
            CreatedVolumes.Add(newWallObj);

            RunEnabler.UpdateReplacerStatus();
        }

        base.OnConfirmMeshCreate(creationDelay:creationDelay);
    }

    private void ConnectWallPoints(Vector3 startPoint, Vector3 endPoint, GameObject cursor)
    {

        Vector3 endFloorPoint = new Vector3(endPoint.x, 0, endPoint.z);
        CreateNewMarker(endFloorPoint,  false);


        Vector3 heightLineVector = endPoint - startPoint;
        Vector3 floorLineVector = endFloorPoint - startPoint;

        Vector3 directionNormal = Vector3.Cross(heightLineVector, floorLineVector).normalized;

        Vector3 newPoint1, newPoint2;
        newPoint1 = endPoint + (directionNormal * WallTickness);

        var magOld = Camera.main.transform.position - endPoint;
        var magNew = Camera.main.transform.position - newPoint1;

        //new points are more distant than old ones
        if (magNew.sqrMagnitude > magOld.sqrMagnitude)
            newPoint2 = startPoint + (directionNormal * WallTickness);
        else
        {
            directionNormal = Vector3.Cross(floorLineVector, heightLineVector).normalized;
            newPoint1 = endPoint + (directionNormal * WallTickness);
            newPoint2 = startPoint + (directionNormal * WallTickness);
        }

        RemoveMarker(2);

        CreateNewMarker(newPoint1,  false);
        CreateNewMarker(newPoint2,  false);

        //for test. removes last line point, which should be floor
        //RemoveMarker(2);

        //confirm also clears markers
        var baseConfigForFloor = base._createMeshUntilFloor;

        base._createMeshUntilFloor = true;
        base.OnConfirmMeshCreate( creationDelay: false);

        base._createMeshUntilFloor = baseConfigForFloor;
    }

    public override GameObject CreateMeshFromPoints(List<GameObject> meshPoints, bool alwaysGoToFloor = false)
    {
        _baseObjName = "Wall";
        var createdMesh = base.CreateMeshFromPoints(meshPoints, alwaysGoToFloor);
        _currentCreatedWalls.Push(createdMesh);
        return createdMesh;
    }
}
