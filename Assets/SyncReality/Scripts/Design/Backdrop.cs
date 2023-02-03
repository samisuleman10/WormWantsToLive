using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Backdrop : MonoBehaviour
{
    /*public bool anchored
    {
        get {return m_anchored;}
        set {
            if (m_anchored == value) return;
            m_anchored = value;
            Debug.Log("setting to " + value);
        }
    }*/
    public bool anchored;
    public string assetReferencePath;
    public float backdropDistance;
    public float backdropRotation;
    public float backdropHorizontalOffset;
    public float backdropVerticalOffset;
    public GameObject backdropAsset;
    [HideInInspector] public GameObject backdropSpawnedAsset;
    [HideInInspector] public WallFace mappedWallFace;

     

#if UNITY_EDITOR
    public BackdropData GetBackdropData(Backdropper backdropper)
    {
        return new BackdropData(this, mappedWallFace);
    }
    #endif
}
