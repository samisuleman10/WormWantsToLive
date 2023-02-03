using UnityEngine;
using UnityEngine.UI;

public class ReceiverAppStart : MonoBehaviour
{
    [SerializeField]
    private Text messageText;

    [SerializeField]
    private StoryTeller storyTeller;

    private void Start()
    {
        var arguments = "No message received from previous app.";

        AndroidJavaClass UnityPlayer = new AndroidJavaClass(AndroidConstsUtils.ANDROID_PLAYER_NAME);
        AndroidJavaObject currentActivity = UnityPlayer.GetStatic<AndroidJavaObject>(AndroidConstsUtils.ANDROID_CURRENT_ACTIVITY_NAME);

        AndroidJavaObject intent = currentActivity.Call<AndroidJavaObject>(AndroidConstsUtils.ANDROID_GET_INTENT_NAME);
        bool hasExtra = intent.Call<bool>(AndroidConstsUtils.ANDROID_HAS_EXTRA_NAME, AndroidConstsUtils.ANDROID_ARGUMENTS_NAME);

        if (hasExtra)
        {
            AndroidJavaObject extras = intent.Call<AndroidJavaObject>(AndroidConstsUtils.ANDROID_GET_EXTRAS_NAME);
            arguments = extras.Call<string>(AndroidConstsUtils.ANDROID_GET_STRING_NAME, AndroidConstsUtils.ANDROID_ARGUMENTS_NAME);
            Debug.Log("From ReceiverAppStart, arguments = " + arguments);
        }

        if(messageText != null)
            messageText.text = arguments;

        if(storyTeller!= null && arguments != "No message received from previous app.")
            storyTeller.RunFromManualScanner(arguments);
    }
}
