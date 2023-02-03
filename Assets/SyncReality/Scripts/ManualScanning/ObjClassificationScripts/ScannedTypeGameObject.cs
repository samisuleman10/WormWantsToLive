using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScannedTypeGameObject : MonoBehaviour
{
    [HideInInspector]
    public ScannedObject ScannedObjectData;

    [HideInInspector]
    public Stack<Vector3> ScannedPoints;
    public ScannedObjectsClassificationType ObjectClassification
    {
        get { return this._objectClassification; }
        set
        {
            if (value != _oldObjectClassification)
            {
                _objectClassification = value;
                DoClassificationLogic(value);
                _oldObjectClassification = value;
            }
        }
    }

    public bool IgnoreClassificationData = false;

    [SerializeField]
    private ScannedObjectsClassificationType _objectClassification;
    private ScannedObjectsClassificationType _oldObjectClassification;



#if UNITY_EDITOR
    void OnValidate()
    {
        if(_objectClassification != _oldObjectClassification)
        {
            DoClassificationLogic(_objectClassification);
            _oldObjectClassification = _objectClassification;
        }
    }
#endif

    public void DoClassificationLogic(ScannedObjectsClassificationType newClassification)
    {
        if(IgnoreClassificationData)
            return; 
        this._objectClassification = newClassification;
        ScannedObjectData = ScannedObjectFactory.CreateScannedObject(this);
    }

    
    
}
