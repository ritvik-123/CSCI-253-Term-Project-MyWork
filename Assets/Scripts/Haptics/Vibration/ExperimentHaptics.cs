using UnityEngine;
using UnityEngine.XR;

public class ExperimentProximityAndGrabHaptics_Generic : MonoBehaviour
{
    [Header("Controllers (same as in ManipulationControl)")]
    public GameObject leftController;
    public GameObject rightController;

    [Header("Proximity (pre-grab)")]
    [Tooltip("Meters at which hiss starts.")]
    public float proximityRadius = 0.5f;
    [Tooltip("Pulse duration for each hiss (seconds). Keep short.")]
    public float hissPulseDuration = 0.02f;
    [Tooltip("Interval (s) when FAR (at radius).")]
    public float hissMaxInterval = 0.25f;
    [Tooltip("Interval (s) when NEAR (at contact).")]
    public float hissMinInterval = 0.05f;
    [Tooltip("Amplitude range (0..1) for hiss.")]
    public Vector2 hissAmplitudeRange = new Vector2(0.05f, 0.35f);
    [Tooltip("Curve mapping normalized proximity [0..1] => amplitude (near = 1).")]
    public AnimationCurve hissAmplitudeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Grabbed (continuous)")]
    [Tooltip("Meters at which the near/far mapping reaches max strength.")]
    public float grabbedReferenceDistance = 0.35f;
    [Tooltip("Short tick used to feel continuous (seconds).")]
    public float grabbedTickDuration = 0.03f;
    [Tooltip("Amplitude range (0..1) for grabbed cue (near=strong).")]
    public Vector2 grabbedAmplitudeRange = new Vector2(0.05f, 0.6f);
    [Tooltip("Curve mapping proximity-to-hand [0..1] => amplitude.")]
    public AnimationCurve grabbedAmplitudeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    float leftHissTimer, rightHissTimer;

    void Update()
    {
        bool leftHolding = (transform.parent == leftController?.transform);
        bool rightHolding = (transform.parent == rightController?.transform);
        bool grabbed = leftHolding || rightHolding;

        if (!grabbed)
        {
            // --- PRE-GRAB HISS: choose per-hand independently ---
            ProximityHiss(leftController, XRNode.LeftHand, ref leftHissTimer);
            ProximityHiss(rightController, XRNode.RightHand, ref rightHissTimer);
        }
        else
        {
            // Silence hiss timers while grabbed
            leftHissTimer = rightHissTimer = 0f;

            // --- GRABBED CONTINUOUS ---
            if (leftHolding)
                GrabbedDistanceCue(leftController, XRNode.LeftHand);
            if (rightHolding)
                GrabbedDistanceCue(rightController, XRNode.RightHand);
        }
    }

    void ProximityHiss(GameObject controllerGO, XRNode node, ref float timer)
    {
        if (controllerGO == null) return;

        float d = Vector3.Distance(transform.position, controllerGO.transform.position);
        if (d > proximityRadius) { timer = 0f; return; }

        float norm = Mathf.Clamp01(1f - d / Mathf.Max(0.0001f, proximityRadius)); // 0 far, 1 near
        float interval = Mathf.Lerp(hissMaxInterval, hissMinInterval, norm);
        float amp = Mathf.Lerp(hissAmplitudeRange.x, hissAmplitudeRange.y, hissAmplitudeCurve.Evaluate(norm));

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SendHaptic(node, amp, hissPulseDuration);
            timer = Mathf.Max(0.01f, interval);
        }
    }

    void GrabbedDistanceCue(GameObject controllerGO, XRNode node)
    {
        if (controllerGO == null) return;

        float dist = Vector3.Distance(transform.position, controllerGO.transform.position);
        float proximity = Mathf.Clamp01(1f - dist / Mathf.Max(0.0001f, grabbedReferenceDistance)); // 0 far, 1 near
        float amp = Mathf.Lerp(grabbedAmplitudeRange.x, grabbedAmplitudeRange.y,
                               grabbedAmplitudeCurve.Evaluate(proximity));

        SendHaptic(node, amp, grabbedTickDuration);
    }

    // --- XR haptics helper ---
    static void SendHaptic(XRNode node, float amplitude, float duration)
    {
        var device = InputDevices.GetDeviceAtXRNode(node);
        if (!device.isValid) return;

        if (device.TryGetHapticCapabilities(out var caps) && caps.supportsImpulse)
        {
            uint channel = 0;
            device.SendHapticImpulse(channel, Mathf.Clamp01(amplitude), Mathf.Max(0f, duration));
        }
    }
}