using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;
using System;

[Serializable]
public class ScannedSimpleObject : ScannedObject, ISerializable
{

    private readonly ScannedObjectsClassificationType _thisClassification;
    public override ScannedObjectsClassificationType ClassificationType => _thisClassification;

    private MeshCollider _boxCollider;

    public MeshCollider GetCollider()
    {
        return _boxCollider;
    }

    public ScannedSimpleObject(ScannedTypeGameObject _thisComponent) : base(_thisComponent)
    {
        _thisClassification = _thisComponent.ObjectClassification;
        _boxCollider = _thisComponent.gameObject.GetComponentInChildren<MeshCollider>();
    }



    public ScannedSimpleObject(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        var collidersBasic = (MeshColliderBasic)info.GetValue("MeshCollider", typeof(MeshColliderBasic));
        _boxCollider = collidersBasic.GetMeshCollider(_baseObjectComponent.gameObject, true);
    }
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        if (_boxCollider != null)
        {
            info.AddValue("MeshCollider", new MeshColliderBasic(_boxCollider), typeof(MeshColliderBasic));
        }
        else Debug.LogError("No Collider found for: " + this._name + " at ScannedBox GetObjectData");

        base.GetObjectData(info, context);
    }
}
