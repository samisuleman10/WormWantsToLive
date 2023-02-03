using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnchoringWithDataJsonSaver : MonoBehaviour
{
    [SerializeField] private DataJsonSaver dataJsonSaver;
    [SerializeField] private ModuleManager moduleManager;
    [SerializeField] private SyncSpatialAnchorLoader spatialAnchorLoader;

    private List<GameObject> _currentSyncs = new List<GameObject>();
    private OVRSpatialAnchor _spatialAnchor;
    private bool _isCurrentRoomLoaded;

    private void Start()
    {
        if (FindObjectOfType<StoryTeller>().executeOnStart == true)
              return;
        spatialAnchorLoader.LoadAnchorsByUuid();
        dataJsonSaver.OnSendSavedScan += OnSendSavedScan;
        dataJsonSaver.OnRoomLoaded += OnRoomLoaded;
        if (SpatialAnchorHandler.Instance != null)
            SpatialAnchorHandler.Instance.OnAnchorGenerated += OnAnchorGenerated;
        moduleManager.onPipelineExecuted.AddListener(OnPipelineExecuted);
        spatialAnchorLoader.onAnchorInstantiated += onAnchorInstantiated;
    }

    private void OnDestroy()
    {
        dataJsonSaver.OnSendSavedScan -= OnSendSavedScan;
        dataJsonSaver.OnRoomLoaded -= OnRoomLoaded;
        if(SpatialAnchorHandler.Instance != null)
            SpatialAnchorHandler.Instance.OnAnchorGenerated -= OnAnchorGenerated;
        moduleManager.onPipelineExecuted.RemoveListener(OnPipelineExecuted);
        spatialAnchorLoader.onAnchorInstantiated -= onAnchorInstantiated;
    }

    private void onAnchorInstantiated(Transform anchor)
    {
        _spatialAnchor = anchor.GetComponent<OVRSpatialAnchor>();
    }

    private void OnPipelineExecuted(List<GameObject> arg0)
    {
        _currentSyncs.AddRange(arg0);
        MoveSyncsToAnchor();
    }

    private void MoveSyncsToAnchor()
    {
        if (_spatialAnchor == null)
        {
            Debug.LogError("Could not anchor Syncs because spatial anchor is null.");
        }
        else
        {
            foreach (var item in _currentSyncs)
            {
                if (item == null || item.transform == null)
                    continue;

                item.transform.SetParent(_spatialAnchor.transform);
            }
        }
    }

    private void OnSendSavedScan(Vector3 positionOfNewAnchor,Quaternion rotationOfNewAnchor)
    {
        var isNewRoom = _isCurrentRoomLoaded == false;
        if (isNewRoom)
        {
            if (_spatialAnchor != null)
                _spatialAnchor.Erase(OnEraseAnchorCompleted);

            SpatialAnchorHandler.Instance.SetPositionOfNewAnchor(positionOfNewAnchor, rotationOfNewAnchor);
            SpatialAnchorHandler.Instance.GenerateAnchor();
        }
    }

    private void OnEraseAnchorCompleted(OVRSpatialAnchor anchor, bool success)
    {
        if (!success)
        {
            Debug.Log("OnEraseAnchor is unsuccessful.");
            return;
        }

        SpatialAnchorAudioHandler.Instance.PlayDeleteSound();

        var id = anchor.Uuid.ToString();

        var playerUuidCount = PlayerPrefs.GetInt(SpatialAnchorUtils.COUNTER_UUIDS_PLAYERPREFS_TAG);

        for (int i = 0; i < playerUuidCount; ++i)
        {
            var uuidKey = "uuid" + i;
            var currentUuid = PlayerPrefs.GetString(uuidKey);
            if (currentUuid == id)
            {
                Debug.Log("Deleting from PlayerPrefs, anchor with id = " + currentUuid);
                PlayerPrefs.DeleteKey(uuidKey);
                continue;
            }
        }
        playerUuidCount -= 1;
        PlayerPrefs.SetInt(SpatialAnchorUtils.COUNTER_UUIDS_PLAYERPREFS_TAG, playerUuidCount);
        Destroy(anchor.gameObject);
    }

    private void OnAnchorGenerated(Transform anchor)
    {
         _spatialAnchor = anchor.GetComponent<OVRSpatialAnchor>();
    }

    private void OnRoomLoaded(Transform room)
    {
        _isCurrentRoomLoaded = true;

        if (_spatialAnchor == null)
            Debug.Log("There is no anchor in the scene so the room can't be attached to it.");
        else
        {
            var referencePoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
            referencePoint.transform.SetParent(room.transform,true);
            var originalPosition = referencePoint.transform.position;

            room.transform.position = new Vector3(_spatialAnchor.transform.position.x,
                _spatialAnchor.transform.position.y,
                _spatialAnchor.transform.position.z);

            room.transform.rotation = new Quaternion(_spatialAnchor.transform.rotation.x,
                _spatialAnchor.transform.rotation.y,
                _spatialAnchor.transform.rotation.z,
                _spatialAnchor.transform.rotation.w);

            
            // This offset will lose precision as it moves away from origin, for now it seems ok. Including magnitude might improve it
            var anchorOffset = referencePoint.transform.position - originalPosition;
            ApplyAnchorToWallpoints(room.gameObject, anchorOffset);
            
            GameObject.Destroy(referencePoint);
        }
    }

    private void ApplyAnchorToWallpoints(GameObject room, Vector3 offset)
    {
        var _scannedGameObjs = room.GetComponentsInChildren<ScannedTypeGameObject>().ToList();

        foreach (var scannedData in _scannedGameObjs)
        {
            if (scannedData.ScannedPoints == null || scannedData.ScannedPoints.Count == 0)
                continue;
            var scannedPointsList = scannedData.ScannedPoints.ToList();

            for (int i = 0; i < scannedPointsList.Count; i++)
                scannedPointsList[i] += offset;

            if (scannedData.ScannedObjectData != null)
                scannedData.ScannedObjectData.ScannedPoints = new List<Vector3>(scannedPointsList);

            scannedPointsList.Reverse();
            scannedData.ScannedPoints = new Stack<Vector3>(scannedPointsList);
        }
    }
}
