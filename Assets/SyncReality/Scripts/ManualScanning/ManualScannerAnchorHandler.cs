using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using static OVRSpatialAnchor;

public class ManualScannerAnchorHandler : MonoBehaviour
{
#if false
    public float MaximumAnchorDistance = 4f;
    public bool ShowAnchor = true;

    private SpatialAnchorLoader _anchorLoader;
    private OVRCameraRig _cameraRig;
    private OVRSpatialAnchor _scanAnchor;
    private MeshRenderer _anchorView;
 

    private void Awake()
    {
        _cameraRig = GameObject.Find("OVRCameraRig")?.GetComponent<OVRCameraRig>();
        if(_cameraRig == null)
            Debug.LogError("CameraRig not found at ManualScannerAnchorHandler");

        _anchorLoader = GetComponent<SpatialAnchorLoader>();
        _anchorView = GetComponent<MeshRenderer>();
        _anchorView.enabled = false;


        //_scanAnchor = GetComponent<OVRSpatialAnchor>();
        Assert.IsNotNull(_anchorLoader, "SpatialAnchorLoader not found at ManualScannerAnchorHandler");

            
        var id = PlayerPrefs.GetString(SpatialAnchorUtils.MANUALSCAN_ANCHOR_PREFID);
        //No previous anchor

        //Debug.LogError("Current Anchor ID = "+ id);
        if (string.IsNullOrEmpty(id))
        {
            EraseCurrentAnchor();
            return;
        }


        var options = new OVRSpatialAnchor.LoadOptions
        {
            MaxAnchorCount = 100,
            Timeout = 0,
            StorageLocation = OVRSpace.StorageLocation.Local,
            Uuids = new Guid[1] { new Guid(id) }
        };

        OVRSpatialAnchor.LoadUnboundAnchors(options, arc => OnLoadComplete(arc));
    }

    public void LoadAnchor()
    {
        if(_scanAnchor != null)
        {
            if (!PlayerPrefs.HasKey(Anchor.NumUuidsPlayerPref))
                PlayerPrefs.SetInt(Anchor.NumUuidsPlayerPref, 1);
            PlayerPrefs.SetString("uuid0", _scanAnchor.Uuid.ToString());

            _anchorLoader.LoadAnchorsByUuid();
        }
    }

    public void SaveAnchor()
    {
        if (_scanAnchor == null)
            _scanAnchor = this.gameObject.AddComponent<OVRSpatialAnchor>();

        //OVRManager.SpaceSaveComplete += OVRManager_SpaceSaveComplete;

        Invoke(nameof(SaveAnchorDelay), 0.1f);
        
    }

    private void SaveAnchorDelay()
    {
        _scanAnchor.Save(OnSaveCompleted);
    }
    private void OVRManager_SpaceSaveComplete(ulong requestId, OVRSpace space, bool result, Guid uuid)
    {
        Debug.LogError("Save Space Completed");

        if (result)
        {
            Debug.LogError("Save Space Success");
        }
    }

    private void OnLoadComplete(UnboundAnchor[] result)
    {

        if (result != null && result.Length > 0)
        {
            result[0].Localize((arc,result) => OnLocalizeComplete(arc,result));
            return;
        }
        //Didnt Work
        EraseCurrentAnchor();
    }

    private IEnumerator LocalizeDelay(UnboundAnchor anchor)
    {
        yield return new WaitForSeconds(0.1f);
        anchor.Localize((arc, result) => OnLocalizeComplete(arc, result));
    }
    private void OnLocalizeComplete(UnboundAnchor arc, bool result)
    {
        if (result)
        {
            if (_scanAnchor == null)
                _scanAnchor = this.gameObject.AddComponent<OVRSpatialAnchor>();

            arc.BindTo(_scanAnchor);
            Invoke(nameof(EraseAnchorIfFar), 0.1f);
            if (_anchorView != null)
                _anchorView.enabled = ShowAnchor;
        }
        else
        {
            EraseCurrentAnchor();
        }
    }

    private void EraseAnchorIfFar()
    {
        if (Vector3.Distance(_cameraRig.centerEyeAnchor.transform.position, this.transform.position) > MaximumAnchorDistance)
                EraseCurrentAnchor();
    }
       
    private void EraseCurrentAnchor()
    {
        PlayerPrefs.SetString(SpatialAnchorUtils.MANUALSCAN_ANCHOR_PREFID, String.Empty);
        if (_scanAnchor != null)
        {
            _scanAnchor.Erase();
            Destroy(_scanAnchor);
        }
        if (_anchorView != null)
            _anchorView.enabled = false;

        this.transform.position = Vector3.zero;
    }

    private void OnSaveCompleted(OVRSpatialAnchor anchor, bool result )
    {
        if(result)
        {
            if (_anchorView != null)
                _anchorView.enabled = ShowAnchor;

            int playerNumUuids = PlayerPrefs.GetInt(SpatialAnchorUtils.COUNTER_UUIDS_PLAYERPREFS_TAG, 0);
            PlayerPrefs.SetInt(SpatialAnchorUtils.COUNTER_UUIDS_PLAYERPREFS_TAG, ++playerNumUuids);

            PlayerPrefs.SetString(SpatialAnchorUtils.MANUALSCAN_ANCHOR_PREFID, anchor.Uuid.ToString());
        }
    }
#endif
}
