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
    public AudioClip loopClip;          // plays while in-range (not grabbed)
    public AudioClip grabClip;          // one-shot when grabbed
    public AudioClip dropClip;          // one-shot when released
    public AudioClip goalClip;          // one-shot when goal reached

    [Header("Levels")]
    [Range(0f,1f)] public float baseVolume = 0.25f; // baseline at outer radius
    [Range(0f,1f)] public float nearBoost  = 0.35f; // extra gain toward inner radius

    [Header("Optional")]
    [Range(0f,1f)] public float pitchBend = 0.0f;   // 0 = off
    [Range(0f,1f)] public float spatialBlend = 1f;  // 1 = fully 3D

    AudioSource loopSrc, oneShotSrc;
    bool wasGrabbed;

    float loopResumeAtTime = 0f;   // when it's allowed to resume


    void Awake()
    {
        // looping source for proximity hum
        loopSrc = gameObject.AddComponent<AudioSource>();
        loopSrc.loop = true;
        loopSrc.playOnAwake = false;
        loopSrc.spatialBlend = spatialBlend;
        loopSrc.rolloffMode = AudioRolloffMode.Custom; // we control volume manually
        loopSrc.clip = loopClip;

        // one-shot source for grab/drop
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
        bool grabbed = (transform.parent == leftController || transform.parent == rightController);

        // transitions
        if (grabbed && !wasGrabbed && grabClip)
            oneShotSrc.PlayOneShot(grabClip, 1f);           // just grabbed

        if (!grabbed && wasGrabbed)
        {
            if (dropClip) oneShotSrc.PlayOneShot(dropClip, 1f);
            if (loopSrc.isPlaying) loopSrc.Stop();

            // block the loop until 1s from now (set 10f if you want 10 seconds)
            loopResumeAtTime = Time.time + 1.3f;
        }
        bool goalReached = GetComponent<SimpleGoalChecker>().goalReached;
        if (goalReached && goalClip)
        {
            oneShotSrc.PlayOneShot(goalClip, 1f);
            goalReached = false; // prevent replaying

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
        // optional: reset the gate so it won't auto-play on re-enable
        loopResumeAtTime = Time.time + 0.5f;
    }



    float ClosestControllerDistance()
    {
        float best = float.PositiveInfinity;
        if (leftController)  best = Mathf.Min(best, Vector3.Distance(transform.position, leftController.position));
        if (rightController) best = Mathf.Min(best, Vector3.Distance(transform.position, rightController.position));
        return best;
    }
}