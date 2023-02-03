using System;
using System.Collections.Generic;
using System.Linq;
using Syncreality;
using UnityEngine;

/// <summary>
/// Component instantiates all rooms from the device storage.
/// </summary>

public class RoomListLoader : BasicEventManager<RoomListLoader>
{
    [Header("Library References")]
    [SerializeField]
    private GameObject RoomListItem;

    [Header("In Scene References")]
    [SerializeField]
    private Transform RoomListContainer;
    [SerializeField]
    private SpatialAnchorLoader SpatialAnchorLoader;

    private ProfileData _profileData = new ProfileData();
    private Action<OVRSpatialAnchor,bool> OnErase;

    private void OnEnable()
    {
        LoadAllRoomsFromPublicFolder();
    }

    private void LoadAllRoomsFromPublicFolder()
    {
        var FileIOWrapper = new SerializationUtils();

        var serializationHandler = new SerializationHandler(FileIOWrapper);

        var androidGlobalPath = SpatialAnchorUtils.ANDROID_ROOMPLAN_PUBLIC_FOLDER_LOCATION;
        var fileName = SpatialAnchorUtils.PROFILES_FILE_NAME;
        var extension = SpatialAnchorUtils.JSON_FILE_EXTENTION;

        var androidGlobalPAth = androidGlobalPath + fileName + extension;
        var result = serializationHandler.Deserialize(androidGlobalPAth);

        _profileData = JsonUtility.FromJson<ProfileData>(result);
        if (_profileData == null) return;

        StartProcess(_profileData.roomDatas);
    }

    public void StartProcess(List<RoomData> rooms)
    {
        ClearContainerList();
        foreach (var room in rooms)
        {
            var roomObject = Instantiate(RoomListItem, RoomListContainer);
            var roomListItem = roomObject.GetComponent<RoomListItem>();
            roomListItem.Set(room.RoomName, room.id,IsRoomAnchoredToCurrentApp(room));
            roomListItem.onDeleteButtonClicked += DeleteRoom; 
        }
    }

    private void ClearContainerList()
    {
        foreach(Transform child in RoomListContainer.GetComponentInChildren<Transform>())
        {
            if (child == RoomListContainer) continue;
            Destroy(child.gameObject);
        }
    }

    private void DeleteRoom(int roomId)
    {
        var roomToDelete = _profileData.roomDatas.Find(x => x.id == roomId);
        var anchorToDelte = roomToDelete.anchors.Find(x => x.AppIdentifier == Application.identifier);
        SpatialAnchorLoader.LoadAnchorsByUuid(/*anchorToDelte.AnchorId*/);
        Invoke(nameof(EraseAnchor), 1);

        _profileData.roomDatas.Remove(roomToDelete);

        var dataToJson = JsonUtility.ToJson(_profileData);
        var FileIOWrapper = new SerializationUtils();
        var serializationHandler = new SerializationHandler(FileIOWrapper);

        var androidGlobalPath = SpatialAnchorUtils.ANDROID_ROOMPLAN_PUBLIC_FOLDER_LOCATION;
        var fileName = SpatialAnchorUtils.PROFILES_FILE_NAME;
        var extension = SpatialAnchorUtils.JSON_FILE_EXTENTION;

        var resultPath = serializationHandler.SerializeWithoutRootPath(androidGlobalPath, fileName, dataToJson, extension);

        StartProcess(_profileData.roomDatas);
    }

    private void EraseAnchor()
    {
        var OVRSpatialAnchor = GameObject.FindGameObjectWithTag("OVRSpatialAnchor");
        if (OVRSpatialAnchor == null)
        {
            Debug.Log("Anchor in scene == null");
            return;
        }
        OVRSpatialAnchor.GetComponent<OVRSpatialAnchor>().Erase((anchor, succeded) => RemoveAnchorGameobject(anchor, succeded));
    }

    private void RemoveAnchorGameobject(OVRSpatialAnchor anchor , bool succeded)
    {
        if(succeded)
            Destroy(anchor.gameObject);
    }

    private bool IsRoomAnchoredToCurrentApp(RoomData room)
    {
        return room.anchors.Any(x => x.AppIdentifier == Application.identifier);
    }
}
