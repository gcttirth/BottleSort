using UnityEngine;


public class HapticFeedback : MonoBehaviour {
    public static readonly long[] successPattern = { 0, 100, 25 , 50, 10, 20 };
    public static readonly long[] shortPattern = { 0, 10 };
    public static readonly long[] longPattern = { 0, 100, 50, 100, 25, 50 };

    public static void TriggerHaptic(long[] pattern, int repeat)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
                AndroidJavaObject vibrator = context.Call<AndroidJavaObject>("getSystemService", "vibrator");

                if (vibrator.Call<bool>("hasVibrator"))
                {
                    vibrator.Call("cancel");
                    vibrator.Call("vibrate", pattern, repeat);
                }
            }
        }
    }

    public static void CancelHaptic()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
                AndroidJavaObject vibrator = context.Call<AndroidJavaObject>("getSystemService", "vibrator");

                if (vibrator.Call<bool>("hasVibrator"))
                {
                    vibrator.Call("cancel");
                }
            }
        }
    }
}
