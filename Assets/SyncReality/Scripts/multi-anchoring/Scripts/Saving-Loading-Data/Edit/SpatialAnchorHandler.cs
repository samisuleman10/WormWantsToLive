using UnityEngine;
using System;

/// <summary>
/// Component instantiate, erases and saves an Anchor.
/// </summary>

public class SpatialAnchorHandler : BasicEventManager<SpatialAnchorHandler>
{
    [Header("Library references")]
    public GameObject anchor;

    [Header("Scene references")]
    public Transform AnchorParent;

    public Action<Transform>  OnAnchorGenerated;
    public Action<Transform, string>  OnAnchorSaved;
    private Action<OVRSpatialAnchor, bool> _onEraseCompleted;

    private OVRSpatialAnchor _spatialAnchor;
    private int _delay = 1;
    private Vector3 _newAnchorPosition = Vector3.zero;
    private Quaternion _newAnchorRotation = new Quaternion(0,0,0,0);
    
    //private void OnApplicationPause(bool pauseStatus)
    //{
    //    if (pauseStatus) return;

    //    RemoveAllListeners();
    //    GenerateAnchor();
    //}

    protected override void OnDestroy()
    {
        base.OnDestroy();
        RemoveAllListeners();
    }

    public void SetPositionOfNewAnchor(Vector3 position)//todo why doesn't this also have rotation?
    {
        _newAnchorPosition = position;
    }

    public void SetPositionOfNewAnchor(Vector3 position, Quaternion rotation)
    {
        _newAnchorPosition = position;
        _newAnchorRotation = rotation;
    }

    private void RemoveAllListeners()
    {
        OVRManager.SpaceSaveComplete -= OnSpaceSaveComplete;
        _onEraseCompleted -= OnEraseAnchorCompleted;
        //ObjectTranslationByControllers.Instance.onTranslationFinished -= InstantiateAnchor;
    }

    public void GenerateAnchor()
    {
        OVRManager.SpaceSaveComplete += OnSpaceSaveComplete;
        _onEraseCompleted += OnEraseAnchorCompleted;

        InstantiateAnchor();
        Debug.Log("From SpatialAnchorHandler, GenerateAnchor _newAnchorPosition = " + _newAnchorPosition);
        Debug.Log("From SpatialAnchorHandler, GenerateAnchor _newAnchorRotation = " + _newAnchorRotation);

        //Invoke(nameof(InstantiateAnchor),5);
        //todo remove delays and implement code with listeners
    }

    private void OnSpaceSaveComplete(ulong requestId, OVRSpace space, bool result, Guid uuid)
    {
        if (!result)
        {
            Debug.Log("Anchor failed to be saved: " + requestId);
            return;
        }

        if (!PlayerPrefs.HasKey(SpatialAnchorUtils.COUNTER_UUIDS_PLAYERPREFS_TAG))
        {
            PlayerPrefs.SetInt(SpatialAnchorUtils.COUNTER_UUIDS_PLAYERPREFS_TAG, 0);
        }

        if (_spatialAnchor == null)
        {
            var OVRSpatialAnchor = GameObject.FindGameObjectWithTag("OVRSpatialAnchor");
            if (OVRSpatialAnchor == null)
            {
                Debug.Log("Anchor in scene == null");
                return;
            }
            _spatialAnchor = OVRSpatialAnchor.GetComponent<OVRSpatialAnchor>();
        }

        int playerNumUuids = PlayerPrefs.GetInt(SpatialAnchorUtils.COUNTER_UUIDS_PLAYERPREFS_TAG);
        PlayerPrefs.SetString("uuid" + playerNumUuids, uuid.ToString());
        PlayerPrefs.SetInt(SpatialAnchorUtils.COUNTER_UUIDS_PLAYERPREFS_TAG, ++playerNumUuids);

        OnAnchorSaved?.Invoke(_spatialAnchor.transform, uuid.ToString());
    }

    private void Update()
    {
        bool PrimaryHandTriggerPressed = OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger);
        bool SecondaryHandTriggerPressed = OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger);

        if (_spatialAnchor != null && (PrimaryHandTriggerPressed || SecondaryHandTriggerPressed))
            _spatialAnchor.Erase(_onEraseCompleted);
    }

    private void OnEraseAnchorCompleted(OVRSpatialAnchor anchor, bool success)
    {//todo copied in AnchoringWithDataJsonSaver
        //if (!success) return;

        //SpatialAnchorAudioHandler.Instance.PlayDeleteSound();

        //Destroy(anchor.gameObject);

        //var id = anchor.Uuid.ToString();
        //var playerUuidCount = PlayerPrefs.GetInt("numUuids");
        
        //for (int i = 0; i < playerUuidCount; ++i)
        //{
        //    var uuidKey = "uuid" + i;
        //    var currentUuid = PlayerPrefs.GetString(uuidKey);
        //    if(currentUuid == id)
        //    {
        //        PlayerPrefs.DeleteKey(uuidKey);
        //    }
        //}

        ////PlayerPrefs.DeleteKey("uuid0");

        //ObjectTranslationByControllers.Instance.SetObjectToTranslate(AnchorParent);
        //ObjectTranslationByControllers.Instance.onTranslationFinished += () => InstantiateAnchor();
    }

    private void InstantiateAnchor()
    {
        var newAnchor = Instantiate(anchor, _newAnchorPosition, _newAnchorRotation);
        _spatialAnchor = newAnchor.GetComponent<OVRSpatialAnchor>();

        SpatialAnchorAudioHandler.Instance.PlayPlacementSound();

        Invoke(nameof(SaveAnchor), _delay);

        OnAnchorGenerated?.Invoke(_spatialAnchor.transform);
    }

    private void SaveAnchor()
    {
        if (!_spatialAnchor)
        {
            Debug.Log("_spatialAnchor is null");
            return;
        }

        _spatialAnchor.Save();
    }
}
