using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Component is a represents a room. It is instantiated from RoomListLoader after the rooms are being
/// read from the device.
/// </summary>

public class RoomListItem : MonoBehaviour
{
    [Header("Scene references")]
    [SerializeField]
    private Text text;
    [SerializeField]
    private Button selectButton;
    [SerializeField]
    private Button deleteButton;
    [SerializeField]
    private Image anchorCheckImage;

    [Header("Library references")]
    [SerializeField]
    private Sprite IsAnchoredImage;
    [SerializeField]
    private Sprite NotAnchoredImage;

    private int id;
    private bool isRoomAnchored;

    public Action<int> onDeleteButtonClicked;

    private void Start()
    {
        selectButton.onClick.AddListener(LoadRoomWithThisID);
        deleteButton.onClick.AddListener(DeleteRoomWithThisID);
    }

    private void OnDestroy()
    {

        selectButton.onClick.RemoveListener(LoadRoomWithThisID);
        deleteButton.onClick.RemoveListener(DeleteRoomWithThisID);
    }

    public void Set(string name, int id, bool isAnchoredToCurrentApp)
    {
        text.text = name;
        this.id = id;
        isRoomAnchored = isAnchoredToCurrentApp;
        SetAnchorCheckImage(isAnchoredToCurrentApp);
    }

    private void DeleteRoomWithThisID()
    {
        onDeleteButtonClicked?.Invoke(id);
    }

    private void LoadRoomWithThisID()
    {
        if(isRoomAnchored)
        {
            PlayerPrefs.SetInt(SpatialAnchorUtils.ANCHOR_TO_LOAD_ID_PLAYERPREFS_TAG, id);
            SceneManager.LoadScene(SpatialAnchorUtils.EXPERIENCE_SCENE_NAME, LoadSceneMode.Single);
        }
        else
        {
            PlayerPrefs.SetInt(SpatialAnchorUtils.ANCHOR_TO_LOAD_ID_PLAYERPREFS_TAG, id);
            var roomLoadingType = (int)RoomLoadingType.LoadUnanchoredRoom;
            PlayerPrefs.SetInt(SpatialAnchorUtils.ROOM_LOADING_TYPE_PLAYERPREFS_TAG, roomLoadingType);
            SceneManager.LoadScene(SpatialAnchorUtils.EDIT_SCENE_NAME, LoadSceneMode.Single);
        }
    }

    private void SetAnchorCheckImage(bool isAnchoredToCurrentApp)
    {
        anchorCheckImage.sprite = isAnchoredToCurrentApp ? IsAnchoredImage : NotAnchoredImage;
    }
}
