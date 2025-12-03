using UnityEngine;

public class AudioControl : MonoBehaviour
{
    [Header("Controllers")]
    public Transform leftController;
    public Transform rightController;

    [Header("Radii (m)")]
    public float outerRadius = 0.25f;
    public float innerRadius = 0.08f;

    [Header("Clips")]
    public AudioClip loopClip;
    public AudioClip grabClip;
    public AudioClip dropClip;
    public AudioClip goalClip;

    [Header("Levels")]
    [Range(0f, 1f)] public float baseVolume = 0.25f;
    [Range(0f, 1f)] public float nearBoost = 0.35f;

    [Header("Optional")]
    [Range(0f, 1f)] public float pitchBend = 0f;
    [Range(0f, 1f)] public float spatialBlend = 1f;

    AudioSource loopSrc;
    AudioSource oneShotSrc;

    AudioMetrics metrics;

    bool wasGrabbed = false;
    bool wasInRangeForCue = false;
    float loopResumeAtTime = 0f;

    private void Awake()
    {
        loopSrc = gameObject.AddComponent<AudioSource>();
        loopSrc.loop = true;
        loopSrc.playOnAwake = false;
        loopSrc.spatialBlend = spatialBlend;
        loopSrc.rolloffMode = AudioRolloffMode.Custom;
        loopSrc.clip = loopClip;

        oneShotSrc = gameObject.AddComponent<AudioSource>();
        oneShotSrc.playOnAwake = false;
        oneShotSrc.spatialBlend = spatialBlend;
        oneShotSrc.rolloffMode = AudioRolloffMode.Custom;

        metrics = GetComponent<AudioMetrics>();
        if (!metrics)
        {
            metrics = gameObject.AddComponent<AudioMetrics>();
        }
    }

    private void OnEnable()
    {
        metrics.StartTrial();
    }

    private void Update()
    {
        bool grabbed = ObjectGrabbed();
        float dist = ClosestControllerDistance();

        bool insideBubble = dist <= outerRadius;
        bool inRangeForCue = !grabbed && insideBubble;
        bool allowLoop = Time.time >= loopResumeAtTime;

        // Cue onset
        if (inRangeForCue && !wasInRangeForCue && allowLoop)
            metrics.RegisterCueOnset();

        // Misinterpretation
        if (!inRangeForCue && wasInRangeForCue && metrics.waitingForAction && !grabbed)
            metrics.RegisterMisinterpretation();

        // Grab
        if (grabbed && !wasGrabbed)
        {
            if (grabClip) oneShotSrc.PlayOneShot(grabClip);

            if (!insideBubble)
                metrics.RegisterWrongGrab();

            metrics.RegisterGrab();
        }

        // Premature Release
        if (!grabbed && wasGrabbed && !metrics.goalCompleted)
        {
            if (dropClip) oneShotSrc.PlayOneShot(dropClip);
            loopSrc.Stop();
            loopResumeAtTime = Time.time + 1.3f;

            metrics.RegisterPrematureRelease();
        }

        // Audio loop behavior
        if (inRangeForCue && allowLoop)
        {
            if (!loopSrc.isPlaying && loopClip) loopSrc.Play();

            float clamped = Mathf.Clamp(dist, innerRadius, outerRadius);
            float t = Mathf.InverseLerp(outerRadius, innerRadius, clamped);

            loopSrc.volume = baseVolume + t * nearBoost;
            loopSrc.pitch = 1f + t * pitchBend;
        }
        else
        {
            if (loopSrc.isPlaying) loopSrc.Stop();
        }

        wasGrabbed = grabbed;
        wasInRangeForCue = inRangeForCue;
    }

    public void PlayGoalAudio()
    {
        if (goalClip)
            oneShotSrc.PlayOneShot(goalClip);

        if (loopSrc.isPlaying)
            loopSrc.Stop();

        metrics.RegisterGoal();

        // Export metrics for THIS sphere only
        var exporter = FindObjectOfType<AudioMetricsCsvExporter>();
        if (exporter != null)
            exporter.ExportAudioMetrics(metrics);
    }

    private bool ObjectGrabbed()
    {
        return transform.parent == leftController || transform.parent == rightController;
    }

    private float ClosestControllerDistance()
    {
        float best = float.PositiveInfinity;
        if (leftController) best = Mathf.Min(best, Vector3.Distance(transform.position, leftController.position));
        if (rightController) best = Mathf.Min(best, Vector3.Distance(transform.position, rightController.position));
        return best;
    }

    public void NotifyReleased() { }
}
