using UnityEngine;

public class IncorrectGrabCounter : MonoBehaviour
{
    // Accessible anywhere: IncorrectGrabCounter.incorrectGrabs
    public static int incorrectGrabs = 0;

    private void OnEnable()
    {
        GrabEventSystem.OnGrab.AddListener(OnGrab);
    }

    private void OnDisable()
    {
        GrabEventSystem.OnGrab.RemoveListener(OnGrab);
    }

    private void OnGrab(GameObject grabbedObj, string hand)
    {
        // Wrong interaction: grabbed the GOAL sphere instead of grabbable
        if (grabbedObj.CompareTag("GoalSphere"))
        {
            incorrectGrabs++;
            Debug.Log($"Incorrect grab #{incorrectGrabs}");
        }
    }
}