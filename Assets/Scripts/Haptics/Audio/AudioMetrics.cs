using UnityEngine;

public class AudioMetrics : MonoBehaviour
{
    [Header("Metrics")]
    public float trialStartTime = -1f;
    public float taskCompletionTime = -1f;

    public int wrongGrabCount = 0;
    public int prematureReleaseCount = 0;
    public int badPlacementCount = 0;
    public int stateMisinterpretationCount = 0;

    public float lastCueLatencyMs = -1f;
    public float avgCueLatencyMs = -1f;

    [HideInInspector] public bool waitingForAction = false;
    [HideInInspector] public bool goalCompleted = false;

    float lastCueTime = -1f;
    float latencyAccum = 0f;
    int latencySamples = 0;

    public void StartTrial()
    {
        trialStartTime = Time.time;
        taskCompletionTime = -1f;

        wrongGrabCount = 0;
        prematureReleaseCount = 0;
        badPlacementCount = 0;
        stateMisinterpretationCount = 0;

        lastCueLatencyMs = -1f;
        avgCueLatencyMs = -1f;

        waitingForAction = false;
        goalCompleted = false;

        lastCueTime = -1f;
        latencyAccum = 0f;
        latencySamples = 0;
    }

    public void RegisterCueOnset()
    {
        lastCueTime = Time.time;
        waitingForAction = true;
    }

    public void RegisterGrab()
    {
        if (waitingForAction && lastCueTime > 0f)
        {
            float ms = (Time.time - lastCueTime) * 1000f;
            lastCueLatencyMs = ms;

            latencyAccum += ms;
            latencySamples++;
            avgCueLatencyMs = latencyAccum / latencySamples;
        }

        waitingForAction = false;
    }

    public void RegisterWrongGrab()
    {
        wrongGrabCount++;
    }

    public void RegisterPrematureRelease()
    {
        prematureReleaseCount++;
    }

    public void RegisterBadPlacement()
    {
        badPlacementCount++;
    }

    public void RegisterMisinterpretation()
    {
        stateMisinterpretationCount++;
        waitingForAction = false;
    }

    public void RegisterGoal()
    {
        if (goalCompleted) return;

        taskCompletionTime = Time.time - trialStartTime;
        goalCompleted = true;
    }
}
