using UnityEngine;


public class HapticFeedback : MonoBehaviour {
    public static readonly long[] successPattern = { 0, 100 };
    public static readonly long[] failurePattern = { 0, 200, 100, 200 };
    public static void Vibrate(long[] pattern, int repeat) {
        AndroidJavaClass jc = new AndroidJavaClass("com.yourcompany.yourgame.AndroidHapticFeedback");
        jc.CallStatic("Vibrate", pattern, repeat);
    }
}
