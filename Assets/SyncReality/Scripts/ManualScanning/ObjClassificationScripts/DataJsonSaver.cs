using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.IO;
using System;
using static ModalViewManager;

//Get Centroid of room extremes
//Generate Anchor
//Save room with anchor in
//Load Room with anchor in

public class DataJsonSaver : MonoBehaviour
{

    public Material MScanVolume;
    public bool ApplyNudgeToScannedPoints = true;

    [Serializable]
    public class SerializedRoom
    {
        public RoomType RoomType;

        public List<ScannedObject> ScannedObjsClear = new List<ScannedObject>();
        public List<ScannedSimpleObject> ScannedSimpleObjs = new List<ScannedSimpleObject>();
        public List<ScannedComplexObject> ScannedComplexObjs = new List<ScannedComplexObject>();


        public SerializedRoom() { }
    }

    public GameObject BaseToSaveGameobj;
    public string SaveFilename = "Dataobjs.json";

    private JsonSerializerSettings _settings;
    private FindOuterWalls _roomDimensionsParser;

    private string _tempSaveString;
    //private bool _pressedRun = false;
    private Vector3 _thisOriginalPosition;
    private Vector3 _thisPermOriginalRotation;

    //public Vector3 anchorPosition = new Vector3(-100, -100, -100);
    //public Quaternion anchorRotation = new Quaternion(-100, -100, -100, -100);

    public Action<Transform> OnRoomLoaded;
    public Action<Vector3, Quaternion> OnSendSavedScan;

    public void Awake()
    {
        _thisOriginalPosition = transform.position;
        _thisPermOriginalRotation = transform.position;
        _settings = new JsonSerializerSettings();
        //_settings.ContractResolver = ShouldSerializeContractResolver.Instance;
        _settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        //_settings.TypeNameHandling = TypeNameHandling.Auto;
        _settings.TypeNameHandling = TypeNameHandling.Auto;
        _settings.MetadataPropertyHandling = MetadataPropertyHandling.Ignore;

        _roomDimensionsParser = this.GetComponent<FindOuterWalls>();

        if (BaseToSaveGameobj == null)
            BaseToSaveGameobj = this.gameObject;

    }

    public void SaveScannedType(ScannedTypeGameObject obj)
    {

    }


    public void SaveSelectedRoom(bool doSave = true)
    {

        #region Old Scan Anchoring code
        // Old Scan Anchoring Code
        /*var oldPosition = BaseToSaveGameobj.transform.position;

        Debug.LogError("Save Position = " + oldPosition);
        var oldRotation = BaseToSaveGameobj.transform.rotation;

        BaseToSaveGameobj.transform.position = Vector3.zero;
        BaseToSaveGameobj.transform.rotation = Quaternion.identity;

        // In the end
        BaseToSaveGameobj.transform.position = oldPosition;
        BaseToSaveGameobj.transform.rotation = oldRotation;

        */
        #endregion


        var selectedRoom = RoomSelector.GetCurrentSelectedRoom();
        if (selectedRoom == null)
        {
            Debug.LogError("No selected Room to save");
            return;
        }


        var _scannedGameObjs = selectedRoom.GetComponentsInChildren<ScannedTypeGameObject>().ToList();
        //var _scannedGameObjs = BaseToSaveGameobj.GetComponentsInChildren<ScannedTypeGameObject>().ToList();

        //Nudge adjustments
        if (ApplyNudgeToScannedPoints)
        {
            var _nudgeDiff = this.transform.position - _thisOriginalPosition;

            foreach (var scannedData in _scannedGameObjs)
            {
                if (scannedData.ScannedPoints == null || scannedData.ScannedPoints.Count == 0)
                    continue;
                var scannedPointsList = scannedData.ScannedPoints.ToList();

                for (int i = 0; i < scannedPointsList.Count; i++)
                    scannedPointsList[i] += _nudgeDiff;

                if (scannedData.ScannedObjectData != null)
                    scannedData.ScannedObjectData.ScannedPoints = new List<Vector3>(scannedPointsList);

                scannedPointsList.Reverse();
                scannedData.ScannedPoints = new Stack<Vector3>(scannedPointsList);

                _thisOriginalPosition += _nudgeDiff;
            }
        }

        var _scannedObjs = _scannedGameObjs.ConvertAll<ScannedObject>(o => o.ScannedObjectData != null && o.ScannedObjectData.ClassificationType != 0 ? o.ScannedObjectData :
                                               ScannedObjectFactory.CreateScannedObject(o)).ToList<ScannedObject>();
        //Updates if there were changes in base gameobj
        foreach (var obj in _scannedObjs)
            obj.UpdateBaseObject();

        //Conversion for type serialization without JIT. .Net 8.0 also can handle the base class serialization
        var _simpleObjs = _scannedObjs.Where(o => o is ScannedSimpleObject).ToList().ConvertAll<ScannedSimpleObject>(o => (ScannedSimpleObject)o);
        var _complexObjs = _scannedObjs.Where(o => o is ScannedComplexObject).ToList().ConvertAll<ScannedComplexObject>(o => (ScannedComplexObject)o);
        _scannedObjs = _scannedObjs.Except(_simpleObjs).Except(_complexObjs).ToList();

        var roomType = selectedRoom.GetComponent<RoomTypeGameObject>();

        SerializedRoom _serObj = new SerializedRoom() { RoomType = roomType.RoomType, ScannedObjsClear = _scannedObjs, ScannedComplexObjs = _complexObjs, ScannedSimpleObjs = _simpleObjs };

        //Removes Walls per request
        var _scannedMockdata = GlimpsyUtil.ConvertFromScannedTypeGameObjectToMockData(_scannedGameObjs.Where(data => data.ObjectClassification != ScannedObjectsClassificationType.Walls).ToList());

        //ScannedData
        string serializedObjs = JsonConvert.SerializeObject(_serObj, _settings);
        if(doSave)
            using (StreamWriter sw = File.CreateText(Application.persistentDataPath + "/" + SaveFilename))
            {
                sw.Write(serializedObjs);
            }

        //RoomDimensions
        if (_roomDimensionsParser != null)
        {
            if (!Directory.Exists(Application.persistentDataPath + "/ForTheTool/"))
                Directory.CreateDirectory(Application.persistentDataPath + "/ForTheTool/");

            var roomDimensions = _roomDimensionsParser.GetRoomDimensions(BaseToSaveGameobj);
            string serializedRoom = JsonConvert.SerializeObject(roomDimensions, _settings);


            if (doSave)
                using (StreamWriter sw = File.CreateText(Application.persistentDataPath + "/ForTheTool/" + "RoomDimensions"+SaveFilename))
                {
                    sw.Write(serializedRoom);
                }

            // Mockdata == RoomLayout
            RoomLayout newRoomlayout = new RoomLayout();
            newRoomlayout.mockDatas = _scannedMockdata;
            newRoomlayout.roomDimensions = roomDimensions;

            //Designer area added. Fixed the tripple call to JsonUtility
            string serializedRoomLayout = JsonUtility.ToJson(newRoomlayout, true);
            FindObjectOfType<StoryTeller>().ReceiveScanFromManualScanner(serializedRoomLayout);
            _tempSaveString = serializedRoomLayout;
            if (doSave)
                using (StreamWriter sw = File.CreateText(Application.persistentDataPath + "/ForTheTool/" + "RoomLayoutMockData" + SaveFilename))
                {
                    sw.Write(serializedRoomLayout);
                }
        }

    }

    public void RemoveAllScannedObjects()
    {
        foreach (Transform children in BaseToSaveGameobj.transform)
        {
            if(children.tag != "OVRSpatialAnchor")
            {
              //  Debug.Log("Destroying " + children.name);
                Destroy(children.gameObject);
            }
        }
    }
 

    public void LoadFileChose()
    {
        //Clears delegates
        if(ModalViewManager.GetFilepathLoaded != null)
            foreach (Delegate d in ModalViewManager.GetFilepathLoaded?.GetInvocationList())
            {
                ModalViewManager.GetFilepathLoaded -= (SendFilePath)d;
            }

        ModalViewManager.GetFilepathLoaded += LoadFileChosen;
        ModalViewManager.GetFileFromFolder("");
    }

    private void LoadFileChosen(string filename)
    {
        ModalViewManager.GetFilepathLoaded -= LoadFileChosen;
        LoadRoomFromFile(filename);
    }

    public void LoadRoomFromFile(string fileName, bool loadAdditive = false)
    {
        //Vector3 loadDiff = Vector3.zero;
        if (!loadAdditive)
        {
            RemoveAllScannedObjects();

            //loadDiff =  this.transform.position - _thisOriginalPosition; 

            this.transform.position = _thisPermOriginalRotation;
            _thisOriginalPosition = _thisPermOriginalRotation;

            //Unless we save every nudge, we cannot restore back to origin the saved points. 
        }

        using (StreamReader sw = File.OpenText(Application.persistentDataPath + "/" + fileName))
        {
            string output = sw.ReadToEnd();

            var serObj = JsonConvert.DeserializeObject<SerializedRoom>(output, _settings);
            var newRoom = RoomSelector.CreateNewRoom(serObj.RoomType);



            List<ScannedObject> scannedObjects = new List<ScannedObject>();
            scannedObjects.AddRange(serObj.ScannedObjsClear);
            scannedObjects.AddRange(serObj.ScannedSimpleObjs);
            scannedObjects.AddRange(serObj.ScannedComplexObjs);

            //List<ScannedObject> scannedObjects = JsonConvert.DeserializeObject<List<ScannedObject>>(output, _settings).ToList();
            List<ScannedTypeGameObject> _scannedComp = scannedObjects.ConvertAll<ScannedTypeGameObject>(o => o.BaseObjectComponent);

            foreach (var scanOjb in _scannedComp)
            {
                if (scanOjb.ScannedPoints != null)
                {
                    var pointList = scanOjb.ScannedPoints.ToList();
                    //for(int i = 0 ; i < pointList.Count; i++) 
                    //    pointList[i] += loadDiff;
                    scanOjb.ScannedPoints = new Stack<Vector3>(scanOjb.ScannedPoints.ToList());
                }
            }

            foreach (ScannedTypeGameObject obj in _scannedComp)
            {
                obj.transform.SetParent(newRoom.transform, true);

                foreach (MeshRenderer renderer in obj.GetComponentsInChildren<MeshRenderer>())
                    renderer.material = MScanVolume;
            }

            OnRoomLoaded?.Invoke(newRoom.transform);

            RunEnabler.UpdateReplacerStatus();

            //newRoom.transform.position += new Vector3(1, 0, 0);
        }
    }

    public class ShouldSerializeContractResolver : DefaultContractResolver
    {
        public static readonly ShouldSerializeContractResolver Instance = new ShouldSerializeContractResolver();

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            property.ShouldSerialize = (object o) => { return property.DeclaringType != typeof(Rigidbody); };

            return property;
        }
    }

    public void SendSavedScan()
    {
        var room = RoomSelector.GetCurrentSelectedRoom();
        if(room != null)
            OnSendSavedScan?.Invoke(room.transform.position, room.transform.rotation);
        else
        {
            Debug.LogError("The anchor is not generated because the scanned room is null.");
        }

        SaveSelectedRoom(false);
        RemoveAllScannedObjects();
        FindObjectOfType<StoryTeller>()?.DeconstructSpawnedEnvironment();
        FindObjectOfType<StoryTeller>()?.RunFromManualScanner(_tempSaveString);
        //_pressedRun = false;

    }
}