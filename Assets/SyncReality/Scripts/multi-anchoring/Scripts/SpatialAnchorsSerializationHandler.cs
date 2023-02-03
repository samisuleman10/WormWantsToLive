using Syncreality;
using UnityEngine;

public class SpatialAnchorsSerializationHandler : SerializationHandler
{
    public const string ANCHORS_FILE_NAME_IN_PLAYERPREFS = "SerializedAnchorsPath";
    public const string ANCHORS_PATH = "/AnchoringSerialization";
    public const string ANCHORS_FILE_NAME_IN_STORAGE = "SpatialAnchorData";
    public const string ANCHORS_EXTENTION = ".json";

    public string GetAnchorPathFromPlayerPrefs => PlayerPrefs.GetString(ANCHORS_FILE_NAME_IN_PLAYERPREFS);

    public SpatialAnchorsSerializationHandler(IFileIOWrapper FileIOWrapper) : base(FileIOWrapper)//SerializationHandler(FileIOWrapper)
    {

    }

    public override string Serialize(string path, string fileName, string data, string extension/*"/AnchoringSerialization, ".json""*/)
    {
        var result = base.Serialize(path, fileName, data, extension);
        PlayerPrefs.SetString(ANCHORS_FILE_NAME_IN_PLAYERPREFS, result);
        return result;
    }

    //public SerializedAnchorData GetDeserializedAnchor(string path)//PlayerPrefs.GetString(ANCHORS_FILE_NAME);
    //{
    //    Debug.Log("GetDeserializedAnchor");
    //    var result = base.Deserialize(path);
    //    Debug.Log("From GetDeserializedAnchor, result = " + result);
    //    var SerializedAnchorData = JsonUtility.FromJson<SerializedAnchorData>(result);
    //    Debug.Log("From GetDeserializedAnchor, SerializedAnchorData = " + SerializedAnchorData);

    //    var Uuid = SerializedAnchorData.spaceUuid;
    //    Debug.Log("From GetDeserializedAnchor, Uuid = " + Uuid);

    //    var meshifiedGuardians = SerializedAnchorData.guardians;
    //    Debug.Log("From GetDeserializedAnchor, meshifiedGuardians = " + meshifiedGuardians);


    //    foreach (var guardian in meshifiedGuardians.scannedObjects)
    //    {
    //        Debug.Log("From GetDeserializedAnchor, guardian.uuid = " + guardian.uuid);

    //        guardian.DeserializeMesh();
    //    }
    //    var xrRigTransform = SerializedAnchorData.xrRigTransform;
    //    Debug.Log("From GetDeserializedAnchor, xrRigTransform.uuid = " + xrRigTransform.uuid);
    //    // var localPositions = SerializedAnchorData.localPositionsByAnchor;

    //    var deserializedAnchor = new SerializedAnchorData(Uuid, meshifiedGuardians, xrRigTransform);//, localPositions);
    //    return deserializedAnchor;
    //}

    public void DeleteAnchors()
    {
        if (FileIOWrapper.FileExists(GetAnchorPathFromPlayerPrefs))
            FileIOWrapper.Delete(GetAnchorPathFromPlayerPrefs);

        PlayerPrefs.DeleteKey(ANCHORS_FILE_NAME_IN_PLAYERPREFS);
    }
}


//dataPath = /storage/emulated/0/Android/data/com.Syncreality.AWEDemo_02/files
//dataPath = /storage/emulated/0/Android/data/com.Syncreality.AWEDemo_02/files

//folderPath = /storage/emulated/0/Android/data/com.Syncreality.AWEDemo_02/files/AnchoringSerialization
//folderPath = /storage/emulated/0/Android/data/com.Syncreality.AWEDemo_02/files/AnchoringSerialization

//storage/emulated/0/Android/data/com.Syncreality.AWEDemo_02/files/AnchoringSerialization/29-06-2022_11-05-53.json



/*
 * Saving to: /storage/emulated/0/Android/data/com.Syncreality.AWEDemo_02/files/AnchoringSerialization/29-06-2022_10-45-57.json
 *Serialize file to: /storage/emulated/0/Android/data/com.Syncreality.AWEDemo_02/files/AnchoringSerialization/SpatialAnchorData.json
 *
/*#
  private void SerializeAnchorData(SerializedAnchorData anchorData)
        {
            var dataPath = Application.isEditor ? Application.dataPath : Application.persistentDataPath;

            string folderPath = dataPath + "/AnchoringSerialization";

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            var path = Path.Combine(folderPath + "/", DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss") + ".json");

            Debug.Log("anchorData: " + JsonUtility.ToJson(anchorData));
            Debug.Log("Saving to: " + path);
            meshifiedSourcePath = path;

            PlayerPrefs.SetString("globalPath", meshifiedSourcePath);
            File.WriteAllText(path, JsonUtility.ToJson(anchorData));//is this working with stringified object?

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

public SerializedAnchorData Deserialize()
        {
            meshifiedSourcePath = PlayerPrefs.GetString("globalPath");
            currentFillers.Clear();
            currentGuardians.Clear();
            currentMocks.Clear();

            string json = File.ReadAllText(meshifiedSourcePath);
            var SerializedAnchorData = JsonUtility.FromJson<SerializedAnchorData>(json);
            var Uuid = SerializedAnchorData.spaceUuid;
            var meshifiedGuardians = SerializedAnchorData.guardians;

            foreach (var guardian in meshifiedGuardians.scannedObjects)
                guardian.DeserializeMesh();
            var xrRigTransform = SerializedAnchorData.xrRigTransform;

            SerializedAnchorData deserializedAnchor = new SerializedAnchorData(Uuid, meshifiedGuardians, xrRigTransform);
            return deserializedAnchor;
        }
 
 
 */
