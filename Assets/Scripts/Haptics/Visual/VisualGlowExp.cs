using UnityEngine;

[DisallowMultipleComponent]
public class VisualGlowPulseVanish : MonoBehaviour
{
    [Header("References")]
    public Renderer shellRenderer;
    public Transform leftController;
    public Transform rightController;

    [Header("Detection")]
    public float inRangeRadius = 0.25f;

    [Header("Shader Properties (URP Lit defaults)")]
    public string baseColorProp = "_BaseColor";
    public string emissionProp = "_EmissionColor";

    [Header("Auto-scale bubble to grab radius")]
    public bool autoScaleToGrabRadius = true;
    public float grabRadius = 2f;
    public float shellPadding = 0.0f;

    [System.Serializable]
    public class StateSettings
    {
        [ColorUsage(true, true)] public Color color = new(0.5f, 0.8f, 1f, 0.25f);
        [Range(0f, 1f)] public float alphaMin = 0f;
        [Range(0f, 1f)] public float alphaMax = 0.4f;
        [Range(0f, 1f)] public float emissionMin = 0.05f;
        [Range(0f, 5f)] public float emissionMax = 2.5f;
        [Min(0f)] public float pulseSpeed = 2.0f;
    }

    [Header("State Settings")]
    public StateSettings idle = new();
    public StateSettings inRange = new();
    public StateSettings grabbed = new();

    private Material mat;

    bool goalReached = false;

    void Awake()
    {
        if (!shellRenderer) shellRenderer = GetComponentInChildren<Renderer>();
        if (shellRenderer)
        {
            mat = shellRenderer.material;
            mat.EnableKeyword("_EMISSION");
        }
        if (autoScaleToGrabRadius) AutoScaleShellToRadius();
    }

    void AutoScaleShellToRadius()
    {
        var mf = shellRenderer.GetComponent<MeshFilter>();
        if (!mf || !mf.sharedMesh) return;

        float meshRadiusLocal = mf.sharedMesh.bounds.extents.x;
        float parentScaleX = transform.lossyScale.x;
        float targetRadius = Mathf.Max(0.0001f, grabRadius + shellPadding);
        float childScale = targetRadius / (meshRadiusLocal * parentScaleX);

        shellRenderer.transform.localScale = Vector3.one * childScale;
    }

    void Update()
    {
        if (!mat) return;

        if (goalReached)
        {
            if (shellRenderer) shellRenderer.enabled = false;
            return;
        }
        bool isGrabbed = transform.parent == leftController || transform.parent == rightController;

        // 🔸 Hide ONLY the bubble when grabbed
        if (shellRenderer) shellRenderer.enabled = !isGrabbed;

        // (Optional) if hidden, skip updates
        if (isGrabbed) return;

        bool isInRange = false;
        if (!isGrabbed)
        {
            float d = float.PositiveInfinity;
            if (leftController)  d = Mathf.Min(d, Vector3.Distance(transform.position, leftController.position));
            if (rightController) d = Mathf.Min(d, Vector3.Distance(transform.position, rightController.position));
            isInRange = d <= inRangeRadius;
        }

        StateSettings s = isInRange ? inRange : idle; // grabbed path returns above

        float pulse    = Mathf.Sin(Time.time * s.pulseSpeed) * 0.5f + 0.5f;
        float alpha    = Mathf.Lerp(s.alphaMin,    s.alphaMax,    pulse);
        float emission = Mathf.Lerp(s.emissionMin, s.emissionMax, pulse);

        Color c = s.color; c.a = alpha;
        mat.SetColor(baseColorProp, c);
        mat.SetColor(emissionProp,  c * emission);

        // kill gloss when near invisible (bubble only)
        float glossOff = alpha < 0.05f ? 0f : 1f;
        if (mat.HasProperty("_Smoothness"))            mat.SetFloat("_Smoothness", glossOff * 0.2f);
        if (mat.HasProperty("_SpecularHighlights"))    mat.SetFloat("_SpecularHighlights", glossOff);
        if (mat.HasProperty("_EnvironmentReflections"))mat.SetFloat("_EnvironmentReflections", glossOff);
    }

    public void OnGoalReached()
    {
        goalReached = true;
    }

}