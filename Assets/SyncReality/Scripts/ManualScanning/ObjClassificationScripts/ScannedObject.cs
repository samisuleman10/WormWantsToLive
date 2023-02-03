using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class ScannedObject : ISerializable
{
    private ScannedObjectsClassificationType _classificationType;
    public virtual ScannedObjectsClassificationType ClassificationType { get { return _classificationType; } }

    protected ScannedTypeGameObject _baseObjectComponent;
    public ScannedTypeGameObject BaseObjectComponent { get { return _baseObjectComponent; } }
    
    public string _name;
    public Vector3 _position;
    public Vector3 _localScale;
    public Quaternion _rotation;

    public List<Vector3> ScannedPoints;


    public ScannedObject(ScannedTypeGameObject _thisGameObject)
    {
        _baseObjectComponent = _thisGameObject;
        _name = _baseObjectComponent.gameObject.name;
        _position = _baseObjectComponent.gameObject.transform.position;
        _localScale = _baseObjectComponent.gameObject.transform.localScale; 
        _rotation = _baseObjectComponent.gameObject.transform.rotation;
        _classificationType = _baseObjectComponent.ObjectClassification;
        if (_thisGameObject.ScannedPoints != null)
        {
            var list = _thisGameObject.ScannedPoints.ToList();
            //list.Reverse();
            ScannedPoints = list;
        }
    }

    /// <summary>
    /// Updates properties from the base gameobject
    /// </summary>
    public void UpdateBaseObject()
    {
        _position = _baseObjectComponent.gameObject.transform.position;
        _localScale = _baseObjectComponent.gameObject.transform.localScale;
        _rotation = _baseObjectComponent.gameObject.transform.rotation;
    }

    public ScannedObject(SerializationInfo info,  StreamingContext context)
    {
        _name = info.GetString("name");

        _classificationType = (ScannedObjectsClassificationType)info.GetValue("classificationType", typeof(ScannedObjectsClassificationType));
        _position = (Vector3)info.GetValue("position",typeof (Vector3));
        _localScale = (Vector3)info.GetValue("scale", typeof(Vector3));
        _rotation = (Quaternion)info.GetValue("rotation", typeof(Quaternion));
        ScannedPoints = ((ListBasic)info.GetValue("ListWallPoints", typeof(ListBasic))).List;

        var newGameObj = new GameObject(_name);
        _baseObjectComponent = newGameObj.AddComponent<ScannedTypeGameObject>();
        _baseObjectComponent.ObjectClassification = _classificationType;
        _baseObjectComponent.ScannedObjectData = this;

        if(ScannedPoints!=null)
        {
            var reverseList = ScannedPoints?.ToList();
            _baseObjectComponent.ScannedPoints = new Stack<Vector3>(reverseList);
        }
        
        _baseObjectComponent.transform.SetPositionAndRotation(_position, _rotation);
        _baseObjectComponent.transform.localScale = _localScale;
        
    }
    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("classificationType", _classificationType, typeof(ScannedObjectsClassificationType));
        info.AddValue("name", _name, typeof(string));

        info.AddValue("position", _position, typeof(Vector3));
        info.AddValue("rotation", _rotation, typeof(Quaternion));
        info.AddValue("scale", _localScale, typeof(Vector3));

        info.AddValue("ListWallPoints", new ListBasic() { List = ScannedPoints }, typeof(ListBasic));

        //info.AddValue("obj", _localScale, typeof(GameObject));
    }


    
}
