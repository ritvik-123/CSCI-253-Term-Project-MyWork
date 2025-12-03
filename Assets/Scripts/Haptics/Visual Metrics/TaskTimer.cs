using UnityEngine;

public class TaskTimer : MonoBehaviour
{
    public static float startTime;
    public static float lastTaskTime;
    public static bool hasStarted;

    void Awake()
    {
        ResetTimer();
    }

    public static void OnFirstGrab()
    {
        if (!hasStarted)
        {
            hasStarted = true;
            startTime = Time.time;
        }
    }

    public static void EndTask()
    {
        if (!hasStarted) return;
        lastTaskTime = Time.time - startTime;
    }

    public static void ResetTimer()
    {
        hasStarted = false;
        lastTaskTime = 0f;
        startTime = 0f;
    }
}