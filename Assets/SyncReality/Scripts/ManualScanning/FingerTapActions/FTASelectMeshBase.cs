using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class FTASelectMeshBase : FingerTapActionBase
{
    //TODO Store renderer to optimize getcomponent<>

    [Header("Select and Edit Mesh Config")]
    public Material SelectedMeshMaterial;
    

    protected Dictionary<GameObject, Tuple<MeshFilter, List<int>>> _markersToBaseObject = new Dictionary<GameObject, Tuple<MeshFilter, List<int>>>();

    protected static bool _IsCurrentlySelecting = false;
    protected static List<GameObject> _CurrentlySelectedObjs = new List<GameObject>();

    //current selected and pre select GameObjects and its markers 
    private Dictionary<GameObject, List<GameObject>> _selectedObjMarkers = new Dictionary<GameObject, List<GameObject>>();
    private Dictionary<GameObject, Material> _originalMaterials = new Dictionary<GameObject, Material>();
    
    protected static LaserEditVisualizer FingerLaser;


    /// <summary>
    /// Creates markers for selected mesh by index finger
    /// </summary>
    /// <param name="target"></param>
    /// <param name="cursorPosition"></param>
    public override void OnSelect(GameObject target, bool editGestureEnabled)
    {
        if(editGestureEnabled)
        {   
            if (_CurrentlySelectedObjs.Contains(target))
                DelesectObject(target);
            else
                SelectObject(target);
        }
    }
    public override void OnCancel(GameObject target, GameObject cursorPosition)
    {
        CancelMethod();
    }

    public void CancelMethod()
    {
        var keysCopy = new List<GameObject>(_selectedObjMarkers.Keys);
        foreach (var selectedObj in keysCopy)
        {
            DelesectObject(selectedObj);
        }

        _selectedObjMarkers.Clear();
        keysCopy.Clear();

        _IsCurrentlySelecting = false;
        _CurrentlySelectedObjs.Clear();
    }

    public override void OnConfirm(ScannedObjectsClassificationType objectTypes = ScannedObjectsClassificationType.NonSelected, bool creationDelay = true)
    {
        CancelMethod();
    }
    public void SelectObject(GameObject toSelect)
    {
        //Object is not already selected
        if (_CurrentlySelectedObjs.Contains(toSelect))
            return;

        var meshFilter = toSelect.GetComponent<MeshFilter>();
        var renderer = toSelect.GetComponent<MeshRenderer>();
        //Mesh not available
        if (meshFilter == null || renderer == null || !meshFilter.sharedMesh.isReadable)
        {
            Debug.LogError("MeshRenderer data invalid - FTASelect");
            return;
        }

        //Preserve original material
        if (!_originalMaterials.ContainsKey(toSelect))
            _originalMaterials.Add(toSelect, renderer.material);

        _CurrentlySelectedObjs.Add(toSelect);

        if (renderer != null && SelectedMeshMaterial != null)
            renderer.material = SelectedMeshMaterial;

        if (_CurrentlySelectedObjs.Count > 0)
        {
            _IsCurrentlySelecting = true;
            FingerLaser.ToggleRenderer(true);
        }

        CreateMeshMarkers(meshFilter);
    }

    private void Start()
    {
        FingerLaser = GameObject.FindObjectOfType<LaserEditVisualizer>();
    }

    protected void DelesectObject(GameObject toDeselect)
    {
        if (!_CurrentlySelectedObjs.Contains(toDeselect))
            return;

        _CurrentlySelectedObjs.Remove(toDeselect);

        //unlink gameobject to markers
        if (_selectedObjMarkers.ContainsKey(toDeselect))
            foreach (var marker in _selectedObjMarkers[toDeselect])
                _markersToBaseObject.Remove(marker);


        if ( _selectedObjMarkers.ContainsKey(toDeselect))
        {
            ClearPoints(_selectedObjMarkers[toDeselect]);
        }
            var renderer = toDeselect?.GetComponent<MeshRenderer>();
            if (renderer != null && _originalMaterials.ContainsKey(toDeselect))
                renderer.material = _originalMaterials[toDeselect];
            
        _selectedObjMarkers.Remove(toDeselect);
        if (_CurrentlySelectedObjs.Count < 1)
        {
            _IsCurrentlySelecting = false;
            FingerLaser.ToggleRenderer(false);
        }
    }

    private void CreateMeshMarkers(MeshFilter meshFilter)
    {
        //One collection of markers per object
        if (_selectedObjMarkers.ContainsKey(meshFilter.gameObject))
            return;

        //Create Markers. Maybe create markers on tap instead of select?
        Matrix4x4 localToWorld = meshFilter.gameObject.transform.localToWorldMatrix;
        Dictionary<Vector3, GameObject> _posCreatedMarkers = new Dictionary<Vector3, GameObject>();
        bool _isNemMarker = false;
        for (int i = 0; i < meshFilter.sharedMesh.vertices.Length; i++)
        {
            _isNemMarker = false;
            GameObject newMarker;
            if (!_posCreatedMarkers.ContainsKey(meshFilter.sharedMesh.vertices[i]))
            {
                newMarker = CreateNewMarker(localToWorld.MultiplyPoint3x4(meshFilter.sharedMesh.vertices[i]), false);
                _posCreatedMarkers.Add(meshFilter.sharedMesh.vertices[i], newMarker);
                _isNemMarker = true;
            }
            else
                newMarker = _posCreatedMarkers[meshFilter.sharedMesh.vertices[i]];

            //Store markers to gameobject. First adds object key
            if (_selectedObjMarkers.ContainsKey(meshFilter.gameObject))
            {
                if (_isNemMarker)
                    _selectedObjMarkers[meshFilter.gameObject].Add(newMarker);
            }
            else
                _selectedObjMarkers.Add(meshFilter.gameObject, new List<GameObject>() { newMarker });

            //Link gameobject to markers

            if (_markersToBaseObject.ContainsKey(newMarker))
                _markersToBaseObject[newMarker].Item2.Add(i);
            else
                _markersToBaseObject.Add(newMarker, new Tuple<MeshFilter, List<int>>(meshFilter, new List<int>() { i }));

            
        }
    }
}
