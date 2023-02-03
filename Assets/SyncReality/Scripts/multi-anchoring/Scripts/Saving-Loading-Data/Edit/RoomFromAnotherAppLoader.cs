using System.Collections.Generic;
using Syncreality;
using UnityEngine;

/// <summary>
/// Component manages the scenario when the user wants to access a room not anchored yet
/// in current app.
/// </summary>

public class RoomFromAnotherAppLoader : RoomProcessesHandler
{
    [Header("Library references")]
    public GameObject RoomBoudaryLibraryGO;
    public GameObject RoomObjectLibraryGO;
    [Header("Scene references")]
    public Transform ParentForCubes;

    private RoomData _roomData = new RoomData();
    private ProfileData _profileData = new ProfileData();
    private List<Transform> _roomObjects = new List<Transform>();
    private List<Transform> _roomBoundaries = new List<Transform>();

    public void StartTheProcess(int roomId)
    {
        ReadRoom(roomId);
        SpatialAnchorHandler.Instance.OnAnchorSaved += OnAnchorGenerated;
    }

    private void OnDestroy()
    {
        SpatialAnchorHandler.Instance.OnAnchorSaved -= OnAnchorGenerated;
    }

    private void OnAnchorGenerated(Transform anchor, string anchorUuid)
    {
        foreach (var roomObject in _roomObjects)
            roomObject.SetParent(anchor);
        foreach (var roomBoundary in _roomBoundaries)
            roomBoundary.SetParent(anchor);

        UpdateRoomWithNewAnchor(anchorUuid);

        onProcessEnded?.Invoke(anchor == null);
    }

    private void UpdateRoomWithNewAnchor(string anchorUuid)
    {
        _roomData.anchors.Add(new AnchorIdentifier(Application.identifier, anchorUuid));

        var dataToJson = JsonUtility.ToJson(_profileData);
        var FileIOWrapper = new SerializationUtils();
        var serializationHandler = new SerializationHandler(FileIOWrapper);

        var androidGlobalPath = SpatialAnchorUtils.ANDROID_ROOMPLAN_PUBLIC_FOLDER_LOCATION;
        var fileName = SpatialAnchorUtils.PROFILES_FILE_NAME;
        var extension = SpatialAnchorUtils.JSON_FILE_EXTENTION;
        var resultPath = serializationHandler.SerializeWithoutRootPath(androidGlobalPath, fileName, dataToJson, extension);
    }

    private void ReadRoom(int roomId)
    {
        _roomData = GetRoomDataFromPublicFolder(roomId);

        foreach (var roomBoundary in _roomData.RoomBoundariesPositions)
        {
            var boundary = Instantiate(RoomBoudaryLibraryGO, ParentForCubes);
            boundary.transform.localPosition = roomBoundary;
            _roomBoundaries.Add(boundary.transform);
        }

        foreach (var roomObject in _roomData.RoomObjectsPositions)
        {
            var rObject = Instantiate(RoomObjectLibraryGO, ParentForCubes);
            rObject.transform.localPosition = roomObject;
            _roomObjects.Add(rObject.transform);
        }

        ObjectTranslationByControllers.Instance.SetObjectToTranslate(ParentForCubes);
        ObjectTranslationByControllers.Instance.onTranslationFinished += SetCentroid;
    }

    private void SetCentroid()
    {
        var centroidPositionByRoomPoints = ArithmeticCalculationUtils.GetCentroidPointBetweenTransformsGlobaly(_roomBoundaries);
        SetCentroidPositionByRoomBoundaries(centroidPositionByRoomPoints);
    }

    private RoomData GetRoomDataFromPublicFolder(int roomId)
    {
        var FileIOWrapper = new SerializationUtils();
        var serializationHandler = new SerializationHandler(FileIOWrapper);

        var androidGlobalPath = SpatialAnchorUtils.ANDROID_ROOMPLAN_PUBLIC_FOLDER_LOCATION;
        var fileName = SpatialAnchorUtils.PROFILES_FILE_NAME;
        var extension = SpatialAnchorUtils.JSON_FILE_EXTENTION;

        var androidGlobalPAth = androidGlobalPath + fileName + extension;
        var result = serializationHandler.Deserialize(androidGlobalPAth);

        _profileData = JsonUtility.FromJson<ProfileData>(result);
        return _profileData.roomDatas.Find(x => x.id == roomId);
    }
}
