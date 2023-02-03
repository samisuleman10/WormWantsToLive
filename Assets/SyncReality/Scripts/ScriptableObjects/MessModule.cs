using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[System.Serializable]
[CreateAssetMenu(fileName = "MessMaker", menuName = "ScriptableObjects/MessMaker", order = 1)]
public class MessModule : ScriptableObject
{
    public string moduleID;
    public GameObject prefab;
    public MessMaker.SurfaceType surfaceType;
    public int spawnAmount;
    public Vector3 scaleVariance;
    public float rotationVariance;
    public float shiftAmount;
    public float shiftVariance;
    public float gravityStrength;

    FlexFloor flexFloor;
    public List<Vector3> points
    {
        get
        {
            if (surfaceType == MessMaker.SurfaceType.Floor)
            {
                var l = FindObjectOfType<LayoutArea>().GetLayoutPointsAsVector3().Select(v => new Vector3(v.x, 0, v.z)).ToList();
                l.Reverse();
                return l;
            }
            if(surfaceType == MessMaker.SurfaceType.Ceiling)
            {
                var l = FindObjectOfType<LayoutArea>().GetLayoutPointsAsVector3().Select(v => new Vector3(v.x, 0, v.z)).ToList();
             
                l = l.Select(v => v + Vector3.up * FindObjectOfType<DesignAccess>().GetCeilingHeight()).ToList();
                l.Reverse();
                return l;
            }
            if (surfaceType == MessMaker.SurfaceType.Backdrop)
            {
                var surrounder = FindObjectOfType<SurrounderModule>();
                var vecs = FindObjectOfType<LayoutArea>().GetLayoutPointsAsVector3();
                var lastI = 0;
                var l = vecs.Where((v, i) =>
                {
                    var f = surrounder.getDirOfVec(vecs[(i + 1) % vecs.Count] - vecs[i], surrounder.getGroundBox().ToArray());
                    if (f == WallFace.North)
                    {
                        lastI = i;
                        return true;
                    }
                    return false;
                }).ToList();
                l.Add(vecs[(lastI + 1) % vecs.Count]);
                var offsetVec= Quaternion.Euler(0,-90,0) * (surrounder.getGroundBox()[1] - surrounder.getGroundBox()[0]);

                l.AddRange(new[] {  surrounder.getGroundBox()[1] - offsetVec, surrounder.getGroundBox()[0] - offsetVec });
                //l.Reverse();
                int k = 0;
                foreach (var li in l)
                {
                    k++;
//                    Debug.Log(k + ": " + li);
                }
                l.Reverse();
                return l;
            }
            return new[] { Vector3.zero }.ToList();
        }
    }
    


}
