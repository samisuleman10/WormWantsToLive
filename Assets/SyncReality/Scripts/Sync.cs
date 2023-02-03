using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Syncreality.Playfield;
using UnityEditor;
using UnityEngine;



/*public enum Classification
{
    
    Misc = 1 << 0,
    Wall = 1 << 1,
    Floor = 1 << 2,
    Ceiling = 1 << 3,
    Window = 1 << 4,
    Table = 1 << 5,
    Seat = 1 << 6,
    Base = 1 << 7,
    Sofa = 1 << 8,
    Door = 1 << 9,
    Portal = 1 << 10,  
}*/
public enum Classification
{
    
    Misc = 1 << 0,
    Table = 1 << 1,
    Closet = 1 << 2,
    Sofa = 1 << 3,
    Seat = 1 << 4
}
public enum SyncQuality
{
    Hero,
    Normal,
    Clutter,
    Virtual
}
[SelectionBase]
public class Sync : MonoBehaviour
{
    public string ID;

    [Header("Handle Settings")]
    public float handleOffset;
    public float handleSize; 

    [Header("Sync Settings")] 
    public Classification classification;

    public List<Classification> classificationList
    {
        get
        {
            return Classification.GetValues(classification.GetType()).Cast<Classification>().Where(v=>classification.HasFlag(v)).ToList();
        }
    }

    

    private SyncMaster _syncMaster;
    private GameObject hoverCube; 
    public Bounds bounds;
    public bool useSizes = false;
    public Vector3 minSize, maxSize;
    public SyncQuality quality;
    [HideInInspector] public List<SyncLink> syncLinks = new List<SyncLink>(); 
    [HideInInspector] public float handleHeight;
    [HideInInspector] public string referencePrefabName;
    [HideInInspector] public string referencePrefabPath;
    [HideInInspector] public bool hasPlayfield;
    [HideInInspector] public ScanVolume scan;
    
    public bool hasFreeArea;
    public Rect freeArea = new Rect(0,0,2,2); 


    public SyncData  GetSyncData()
    {
        return new SyncData(this);
    }

    public void AddPlayfieldToSync()
    {
        hasPlayfield = true;
        GameObject pfObj = Instantiate(_syncMaster.playfieldPrefab);
        pfObj.transform.parent = this.transform;

        var pos = transform.position + new Vector3(0, bounds.size.y + 0.001f, 0);

        pfObj.transform.position = pos;
        pfObj.transform.localScale = new Vector3(bounds.size.x, pfObj.transform.localScale.y, bounds.size.z).ScaleBy(transform.localScale.invert()) * 0.8f;


    }
    public void RemovePlayfieldFromSync()
    {
        hasPlayfield = false;
        DestroyImmediate(transform.GetComponentInChildren<PlayfieldInteraction>().gameObject);

    }
    
    public List<SyncLink> GetSyncLinksFromComponents()
    {
        bool resetSyncLinksList = false;
        for (int i = 0; i < syncLinks.Count; i++)
            if (syncLinks[i] == null)
                resetSyncLinksList = true;
        if (syncLinks.Count == 0)
            resetSyncLinksList = true;
        if (resetSyncLinksList)
        {
            syncLinks = new List<SyncLink>();
            foreach (var link in GetComponents<SyncLink>())
                syncLinks.Add(link);
        }

        return syncLinks;
    }
    public List<GameObject> GetSyncLinksAsGameObjects()
    {
        List<GameObject> returnList = new List<GameObject>();

        foreach (SyncLink link in syncLinks)
            returnList.Add(link.linkedObject); 
        return returnList;
    }
    public List<SyncLink> GetPassiveSyncLinks()
    {
        if (_syncMaster == null)
            _syncMaster = FindObjectOfType<SyncMaster>();
        if (_syncMaster == null)
            Debug.LogWarning("Wrong parent for Sync?");
        List<SyncLink> returnList = new List<SyncLink>();
        foreach (var syncLink in _syncMaster.GetAllSyncLinks())
            if (syncLink.linkedObject == this.gameObject )
                 returnList.Add(syncLink);
        return returnList;
    }
    public List<GameObject> GetPassiveSyncLinksAsGameObjects()
    {
        if (_syncMaster == null)
            _syncMaster = FindObjectOfType<SyncMaster>();
        if (_syncMaster == null)
            Debug.LogWarning("Wrong parent for Sync?");
        List<GameObject> returnList = new List<GameObject>();
        foreach (var syncLink in _syncMaster.GetAllSyncLinks())
            if (syncLink.linkedObject == this.gameObject )
                returnList.Add(syncLink.gameObject);
        return returnList;
    }
    public bool HasSyncLinkForGameObject(GameObject obj)
    {
        foreach (SyncLink link in syncLinks)
            if (link.linkedObject == obj)
                return true;
        return false;
    }
    public bool HasPassiveSyncLinkForGameObject(GameObject obj)
    {
        foreach (SyncLink link in GetPassiveSyncLinks())
            if (link.parentSync.gameObject == obj)
                return true;
        return false;
    }
    

    private SyncLink GetSyncLinkFromLinkedObject(GameObject obj)
    {
        SyncLink returnLink = null;
        foreach (SyncLink link in GetSyncLinksFromComponents())
            if (link.linkedObject == obj || obj == this.gameObject )
                returnLink = link;
        if ( returnLink == null)
             Debug.LogWarning("Did not find SyncLink for gameobject: " + obj.name);
        return returnLink;
    }
    private SyncLink GetSyncLinkFromLinkedObject(SyncLink syncLink)
    {
        SyncLink returnLink = null;
        foreach (SyncLink link in GetSyncLinksFromComponents())
            if (link == syncLink || syncLink.linkedObject == this.gameObject)
                returnLink = link;
        if ( returnLink == null)
            Debug.LogWarning("Did not find SyncLink for gameobject: " + syncLink.ID);
        return returnLink;
    }

 
#if UNITY_EDITOR

    public void CreateSyncLink(GameObject targetObject, bool linkedToSurroundSync = false)
    {
        bool fliparoo = false;

        if (quality != SyncQuality.Virtual) // avoid the only Virtual being secondary
            if (targetObject.GetComponent<Sync>()!= null)
                if (targetObject.GetComponent<Sync>().quality == SyncQuality.Virtual)
                    fliparoo = true; 
        if (fliparoo)
        {
            Debug.LogWarning("No SyncLink for you! You can only create a Virtual-to-non-Virtual SyncLink, not the other way around! ");
        }
        else
        {
            SyncLink syncLink =  Undo.AddComponent<SyncLink>(gameObject);
            syncLink.linkedObjectIsSurroundSync = linkedToSurroundSync;
            syncLink.parentSync = this;
            syncLink.linkedObject = targetObject;
            syncLink.ID = "SL" + syncLink.parentSync.gameObject.name + syncLink.linkedObject.gameObject.name; 
        
            syncLinks.Add(syncLink );

            FindObjectOfType<ToolWindowSettings>().forceUpdateContentSections = true;
            Undo.RegisterCompleteObjectUndo(gameObject, "SyncLinking");
        } 
    
    }
    public void DestroySyncLink(SyncLink destroyedSyncLink)
    {
         Undo.DestroyObjectImmediate(GetSyncLinkFromLinkedObject(destroyedSyncLink));
       FindObjectOfType<ToolWindowSettings>().forceUpdateContentSections = true;
       SceneView.RepaintAll();
    }
    public void DestroySyncLinkObject(GameObject destroyedSyncLink)
    {
        Undo.DestroyObjectImmediate(GetSyncLinkFromLinkedObject(destroyedSyncLink));
        FindObjectOfType<ToolWindowSettings>().forceUpdateContentSections = true;
        SceneView.RepaintAll();

    }
#endif
    /// <summary>
    /// Sets the bounds used to replace ScanVolumes to the bounds of all submeshes
    /// </summary>
    public void CreateCombinedMesh( )
    {

        /*
        var allMF = GetComponentsInChildren<MeshFilter>(false);
//        Debug.Log("Meshes Founds: " + allMF.Length);

        var newMid = transform.InverseTransformPoint(new Vector3(
            allMF.Max(m => m.transform.TransformPoint(m.sharedMesh.bounds.max).x) + (allMF.Min(m => m.transform.TransformPoint(m.sharedMesh.bounds.min).x)),
            allMF.Max(m => m.transform.TransformPoint(m.sharedMesh.bounds.max).y) + (allMF.Min(m => m.transform.TransformPoint(m.sharedMesh.bounds.min).y)),
            allMF.Max(m => m.transform.TransformPoint(m.sharedMesh.bounds.max).z) + (allMF.Min(m => m.transform.TransformPoint(m.sharedMesh.bounds.min).z))
            ) * 0.5f);

        var newSize = (new Vector3(
            allMF.Max(m => (Quaternion.Inverse(transform.rotation) * m.transform.TransformPoint(m.sharedMesh.bounds.max)).x) - (allMF.Min(m => (Quaternion.Inverse(transform.rotation) * m.transform.TransformPoint(m.sharedMesh.bounds.min)).x)),
            allMF.Max(m => (Quaternion.Inverse(transform.rotation) * m.transform.TransformPoint(m.sharedMesh.bounds.max)).y) - (allMF.Min(m => (Quaternion.Inverse(transform.rotation) * m.transform.TransformPoint(m.sharedMesh.bounds.min)).y)),
            allMF.Max(m => (Quaternion.Inverse(transform.rotation) * m.transform.TransformPoint(m.sharedMesh.bounds.max)).z) - (allMF.Min(m => (Quaternion.Inverse(transform.rotation) * m.transform.TransformPoint(m.sharedMesh.bounds.min)).z))));
        */



        var b = combinedMesh();
        bounds = b;
    }

    public Bounds combinedMesh()
    {
        var pos = transform.position;
        var rot = transform.rotation;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;

        var allMF = GetComponentsInChildren<MeshRenderer>(false);
        //        Debug.Log("Meshes Founds: " + allMF.Length);

        var newMid = transform.InverseTransformPoint(new Vector3(
            allMF.Max(m => (m.bounds.max).x) + (allMF.Min(m => (m.bounds.min).x)),
            allMF.Max(m => (m.bounds.max).y) + (allMF.Min(m => (m.bounds.min).y)),
            allMF.Max(m => (m.bounds.max).z) + (allMF.Min(m => (m.bounds.min).z))
            ) * 0.5f).ScaleBy(transform.localScale);

        var newSize = transform.InverseTransformPoint(new Vector3(
            allMF.Max(m => ((m.bounds.max)).x) -
            (
            allMF.Min(m => ((m.bounds.min)).x)),
            allMF.Max(m => ((m.bounds.max)).y) -
            (
            allMF.Min(m => ((m.bounds.min)).y)),
            allMF.Max(m => ((m.bounds.max)).z) -
            (
            allMF.Min(m => ((m.bounds.min)).z)))).ScaleBy( transform.localScale);

        transform.position = pos;
        transform.rotation = rot;

        var b = new Bounds(
            newMid,
            newSize);
        return b;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(1,0,0,0.25f);
        //Gizmos.DrawCube(bounds.center, bounds.size * 1.01f);
        Gizmos.matrix = Matrix4x4.identity;
    }


  
}
    
    
 
