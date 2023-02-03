using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using System;

[Serializable]
public struct MockData
{
    public Matrix4x4 TransformationMatrix4X4;
    public Classification Classification;
    public string GameObjectName;
    public string GroupObjectID;

    
    public MockData(MockPhysical mock )
    {
        this.Classification = mock.Classification;
        this.GameObjectName = mock.gameObject.name;
        this.TransformationMatrix4X4 = mock.transform.localToWorldMatrix;
        this.GroupObjectID = string.Empty;

    }
    
    public MockData(ScannedTypeGameObject obj)
    {
        TransformationMatrix4X4 = obj.transform.localToWorldMatrix;
        Classification = GlimpsyUtil.ConvertFromClassificationTypeToClassification(obj.ObjectClassification);
        GameObjectName = obj.name;
        this.GroupObjectID = string.Empty;

        //this.MeshData = new MockData.MeshCompData();
    }
}

// Auto layout generation templates
public enum GenTemplate
{
    Home,
    Office,
    Demo
}
// Data object that maps scans & mocksetups to the module for generation 
// can use multiple room layouts for larger environments
[System.Serializable]
public class RoomLayout 
{
    // Always needs a roomdimensions
    [SerializeField]
    public RoomDimensions roomDimensions;

    [SerializeField]
    public List<MockPhysical> mockPhysicals;
    
    [SerializeField]
    public List<MockData> mockDatas;

    public RoomLayout()
    {
        roomDimensions = new RoomDimensions();
        roomDimensions.layoutPointPositions = new List<Vector3>();
        mockPhysicals = new List<MockPhysical>();
    }

    public List<Vector3> GetLayoutPositions()
    {
        var ps = roomDimensions.layoutPointPositions;
        int longestIndex = 0;
        var shortestDistance = float.MaxValue;
        for(int i = 0; i < ps.Count; i++)
        {
            if(Vector3.Distance(ps[i], ps[(i+1) % ps.Count]) < shortestDistance)
            {
                longestIndex = i;
            }
        }
        List<Vector3> newPs = new List<Vector3>();

        for (int i = longestIndex; i < ps.Count; i++)
            newPs.Add(ps[i]);
        for (int i = 0; i < longestIndex; i++)
            newPs.Add(ps[i]);

        if (!IsClockwise(newPs))
            newPs.Reverse();


        return newPs;
    }

    bool IsClockwise(IList<Vector3> vertices)
    {
        double sum = 0.0;
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 v1 = vertices[i];
            Vector3 v2 = vertices[(i + 1) % vertices.Count];
            sum += (v2.x - v1.x) * (v2.z + v1.z);
        }
        return sum > 0.0;
    }

}
