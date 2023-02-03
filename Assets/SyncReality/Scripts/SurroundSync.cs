using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Syncreality.Playfield;
using UnityEngine;



[ExecuteInEditMode][Serializable][SelectionBase]
public class SurroundSync : MonoBehaviour
{
    public string ID;

    public GameObject model; 
      
    public bool baseReplacer;   // Replaces every single WallPiece, if no other SurroundSyncs overwrite it
    public bool baseReplaceRandomize;
    public bool baseReplaceFace1;
    public bool baseReplaceFace2;
    public bool baseReplaceFace3;
    public bool baseReplaceFace4;
    public WallFace face; // N,E,S or W face that the SS maps to
    public int mapToIndex;  // Specific WP index that the SS maps to 
    public int offset = 0; // Order-sequence when on same index as other piece
    public int priority;
    public int repeatForAmount;  // End-index of above stretch 
  //  public bool forceCorner;    // Spawns only next to Edge-points 
   // [Range(1,10)]
 //   public int width = 1;
 
    [HideInInspector] public string referencePrefabName;
    [HideInInspector] public string referencePrefabPath;
    public bool hasPlayfield;
    [HideInInspector] public GameObject sceneModel;

    public Bounds bounds;


    public float rotationVaricance;
    public float totalRotationOffset;
    public Vector3 scaleOffset = Vector3.zero;
    public Vector3 totalScaleOffset = Vector3.zero;
    public Vector3 translateVariance = Vector3.zero;
    public Vector3 totalTranslateOffset = Vector3.zero;

    private ModuleManager _moduleManager;
    private DesignArea _designArea;

    public ModuleManager ModuleManagerProp
    { 
        get
        {
            if (_moduleManager == null)
            {
                _moduleManager = FindObjectOfType<ModuleManager>();
            }
            return _moduleManager;
        }
    }
#if UNITY_EDITOR
    private void OnDestroy()
    {
   //todo annoying mystery bug: this gets called on playtime, causing build runtime errors, but no idea where from
        /*if (Application.isPlaying == false)
        {
            if (_designArea == null)
                _designArea = FindObjectOfType<DesignArea>(true); 
            _designArea.wallSplitter.DeleteSurroundSync(this, true); 
        } */
    }

    public void Initialize(GameObject modelObj, WallPiece wallPiece  = null  )
    {
        if (_designArea == null)
            _designArea = FindObjectOfType<DesignArea>(); 
        if (modelObj.name.Length < 8)
            this.gameObject.name = "SSync" + modelObj.name.Substring(0,modelObj.name.Length);
        else
            this.gameObject.name = "SSync" + modelObj.name.Substring(0,8);  
        this.ID = Guid.NewGuid().ToString();
        if (wallPiece != null)  // creation
        {
          //  this.transform.position = wallPiece.transform.position;
            mapToIndex = wallPiece.index;
            face = wallPiece.face;
            baseReplaceFace1 = true;
            baseReplaceFace2 = true;
            baseReplaceFace3 = true;
            baseReplaceFace4 = true;
        } 
        this.transform.SetParent(_designArea.GetFirstActivePresentSyncSetup().surroundSyncsParent.transform); 
     //   sceneModel =  Instantiate(modelObj, transform);
     modelObj.transform.SetParent(this.transform);
     modelObj.transform.localPosition = Vector3.zero;
     
        model = modelObj; 

    }
    #endif

    /*public bool baseReplacer;   // Replaces every single WallPiece, if no other SurroundSyncs overwrite it
    private bool prev_replacesAll;
    public SurrounderModule.WallFace face; // N,E,S or W face that the SS maps to
    private SurrounderModule.WallFace prev_face;
    public int mapToIndex;  // Specific WP index that the SS maps to
    private int prev_mapToIndex;
    public bool replacesExistingPiece; // Replaces any SS that might be already mapped to the WP (at lower priority)
    private bool prev_replacesExistingPiece;
    public bool touchSensitive; // Prefers 'real' walls, however we defined that from Scanner data
    private bool prev_touchSensitive;
    public bool multipleIndexes; // Repeats across a stretch of WPs
    private bool prev_multiplePieces; 
    public int repeatForAmount;  // End-index of above stretch
    private int prev_mapUntilPiece;
    public bool forceCorner;


    private void Update()
    {
        if (  prev_replacesAll != baseReplacer)
        {
            prev_replacesAll = baseReplacer;
            CallRefresh();
        }
        if (  prev_face != face)
        {
            prev_face = face;
            CallRefresh();
        }
        if (  prev_mapToIndex != mapToIndex)
        {
            prev_mapToIndex = mapToIndex;
            CallRefresh();
        }
        if (  prev_replacesExistingPiece != replacesExistingPiece)
        {
            prev_replacesExistingPiece = replacesExistingPiece;
            CallRefresh();
        }
        if (  prev_touchSensitive != touchSensitive)
        {
            prev_touchSensitive = touchSensitive;
            CallRefresh();
        }
        if (  prev_multiplePieces != multipleIndexes)
        {
            prev_multiplePieces = multipleIndexes;
            CallRefresh();
        }
        if (  prev_mapUntilPiece != repeatForAmount)
        {
            prev_mapUntilPiece = repeatForAmount;
            CallRefresh();
        }
        
    }

    private void CallRefresh()
    {
        FindObjectOfType<SurrounderDesign>().InitializeWallPieces(); 
        FindObjectOfType<SurrounderDesign>().SpawnExampleMinis();
    }*/


    public void CreateCombinedMesh()
    { 
        bounds = combinedMesh();
    }

    public Bounds combinedMesh()
    {
        Vector3 originalPos = model.transform.position;
        model.transform.position = Vector3.zero;
        var allMF = model.GetComponentsInChildren<MeshRenderer>(true);
        //        Debug.Log("Meshes Founds: " + allMF.Length);

        var newMid = model.transform.InverseTransformPoint(new Vector3(
            allMF.Max(m => (m.bounds.max).x) + (allMF.Min(m => (m.bounds.min).x)),
            allMF.Max(m => (m.bounds.max).y) + (allMF.Min(m => (m.bounds.min).y)),
            allMF.Max(m => (m.bounds.max).z) + (allMF.Min(m => (m.bounds.min).z))
            ) * 0.5f);

        var newSize = model.transform.InverseTransformPoint(new Vector3(
            allMF.Max(m => ((m.bounds.max)).x) -
            (
            allMF.Min(m => ((m.bounds.min)).x)),
            allMF.Max(m => ((m.bounds.max)).y) -
            (
            allMF.Min(m => ((m.bounds.min)).y)),
            allMF.Max(m => ((m.bounds.max)).z) -
            (
            allMF.Min(m => ((m.bounds.min)).z))));

        var s = newSize.ScaleBy(model.transform.localScale);
        //s.x = Mathf.Max(0.02f, s.x);
        var b = new Bounds(
            newMid.ScaleBy(model.transform.localScale),
            s);
        model.transform.position = originalPos;

        return b;
    }

    public SurroundSyncData GetSurroundSyncData()
    { 
        return new SurroundSyncData(this);
    }
    
    private void OnValidate()
    {
        /*if (ModuleManagerProp != null)
            ModuleManagerProp.UpdatePipeline();*/
        
    }

    public void AddPlayfieldToSurroundSync()
    {
        hasPlayfield = true;
        GameObject pfObj = Instantiate(FindObjectOfType<SyncMaster>().playfieldPrefab);
        pfObj.transform.parent = this.transform.GetChild(0);
        pfObj.transform.localRotation = Quaternion.Euler(90, 0, 0);
        pfObj.transform.localScale = new Vector3(bounds.size.x, 1, bounds.size.y) * 0.8f;
        pfObj.transform.localPosition = new Vector3(0, bounds.extents.y, bounds.extents.z); //todo smart prefab placement
    }

    public void RemovePlayfieldFromSurroundSync()
    {
        hasPlayfield = false;
        DestroyImmediate(transform.GetComponentInChildren<PlayfieldInteraction>().gameObject);
    }
}
