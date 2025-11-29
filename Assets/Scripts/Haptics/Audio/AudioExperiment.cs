using UnityEngine;

public class AudioExperiment : MonoBehaviour
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
    [Range(0f,1f)] public float baseVolume = 0.25f; // baseline at outer radius
    [Range(0f,1f)] public float nearBoost  = 0.35f; // extra gain toward inner radius

    [Header("Optional")]
    [Range(0f,1f)] public float pitchBend = 0.0f;   // 0 = off
    [Range(0f,1f)] public float spatialBlend = 1f;  // 1 = fully 3D

    AudioSource loopSrc, oneShotSrc;
    bool wasGrabbed;
    bool goalCompleted = false;
    float loopResumeAtTime = 0f;   // when loop is allowed to start again

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

    void OnValidate()
    {
        if (innerRadius > outerRadius)
            innerRadius = Mathf.Max(0.001f, outerRadius * 0.5f);
    }

    void Update()
    {
        // grabbed if parented to either controller (matches your ManipulationControl)
        bool grabbed = (transform.parent == leftController || transform.parent == rightController);

        // just grabbed
        if (grabbed && !wasGrabbed && grabClip)
            oneShotSrc.PlayOneShot(grabClip, 1f);

        // just dropped
        if (!grabbed && wasGrabbed && !goalCompleted)
        {
            if (dropClip) oneShotSrc.PlayOneShot(dropClip, 1f);
            if (loopSrc.isPlaying) loopSrc.Stop();
            loopResumeAtTime = Time.time + 1.3f;
        }

        wasGrabbed = grabbed;

        // proximity loop only when not grabbed AND after delay
        float d = ClosestControllerDistance();
        bool inRange = !grabbed && d <= outerRadius;
        bool allowLoopNow = Time.time >= loopResumeAtTime;

        if (inRange && allowLoopNow)
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

    // 🔊 Called from SimpleGoalChecker.onGoalReached
    public void PlayGoalAudio()
    {
        Debug.Log("AudioControl: PlayGoalAudio() called.");

        if (goalClip)
            oneShotSrc.PlayOneShot(goalClip, 1f);

        if (loopSrc.isPlaying)
            loopSrc.Stop();

        goalCompleted = true;
        
        loopResumeAtTime = Time.time + 1.3f;
    }
}