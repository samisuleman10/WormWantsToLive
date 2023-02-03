using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MeshColliderBasic  
{
    public Mesh SharedMesh;
    public string Name;
    public MeshColliderBasic(MeshCollider collider)
    {
        SharedMesh = collider.sharedMesh;
        Name = collider.gameObject.name;
    }
    public MeshColliderBasic() { }

    public MeshCollider GetMeshCollider(GameObject baseObject, bool addComponentToBase = false)
    {
        MeshCollider _meshCollider;
        GameObject baseCompObj;
        MeshFilter _newFilter;

        if (!addComponentToBase)
        {
            baseCompObj = new GameObject(Name + "_");
            baseCompObj.transform.SetParent(baseObject.transform, false);
        }
        else
            baseCompObj = baseObject;

        _meshCollider = baseCompObj.AddComponent<MeshCollider>();
        baseCompObj.AddComponent<MeshRenderer>();
        _newFilter = baseCompObj.AddComponent<MeshFilter>();

        if (SharedMesh != null)
        {
            _meshCollider.sharedMesh = SharedMesh;
            _newFilter.sharedMesh = SharedMesh;
            _meshCollider.sharedMesh.RecalculateBounds();
        }

        return _meshCollider;
    }

    [Serializable]
    public class MeshColliderBasicCollectionSerialized
    {
        public List<MeshColliderBasic> MeshColliders;

        public MeshColliderBasicCollectionSerialized() { }
    }
}

[Serializable]
public class ListBasic
{
    public List<Vector3> List;

    public ListBasic() { }
}
