using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class SimpleGoalChecker : MonoBehaviour
{
    [Header("Assign objects")]
    public Transform item; // the movable item (sphere you grab)
    public Transform goal; // the target (sphere that stays still)

    [Header("Settings")]
    public float triggerDistance = 0.2f; // how close is "placed in the goal"

    [Header("Events")]
    public UnityEvent onGoalReached; // hook to AudioControl.PlayGoalAudio

    public static int goalCount = 0;

    public static float lastPlacementAccuracy = 0f;

    public static System.Action<int> onAnyGoalIncrement;

    bool hasTriggered = false;

    void Update()
    {
        if (hasTriggered) return;
        if (item == null || goal == null) return;

        float dist = Vector3.Distance(item.position, goal.position);

        if (dist <= triggerDistance)
        {
            hasTriggered = true;
            Debug.Log("SimpleGoalChecker: Goal reached!");

            lastPlacementAccuracy = dist;

            onGoalReached?.Invoke(); // this calls AudioControl.PlayGoalAudio

            StartCoroutine(DestroyItemAfterDelay(1.6f));   // wait 1.6 seconds before destroy
        }
    }

    private IEnumerator DestroyItemAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (item != null)
        {
            Destroy(item.gameObject);
            SimpleGoalChecker.goalCount++;
            SimpleGoalChecker.lastPlacementAccuracy = Vector3.Distance(item.position, goal.position);
            onAnyGoalIncrement?.Invoke(SimpleGoalChecker.goalCount);
            TaskTimer.MarkTaskNow();
            MetricsCsvExporter.Instance?.ExportRow();
        }
    }
}
