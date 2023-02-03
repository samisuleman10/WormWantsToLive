using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class FindOuterWalls : MonoBehaviour
{

    public RoomDimensions GetRoomDimensions(GameObject whereWallsAre)
    {
        var ScannedObjects = whereWallsAre.GetComponentsInChildren<ScannedTypeGameObject>()
                            .Where(o => o.ObjectClassification == ScannedObjectsClassificationType.Walls).ToList();
        float maxX = float.MinValue;
        int wallPos = -1;

        //Find the most outer wall based on X
        for(int i =0; i < ScannedObjects.Count; i++)
        {
            if (ScannedObjects[i].ScannedObjectData != null && ScannedObjects[i].ScannedObjectData.GetType() == typeof(ScannedComplexObject))
            {
                var thisScannedWall = (ScannedComplexObject)ScannedObjects[i].ScannedObjectData;
                foreach(var collider in thisScannedWall.MeshColliders)
                {
                    //bounds wont work for a meshCollider
                    foreach(var vertex in collider.sharedMesh.vertices)
                    {
                        if (vertex.x > maxX)
                        {
                            maxX = collider.bounds.max.x;
                            wallPos = i;
                        }
                    }
                }
            }
        }

        RoomDimensions convertedWall = new RoomDimensions();
        //No walls
        if (wallPos == -1)
            return convertedWall;

        var OuterWall = ScannedObjects[wallPos];

        convertedWall.layoutPointPositions = OuterWall.ScannedPoints.ToList();
        //convertedWall.layoutPointPositions.Reverse();


        #region Old Mesh Wall Calculating code
        /*
          
        var OuterWall = ((ScannedComplexObject)ScannedObjects[wallPos].ScannedObjectData);
        List<Vector3> _outerUpperWallPoints = new List<Vector3>();
        List<Vector3> _outerWallFrontPoints = new List<Vector3>();
        
        //Get all Upper face wall points from Meshes
        foreach (var collider in OuterWall.MeshColliders)
        {
            _outerUpperWallPoints.AddRange(collider.sharedMesh.vertices.Where(obj => obj.y > 0.01f));
        }

        var MidWallPoint = _outerUpperWallPoints.Aggregate(new Vector3(0, 0, 0), (s, v) => s + v) / _outerUpperWallPoints.Count;

        // Get closest points. Wall mesh will be changed to plane, so discard second closest point. 


        foreach (var collider in OuterWall.MeshColliders)
        {
            float minDistance = float.MaxValue;
            Vector3 closestPoint =Vector3.zero;
            var topVertices = collider.sharedMesh.vertices.Where(obj => obj.y > 0.01f).ToList();
            foreach (var vertice in topVertices)
            {
                if ((vertice - MidWallPoint).sqrMagnitude < minDistance)
                {
                    minDistance = vertice.sqrMagnitude;
                    closestPoint = vertice;
                }
            }

            topVertices.Remove(closestPoint);
            topVertices = topVertices.OrderBy(v => (v - closestPoint).sqrMagnitude).ToList();

            _outerWallFrontPoints.Add(closestPoint);
            //Takes second ordered point 
            if(topVertices.Count > 1)
                _outerWallFrontPoints.Add(topVertices[1]);
        }

        List<Vector3> distinctlist = new List<Vector3>();

        foreach(var point in _outerWallFrontPoints)
        {
            if(!distinctlist.Any( c => Mathf.Abs(c.x - point.x) <= 0.01f && Mathf.Abs(c.z - point.z) <= 0.01f))
                distinctlist.Add(point);   
        }
        convertedWall.layoutPointPositions = distinctlist;
        */
        #endregion



        return convertedWall;
    }
}
