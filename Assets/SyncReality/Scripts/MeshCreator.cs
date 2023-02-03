using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class MeshCreator : MonoBehaviour
{
    public Material Mat;
    public Transform TapRef;
    public GameObject LocationMarker;
    public Transform ConfirmationMarker;
    public Transform ConfirmationCube;

    private bool ConfirmationOccured = false;
    private bool MeshWasCreated = false;
    private GameObject[] CacheObjects;  
    private GameObject Location1=null;
    private GameObject Location2=null;
    
   private void Update()
   {
       float dist = Vector3.Distance(ConfirmationCube.position, ConfirmationMarker.position);
 
        if(dist<0.05f && !ConfirmationOccured){
            SetLocationMarker();
            ConfirmationOccured = true;
        }

        if (ConfirmationOccured && dist > 0.2f)
            ConfirmationOccured = false;

    }


    void SetLocationMarker()
    {
        
        if(Location1!=null && Location2!= null){
            Debug.LogWarning("Stopped");
            Debug.LogWarning(Location1, Location2);

            return;
            
        }
        
        GameObject A = Instantiate(LocationMarker, TapRef.position, Quaternion.identity);
        //CacheObjects.Append(A);
        Debug.LogWarning("Created Sample Point");
      
      
        if (Location1 == null){
            Location1 = A;
            return;
        }

        if (Location1 != null && Location2 == null)
            Location2 = A;
        
        if(Location1!=null && Location2!=null && !MeshWasCreated)
            CreateMesh();

        if (MeshWasCreated)
            FlushCache();
            
            
    }

    void FlushCache()
    {
        Location1 = null;
        Location2 = null;
        MeshWasCreated = false;
    }

   /* private void Start()
    {
        //Draw Lines
        LineRenderer A;
        LineRenderer B;
        DrawLine(out A,new Vector3(3,0,0), new Vector3(-3,0,0), Color.magenta, 0.2f);
        
        DrawLine(out B, new Vector3(3,0,3), new Vector3(-3,0,-3), Color.cyan, 0.2f);
        
        //FindIntersection

        Vector3 Inters = new Vector3();
        bool Intersecting = false;
        Vector3 ADirection = A.GetPosition(0) - A.GetPosition(1);
        Vector3 BDirection = B.GetPosition(0) - B.GetPosition(1);
        
        LineLineIntersection(out Intersecting,out Inters,A.GetPosition(0),A.GetPosition(0)-A.GetPosition(1),B.GetPosition(0),B.GetPosition(0)- B.GetPosition(1));
        
        //Abort
        if(!Intersecting)
            return;
        
        Debug.LogWarning("Intersection at: " + Inters);
        GameObject F = Instantiate(LocationMarker, Inters, Quaternion.identity);
        F.transform.localScale = new Vector3(18, 18, 18);
        
        //CheckLineAngle for 90°
        CalculateLineAngle(A.GetPosition(0),ADirection,B.GetPosition(0), BDirection, Inters);
    }*/

    void DrawLine(out LineRenderer G, Vector3 start, Vector3 end, Color color, float duration = 0.2f)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = start;
        G = myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = Mat;
     //   lr.SetColors(color, color);
    //    lr.SetWidth(0.1f, 0.1f);
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        //GameObject.Destroy(myLine, duration);
    }


    void CalculateLineAngle(Vector3 Line1, Vector3 Line1Direction, Vector3 Line2, Vector3 Line2Direction, Vector3 IntersectionP)
    {
       
       
       float dot = Vector3.Dot(Line1, Line2-IntersectionP);
       float angleInRadians = Mathf.Acos(dot);
       float angle = Mathf.Rad2Deg * angleInRadians; //to degrees
       
       

       /*
       
       Vector3 targetDir = Line1 - Line2;
       float angle = Vector3.Angle(targetDir, IntersectionP-Line2);
 
       */
       
       Debug.LogWarning("Line Angle: " + angle + "°");

    }
    
    //Calculate the intersection point of two lines. Returns true if lines intersect, otherwise false.
    //Note that in 3d, two lines do not intersect most of the time. So if the two lines are not in the 
    //same plane, use ClosestPointsOnTwoLines() instead.
    public static bool LineLineIntersection(out Boolean Intersected, out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2){
 
        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);
 
        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);
 
        //is coplanar, and not parrallel
        if(Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
        {
            float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
            intersection = linePoint1 + (lineVec1 * s);
            Debug.LogWarning("Intersection present: " + intersection);
            Intersected = true;
            return true;
        }
        else
        {
            intersection = Vector3.zero;
            Debug.LogWarning("No Intersection");
            Intersected = false;
            return false;
        }
    }
    
    
    void CreateMesh()
    {
        
        Debug.LogWarning("Started Mesh Creation");
        Vector3[] vertices = new Vector3[4];
        Vector2[] uv = new Vector2[4];
        int[] triangles = new int[6];


        Vector3 L1 = Location1.transform.position;
        Vector3 L2 = Location2.transform.position;
        
        //Table Plate
        vertices[0] = L1;
        vertices[1] = new Vector3(L1.x,L1.y,L2.z);
        vertices[2] = L2;
        vertices[3] = new Vector3(L2.x,L1.y,L1.z);

        uv[0] = new Vector2(0,1);
        uv[1] = new Vector2(1,1);
        uv[2] = new Vector2(0,0);
        uv[3] = new Vector2(1,0);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;
        triangles[3] = 2;
        triangles[4] = 3;
        triangles[5] = 0;
        
        
        
        Mesh mesh = new Mesh();


        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        GameObject B = new GameObject("Mesh",typeof(MeshFilter), typeof(MeshRenderer));

        B.GetComponent<MeshFilter>().mesh = mesh;

        B.GetComponent<MeshRenderer>().material = Mat;


        MeshWasCreated = true;

      
    }
    
    
    
}
