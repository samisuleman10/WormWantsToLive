using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FingerTapActionBase : MonoBehaviour
{
    [Header("FTA Base Config")]
 
    private Transform RootSpamParent;
    public GameObject PointMarkerTemplate;

    private ScannedObject _scannedObject;

    protected string _markerTag = "Marker";
    protected string _markerLayerName = "Marker";
    protected GameObject _preparedMarker;

    //C# 6.0
    //protected int _isWaitingCreatingDelay { protected get; private set; }

    protected bool _isWaitingCreatingDelay = false;

    protected List<GameObject> pointMarkers = new List<GameObject>();

    private float _creatingDelay = 0.09f;


    private void Start()
    {
        RootSpamParent = RoomSelector.GetRootSpamObj()?.transform;
    }

    private void OnDisable()
    {
        if (_preparedMarker != null)
            Destroy(_preparedMarker);

        ClearPoints();
    }

    public virtual void OnTap(GameObject target, GameObject cursorPosition)    { }
    public virtual void OnRelease(GameObject target, GameObject cursorPosition){}
    public virtual void OnHold(GameObject target, GameObject cursorPosition) { }

    public virtual void OnConfirm(ScannedObjectsClassificationType objectTypes = ScannedObjectsClassificationType.NonSelected, bool creationDelay = true) { }

    public virtual void OnCancel(GameObject target, GameObject cursorPosition) { }

    public virtual void OnSelect(GameObject target, bool ToEdit = false) { }

    public virtual void OnDeselect(GameObject target) { }

    public virtual void OnUndo() { }



    /*public virtual void OnSecondarySelect(GameObject target, GameObject cursorPosition)
    {
    }*/

    public virtual void ActionEventSwitch(FingerTapCursor.FingerTapEventParameters parameters)
    {
        if(parameters == null)
            return;
        //Debug.Log("On ActionEventSwitch");
        switch (parameters.EventtoActivate)
        {
            case FingerTapEvent.Tap:
                {
                    OnTap(parameters.GameData, parameters.SelectorData);
                    break;
                }
            case FingerTapEvent.Hold:
                {
                    OnHold(parameters.GameData, parameters.SelectorData);
                    break;
                }
            case FingerTapEvent.Release:
                {
                    OnRelease(parameters.GameData, parameters.SelectorData);
                    break;
                }
            case FingerTapEvent.Confirm:
                {
                    //Is a classified object
                    var _newType = parameters.GameData?.GetComponent<ScannedTypeGameObject>();
                    if (_newType != null)
                        OnConfirm(_newType.ObjectClassification);
                    else
                        OnConfirm();

                    break;
                }
            case FingerTapEvent.Cancel:
                {
                    OnCancel(parameters.GameData, parameters.SelectorData);
                    break;
                }
            case FingerTapEvent.Select:
                {
                    OnSelect(parameters.GameData, parameters.ChangeStatus);
                    break;
                }
            case FingerTapEvent.Deselect:
                {
                    OnDeselect(parameters.GameData);
                    break;
                }
            case FingerTapEvent.Undo:
                {
                    OnUndo();
                    break;
                }
            default:
                Debug.LogError("Went to default:" + DateTime.Now.Ticks);
                break;
                /* case FingerTapEvent.SecondarySelect:
                     {
                         OnSecondarySelect(actiontarget, cursor);
                         break;
                     }*/
        }
    }

    protected void SetMarkerModel()
    {
        if (PointMarkerTemplate != null)
        {
            var instaPrefab = Instantiate(PointMarkerTemplate, RootSpamParent);
            var instaCollider = instaPrefab.GetComponent<Collider>();
            if(instaCollider == null)
                instaCollider = instaPrefab.GetComponentInChildren<Collider>();
            if (instaCollider == null)
            {
                Debug.LogError("Marker prefab cannot be used because does not contain a collider");
                _preparedMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                _preparedMarker.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
            }
            else
            {
                _preparedMarker = Instantiate(instaCollider.gameObject, RootSpamParent);
                _preparedMarker.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
                Destroy(instaPrefab);
            }
        }
        else
        {
            _preparedMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _preparedMarker.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
        }
        //TODO change to layer to ignore later
        _preparedMarker.tag = _markerTag;

        int layer = LayerMask.NameToLayer(_markerLayerName);
        if (layer > -1)
            _preparedMarker.layer = layer;
        else
            Debug.LogError("Layer for markers not found. Layer name: " + _markerLayerName);


        var colliderSphere = _preparedMarker.GetComponent<SphereCollider>();

        if (colliderSphere == null)
            colliderSphere = _preparedMarker.GetComponentInChildren<SphereCollider>();

        if (colliderSphere != null)
        {
            colliderSphere.radius *= 3f;
            colliderSphere.gameObject.tag = _markerTag;
        }

        var colliderBox = _preparedMarker.GetComponent<BoxCollider>();
        if (colliderBox != null)
        {
            colliderBox.size *= 3f;
            colliderBox.gameObject.tag = _markerTag;
        }
        _preparedMarker.gameObject.SetActive(false);
    }


    protected virtual GameObject CreateNewMarker(Vector3 position, bool waitDelay = true)
    {
        if (_preparedMarker == null)
            SetMarkerModel();

        if (_isWaitingCreatingDelay)
            return null;

        if(waitDelay)
            StartCoroutine(WaitCreationDelay());


        //var newMarker = Instantiate(_preparedMarker, cursor.transform);
        var newMarker = Instantiate(_preparedMarker, RootSpamParent);
        newMarker.transform.position = position;
        newMarker.SetActive(true);
        newMarker.transform.SetParent(RootSpamParent, true);

        pointMarkers.Add(newMarker);

        MarkerManager.AddMarker(newMarker);

        return newMarker;
    }

    protected virtual void RemoveMarker(int position)
    {
        if (position < 0 || position >= pointMarkers.Count)
            return;

        var marker = pointMarkers[position];
        pointMarkers.RemoveAt(position);
        Destroy(marker.gameObject);
    }

    protected virtual void ClearPoints()
    {
        foreach (var p in pointMarkers)
            if (p != null)
                Destroy(p);
        pointMarkers.Clear();

        MarkerManager.ClearNullMarkers();

    }

    protected virtual void ClearPoints(List<GameObject> markers)
    {
        foreach (var p in markers)
            if (p != null)
                Destroy(p);
        markers.Clear();
        MarkerManager.ClearNullMarkers();
    }

    protected IEnumerator WaitCreationDelay()
    {
        _isWaitingCreatingDelay = true;
        yield return new WaitForSeconds(_creatingDelay);
        _isWaitingCreatingDelay = false;
    }
    protected IEnumerator WaitCreationDelay(float time)
    {
        _isWaitingCreatingDelay = true;
        yield return new WaitForSeconds(time);
        _isWaitingCreatingDelay = false;
    }

}
