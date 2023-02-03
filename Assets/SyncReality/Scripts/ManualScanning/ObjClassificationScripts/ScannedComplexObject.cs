using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;
using System;
using Newtonsoft.Json;

[Serializable]
public class ScannedComplexObject : ScannedObject, ISerializable
{
    private readonly ScannedObjectsClassificationType _thisClassification = ScannedObjectsClassificationType.Walls;
    public override ScannedObjectsClassificationType ClassificationType => _thisClassification;
    
    public List<MeshCollider>  MeshColliders = new List<MeshCollider>();


    public ScannedComplexObject(ScannedTypeGameObject _thisGameObject) : base(_thisGameObject) 
    {
        _thisClassification = _thisGameObject.ObjectClassification;
        UpdateColliderList();
    }
    public void UpdateColliderList()
    {
        MeshColliders = this._baseObjectComponent.gameObject.transform.GetComponentsInChildren<MeshCollider>().ToList();
    }

    public ScannedComplexObject(SerializationInfo info, StreamingContext context) : base(info, context) 
    {
        //var collidersBasic =  (List<MeshColliderBasic>)info.GetValue("MeshList", typeof(List<MeshColliderBasic>));
        //MeshColliders = collidersBasic.ConvertAll<MeshCollider>(o => o.GetMeshCollider(_baseObjectComponent.gameObject,false));

        var collidersBasic = (MeshColliderBasic.MeshColliderBasicCollectionSerialized)info.GetValue("MeshList", typeof(MeshColliderBasic.MeshColliderBasicCollectionSerialized));
        MeshColliders = collidersBasic.MeshColliders.ConvertAll<MeshCollider>(o => o.GetMeshCollider(_baseObjectComponent.gameObject, false));

    }
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {

        MeshColliderBasic.MeshColliderBasicCollectionSerialized collection = new MeshColliderBasic.MeshColliderBasicCollectionSerialized();
        collection.MeshColliders = MeshColliders.ConvertAll<MeshColliderBasic>(o => new MeshColliderBasic(o));

        //info.AddValue("MeshList", MeshColliders.ConvertAll<MeshColliderBasic>( o => new MeshColliderBasic(o) ), typeof(List<MeshColliderBasic>));
        info.AddValue("MeshList", collection, typeof(MeshColliderBasic.MeshColliderBasicCollectionSerialized));


        base.GetObjectData(info, context);
    }



}
