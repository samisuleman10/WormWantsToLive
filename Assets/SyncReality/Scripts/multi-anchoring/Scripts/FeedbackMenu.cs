using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FeedbackMenu : MonoBehaviour
{
	public static FeedbackMenu Instance;

	public CanvasGroup Menu;
	public RectTransform panel;
	public RectTransform statusText;
	public Transform Environment;

	[SerializeField]
	private List<Button> _buttonList;

	private Button _selectedButton;
	private int _menuIndex = 0;

	public Action onStartForceGuardian;
	public Action onStopForceGuardian;
	public Action onDisplayAnchors;
	public Action onHideAnchors;
	public Action onDeleteAllAnchors;
	public Action onDeletePlayerPrefs;
	public Action onDeleteSerialization;
	public Action<Transform> onForceMoveItem;

	private bool shouldDisplayMenu;
	private bool _isCoolingDown;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Destroy(this);
		}
	}

	private void Start()
    {
		_selectedButton = _buttonList[0];

	}

	private void Update()
	{
		if(!_isCoolingDown)
		if (OVRInput.Get(OVRInput.Button.PrimaryThumbstick) && OVRInput.Get(OVRInput.Button.SecondaryThumbstick))
		{
			ToggleMenu();
		}

		var areControllersUsedByMenu = ControllersInputHandler.Instance.GetAreControllersUsedByMenu;
		if (!areControllersUsedByMenu) return;

		if (OVRInput.GetDown(OVRInput.RawButton.A))
		{
			if (statusText)
				statusText.localPosition = new Vector3(statusText.localPosition.x, statusText.localPosition.y + 40, statusText.localPosition.z);
		}

		if (OVRInput.GetDown(OVRInput.RawButton.B))
		{
			if (statusText)
				statusText.localPosition = new Vector3(statusText.localPosition.x, statusText.localPosition.y - 40, statusText.localPosition.z);
		}

		HandleMenuNavigation();
	}

	private void ToggleMenu()
    {
		_isCoolingDown = true;
		Invoke(nameof(ResetCoolingDown), 1);
		shouldDisplayMenu = !shouldDisplayMenu;
		Menu.alpha = shouldDisplayMenu? 1:0;
		ControllersInputHandler.Instance.SetGetAreControllersUsedByMenu(shouldDisplayMenu);
	}

	private void ResetCoolingDown()
    {
		_isCoolingDown = false;
	}

	private void HandleMenuNavigation()
	{
		if (OVRInput.GetDown(OVRInput.RawButton.RThumbstickUp))
		{
			NavigateToIndexInMenu(false);
		}
		if (OVRInput.GetDown(OVRInput.RawButton.RThumbstickDown))
		{
			NavigateToIndexInMenu(true);
		}
		if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger))
		{
			_selectedButton.OnSubmit(null);
		}
	}

	private void NavigateToIndexInMenu(bool moveNext)
	{
		if (moveNext)
		{
			_menuIndex++;
			if (_menuIndex > _buttonList.Count - 1)
			{
				_menuIndex = 0;
			}
		}
		else
		{
			_menuIndex--;
			if (_menuIndex < 0)
			{
				_menuIndex = _buttonList.Count - 1;
			}
		}

		Debug.Log("_menuIndex = " + _menuIndex);
		MovePanel(_menuIndex, moveNext);
		_selectedButton.OnDeselect(null);
		_selectedButton = _buttonList[_menuIndex];
		_selectedButton.OnSelect(null);
	}

	private void MovePanel(int menuIndex, bool moveNext)
	{
		if (panel == null) return;
		if (moveNext)
		{
			if (menuIndex >= 4)
				panel.localPosition = new Vector3(panel.localPosition.x, panel.localPosition.y + 100, panel.localPosition.z);
		}

		if (!moveNext)
		{
			if (menuIndex >= 4)
				panel.localPosition = new Vector3(panel.localPosition.x, panel.localPosition.y - 100, panel.localPosition.z);
		}

		if (menuIndex == 0 || menuIndex <= 3)
			panel.localPosition = new Vector3(panel.localPosition.x, 0, panel.localPosition.z);

		if (menuIndex == _buttonList.Count - 1)
			panel.localPosition = new Vector3(panel.localPosition.x, 100 * (_buttonList.Count - 4), panel.localPosition.z);
	}

	public void GoBayckToStartScene()
	{
		SceneManager.LoadScene("Start", LoadSceneMode.Single);
	}

	public void OnStartForceGuardianPressed()
	{
		Debug.Log("StartForceGuardian");
		onStartForceGuardian?.Invoke();
	}

	public void OnStopForceGuardianPressed()
	{
		Debug.Log("OnStopForceGuardian");
		onStopForceGuardian?.Invoke();
	}

	public void OnDisplayAnchorsPressed()
	{
		Debug.Log("DisplayAnchors");
		onDisplayAnchors?.Invoke();
	}

	public void OnHideAnchorsPressed()
	{
		Debug.Log("OnHideAnchors");

		onHideAnchors?.Invoke();
	}

	public void OnDeleteAllAnchorsPressed()
	{
		Debug.Log("OnDeleteAllAnchors");

		onDeleteAllAnchors?.Invoke();
	}

	public void OnDeletePlayerPrefsPressed()
	{
		Debug.Log("OnDeletePlayerPrefs");

		onDeletePlayerPrefs?.Invoke();
	}

	public void OnDeleteSerializationPressed()
	{
		Debug.Log("OnDeleteSerialization");

		onDeleteSerialization?.Invoke();
	}

	public void OnForceMoveEnvironmentPressed()
	{
		Debug.Log("OnForceMoveEnvironment");

		onForceMoveItem?.Invoke(Environment);
		ControllersInputHandler.Instance.SetGetAreControllersUsedByMenu(false);
		Menu.alpha = 0;
	}
}