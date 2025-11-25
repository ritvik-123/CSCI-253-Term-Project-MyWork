using UnityEngine;
using UnityEngine.Events;

public class SimpleGoalChecker : MonoBehaviour
{
    [Header("Assign objects")]
    public Transform item; // the movable item (sphere you grab)
    public Transform goal; // the target (sphere that stays still)

    [Header("Settings")]
    public float triggerDistance = 0.2f; // how close is "placed in the goal"

    [Header("Events")]
    public UnityEvent onGoalReached;

    bool hasTriggered = false;

    public bool goalReached = false;

    void Update()
    {
        if (hasTriggered) return;
        if (item == null || goal == null) return;

        float dist = Vector3.Distance(item.position, goal.position);

        if (dist <= triggerDistance)
        {
            hasTriggered = true;
            onGoalReached?.Invoke();
            Debug.Log("Goal reached!");
            goalReached = true;
            DestroyBoth();
        }
    }
    public void DestroyBoth()
    {
        Destroy(item.gameObject);
    }
}

