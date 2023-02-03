using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteAlways]
public class DistributeObjectOverPoly : MonoBehaviour
{
     

    bool somethingChanged;

    public bool drawInScene = true;

    public MessMaker messMaker;
    [FormerlySerializedAs("designStorage")] public DesignAccess designAccess;
    public float offsetToPoly = 0;
    // Start is called before the first frame update
    void Start()
    {
        messMaker = FindObjectOfType<MessMaker>();
    }

    private StoryTeller _storyTeller;
    // Update is called once per frame
    public void Execute()
    {
        if (_storyTeller == null)
            _storyTeller = FindObjectOfType<StoryTeller>();
        int count = 0;
 
        //int seed = Random.Range(0, 9999);
            
        foreach(var dis in designAccess.GetMessModules())
        { 
            Bounds b = new Bounds(dis.points[0], Vector3.zero);
            for (int i = 0; i < dis.points.Count; i++)
            {
                b.Encapsulate(dis.points[i]);
            }
 
            Random.InitState(1230 + count) ;
            count++;
            int k = 0;
            var poly = dis.points.Select(p => p.flatten()).ToArray();

            var pointsToUse = new List<Vector2>();

            for (int j = 0; j < (dis.spawnAmount) * PolygonArea(poly.ToList()) * 0.0001f;)
            {
                var point = new Vector2(Random.Range(b.min.x, b.max.x), Random.Range(b.min.z, b.max.z));
                if(IsPointInPolygon(point + new Vector2(offsetToPoly, offsetToPoly), poly) &&
                    IsPointInPolygon(point + new Vector2(-offsetToPoly, offsetToPoly), poly) &&
                    IsPointInPolygon(point + new Vector2(-offsetToPoly, -offsetToPoly), poly) &&
                    IsPointInPolygon(point + new Vector2(offsetToPoly, -offsetToPoly), poly))
                {


                    pointsToUse.Add(point);
                    j++;
                }
                else
                {
                    k++;
                    if (k > 100)
                    {
                        k = 0;
                        j++;
                        continue;
                    }
                }
            }


            for(int i = 0; i < 3; i++)
            {
                var regularizedPoints = new List<Vector2>();

                for (int x = 0; x < pointsToUse.Count; x++)
                {
                    var currentPointToShift = pointsToUse[x];
                    for (int y = 0; y < pointsToUse.Count; y++)
                    {
                        if (x == y)
                            continue;

                        var gravPoint = pointsToUse[y];
                        var vecBetween = currentPointToShift - gravPoint;
                        currentPointToShift = currentPointToShift + (vecBetween).normalized * (dis.gravityStrength / vecBetween.magnitude);
                    }
                    regularizedPoints.Add(currentPointToShift);
                }
                pointsToUse = regularizedPoints.ToList(); 
            }

            int t = 0;
            foreach(var point in pointsToUse) {


                var perVal = Mathf.PerlinNoise(point.x * 1.4124124f * 0.1f, point.y * 1.1231324f * 0.1f);
                Vector2 projectedPoint = getIntersectionPointFromMidThroughPoint(point, poly);
                var shiftedPoint = point + (projectedPoint - point) * dis.shiftAmount * Mathf.LerpUnclamped(1f, perVal, dis.shiftVariance);
                DrawFullMesh(dis.prefab, new Vector3(shiftedPoint.x, dis.points[0].y, shiftedPoint.y), Quaternion.identity, dis);
                t++;
            
            }
        } 
        Random.InitState(System.Environment.TickCount);
    }


    Vector2 getNormalDir(Vector2 point, Vector2[] polygon)
    {
        Vector2 res = Vector2.zero;
        
        for(int i = 0; i < polygon.Length; i++)
        {
            var p1 = polygon[i];
            var p2 = polygon[(i + 1) % polygon.Length];

            res += (1f / DistancePointLine(point, p1, p2)) * (p2 - p1).normalized;

        }

        return Quaternion.Euler(0,0,-90) *  res.normalized;

    }

    Vector2 getIntersectionPointFromMidThroughPoint(Vector2 point, Vector2[] polygon)
    {
        for (int i = 0; i < polygon.Length; i++)
        {
            var p = Vector2.zero;
            bool hasIntersection = LineIntersection(polygon[i], polygon[(i + 1) % polygon.Length], point, point + getNormalDir(point,polygon) * 1000f, ref p);
            if (hasIntersection)
            {
                return p;
            }

        }
        return Vector2.zero;
        
    }

    Vector2 midPointOfPoly(Vector2[] polygon)
    {
        Vector2 res = Vector2.zero;
        foreach (var p in polygon)
            res += p;
        return res / polygon.Length;
    }

    public void OnValidate()
    {
        somethingChanged = true;
//        FindObjectOfType<ModuleManager>().Execute();
    }

    void DrawFullMesh(GameObject gameObjectToDraw, Vector3 position, Quaternion rotation, MessModule dis)
    {
        var perVal = Mathf.PerlinNoise(position.x * 1.4124124f * 0.1f, position.z * 1.1231324f * 0.1f);
        Matrix4x4 t = Matrix4x4.TRS(position, rotation * Quaternion.Euler(0, perVal * dis.rotationVariance, 0), Vector3.one + perVal * dis.scaleVariance);
        if (drawInScene)
        {

            foreach (var mr in gameObjectToDraw.GetComponentsInChildren<MeshRenderer>())
            {
                var mf = mr.GetComponent<MeshFilter>();
                var mat = transform.localToWorldMatrix * t * mf.transform.localToWorldMatrix;
            
                Graphics.DrawMesh(mf.sharedMesh, mat, mr.sharedMaterial, 0);
            
            }
        }
        else if (somethingChanged)
        {
            #if UNITY_EDITOR
            ToolWindowSettings.Instance.objectsToDrawInPreview.Add(new ToolWindowSettings.ObjectInfo(0, gameObjectToDraw, t, null, null, WallFace.North));
            #endif
        }
        if(Application.isPlaying)
        {
            var o = Instantiate(gameObjectToDraw, position, rotation, _storyTeller.messParent);
            o.transform.localScale = Vector3.one + perVal * dis.scaleVariance;
        }
    }

    (float distance, int index) getMinDistanceToPolygon(Vector2 point, Vector2[] polygon)
    {
        var distance = float.MaxValue;
        int index = 0;
        for(int i = 0; i < polygon.Length; i++)
        {
            var lineDis = DistancePointLine(point, polygon[i], polygon[(i + 1) % polygon.Length]);
            if (lineDis < distance)
            {
                distance = lineDis;
                index = i;
            }
        }
        return (distance, index);
    }

    // Project /point/ onto a line.
    public static Vector3 ProjectPointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 relativePoint = point - lineStart;
        Vector3 lineDirection = lineEnd - lineStart;
        float length = lineDirection.magnitude;
        Vector3 normalizedLineDirection = lineDirection;
        if (length > .000001f)
            normalizedLineDirection /= length;

        float dot = Vector3.Dot(normalizedLineDirection, relativePoint);
        dot = Mathf.Clamp(dot, 0.0F, length);

        return lineStart + normalizedLineDirection * dot;
    }

    // Calculate distance between a point and a line.
    public static float DistancePointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        return Vector3.Magnitude(ProjectPointLine(point, lineStart, lineEnd) - point);
    }
    

    public bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
    {
        int polygonLength = polygon.Length, i = 0;
        bool inside = false;
        // x, y for tested point.
        float pointX = point.x, pointY = point.y;
        // start / end point for the current polygon segment.
        float startX, startY, endX, endY;
        Vector2 endPoint = polygon[polygonLength - 1];
        endX = endPoint.x;
        endY = endPoint.y;
        while (i < polygonLength)
        {
            startX = endX; startY = endY;
            endPoint = polygon[i++];
            endX = endPoint.x; endY = endPoint.y;
            //
            inside ^= (endY > pointY ^ startY > pointY) /* ? pointY inside [startY;endY] segment ? */
                      && /* if so, test if it is under the segment */
                      ((pointX - endX) < (pointY - endY) * (startX - endX) / (startY - endY));
        }
        return inside;
    }

    float PolygonArea(List<Vector2> points)
    {
        float temp = 0;
        int i = 0;
        for (; i < points.Count; i++)
        {
            if (i != points.Count - 1)
            {
                float mulA = points[i].x * points[i + 1].y;
                float mulB = points[i + 1].x * points[i].y;
                temp = temp + (mulA - mulB);
            }
            else
            {
                float mulA = points[i].x * points[0].y;
                float mulB = points[0].x * points[i].y;
                temp = temp + (mulA - mulB);
            }
        }
        temp *= 0.5f;
        return Mathf.Abs(temp);
    }

    public static bool LineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, ref Vector2 intersection)
    {

        float Ax, Bx, Cx, Ay, By, Cy, d, e, f, num/*,offset*/;

        float x1lo, x1hi, y1lo, y1hi;



        Ax = p2.x - p1.x;

        Bx = p3.x - p4.x;



        // X bound box test/

        if (Ax < 0)
        {

            x1lo = p2.x; x1hi = p1.x;

        }
        else
        {

            x1hi = p2.x; x1lo = p1.x;

        }



        if (Bx > 0)
        {

            if (x1hi < p4.x || p3.x < x1lo) return false;

        }
        else
        {

            if (x1hi < p3.x || p4.x < x1lo) return false;

        }



        Ay = p2.y - p1.y;

        By = p3.y - p4.y;



        // Y bound box test//

        if (Ay < 0)
        {

            y1lo = p2.y; y1hi = p1.y;

        }
        else
        {

            y1hi = p2.y; y1lo = p1.y;

        }



        if (By > 0)
        {

            if (y1hi < p4.y || p3.y < y1lo) return false;

        }
        else
        {

            if (y1hi < p3.y || p4.y < y1lo) return false;

        }



        Cx = p1.x - p3.x;

        Cy = p1.y - p3.y;

        d = By * Cx - Bx * Cy;  // alpha numerator//

        f = Ay * Bx - Ax * By;  // both denominator//



        // alpha tests//

        if (f > 0)
        {

            if (d < 0 || d > f) return false;

        }
        else
        {

            if (d > 0 || d < f) return false;

        }



        e = Ax * Cy - Ay * Cx;  // beta numerator//



        // beta tests //

        if (f > 0)
        {

            if (e < 0 || e > f) return false;

        }
        else
        {

            if (e > 0 || e < f) return false;

        }



        // check if they are parallel

        if (f == 0) return false;

        // compute intersection coordinates //

        num = d * Ax; // numerator //

        //    offset = same_sign(num,f) ? f*0.5f : -f*0.5f;   // round direction //

        //    intersection.x = p1.x + (num+offset) / f;
        intersection.x = p1.x + num / f;



        num = d * Ay;

        //    offset = same_sign(num,f) ? f*0.5f : -f*0.5f;

        //    intersection.y = p1.y + (num+offset) / f;
        intersection.y = p1.y + num / f;



        return true;

    }

}
