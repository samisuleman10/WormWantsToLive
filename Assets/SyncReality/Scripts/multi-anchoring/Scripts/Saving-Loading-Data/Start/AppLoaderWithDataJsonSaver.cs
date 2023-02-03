using UnityEngine;

public class AppLoaderWithDataJsonSaver : MonoBehaviour
{
    [SerializeField]
    private DataJsonSaver dataSaver;

    [SerializeField]
    private string appToLoadBundleId;//com.Syncreality.AWE_Madrid_Demo_rebuild_1

    private void Start()
    {
        //dataSaver.OnScanCreated += LaunchApp;
    }

    private void OnDestroy()
    {
        //dataSaver.OnScanCreated -= LaunchApp;
    }

    public void LaunchApp(string message)
    {
        var fail = false;
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
            Debug.Log("LaunchApp with error : " + e);
            fail = true;
        }
        
        if(!fail)
        {
            currentActivity.Call(AndroidConstsUtils.ANDROID_START_ACTIVITY_NAME, launchIntent);
        }
        androidPlayer.Dispose();
        currentActivity.Dispose();
        packageManager.Dispose();
        launchIntent.Dispose();
    }
}
