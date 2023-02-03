using System.Collections.Generic;
using Syncreality;
using UnityEngine;

/// <summary>
/// Component generates a new room when the StartProcess is called out of roomObjects
/// and roomBoundaries.
/// </summary>

public class NewRoomCreator : RoomProcessesHandler
{
    public List<Transform> roomObjects;
    public List<Transform> roomBoundaries;

    public override void StartProcess()
    {
        var centroidPositionByRoomPoints = ArithmeticCalculationUtils.GetCentroidPointBetweenTransformsGlobaly(roomBoundaries);
        SetCentroidPositionByRoomBoundaries(centroidPositionByRoomPoints);
        SpatialAnchorHandler.Instance.OnAnchorSaved += OnAnchorSaved;
    }

    private void OnDestroy()
    {
        SpatialAnchorHandler.Instance.OnAnchorSaved -= OnAnchorSaved;
    }

    private void OnAnchorSaved(Transform anchor, string anchorUuid)
    {
        foreach (var roomObject in roomObjects)
            roomObject.SetParent(anchor);
        foreach (var roomBoundary in roomBoundaries)
            roomBoundary.SetParent(anchor);

        WriteData(anchorUuid);
    }

    private void WriteData(string anchorUuid)
    {
        var profileData = new ProfileData();
        var FileIOWrapper = new SerializationUtils();
        var serializationHandler = new SerializationHandler(FileIOWrapper);

        var androidGlobalPath = SpatialAnchorUtils.ANDROID_ROOMPLAN_PUBLIC_FOLDER_LOCATION;
        var fileName = SpatialAnchorUtils.PROFILES_FILE_NAME;
        var extension = SpatialAnchorUtils.JSON_FILE_EXTENTION;
        var existingProfilePath = androidGlobalPath + fileName + extension;

        var result = serializationHandler.Deserialize(existingProfilePath);
        if (result != null)
            profileData = JsonUtility.FromJson<ProfileData>(result);

        var roomData = new RoomData();
        roomData.RoomName = "Kitchen";
        var newId = GenerateIdByExistingProfile(profileData.LastIDCount);
        roomData.id = newId;
        profileData.LastIDCount = newId + 1;
        roomData.anchors.Add(new AnchorIdentifier(Application.identifier, anchorUuid));
        roomData.RoomObjectsPositions = ArithmeticCalculationUtils.GetLocalPostitionsListFromTransformList(roomObjects);
        roomData.RoomBoundariesPositions = ArithmeticCalculationUtils.GetLocalPostitionsListFromTransformList(roomBoundaries);

        List<RoomData> roomDatas = new List<RoomData>();
        if (profileData.roomDatas != null || profileData.roomDatas.Count != 0)
            roomDatas.AddRange(profileData.roomDatas);

        roomDatas.Add(roomData);
        profileData.roomDatas = roomDatas;

        var dataToJson = JsonUtility.ToJson(profileData);
        var resultPath = serializationHandler.SerializeWithoutRootPath(androidGlobalPath, fileName, dataToJson, extension);
        onProcessEnded?.Invoke(resultPath != null);
    }

    private int GenerateIdByExistingProfile(int lastId)
    {
        return lastId + 1;
    }
}
