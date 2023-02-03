using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Component gestionates the loading proccess of Edit Scene from Start Scene. When Edit scene is
/// loaded, SpatialAnchorUtils.ROOM_LOADING_TYPE_PLAYERPREFS_TAG value is read from PlayerPrefs and
/// starts the room proces by RoomLoadingType : either creates a new room or anchor the existing room
/// from another app.
/// </summary>

public class RoomTypeLoaderInStart : MonoBehaviour
{
    public void LoadEditSceneWithNewRoom()
    {
        var key = RoomLoadingType.NewRoom;
        PlayerPrefs.SetInt(SpatialAnchorUtils.ROOM_LOADING_TYPE_PLAYERPREFS_TAG, (int)key);
        SceneManager.LoadScene(SpatialAnchorUtils.EDIT_SCENE_NAME, LoadSceneMode.Single);
    }

    public void LoadEditSceneWithUnanchoredRoom()
    {
        var key = RoomLoadingType.LoadUnanchoredRoom;
        PlayerPrefs.SetInt(SpatialAnchorUtils.ROOM_LOADING_TYPE_PLAYERPREFS_TAG, (int)key);
        SceneManager.LoadScene(SpatialAnchorUtils.EDIT_SCENE_NAME, LoadSceneMode.Single);
    }
}
