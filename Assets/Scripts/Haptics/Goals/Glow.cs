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
    public float pulseSpeed = 3f;
    public float pulseMin = 0.5f;
    public float pulseMax = 1.5f;

    [Header("Transparency")]
    [Range(0f, 1f)]
    public float bubbleAlpha = 0.5f;

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

        SetColorWithAlpha(idleColor);
    }

    void Update()
    {
        if (!item) return;

        float dist = Vector3.Distance(item.position, transform.position);

        bool isGrabbed = false;
        var mc = item.GetComponent<ManipulationControl>();
        if (mc != null)
            isGrabbed = mc.IsGrabbed;

        // If goal reached → stay green
        if (goalReached)
            return;

        if (dist <= goalDistance)
        {
            goalReached = true;
            SetColorWithAlpha(goalReachedColor);
            return;
        }

        // Pulse if grabbed OR within pulse distance
        if (isGrabbed || dist <= pulseDistance)
        {
            float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
            float intensity = Mathf.Lerp(pulseMin, pulseMax, t);

            Color c = pulseColor * intensity;
            SetColorWithAlpha(c);
        }
        else
        {
            SetColorWithAlpha(idleColor);
        }
    }

    private void SetColorWithAlpha(Color c)
    {
        c.a = bubbleAlpha;
        rend.material.color = c;
    }
}
