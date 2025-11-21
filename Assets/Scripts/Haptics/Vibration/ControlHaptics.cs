using UnityEngine;
using UnityEngine.XR;

public class ControlGrabHaptics_Generic : MonoBehaviour
{
    [Header("Controllers (same as in ManipulationControl)")]
    public GameObject leftController;
    public GameObject rightController;

    [Header("Haptics")]
    [Range(0f, 1f)] public float holdAmplitude = 0.25f;
    [Tooltip("Short tick to feel continuous, seconds.")]
    public float holdTickDuration = 0.03f;

    void Update()
    {
        // Grab is defined by your parenting logic
        bool leftHolding = (transform.parent == leftController?.transform);
        bool rightHolding = (transform.parent == rightController?.transform);

        if (leftHolding)
            SendHaptic(XRNode.LeftHand, holdAmplitude, holdTickDuration);

        if (rightHolding)
            SendHaptic(XRNode.RightHand, holdAmplitude, holdTickDuration);
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
