using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PlacementRelation
{
    public PlacementRule placementRule;
    public bool onTop;
    public bool onSide;
    public float likeliness;
}

public class PlacementRule : MonoBehaviour
{
    public int amount;
    public Classification classification;
    public Vector3 minSize, maxSize;
    public bool canBeFlipped = false;
    [Header("IF ON ITS OWN")]
    public float likelinessToBePlacedOnWalls;
    public float likelinessToBePlacedWithoutWalls;
    public bool alignedToWall = false;
    public bool randomRotation = false;

    [Header("RELATION TO OTHERS")]
    public List<PlacementRelation> allowedConnectionsTo = new List<PlacementRelation>();


    [HideInInspector]
    public int leftAmountToSet = 0;

    public bool stacking = false;

    public void Init()
    {
        leftAmountToSet = amount;
    }


}
