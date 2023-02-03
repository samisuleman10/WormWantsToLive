using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using JetBrains.Annotations;

#if UNITY_EDITOR
[Serializable]
public struct DesignAreaData
{
    /*public int wallPiecesAll;
    public int wallPiecesOne;
    public int wallPiecesTwo;
    public int wallPiecesThree;
    public int wallPiecesFour;
    public float ceilingHeight;*/

    public DesignAreaData(DesignArea designArea)
    {
        /*wallPiecesAll = designArea.wallPiecesAll; 
        wallPiecesOne = designArea.wallPiecesOne;
        wallPiecesTwo = designArea.wallPiecesTwo;
        wallPiecesThree = designArea.wallPiecesAll;
        wallPiecesFour = designArea.wallPiecesFour;
    //    ceilingHeight = designArea.flexFloor.ceilingHeight;
       ceilingHeight =5;*/
    }
}
#endif

[Serializable]
public struct SyncData
{
    public Matrix4x4 TransformationMatrix4X4;
    public string Name;
    public string ID;
    public Classification classification;
    public SyncQuality syncQuality;
    public Bounds meshBounds;
    public bool useSizes;
    public Vector3 minSize;
    public Vector3 maxSize;
    public string referencePrefabName;
    public string referencePrefabPath;
    

    
    public SyncData(Sync sync )
    {
        this.Name = sync.gameObject.name;
        this.ID = sync.ID;
        this.TransformationMatrix4X4 = sync.transform.localToWorldMatrix;
        this.classification = sync.classification;
        this.syncQuality = sync.quality;
        this.meshBounds = sync.bounds;
        this.useSizes = sync.useSizes;
        this.minSize = sync.minSize;
        this.maxSize = sync.maxSize;
        this.referencePrefabName = sync.referencePrefabName;
        this.referencePrefabPath = sync.referencePrefabPath;
    }
}
[Serializable]
public struct SurroundSyncData
{
    public string Name;
    public string ID;
    public bool basePiece;
    public bool baseReplaceFace1; 
    public bool baseReplaceFace2; 
    public bool baseReplaceFace3; 
    public bool baseReplaceFace4; 
    public WallFace face;
    public int mapToIndex;
    public int priority;
    public Bounds meshBounds;
    public int indexOrder;
    public int repeatForAmount;   
    public string referencePrefabName;
    public string referencePrefabPath;

    
    public SurroundSyncData(SurroundSync ssync)
    {
        this.Name = ssync.gameObject.name;
        this.ID = ssync.ID;
        this.basePiece = ssync.baseReplacer;
        this.baseReplaceFace1 = ssync.baseReplaceFace1;
        this.baseReplaceFace2 = ssync.baseReplaceFace2;
        this.baseReplaceFace3 = ssync.baseReplaceFace3;
        this.baseReplaceFace4 = ssync.baseReplaceFace4;
        this.face = ssync.face;
        this.meshBounds = ssync.bounds;
        this.mapToIndex = ssync.mapToIndex;
        this.priority = ssync.priority;
        this.indexOrder = ssync.offset;
        this.repeatForAmount = ssync.repeatForAmount;  
        this.referencePrefabPath = ssync.referencePrefabPath;
        this.referencePrefabName = ssync.referencePrefabName; 
    }
}
[Serializable]
public struct SyncLinkData
{ 
    public string Name;
    public string ID;
    public string parentObjectID;
    public string linkedObjectID;
    public bool linkedObjectIsSurroundSync;
    public float minDistance;
    public float preferredDistance;
    public float maxDistance;
    public bool topOf;
    public bool bottomOf;
    public bool rotationConstraint;
    public bool rotationOutside;
    public float minAngle; 
    public float maxAngle;

    public SyncLinkData(SyncLink syncLink )
    {
        this.Name = syncLink.ID;
        this.ID = syncLink.ID;
        this.linkedObjectIsSurroundSync = syncLink.linkedObjectIsSurroundSync;
        this.parentObjectID = syncLink.parentSync.ID;
        this.linkedObjectID = syncLink.linkedObjectIsSurroundSync ? syncLink.linkedObject.GetComponent<SurroundSync>().ID : syncLink.linkedObject.GetComponent<Sync>().ID; 
        this.minDistance = syncLink.minDistance;
        this.preferredDistance = syncLink.preferredDistance;
        this.maxDistance = syncLink.maxDistance;
        this.topOf = syncLink.topOf;
        this.bottomOf = syncLink.bottomOf;
        this.rotationConstraint = syncLink.rotationConstraint;
        this.rotationOutside = syncLink.rotationOutside;
        this.minAngle = syncLink.minAngle;
        this.maxAngle = syncLink.maxAngle;
    }
}
[Serializable]
public struct BackdropData
{
    public string name;
    public WallFace wallFace;
    public bool anchored;
    public string assetReferencePath;
    public float backdropDistance;
    public float backdropRotation;
    public float backdropHorizontalOffset;
    public float backdropVerticalOffset; 
    
    public BackdropData(Backdrop backdrop, WallFace face)
    {
        name = backdrop.gameObject.name;
        wallFace = face;
        anchored = backdrop.anchored;
        assetReferencePath = backdrop.assetReferencePath;
        backdropDistance = backdrop.backdropDistance;
        backdropRotation = backdrop.backdropRotation;
        backdropHorizontalOffset = backdrop.backdropHorizontalOffset;
        backdropVerticalOffset = backdrop.backdropVerticalOffset;
    }
}
[Serializable]
public struct FloorData
{
    public string materialReferencePath;
    
    public FloorData(string referencePath)
    {
        this.materialReferencePath = referencePath;
    }
}
[Serializable]
public struct CeilingData
{
    public string materialReferencePath;
    
    public CeilingData(string referencePath)
    {
        this.materialReferencePath = referencePath;
    }
}
[Serializable]
public struct MessData
{
    public List<string> anchoredIDs;
    
    public MessData(List<string> givenAnchors)
    {
        this.anchoredIDs = givenAnchors.ToList();
    }
}
[System.Serializable]
public class SyncLayout
{
    #if UNITY_EDITOR
    public DesignAreaData designAreaData;
    #endif
    public List<SyncData> syncDatas; 
    public List<SurroundSyncData> surroundSyncDatas; 
    public List<SyncLinkData> syncLinkDatas;
    public List<BackdropData> backdropDatas;
    public FloorData floorData;
    public CeilingData ceilingData;
    public MessData messData;

}
