using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Component gestionates the menu pages by button click.
/// </summary>

public class StartMainMenuHandler : MonoBehaviour
{
    [Header("Pages")]
    [SerializeField]
    private GameObject firstPage;

    [SerializeField]
    private GameObject secondPage;

    [Header("Buttons")]
    [SerializeField]
    private Button creteNewRoomButton;

    [SerializeField]
    private Button loadExistingRoomButton;

    [SerializeField]
    private Button backButton;

    [Header("Others")]
    [SerializeField]
    private RoomTypeLoaderInStart roomLoader;

    private void Start()
    {
        creteNewRoomButton.onClick.AddListener(CreteNewRoomButtonClicked);
        loadExistingRoomButton.onClick.AddListener(LoadExistingRoomButtonClicked);
        backButton.onClick.AddListener(GoBack);
    }

    private void OnDestroy()
    {
        if (creteNewRoomButton != null && loadExistingRoomButton != null && backButton != null)
        {
            creteNewRoomButton.onClick.RemoveListener(CreteNewRoomButtonClicked);
            loadExistingRoomButton.onClick.RemoveListener(LoadExistingRoomButtonClicked);
            backButton.onClick.RemoveListener(GoBack);
        }
    }

    private void CreteNewRoomButtonClicked()
    {
        roomLoader.LoadEditSceneWithNewRoom();
    }

    private void LoadExistingRoomButtonClicked()
    {
        secondPage.SetActive(true);
        firstPage.SetActive(false);
    }

    private void GoBack()
    {
        secondPage.SetActive(false);
        firstPage.SetActive(true);
    }
}
