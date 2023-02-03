using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
 
#if UNITY_EDITOR

//than evaluate if making this class MonoBehaviour or not
//this is currently loaded from ControlContent 
//but should be initialized throug some kind of other manager with DI parameters

[InitializeOnLoad, ExecuteInEditMode]
public class SREditorCameraTool: MonoBehaviour
{
    public enum CameraPresetNames
    {
        Design,
        Layout
    }
    [System.Serializable]
    public class Preset
    {
        public string name;
        public Vector3 pos;
        public Vector3 rot;
        public Vector3 pivot;
        public float size;
        public bool twoD;
        public bool ortho;
        public Preset Save(SceneView aView)
        {
            pos = aView.camera.transform.position;
            pivot = aView.pivot;
            rot = aView.rotation.eulerAngles;
            size = aView.size;
            twoD = aView.in2DMode;
            ortho = aView.orthographic;
            return this;
        }
        
        public void Restore(SceneView aView)
        {
            aView.in2DMode = twoD;
            aView.LookAt(pivot, Quaternion.Euler(rot), size, ortho);
        }
    }

    private const string playerPrefsStringBase = "SR.SceneViewPresets";
    
    // ------ Temporary Test Bool ------
    [SerializeField]
    private bool m_designTestBoolSave = false;
    [SerializeField]
    private bool m_designTestBoolRestore = false;
    [SerializeField]
    private bool m_layoutTestBoolSave = false;
    [SerializeField]
    private bool m_layoutTestBoolRestore = false;
    

    private Preset m_presetDesign = new Preset();
    private Preset m_presetLayout = new Preset();
 
    [SerializeField]
    private GameObject m_designAreaObject;
    [SerializeField]
    private GameObject m_layoutAreaObject;

    public GameObject DesignAreaProp
    {
        get
        {
            if (m_designAreaObject == null)
                m_designAreaObject = GameObject.Find("DesignArea");
            return m_designAreaObject;
        }
    }
    public GameObject LayoutAreaProp
    {
        get
        {
            if (m_layoutAreaObject == null)
                m_layoutAreaObject = GameObject.Find("LayoutArea");
            return m_layoutAreaObject;
        }
    }
 
    void Start()
    {
        Initialize();
    }
 
    /// <summary>
    /// Get a default camera preset if there are none
    /// </summary>
    /// <param name="focusGameObject"></param>
    /// <returns></returns>
    private Preset GetDefaultPreset (GameObject focusGameObject)
    {
        var cameraAltitude = 30f; 
        var tempCamPositions = new Vector3(focusGameObject.transform.position.x, focusGameObject.transform.position.y + cameraAltitude, focusGameObject.transform.position.z);

        Preset preset = new Preset();
        preset.name = focusGameObject.name.ToLower().Contains(CameraPresetNames.Design.ToString().ToLower()) ? CameraPresetNames.Design.ToString() : CameraPresetNames.Layout.ToString();
        preset.pos = tempCamPositions;
        preset.rot = (DesignAreaProp.transform.position - tempCamPositions).normalized;
        preset.pivot = tempCamPositions;
        preset.size = 50;
        preset.twoD = false;
        preset.ortho = false;

        return preset;
    }

    /// <summary>
    /// Get a preset with relative position to the focusGameObject
    /// </summary>
    /// <param name="focusGameObject"></param>
    /// <returns></returns>
    private Preset GeneratePresetRelativeToGameobject (GameObject focusGameObject)
    {
        var aView = SceneView.lastActiveSceneView;

        Preset preset = new Preset();
        preset.name = focusGameObject.name.ToLower().Contains(CameraPresetNames.Design.ToString().ToLower()) ? CameraPresetNames.Design.ToString() : CameraPresetNames.Layout.ToString();
        // get the posistion relative to the gameobject
        preset.pos =  focusGameObject.transform.InverseTransformPoint (aView.camera.transform.position);
        preset.rot = aView.camera.transform.rotation.eulerAngles;
        preset.pivot = preset.pos;
        preset.size = aView.size;
        preset.twoD = aView.in2DMode;
        preset.ortho = aView.orthographic;

        return preset;
    }

    /// <summary>
    /// Get a preset in world position calculated from the loaded relative preset
    /// </summary>
    /// <param name="relativePreset"></param>
    /// <param name="focusGameObject"></param>
    /// <returns></returns>
    private Preset GetWorldPresetFromRelativePreset (Preset relativePreset, GameObject focusGameObject)
    {
        Preset preset = new Preset();
        preset.name = relativePreset.name;
        // get world position from the relative preset
        preset.pos =  focusGameObject.transform.TransformPoint (relativePreset.pos);
        preset.rot = relativePreset.rot;
        preset.pivot = preset.pos;
        preset.size = 0f;
        preset.twoD = relativePreset.twoD;
        preset.ortho = relativePreset.ortho;

        return preset;
    }

    // FD:: Temporary tests
    // To test this functionality, this component should be added to the ToolWindowSettings gameObject in main scene
    // and must add references to DesignArea and LayoutArea gameObjects
    // TODO:: Remove this after UI implementation
    // This is strictly for testing the functionality before attaching the Move and SAve functions to UI buttons
    private void OnValidate ()
    {
        if (m_designTestBoolRestore == true)
        {
            m_designTestBoolRestore = false;
            MoveCameraToPreset (CameraPresetNames.Design);
        }
        if (m_designTestBoolSave == true)
        {
            m_designTestBoolSave = false;
            SaveCameraPreset (CameraPresetNames.Design);
        }

        if (m_layoutTestBoolRestore == true)
        {
            m_layoutTestBoolRestore = false;
            MoveCameraToPreset (CameraPresetNames.Layout);
        }
        if (m_layoutTestBoolSave == true)
        {
            m_layoutTestBoolSave = false;
            SaveCameraPreset (CameraPresetNames.Layout);
        }
    }
    
    private void Initialize()
    {
        string tempJsonDesign = EditorPrefs.GetString(playerPrefsStringBase + CameraPresetNames.Design, "");
        string tempJsonLayout = EditorPrefs.GetString(playerPrefsStringBase + CameraPresetNames.Layout, "");

        if (tempJsonDesign != "")
        {
            m_presetDesign = JsonUtility.FromJson<Preset>(tempJsonDesign);
        }
        else
        {
            m_presetDesign = GetDefaultPreset(DesignAreaProp);
        }
        if (tempJsonLayout != "")
        {
            m_presetLayout = JsonUtility.FromJson<Preset>(tempJsonLayout);
        }
        else
        {
            m_presetLayout = GetDefaultPreset(LayoutAreaProp);
        }
    }

    /// <summary>
    /// Move the camera to the specified preset position
    /// </summary>
    /// <param name="presetName"></param>
    public void MoveCameraToPreset (CameraPresetNames presetName)
    {
//        Debug.Log ("FD:: movecamera to preset " + presetName);
        var focusGameObject = presetName == CameraPresetNames.Design ? DesignAreaProp : LayoutAreaProp;
        var selectedPreset = presetName == CameraPresetNames.Design ? m_presetDesign : m_presetLayout;

        GetWorldPresetFromRelativePreset (selectedPreset, focusGameObject).Restore(SceneView.lastActiveSceneView);
    }

    public void SaveCameraPreset (CameraPresetNames presetName)
    {
        Debug.Log ("FD:: SaveCameraPreset to preset " + presetName);
        if (presetName == CameraPresetNames.Design)
        {
            m_presetDesign = GeneratePresetRelativeToGameobject (DesignAreaProp);
            UpdateCameraPresetPosition (CameraPresetNames.Design);
        }
        if (presetName == CameraPresetNames.Layout)
        {
            m_presetLayout = GeneratePresetRelativeToGameobject (LayoutAreaProp);
            UpdateCameraPresetPosition (CameraPresetNames.Layout);
        }
            
    }
    
    /// <summary>
    /// Overwrite a new position preset for the camera on the specified name
    /// in the playerPrefs saved parameters in json format.
    /// </summary>
    private void UpdateCameraPresetPosition (CameraPresetNames presetName, bool resetToDefaults = false)
    {
        string finalString = "";
        var focusGameObject = presetName == CameraPresetNames.Design ? DesignAreaProp : LayoutAreaProp;

        if (resetToDefaults)
        {
            finalString = JsonUtility.ToJson(GetDefaultPreset (focusGameObject));
        }
        else
        {
            // var tempPresets = new Preset();
            // tempPresets.Save(SceneView.lastActiveSceneView);
            // tempPresets.name = presetName.ToString();
            // finalString = JsonUtility.ToJson(tempPresets);

            finalString = JsonUtility.ToJson(GeneratePresetRelativeToGameobject (focusGameObject));
        }
        EditorPrefs.SetString(playerPrefsStringBase + presetName.ToString(), finalString);
    }

    /// <summary>
    /// Overwrite a new position preset for the camera on Design area
    /// </summary>
    public void SaveDesignCameraPreset (bool resetToDefaults = false)
    {
        UpdateCameraPresetPosition (CameraPresetNames.Design, resetToDefaults);
    }
    /// <summary>
    /// Overwrite a new position preset for the camera on Layout area
    /// </summary>
    public void SaveLayoutCameraPreset (bool resetToDefaults = false)
    {
        UpdateCameraPresetPosition (CameraPresetNames.Layout, resetToDefaults);
    }
    
}

#endif