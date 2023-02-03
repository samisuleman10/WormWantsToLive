using UnityEngine;
using UnityEngine.UI;

public class AppLoaderWithMessage : MonoBehaviour
{
    [SerializeField]
    private Button loadAnotherApp;

    [SerializeField]
    private string appToLoadBundleId;

    [SerializeField]
    private TextAsset jsonFile;

    private void Start()
    {
        loadAnotherApp.onClick.AddListener(LaunchApp);
    }

    private void OnDestroy()
    {
        loadAnotherApp.onClick.RemoveListener(LaunchApp);
    }

    public void LaunchApp()
    {
        bool fail = false;
        string message = jsonFile.text;
        AndroidJavaClass androidPlayer = new AndroidJavaClass(AndroidConstsUtils.ANDROID_PLAYER_NAME);
        AndroidJavaObject currentActivity = androidPlayer.GetStatic<AndroidJavaObject>(AndroidConstsUtils.ANDROID_CURRENT_ACTIVITY_NAME);
        AndroidJavaObject packageManager = currentActivity.Call<AndroidJavaObject>(AndroidConstsUtils.ANDROID_GET_PACKAGE_MANAGER_NAME);
        AndroidJavaObject launchIntent = null;

        try
        {
            launchIntent = packageManager.Call<AndroidJavaObject>(AndroidConstsUtils.ANDROID_GET_LAUNCH_INTENT_FOR_PACKAGE_NAME, appToLoadBundleId);
            launchIntent.Call<AndroidJavaObject>(AndroidConstsUtils.ANDROID_PUT_EXTRA_NAME, AndroidConstsUtils.ANDROID_ARGUMENTS_NAME, message);
        }
        catch (System.Exception e)
        {
            Debug.Log("Could not launch Anchoir Intent, error = " + e);
            fail = true;
        }

        if (fail)
        {
            Debug.Log("From AppLoaderWithMessage, App not found, bundleId = " + appToLoadBundleId);
        }
        else
        {
            currentActivity.Call(AndroidConstsUtils.ANDROID_START_ACTIVITY_NAME, launchIntent);
        }
        androidPlayer.Dispose();
        currentActivity.Dispose();
        packageManager.Dispose();
        launchIntent.Dispose();
    }

}
