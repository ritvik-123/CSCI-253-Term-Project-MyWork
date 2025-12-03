using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class SimpleGoalChecker : MonoBehaviour
{
    [Header("Object References")]
    public Transform item;   // movable sphere
    public Transform goal;   // target marker

    [Header("Settings")]
    public float triggerDistance = 0.2f;   // how close counts as "goal reached"

    [Header("Events")]
    public UnityEvent onGoalReached;       // typically calls AudioControl.PlayGoalAudio

    public static int goalCount = 0;
    public static float lastPlacementAccuracy = 0f;
    public static System.Action<int> onAnyGoalIncrement;

    bool hasTriggered = false;

    void Update()
    {
        if (hasTriggered) return;
        if (!item || !goal) return;

        float dist = Vector3.Distance(item.position, goal.position);

        if (dist <= triggerDistance)
        {
            hasTriggered = true;
            lastPlacementAccuracy = dist;

            Debug.Log("SimpleGoalChecker: Goal reached!");

            onGoalReached?.Invoke();  // This triggers AudioControl.PlayGoalAudio

            StartCoroutine(DestroyItemAfterDelay(1.5f));
        }
    }

    private IEnumerator DestroyItemAfterDelay(float delay)
    {
        // Cache references BEFORE waiting
        Transform itemRef = item;
        GameObject itemObj = item ? item.gameObject : null;
        Vector3 goalPos = goal.position;

        yield return new WaitForSeconds(delay);

        if (itemObj != null)
        {
            // Compute accuracy BEFORE destruction
            float finalAccuracy = Vector3.Distance(itemRef.position, goalPos);

            // Destroy
            Destroy(itemObj);

            // Update global counters
            goalCount++;
            lastPlacementAccuracy = finalAccuracy;
            onAnyGoalIncrement?.Invoke(goalCount);

            // Record task time
            TaskTimer.EndTask();

            // Export metrics
            VisualMetricsCsvExporter.Instance?.ExportRow();
        }
    }

}
