#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DistributeObjectOverPoly))]
public class DistributeObjectOverPolyEditor : Editor
{

    void OnSceneGUI()
    {
        var dis = (DistributeObjectOverPoly)target;
        Handles.matrix = dis.transform.localToWorldMatrix;
        /*for (int i = 0; i < dis.points.Count; i++)
        {
            var pos = Handles.PositionHandle(dis.points[i], Quaternion.Euler(0, -Vector2.SignedAngle(Vector2.up, dis.points[(i + 1) % dis.points.Count].flatten() - dis.points[i].flatten()), 0));
            if (dis.points[i] != pos)
            {
                dis.OnValidate();
                dis.points[i] = pos;
            }
        }*/
        
    }

}
#endif