using UnityEngine;

public class AudioExperiment : MonoBehaviour
{
    [Header("Controllers")]
    public Transform leftController;
    public Transform rightController;

    [Header("Radii (m)")]
    public float outerRadius = 0.25f;   // influence / bubble
    public float innerRadius = 0.08f;   // solid sphere

    [Header("Clips")]
    public AudioClip loopClip;
    public AudioClip grabClip;

    [Header("Levels")]
    [Range(0f,1f)] public float baseVolume = 0.25f; // Control baseline
    [Range(0f,1f)] public float nearBoost  = 0.35f; // added as you approach inner

    [Header("Optional")]
    [Range(0f,1f)] public float pitchBend = 0.10f;  // 0 = off
    [Range(0f,1f)] public float spatialBlend = 1f;  // 3D

    AudioSource loopSrc, oneShotSrc;
    bool wasGrabbed;

    void Awake()
    {
        loopSrc = gameObject.AddComponent<AudioSource>();
        loopSrc.loop = true; loopSrc.playOnAwake = false;
        loopSrc.spatialBlend = spatialBlend; loopSrc.rolloffMode = AudioRolloffMode.Custom;
        loopSrc.clip = loopClip;

        oneShotSrc = gameObject.AddComponent<AudioSource>();
        oneShotSrc.playOnAwake = false; oneShotSrc.spatialBlend = spatialBlend;
        oneShotSrc.rolloffMode = AudioRolloffMode.Custom;
    }

    void OnValidate()
    {
        if (innerRadius > outerRadius) innerRadius = Mathf.Max(0.001f, outerRadius * 0.5f);
    }

    void Update()
    {
        bool grabbed = (transform.parent == leftController || transform.parent == rightController);

        if (grabbed && !wasGrabbed && grabClip) oneShotSrc.PlayOneShot(grabClip, 1f);
        wasGrabbed = grabbed;

        float d = ClosestControllerDistance();
        bool inRange = !grabbed && d <= outerRadius;

        if (inRange)
        {
            if (!loopSrc.isPlaying && loopClip) loopSrc.Play();

            float clamped = Mathf.Clamp(d, innerRadius, outerRadius);
            float t = Mathf.InverseLerp(outerRadius, innerRadius, clamped); // 0..1 outer→inner

            loopSrc.volume = Mathf.Clamp01(baseVolume + t * nearBoost);
            loopSrc.pitch  = 1f + t * pitchBend;
        }
        else if (loopSrc.isPlaying) loopSrc.Stop();
    }

    float ClosestControllerDistance()
    {
        float best = float.PositiveInfinity;
        if (leftController)  best = Mathf.Min(best, Vector3.Distance(transform.position, leftController.position));
        if (rightController) best = Mathf.Min(best, Vector3.Distance(transform.position, rightController.position));
        return best;
    }
}