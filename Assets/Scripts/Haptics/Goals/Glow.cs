using UnityEngine;

public class Glow : MonoBehaviour
{
    [Header("Assign the Item (movable object)")]
    public Transform item;

    [Header("Distance Settings")]
    public float pulseDistance = 2f;
    public float goalDistance = 0.25f;

    [Header("Colors")]
    public Color idleColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    public Color pulseColor = new Color(0.3f, 0.6f, 1f, 1f);
    public Color goalReachedColor = Color.green;

    [Header("Pulse Settings")]
    public float pulseSpeed = 3f; // how fast alpha pulses

    [Header("Transparency")]
    [Range(0f, 1f)]
    public float bubbleAlpha = 0.5f; // max alpha for all states

    [Header("Brightness")]
    [Range(1f, 5f)]
    public float brightness = 1.5f; // scales RGB for all states

    private Renderer rend;
    private bool goalReached = false;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (!rend)
        {
            Debug.LogError("Glow: No Renderer found!");
            enabled = false;
            return;
        }

        // Start in idle state
        ApplyColor(idleColor, 1f);
    }

    void Update()
    {
        if (!item) return;

        float dist = Vector3.Distance(item.position, transform.position);

        // Get grab state
        bool isGrabbed = false;
        var mc = item.GetComponent<ManipulationControl>();
        if (mc != null)
            isGrabbed = mc.IsGrabbed;

        // If goal already reached, stay in that state
        if (goalReached)
            return;

        // Check for goal
        if (dist <= goalDistance)
        {
            goalReached = true;
            ApplyColor(goalReachedColor, 1f); // full bubbleAlpha at goal
            return;
        }

        // Pulse if grabbed OR close enough
        if (isGrabbed || dist <= pulseDistance)
        {
            // t goes 0 → 1 → 0 → 1 ...
            float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;

            // Pulse alpha between fully transparent (0) and bubbleAlpha (1 * bubbleAlpha)
            ApplyColor(pulseColor, t);
        }
        else
        {
            // Idle state: constant idleColor at bubbleAlpha
            ApplyColor(idleColor, 1f);
        }
    }

    /// <summary>
    /// Applies brightness and alpha to a base color, then sets the material color.
    /// alphaMultiplier is 0–1 and is multiplied by bubbleAlpha.
    /// </summary>
    private void ApplyColor(Color baseColor, float alphaMultiplier)
    {
        Color c = baseColor;

        // Brighten RGB
        c.r *= brightness;
        c.g *= brightness;
        c.b *= brightness;

        // Alpha = bubbleAlpha * alphaMultiplier (0..bubbleAlpha)
        c.a = bubbleAlpha * Mathf.Clamp01(alphaMultiplier);

        rend.material.color = c;
    }
}
