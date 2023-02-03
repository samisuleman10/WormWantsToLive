using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FTAEditMesh : FTASelectMeshBase
{


    protected GameObject _currentPreSelectMarker = null;
    private Coroutine _deselectMarkerRoutine;

    public float _waitPreSelectTime = 0.8f;
    public float _markerScale = 2.9f;

    private GameObject _selectedMarker;
    private Transform _selectedMarkerOriginalParent;
    
    private Tuple<MeshFilter, List<int>> _selectedMeshData;
    private HashSet<GameObject> _markersWithEffect = new HashSet<GameObject> ();


    static void ToggleLaserStatus(bool status)
    {
        FingerLaser?.ToggleRenderer(status);        
    }

    public override void OnCancel(GameObject target, GameObject cursorPosition)
    {
        base.OnCancel(target, cursorPosition);
        ToggleLaserStatus(false);
    }

    public override void OnConfirm(ScannedObjectsClassificationType objectTypes = ScannedObjectsClassificationType.NonSelected, bool creationDelay = true)
    {
        //FTAEditMesh.ChangeModuleStatus(true);
        base.OnConfirm(objectTypes, creationDelay: creationDelay);
        ToggleLaserStatus(false);

    }

    public override void OnSelect(GameObject target, bool toEdit)
    {
        if (target == null)
            return;
        //Not a marker
        if (target.tag != _markerTag && _selectedMarker == null)
        {
            base.OnSelect(target, toEdit);
        }
        else
        {
            //Nothing is selected, do pre select
            if (_selectedMarker == null)
            {
                //still the same marker selected
                if (target == _currentPreSelectMarker)
                {
                    if (_deselectMarkerRoutine != null)
                        StopCoroutine(_deselectMarkerRoutine);
                    StartCoroutine(ApplyEffectWhilePreSelected(target));
                }
                else
                {
                    _currentPreSelectMarker = target;
                    StartCoroutine(ApplyEffectWhilePreSelected(target));
                }
            }
        }
    }
    public override void OnDeselect(GameObject target )
    {
        //if (!_IsCurrentlySelecting)
        //    return;

        if ((_currentPreSelectMarker != null && _currentPreSelectMarker.tag == _markerTag) )
            _deselectMarkerRoutine = StartCoroutine(CancelPreselect());

    }
    public override void OnTap(GameObject target, GameObject cursorPosition)
    {
        if (!_IsCurrentlySelecting)
            return;

            if (_currentPreSelectMarker!= null && !_markersToBaseObject.ContainsKey(_currentPreSelectMarker))
                Debug.LogWarning("Selected marker could not find Mesh");
            else
            {
                //changes selected to non selected. Stops edditing marker
                if (_selectedMarker != null)
                {
                    _selectedMarker.transform.SetParent(_selectedMarkerOriginalParent, true);
                    _selectedMarker = null;
                    _selectedMarkerOriginalParent = null;
                    ToggleLaserStatus(true);
                }

                //Changes preselect to select
                if (_currentPreSelectMarker != null)
                {
                    _selectedMarkerOriginalParent = _currentPreSelectMarker.transform.parent;
                    _selectedMarker = _currentPreSelectMarker;
                    _selectedMeshData = _markersToBaseObject[_currentPreSelectMarker];

                    _currentPreSelectMarker.transform.SetParent(cursorPosition.transform, true);
                    _currentPreSelectMarker.transform.position = cursorPosition.transform.position;
                    _currentPreSelectMarker = null;
                    ToggleLaserStatus(false);
                }
            }

    }

    public virtual void Update()
    {
        if (_selectedMarker != null)
        {
            var mesh = _selectedMeshData.Item1.mesh;

            Matrix4x4 worldToLocal = _selectedMeshData.Item1.gameObject.transform.worldToLocalMatrix;

            var vertices = mesh.vertices;
            foreach (var index in _selectedMeshData.Item2)
            {
                vertices[index] = worldToLocal.MultiplyPoint3x4(_selectedMarker.transform.position);
            }
            mesh.SetVertices(vertices);
        }
    }

    private IEnumerator ApplyEffectWhilePreSelected(GameObject marker)
    {
        yield return null;
        if (marker == null)
            yield break;
          
        ApplyPreSelectEffectToMarker(marker);
        yield return new WaitUntil(() => marker != _currentPreSelectMarker);
        if(marker != null)
            ApplyRegularEffectToMarker(marker);
        else
            ApplyRegularEffectToMarker(_currentPreSelectMarker);
    }
    private IEnumerator CancelPreselect()
    {
        yield return new WaitForSeconds(_waitPreSelectTime);
        _currentPreSelectMarker = null;
    }

    private void ApplyPreSelectEffectToMarker(GameObject marker)
    {
        if (!_markersWithEffect.Contains(marker))
        {
            _markersWithEffect.Add(marker);
            marker.transform.localScale *= _markerScale;
        }
    }
    private void ApplyRegularEffectToMarker(GameObject marker)
    {
        if (_markersWithEffect.Contains(marker))
        {
            _markersWithEffect.Remove(marker);
            marker.transform.localScale /= _markerScale;
        }
    }

}
