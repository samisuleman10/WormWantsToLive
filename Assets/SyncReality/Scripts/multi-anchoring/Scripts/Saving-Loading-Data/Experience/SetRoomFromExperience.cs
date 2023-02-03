using System.Collections.Generic;
using Syncreality;
using UnityEngine;

/// <summary>
/// Component waits for the anchor to be loaded by SpatialAnchorLoader and then places the room Boudary and
/// Objects as children of the anchor making sure that their local position is valid.
/// </summary>

public class SetRoomFromExperience : MonoBehaviour
{
    [Header("Library References")]
    public GameObject RoomBoudaryLibraryGO;
    public GameObject RoomObjectLibraryGO;
    public SpatialAnchorLoader spatialAnchorLoader;
    private RoomData _room = new RoomData();

    private void Start()
    {
        ReadRoom();
    }

    private void ReadRoom()
    {
        var id = PlayerPrefs.GetInt(SpatialAnchorUtils.ANCHOR_TO_LOAD_ID_PLAYERPREFS_TAG);
        _room = GetRoomDataByAnchorId(id);
        var anchor = _room.anchors.Find(x => x.AppIdentifier == Application.identifier);
        if (anchor == null)
            Debug.LogError("Doesn't exist anchor for this application.");

        spatialAnchorLoader.LoadAnchorsByUuid(/*anchor.AnchorId*/);
        Invoke(nameof(InstantateTheRoomToCurrentAnchor), 5);
    }

    private void InstantateTheRoomToCurrentAnchor()
    {
        var anchorId = PlayerPrefs.GetInt(SpatialAnchorUtils.ANCHOR_TO_LOAD_ID_PLAYERPREFS_TAG, -1);
        if (anchorId == -1)
        {
            Debug.LogError("The anchor id is not valid, room can not be loaded ");
            return;
        }

        var OVRSpatialAnchor = GameObject.FindGameObjectWithTag("OVRSpatialAnchor");
        if (OVRSpatialAnchor == null)
        {
            Debug.Log("Anchor in scene == null");
            return;
        }

        InstantiateRoomBoundaries(_room.RoomBoundariesPositions, OVRSpatialAnchor.transform);
        InstantiateRoomObjects(_room.RoomObjectsPositions, OVRSpatialAnchor.transform);
    }

    private void InstantiateRoomBoundaries(List<Vector3> positions, Transform anchor)
    {
        foreach (var roomBoundary in positions)
        {
            var boundary = Instantiate(RoomBoudaryLibraryGO, anchor);
            boundary.transform.localPosition = roomBoundary;
        }
    }

    private void InstantiateRoomObjects(List<Vector3> positions, Transform anchor)
    {
        foreach (var roomObject in positions)
        {
            var rObject = Instantiate(RoomObjectLibraryGO, anchor);
            rObject.transform.localPosition = roomObject;
        }
    }

    private RoomData GetRoomDataByAnchorId(int id)
    {
        var FileIOWrapper = new SerializationUtils();
        var serializationHandler = new SerializationHandler(FileIOWrapper);

        var androidGlobalPath = SpatialAnchorUtils.ANDROID_ROOMPLAN_PUBLIC_FOLDER_LOCATION;
        var fileName = SpatialAnchorUtils.PROFILES_FILE_NAME;
        var extension = SpatialAnchorUtils.JSON_FILE_EXTENTION;
        var existingProfilePath = androidGlobalPath + fileName + extension;

        var result = serializationHandler.Deserialize(existingProfilePath);
        var _profileData = JsonUtility.FromJson<ProfileData>(result);
        if (_profileData == null)
            Debug.Log("From SetRoomFromExperience, _profileData is null.");

        return _profileData.roomDatas.Find(x => x.id == id);
    }
}