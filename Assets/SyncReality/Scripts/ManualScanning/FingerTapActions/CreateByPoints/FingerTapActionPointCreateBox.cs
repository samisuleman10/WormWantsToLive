using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class FingerTapActionPointCreateBox : FingerTapActionPointCreateBase
{
    private Stack<GameObject> _createdBoxes = new Stack<GameObject>();
    

    private float _boxEditDistance = 0.03f;
    private float _originalLaserDistance = 0.3f;

    public override GameObject CreateMeshFromPoints(List<GameObject> meshPoints, bool alwaysGoToFloor = false)
    {
        _baseObjName = "Box";
        var newBox = base.CreateMeshFromPoints(meshPoints, alwaysGoToFloor);
        _createdBoxes.Push(newBox);
        return newBox;
    }


    /// <summary>
    /// Ready to be used, if wanted to change meshcollider to box, but boxcolliders will have differences from the generated mesh
    /// </summary>
    /// <param name="meshPoints"></param>
    /// <returns></returns>
    private GameObject CreateMeshFromPointy(List<GameObject> meshPoints)
    {
        var result = base.CreateMeshFromPoints(meshPoints);

        var collider = result.GetComponent<Collider>();
        if(collider != null)
        {
            Destroy(collider);
            result.AddComponent<BoxCollider>();
        }


        return result;
    }

    protected override void OnCancelMeshCreate(GameObject target, GameObject cursorPosition)
    {
        if (_createdBoxes.Count > 0)
        { 
            var boxToRemove = _createdBoxes.Pop();
            Destroy(boxToRemove);
        }
        FTCursorsEventManager.ChangeCursorRayDistance(_originalLaserDistance);
        base.OnCancelMeshCreate(target, cursorPosition);
    }

    protected override void OnConfirmMeshCreate(ScannedObjectsClassificationType objectType = ScannedObjectsClassificationType.NonSelected, bool creationDelay = true)
    {
        var RootSpam = RoomSelector.GetCurrentSelectedRoom();
        if (RootSpam == null)
            return;

        if (_createdBoxes.Count > 1)
        {
            GameObject baseObj = new GameObject("Boxes");
            baseObj.transform.SetParent(RootSpam.transform);
            foreach (var obj in _createdBoxes)
            {
                obj.transform.SetParent(baseObj.transform);
            }
            _createdBoxes.Clear();

            var newComponent = baseObj.AddComponent<ScannedTypeGameObject>();

            if(objectType == ScannedObjectsClassificationType.NonSelected)
                newComponent.ObjectClassification = ScannedObjectsClassificationType.Sofa;
            else
                newComponent.ObjectClassification = objectType;
        }
        if(_createdBoxes.Count == 1)
        {
            var box = _createdBoxes.Pop();
            box.transform.SetParent(RootSpam.transform);
            var newComponent = box.AddComponent<ScannedTypeGameObject>();
            if (objectType == ScannedObjectsClassificationType.NonSelected)
                newComponent.ObjectClassification = ScannedObjectsClassificationType.Table;
            else
                newComponent.ObjectClassification = objectType;
        }

        FTCursorsEventManager.ChangeCursorRayDistance(_originalLaserDistance);
        base.OnConfirmMeshCreate(creationDelay:creationDelay);
    }
    protected override void OnTapMeshCreate(GameObject target, GameObject cursor)
    {

        pointMarkers = pointMarkers.Where(p => p != null).ToList();

        //Is holding, Clicked on something, or doesnt have enough points
        if (target != null || _isHoldingMarker || (this.pointMarkers.Count < 3) )
            base.OnTapMeshCreate(target, cursor);

        if(pointMarkers.Count == 1)
        {
            _originalLaserDistance = FingerTapCursor.RayDistance;
            FTCursorsEventManager.ChangeCursorRayDistance(_boxEditDistance);
        }

        if (pointMarkers.Count == 3)
        {
            _isWaitingCreatingDelay = false;
            Vector3 midpoint = (pointMarkers[0].transform.position + pointMarkers[1].transform.position + pointMarkers[2].transform.position) / 3;

            Vector3 diffToMidpoint0 = pointMarkers[0].transform.position - midpoint;
            Vector3 diffToMidpoint1 = pointMarkers[1].transform.position - midpoint;
            Vector3 diffToMidpoint2 = pointMarkers[2].transform.position - midpoint;

            List<int> pointPos = new List<int>() { 0, 1, 2 };
            int minMagnitudePos;

            //Which is the lonely point
            if (diffToMidpoint0.sqrMagnitude < diffToMidpoint1.sqrMagnitude && diffToMidpoint0.sqrMagnitude < diffToMidpoint2.sqrMagnitude)
                minMagnitudePos = 0;
            else
                if (diffToMidpoint1.sqrMagnitude < diffToMidpoint2.sqrMagnitude)
                minMagnitudePos = 1;
            else
                minMagnitudePos = 2;

            pointPos.Remove(minMagnitudePos);

            Vector3 midBoxPoint = (pointMarkers[pointPos[0]].transform.position + pointMarkers[pointPos[1]].transform.position) / 2;
            Vector3 newPointDirection = midBoxPoint - pointMarkers[minMagnitudePos].transform.position;

            Vector3 newPoint = midBoxPoint + newPointDirection;

            //Switch point 2 and 3 if crossing the middle
            float diffInDirections;
            for (int i = 0; i < pointMarkers.Count - 1; i++)
            {
                diffToMidpoint0 = midBoxPoint - pointMarkers[i].transform.position;
                diffToMidpoint1 = pointMarkers[i + 1].transform.position - midBoxPoint;
                diffInDirections = Vector3.Dot(diffToMidpoint0.normalized, diffToMidpoint1.normalized);
                if (diffInDirections > 0.98f)
                {
                    SwitchPoints(1-i, 2-i);
                    break;
                }
            }
            CreateNewMarker(newPoint,  false);

            base.OnConfirmMeshCreate( creationDelay: true);
            OnConfirmMeshCreate(objectType: FingerTapCursor.GetCurrentCursorObjClassification().ObjectClassification, creationDelay: true);
        }
    }


    private void SwitchPoints(int pos1, int pos2)
    {
        var secondPoint = pointMarkers[pos1];
        pointMarkers[pos1] = pointMarkers[pos2];
        pointMarkers[pos2] = secondPoint;

        LineMaker.SetPosition(pos1, pointMarkers[pos1].transform.position);
        LineMaker.SetPosition(pos2, pointMarkers[pos2].transform.position);

        var collider1 = pointMarkers[pos1].GetComponent<Collider>();
        if (collider1 == null)
            collider1 = pointMarkers[pos1].GetComponentInChildren<Collider>();

        var collider2 = pointMarkers[pos2].GetComponent<Collider>();
        if (collider2 == null)
            collider2 = pointMarkers[pos2].GetComponentInChildren<Collider>();

        if (collider1 == null || collider2 == null)
        {
            Debug.LogError("Marker created with problems: FingerTapActionPointCreateBox OnTap");
            return;
        }

        _lineRendererPositions[collider1.gameObject] = pos1;
        _lineRendererPositions[collider2.gameObject] = pos2;

    }

}
