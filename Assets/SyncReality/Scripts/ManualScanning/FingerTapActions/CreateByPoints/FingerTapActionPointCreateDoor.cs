using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerTapActionPointCreateDoor : FingerTapActionPointCreateBase
{
    public float DoorTickness = 0.1f;

    protected override void OnTapMeshCreate(GameObject target, GameObject cursor)
    {
        //TODO Needs the fix applied in create box, for the direction of third point

        //Is holding, Clicked on something, or doesnt have enough points
        if (target != null || _isHoldingMarker || (this.pointMarkers.Count < 2)  )
            base.OnTap(target, cursor);

        if(this.pointMarkers.Count == 2)
        {
            _isWaitingCreatingDelay = false;
            Vector3 midpoint = (pointMarkers[0].transform.position + pointMarkers[1].transform.position) / 2;
            Vector3 lineVector = pointMarkers[0].transform.position - pointMarkers[1].transform.position ;

            Vector3 directionNormal = new Vector3 (lineVector.x, -lineVector.y, midpoint.z);

            Vector3 newPoint1, newPoint2;
            newPoint1 = pointMarkers[1].transform.position + (directionNormal.normalized * DoorTickness);

            var magOld = Camera.main.transform.position - pointMarkers[1].transform.position;
            var magNew = Camera.main.transform.position - newPoint1;

            //new points are more distant than old ones
            if (magNew.sqrMagnitude > magOld.sqrMagnitude)
                newPoint2 = pointMarkers[0].transform.position + (directionNormal.normalized * DoorTickness);
            else
            {
                directionNormal = new Vector3(-lineVector.x, lineVector.y, midpoint.z);
                newPoint1 = pointMarkers[1].transform.position + (directionNormal.normalized * DoorTickness);
                newPoint2 = pointMarkers[0].transform.position + (directionNormal.normalized * DoorTickness);
            }

            CreateNewMarker(newPoint1,  false);
            CreateNewMarker(newPoint2,  false);

        }

    }
}
