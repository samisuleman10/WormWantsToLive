using UnityEngine;

public class ControllersInputHandler : MonoBehaviour
{
	public static ControllersInputHandler Instance;

	private bool _areControllersUsedByMenu =  true;

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

	public bool GetAreControllersUsedByMenu => _areControllersUsedByMenu;

	public void SetGetAreControllersUsedByMenu(bool usedByControllers)
    {
		_areControllersUsedByMenu = usedByControllers;
	}
}
