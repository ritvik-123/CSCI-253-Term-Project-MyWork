using UnityEngine;

public class TaskTimer : MonoBehaviour
{
    public static TaskTimer Instance;

    public static float lastTaskTime = 0f;  // what CSV reads
    public static float startTime = 0f;     // when the trial started
    public static bool hasStarted = false;  // did we see the first grab?

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        GrabEventSystem.OnGrab.AddListener(OnAnyGrab);
    }

    private void OnDisable()
    {
        GrabEventSystem.OnGrab.RemoveListener(OnAnyGrab);
    }

    // Start timer on the VERY FIRST grab
    private void OnAnyGrab(GameObject obj, string hand)
    {
        if (!hasStarted)
        {
            hasStarted = true;
            startTime = Time.time;
            // Debug.Log("TaskTimer: timer started at " + startTime);
        }
    }

    // Called whenever a task (placement) is considered complete
    public static void MarkTaskNow()
    {
        if (!hasStarted) return;
        lastTaskTime = Time.time - startTime;
        // Debug.Log("TaskTimer: lastTaskTime = " + lastTaskTime);
    }

    // Optional helper if you want to reset between participants
    public static void ResetTimer()
    {
        hasStarted = false;
        startTime = 0f;
        lastTaskTime = 0f;
    }
}
