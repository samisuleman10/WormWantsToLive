using System;
using System.Collections.Generic;
using UnityEngine;

public static class ArithmeticCalculationUtils
{
    public static Vector3 GetCentroidPointBetweenPoints(List<Vector3> points)
    {
        var centroidPoint = new Vector3(0,0,0);
        foreach(var point in points)
        {
            centroidPoint.x += point.x;
            centroidPoint.y += point.y;
            centroidPoint.z += point.z;
        }

        centroidPoint.x = (float)Math.Round(centroidPoint.x/points.Count,1);
        centroidPoint.y = (float)Math.Round(centroidPoint.y/points.Count,1);
        centroidPoint.z = (float)Math.Round(centroidPoint.z/points.Count,1);

        return centroidPoint;
    }

    public static Vector3 GetCentroidPointBetweenTransformsGlobaly(List<Transform> points)
    {
        var centroidPoint = new Vector3(0, 0, 0);
        foreach (var point in points)
        {
            centroidPoint.x += point.position.x;
            centroidPoint.y += point.position.y;
            centroidPoint.z += point.position.z;
        }

        centroidPoint.x = (float)Math.Round(centroidPoint.x / points.Count, 1);
        centroidPoint.y = (float)Math.Round(centroidPoint.y / points.Count, 1);
        centroidPoint.z = (float)Math.Round(centroidPoint.z / points.Count, 1);

        return centroidPoint;
    }

    public static Vector3 GetCentroidPointBetweenTransformsLocaly(List<Transform> points)
    {
        var centroidPoint = new Vector3(0, 0, 0);
        foreach (var point in points)
        {
            centroidPoint.x += point.localPosition.x;
            centroidPoint.y += point.localPosition.y;
            centroidPoint.z += point.localPosition.z;
        }

        centroidPoint.x = (float)Math.Round(centroidPoint.x / points.Count, 1);
        centroidPoint.y = (float)Math.Round(centroidPoint.y / points.Count, 1);
        centroidPoint.z = (float)Math.Round(centroidPoint.z / points.Count, 1);

        return centroidPoint;
    }

    public static List<Vector3> GetGlobalPostitionsListFromTransformList(List<Transform> transforms)
    {
        var list = new List<Vector3>();
        foreach (var b in transforms)
        {
            list.Add(b.position);
        }
        return list;
    }

    public static List<Vector3> GetLocalPostitionsListFromTransformList(List<Transform> transforms)
    {
        var list = new List<Vector3>();
        foreach (var b in transforms)
        {
            list.Add(b.localPosition);
        }
        return list;
    }
}
