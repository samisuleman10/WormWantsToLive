using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ScannedObjectFactory 
{

    public static List<ScannedObjectsClassificationType> SimpleTypes = new List<ScannedObjectsClassificationType>()
    {  ScannedObjectsClassificationType.Table, ScannedObjectsClassificationType.Door, ScannedObjectsClassificationType.Sofa, ScannedObjectsClassificationType.Closet, ScannedObjectsClassificationType.Misc, ScannedObjectsClassificationType.Seat };
    //{ ScannedObjectsClassificationType.Table, ScannedObjectsClassificationType.Door };

    public static List<ScannedObjectsClassificationType> ComplexTypes = new List<ScannedObjectsClassificationType>()
    { };
    //{ ScannedObjectsClassificationType.Walls, ScannedObjectsClassificationType.Boxes, ScannedObjectsClassificationType.Sofa, ScannedObjectsClassificationType.Chairs};


    public static ScannedObject CreateScannedObject(ScannedTypeGameObject oldObj)
    {
         
        switch(oldObj.ObjectClassification)
        {
            case var _ when ComplexTypes.Contains(oldObj.ObjectClassification):
                //Complex objects are the ones with multiple colliders
                return new ScannedComplexObject(oldObj);

            //case ScannedObjectsClassificationType.Door:
            case var _ when SimpleTypes.Contains(oldObj.ObjectClassification):
                //Simple objects have one collider
                return new ScannedSimpleObject(oldObj);

            default:
                //Objects with no colliders
                //return new ScannedObject(oldObj);
                return new ScannedComplexObject(oldObj);

        }
    }
}
