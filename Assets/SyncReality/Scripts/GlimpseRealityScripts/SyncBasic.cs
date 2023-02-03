using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.UISystemProfilerApi;



/*
public enum ObjectClassification
{
    Misc = 1 << 0,
    Wall = 1 << 1,
    Floor = 1 << 2,
    Ceiling = 1 << 3,
    Door = 1 << 4,
    Window = 1 << 5,
    Table = 1 << 6,
    Seat = 1 << 7
}

*/

public static class GlimpsyUtil
{
    public static Classification ConvertFromClassificationTypeToClassification(ScannedObjectsClassificationType type)
    {
        switch(type)
        {
            case ScannedObjectsClassificationType.Table:
                return Classification.Table;
                
            /*case ScannedObjectsClassificationType.Walls:
                return Classification.Wall;*/

            case ScannedObjectsClassificationType.Sofa:
                return Classification.Sofa;
            case ScannedObjectsClassificationType.Seat:
                return Classification.Seat;

            case ScannedObjectsClassificationType.Closet:
                return Classification.Closet;

            default:
                return Classification.Misc;

        }
    }

    public static List<MockData> ConvertFromScannedTypeGameObjectToMockData(List<ScannedTypeGameObject> obj)
    {
        List<MockData> result = new List<MockData>();
        foreach (var scannedObj in obj)
        {
            switch (scannedObj.ObjectClassification)
            {
                case var _ when ScannedObjectFactory.SimpleTypes.Contains(scannedObj.ObjectClassification):
                    ScannedSimpleObject boxData;
                    //Optimization, so it only calls factory if it needs to
                    if (scannedObj.ScannedObjectData is ScannedSimpleObject)
                        boxData = (ScannedSimpleObject)scannedObj.ScannedObjectData;
                    else
                        boxData = (ScannedSimpleObject)ScannedObjectFactory.CreateScannedObject(scannedObj);
                    var newMock = GetMockFromBaseScanned(boxData, scannedObj);
                    result.Add(newMock);
                    break;

                case var _ when ScannedObjectFactory.ComplexTypes.Contains(scannedObj.ObjectClassification):
                    ScannedComplexObject wallData;
                    if (scannedObj.ScannedObjectData is ScannedComplexObject)
                        wallData = (ScannedComplexObject)scannedObj.ScannedObjectData;
                    else
                        wallData = (ScannedComplexObject)ScannedObjectFactory.CreateScannedObject(scannedObj);
                    result.AddRange(GetMocksFromComplexObj(wallData, scannedObj));
                    break;

                default:
                    result.Add(new MockData(scannedObj));
                    break;
            }
        }
        return result;
    }

    //TODO change to base complextype, simpletype
    private static List<MockData> GetMocksFromComplexObj(ScannedComplexObject obj, ScannedTypeGameObject gameObj)
    {
        List<MockData> result = new List<MockData>();
        if (obj.MeshColliders.Count < 1)
            return result;

        string groupKey = obj.MeshColliders[0].GetInstanceID().ToString() + obj.MeshColliders[0].gameObject.name + Random.Range(-100, 100);
        foreach (var bounds in obj.MeshColliders )
        {
            var newMockData = new MockData(gameObj);
            newMockData.GroupObjectID = groupKey;
            newMockData.TransformationMatrix4X4 = GetMatrixFromCollider(bounds);
            result.Add(newMockData);
        }

        return result;
    }

    //TODo change from box to basetype
    private static MockData GetMockFromBaseScanned(ScannedSimpleObject obj, ScannedTypeGameObject gameObj)
    {
        MockData result = new MockData(gameObj);

        result.TransformationMatrix4X4 = GetMatrixFromCollider(obj.GetCollider());

        return result;

    }

    private static UnityEngine.Matrix4x4 GetMatrixFromCollider(MeshCollider obj)
    {
        UnityEngine.Matrix4x4 result = Matrix4x4.zero;
        var mesh = obj.sharedMesh;

        var vertices = mesh.vertices;

        if ( vertices.Length < 6)
            return result;

        var newCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        newCube.transform.SetParent(obj.transform.parent,true);

        var center = vertices.Aggregate(Vector3.zero, (x, y) => x + y) / vertices.Length;


        //surface verts
        var topvertices = vertices.Where(x => x.y > center.y).Distinct().ToList();

        Vector3 MinVert, MaxVert;
        MinVert = MaxVert = topvertices[0];

        //calculate starting vert
        foreach (var vert in topvertices)
            if(vert.x <= MinVert.x && vert.z <= MinVert.z)
                MinVert = vert;
        //calculate most distant vert to exclude
        foreach (var vert in topvertices)
            if ((MinVert - vert).sqrMagnitude > (MinVert - MaxVert).sqrMagnitude)
                MaxVert = vert;

        var floorVert = vertices.First(x => x.x.Equals(MinVert.x) && x.z.Equals(MinVert.z) && x.y < MinVert.y);

        topvertices.Remove(MaxVert);
        topvertices.Remove(MinVert);

        if(topvertices.Count < 2)
            return result;

        Vector3 vector1 = topvertices[0] - MinVert;
        Vector3 vector2 = topvertices[1] - MinVert;

        Vector3 vectorFloor = floorVert - MinVert;

        if(vector1.sqrMagnitude < vector2.sqrMagnitude)
        {
            var _auxVec = vector1;
            vector1 = vector2;
            vector2 = _auxVec;
        }


        Vector3 newScaleVec =  new Vector3( vector1.magnitude, Mathf.Abs(vectorFloor.y), vector2.magnitude);

  
        newCube.transform.localScale = Vector3.Scale(newScaleVec, obj.transform.localScale);
        
        newCube.transform.position = obj.transform.TransformPoint(center);

        var angle = Vector3.SignedAngle(Vector3.right, vector1.normalized, Vector3.up);

        newCube.transform.rotation *= Quaternion.Euler(0, angle,0);

        //Quaternion.FromToRotation(Vector3.ri)

        //newCube.transform.Rotate(normalized.x, 0f, normalized.z);


        result = newCube.transform.localToWorldMatrix;

        GameObject.Destroy(newCube);

        return result;
    }

}

public class SyncBasic : MonoBehaviour
{

}
