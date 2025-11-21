using UnityEngine;

public class VisualGlowPulseControl : MonoBehaviour
{
    [Header("Assign Renderers")]
    public Renderer solidRenderer;   // solid (parent) mesh
    public Renderer shellRenderer;   // bubble (child) mesh

    [Header("Controllers (optional for in-range)")]
    public Transform leftController;
    public Transform rightController;

    [Header("Bubble Size")]
    [Tooltip("Bubble target ≈ multiplier × solid’s world radius")]
    public float bubbleScaleMultiplier = 1.2f;
    public bool scaleOnStart = true;

    [Header("Detection")]
    public float inRangeRadius = 0.25f;

    [Header("Shader Properties (URP/Lit)")]
    public string baseColorProp = "_BaseColor";
    public string emissionProp  = "_EmissionColor";

    [Header("State Colors")]
    public Color idleColor    = new(0.45f, 0.85f, 1f, 0.22f);
    public Color inRangeColor = new(0.55f, 0.95f, 1f, 0.30f);
    public Color grabbedColor = new(1.00f, 0.75f, 0.35f, 0.40f);

    [Header("Pulse Speeds (Hz-ish)")]
    public float idleSpeed = 2.2f;
    public float inRangeSpeed = 2.8f;
    public float grabbedSpeed = 3.2f;

    [Header("Shared Pulse Ranges")]
    [Range(0f, 1f)] public float alphaMin = 0.10f;
    [Range(0f, 1f)] public float alphaMax = 0.40f;
    [Range(0f, 5f)] public float emissionMin = 0.10f;
    [Range(0f, 5f)] public float emissionMax = 2.00f;

    Material mat;

    void Awake()
    {
        if (!solidRenderer || !shellRenderer)
        {
            Debug.LogWarning("Assign both solidRenderer (solid sphere) and shellRenderer (bubble).");
            return;
        }

        mat = shellRenderer.material;
        mat.EnableKeyword("_EMISSION");

        if (scaleOnStart) FitBubbleToSolid();
    }

    void OnValidate()
    {
        // keep 1.2× look while tweaking in editor
        if (!Application.isPlaying && solidRenderer && shellRenderer)
            FitBubbleToSolid();
    }

    void FitBubbleToSolid()
    {
        // target world radius from the solid
        float solidR = solidRenderer.bounds.extents.x;
        float targetR = solidR * Mathf.Max(1.0f, bubbleScaleMultiplier);

        // current bubble world radius
        float bubbleR = shellRenderer.bounds.extents.x;
        if (bubbleR <= 1e-6f) return;

        float factor = targetR / bubbleR;
        shellRenderer.transform.localScale *= factor; // precise resize to target
    }

    void Update()
    {
        if (!mat) return;

        // state: grabbed > inRange > idle (bubble always visible)
        bool grabbed = (transform.parent == leftController || transform.parent == rightController);

        bool inRange = false;
        if (!grabbed)
        {
            float d = float.PositiveInfinity;
            if (leftController)  d = Mathf.Min(d, Vector3.Distance(transform.position, leftController.position));
            if (rightController) d = Mathf.Min(d, Vector3.Distance(transform.position, rightController.position));
            inRange = d <= inRangeRadius;
        }

        Color baseC; float speed;
        if (grabbed)      { baseC = grabbedColor; speed = grabbedSpeed; }
        else if (inRange) { baseC = inRangeColor; speed = inRangeSpeed; }
        else              { baseC = idleColor;    speed = idleSpeed; }

        // pulse 0..1
        float t = Mathf.Sin(Time.time * speed) * 0.5f + 0.5f;

        // shared ranges for transparency + emission
        Color c = baseC; 
        c.a = Mathf.Lerp(alphaMin, alphaMax, t);
        float e = Mathf.Lerp(emissionMin, emissionMax, t);

        mat.SetColor(baseColorProp, c);
        mat.SetColor(emissionProp,  c * e);
    }
}