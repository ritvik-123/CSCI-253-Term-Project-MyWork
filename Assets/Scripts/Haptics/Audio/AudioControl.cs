using UnityEngine;
using System.IO;
using System.Globalization;

public class AudioControl : MonoBehaviour
{
    [Header("Controllers")]
    public Transform leftController;
    public Transform rightController;

    [Header("Radii (m)")]
    public float outerRadius = 0.25f;   // "in-range" bubble
    public float innerRadius = 0.08f;   // solid sphere (max volume)

    [Header("Clips")]
    public AudioClip loopClip;   // plays while in-range (not grabbed)
    public AudioClip grabClip;   // one-shot when grabbed
    public AudioClip dropClip;   // one-shot when released
    public AudioClip goalClip;   // one-shot when goal reached

    [Header("Levels")]
    [Range(0f, 1f)] public float baseVolume = 0.25f; // baseline at outer radius
    [Range(0f, 1f)] public float nearBoost  = 0.35f; // extra gain toward inner radius

    [Header("Optional")]
    [Range(0f, 1f)] public float pitchBend = 0.0f;   // 0 = off
    [Range(0f, 1f)] public float spatialBlend = 1f;  // 1 = fully 3D

    // ─────────────────────────────────────────────
    // CSV / TRIAL META
    // ─────────────────────────────────────────────
    [Header("CSV / Trial Meta")]
    public string participantId = "P01";
    public string condition = "audio";
    public int trialIndex = 0;
    public bool autoIncrementTrialIndex = true;
    public bool autoWriteCsvOnGoal = true;
    public string csvFileName = "audio_metrics.csv";

    // ─────────────────────────────────────────────
    // METRICS (per trial)
    // ─────────────────────────────────────────────

    [Header("Metrics (per trial)")]
    [Tooltip("Time when the trial started (Time.time).")]
    public float taskStartTime = -1f;

    [Tooltip("Time (s) from trial start to successful placement (goal reached).")]
    public float taskCompletionTime = -1f;

    [Tooltip("Number of grabs that happened while the controller was NOT inside the outer radius (no 'in-range' cue).")]
    public int wrongGrabCount = 0;

    [Tooltip("Number of releases that occurred before the goal was completed.")]
    public int prematureReleaseCount = 0;

    [Tooltip("Number of times the object was placed outside the pad tolerance (bad placement).")]
    public int badPlacementCount = 0;

    [Tooltip("Number of times the user moved away/left the in-range bubble without acting (grab).")]
    public int stateMisinterpretationCount = 0;

    [Tooltip("Latency (ms) from last in-range cue onset to the grab that followed.")]
    public float lastCueToActionLatencyMs = -1f;

    [Tooltip("Average cue→grab latency (ms) over this trial.")]
    public float avgCueToActionLatencyMs = -1f;

    float cueLatencyAccumMs = 0f;
    int   cueLatencySampleCount = 0;

    float lastCueTime = -1f;          // when the last "in-range" cue turned on
    bool  cueWaitingForAction = false;

    // internal audio state
    AudioSource loopSrc, oneShotSrc;
    bool wasGrabbed;
    bool goalCompleted = false;
    float loopResumeAtTime = 0f;   // when loop is allowed to start again

    // previous in-range state for misinterpretation detection
    bool wasInRangeForCue = false;

    void Awake()
    {
        // looping source for proximity hum
        loopSrc = gameObject.AddComponent<AudioSource>();
        loopSrc.loop = true;
        loopSrc.playOnAwake = false;
        loopSrc.spatialBlend = spatialBlend;
        loopSrc.rolloffMode = AudioRolloffMode.Custom;
        loopSrc.clip = loopClip;

        // one-shot source for grab/drop/goal
        oneShotSrc = gameObject.AddComponent<AudioSource>();
        oneShotSrc.playOnAwake = false;
        oneShotSrc.spatialBlend = spatialBlend;
        oneShotSrc.rolloffMode = AudioRolloffMode.Custom;
    }

    void OnEnable()
    {
        // Auto-start a new trial when this item becomes active
        if (taskStartTime < 0f)
        {
            StartTask();
        }
    }

    void OnValidate()
    {
        if (innerRadius > outerRadius)
            innerRadius = Mathf.Max(0.001f, outerRadius * 0.5f);
    }

    void Update()
    {
        // grabbed if parented to either controller (matches your ManipulationControl)
        bool grabbed = (transform.parent == leftController || transform.parent == rightController);

        // distance to closest controller
        float d = ClosestControllerDistance();

        // inside bubble purely geometrically
        bool insideBubble = d <= outerRadius;

        // "in-range" for cue: bubble + not currently grabbed
        bool inRangeForCue = !grabbed && insideBubble;

        bool allowLoopNow = Time.time >= loopResumeAtTime;

        // ─────────────────────────────────────────────
        // METRICS: cue onset / misinterpretation / latency
        // ─────────────────────────────────────────────

        // Cue onset: we just entered the in-range bubble and are allowed to play the loop
        if (inRangeForCue && !wasInRangeForCue && allowLoopNow)
        {
            lastCueTime = Time.time;
            cueWaitingForAction = true;
            // Debug.Log($"[Metrics] Cue ON at t={lastCueTime:F3}");
        }

        // State misinterpretation: user leaves the bubble without acting (no grab)
        if (!inRangeForCue && wasInRangeForCue && cueWaitingForAction && !grabbed)
        {
            stateMisinterpretationCount++;
            cueWaitingForAction = false;
            // Debug.Log("[Metrics] State misinterpretation (left bubble without grab).");
        }

        // ─────────────────────────────────────────────
        // Transitions: grabs / drops
        // ─────────────────────────────────────────────

        // just grabbed
        if (grabbed && !wasGrabbed)
        {
            if (grabClip)
                oneShotSrc.PlayOneShot(grabClip, 1f);

            // Error: wrong grab (grabbed while not in proximity bubble)
            if (!insideBubble)
            {
                wrongGrabCount++;
                // Debug.Log("[Metrics] Wrong grab (outside bubble).");
            }

            // Cue→action latency: from last cue onset to this grab
            if (cueWaitingForAction && lastCueTime > 0f)
            {
                float latencyMs = (Time.time - lastCueTime) * 1000f;
                lastCueToActionLatencyMs = latencyMs;

                cueLatencyAccumMs += latencyMs;
                cueLatencySampleCount++;
                avgCueToActionLatencyMs = cueLatencyAccumMs / Mathf.Max(1, cueLatencySampleCount);

                cueWaitingForAction = false;
                // Debug.Log($"[Metrics] Cue->Grab latency = {latencyMs:F1} ms");
            }
        }

        // just dropped (release before goal)
        if (!grabbed && wasGrabbed && !goalCompleted)
        {
            if (dropClip) oneShotSrc.PlayOneShot(dropClip, 1f);
            if (loopSrc.isPlaying) loopSrc.Stop();
            loopResumeAtTime = Time.time + 1.3f;

            // Error: premature release (released before goalCompleted)
            prematureReleaseCount++;
            // Debug.Log("[Metrics] Premature release.");
        }

        wasGrabbed = grabbed;
        wasInRangeForCue = inRangeForCue;

        // ─────────────────────────────────────────────
        // Audio logic (same behavior, driven by inRangeForCue)
        // ─────────────────────────────────────────────

        if (inRangeForCue && allowLoopNow)
        {
            if (!loopSrc.isPlaying && loopClip) loopSrc.Play();

            float clamped = Mathf.Clamp(d, innerRadius, outerRadius);
            float t = Mathf.InverseLerp(outerRadius, innerRadius, clamped);

            loopSrc.volume = Mathf.Clamp01(baseVolume + t * nearBoost);
            loopSrc.pitch  = 1f + t * pitchBend;
        }
        else
        {
            if (loopSrc.isPlaying) loopSrc.Stop();
        }
    }

    void OnDisable()
    {
        loopResumeAtTime = Time.time + 0.5f;
    }

    float ClosestControllerDistance()
    {
        float best = float.PositiveInfinity;
        if (leftController)  best = Mathf.Min(best, Vector3.Distance(transform.position, leftController.position));
        if (rightController) best = Mathf.Min(best, Vector3.Distance(transform.position, rightController.position));
        return best;
    }

    // 🔊 Called from SimpleGoalChecker.onGoalReached via UnityEvent
    public void PlayGoalAudio()
    {
        Debug.Log("AudioControl: PlayGoalAudio() called.");

        if (goalClip)
            oneShotSrc.PlayOneShot(goalClip, 1f);

        if (loopSrc.isPlaying)
            loopSrc.Stop();

        goalCompleted = true;
        loopResumeAtTime = Time.time + 1.3f;

        // METRICS: task completion time
        if (taskStartTime >= 0f && taskCompletionTime < 0f)
        {
            taskCompletionTime = Time.time - taskStartTime;
            Debug.Log($"[Metrics] Task completed in {taskCompletionTime:F3} s. " +
                      $"Errors - wrong grabs: {wrongGrabCount}, premature releases: {prematureReleaseCount}, " +
                      $"bad placements: {badPlacementCount}, misinterpretations: {stateMisinterpretationCount}, " +
                      $"avg cue->action latency: {avgCueToActionLatencyMs:F1} ms");

            // Auto-write CSV row when goal is reached
            WriteCsvIfConfigured();
        }
        else
        {
            Debug.LogWarning("[Metrics] PlayGoalAudio() called but taskStartTime was not set. Did StartTask() run?");
        }
    }

    // ─────────────────────────────────────────────
    // PUBLIC API
    // ─────────────────────────────────────────────

    /// <summary>
    /// Call this at the beginning of each trial OR rely on OnEnable auto-call.
    /// Resets all per-trial metrics.
    /// </summary>
    public void StartTask()
    {
        taskStartTime = Time.time;
        taskCompletionTime = -1f;

        wrongGrabCount = 0;
        prematureReleaseCount = 0;
        badPlacementCount = 0;
        stateMisinterpretationCount = 0;

        lastCueToActionLatencyMs = -1f;
        avgCueToActionLatencyMs = -1f;
        cueLatencyAccumMs = 0f;
        cueLatencySampleCount = 0;

        lastCueTime = -1f;
        cueWaitingForAction = false;
        goalCompleted = false;

        // Debug.Log("[Metrics] Task started.");
    }

    /// <summary>
    /// Call this from your goal/tolerance logic if the object was placed outside the acceptable pad zone.
    /// </summary>
    public void RegisterBadPlacement()
    {
        badPlacementCount++;
        // Debug.Log("[Metrics] Bad placement (outside pad tolerance).");
    }

    /// <summary>
    /// Called by ManipulationControl when the user releases the object.
    /// Currently a no-op because drops are already detected in Update().
    /// </summary>
    public void NotifyReleased()
    {
        // Keep this as a hook if you want extra logging later.
    }

    // ─────────────────────────────────────────────
    // CSV EXPORT HELPERS
    // ─────────────────────────────────────────────

    void WriteCsvIfConfigured()
    {
        if (!autoWriteCsvOnGoal) return;

        int currentTrialIndex = trialIndex;
        if (autoIncrementTrialIndex)
        {
            trialIndex++;
        }

        AppendCsvRowToFile(participantId, currentTrialIndex, condition, csvFileName);
    }

    /// <summary>
    /// CSV header line. Call once when creating a new file.
    /// </summary>
    public static string GetCsvHeader()
    {
        return "participantId,trialIndex,condition," +
               "taskCompletionTime_s," +
               "wrongGrabCount,prematureReleaseCount,badPlacementCount,stateMisinterpretationCount," +
               "lastCueToActionLatency_ms,avgCueToActionLatency_ms";
    }

    /// <summary>
    /// Returns a CSV row for the current trial metrics.
    /// Use InvariantCulture so decimals use '.' regardless of OS.
    /// </summary>
    public string GetCsvRow(string participantId, int trialIndex, string condition)
    {
        CultureInfo ci = CultureInfo.InvariantCulture;

        string[] fields =
        {
            EscapeCsv(participantId),
            trialIndex.ToString(ci),
            EscapeCsv(condition),
            taskCompletionTime.ToString(ci),
            wrongGrabCount.ToString(ci),
            prematureReleaseCount.ToString(ci),
            badPlacementCount.ToString(ci),
            stateMisinterpretationCount.ToString(ci),
            lastCueToActionLatencyMs.ToString(ci),
            avgCueToActionLatencyMs.ToString(ci)
        };

        return string.Join(",", fields);
    }

    /// <summary>
    /// Appends the current trial's CSV row to a file in Application.persistentDataPath.
    /// If the file doesn't exist, it also writes the header.
    /// </summary>
    public void AppendCsvRowToFile(
        string participantId,
        int trialIndex,
        string condition,
        string fileName = "audio_metrics.csv")
    {
        string dir = Application.persistentDataPath;
        string path = Path.Combine(dir, fileName);
        bool fileExists = File.Exists(path);

        try
        {
            using (var writer = new StreamWriter(path, append: true))
            {
                if (!fileExists)
                {
                    writer.WriteLine(GetCsvHeader());
                }

                writer.WriteLine(GetCsvRow(participantId, trialIndex, condition));
            }

            Debug.Log($"[CSV] Appended metrics row to: {path}");
        }
        catch (IOException ex)
        {
            Debug.LogError($"[CSV] Failed to write metrics to {path}: {ex.Message}");
        }
    }

    /// <summary>
    /// Minimal CSV escaping (wrap in quotes if needed, escape inner quotes).
    /// </summary>
    static string EscapeCsv(string raw)
    {
        if (string.IsNullOrEmpty(raw))
            return "";

        bool mustQuote = raw.Contains(",") || raw.Contains("\"") || raw.Contains("\n");
        if (!mustQuote) return raw;

        string escaped = raw.Replace("\"", "\"\"");
        return $"\"{escaped}\"";
    }
}
