using UnityEngine;

public class DayNightControllerLite : MonoBehaviour
{
    [Header("Time")]
    [Range(0, 24)] public float timeOfDay = 8f;   // 0..24
    public bool auto = true;
    [Tooltip("Real minutes for a full 24h cycle")]
    public float dayLengthMinutes = 2f;

    [Header("Scene Refs")]
    public Light sun;                 // Directional Light
    public Material envSkybox;        // Optional: assign your skybox; we'll instance it

    [Header("Lighting")]
    public Color nightAmbient = new Color(0.02f, 0.05f, 0.1f);
    public Color dayAmbient   = new Color(0.6f, 0.65f, 0.7f);
    public float maxSunIntensity = 1.2f;

    [Header("Skybox (optional)")]
    public string exposureProperty = "_Exposure";
    public float nightExposure = 0.7f;
    public float dayExposure   = 1.1f;

    float secPerGameDay;
    Material runtimeSky;

    void Awake()
    {
        if (sun == null) sun = RenderSettings.sun;

        // Make a runtime instance so edits don't change your asset
        if (envSkybox != null)
        {
            runtimeSky = new Material(envSkybox);
            RenderSettings.skybox = runtimeSky;
        }
        else
        {
            runtimeSky = RenderSettings.skybox;
        }

        secPerGameDay = Mathf.Max(0.01f, dayLengthMinutes * 60f);
        ApplyLighting();
    }

    void Update()
    {
        if (!auto) return;

        timeOfDay += (24f / secPerGameDay) * Time.deltaTime;
        if (timeOfDay >= 24f) timeOfDay -= 24f;

        ApplyLighting();
    }

    // --- Public API for UI ---
    public void QuickSet(float hour)
    {
        auto = false;
        timeOfDay = Mathf.Repeat(hour, 24f);
        ApplyLighting();
    }

    public void SetAuto(bool on) => auto = on;

    // --- Core lighting ---
    void ApplyLighting()
    {
        if (sun != null)
        {
            // Rotate sun: 0..24 → -90..270 (sunrise ~6:00)
            float sunAngle = (timeOfDay / 24f) * 360f - 90f;
            sun.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);

            // Daylight factor from sun height (0 at night, 1 at noon)
            float daylight = Mathf.Clamp01(Vector3.Dot(sun.transform.forward, Vector3.down));

            // Fade sun intensity & color
            sun.intensity = Mathf.Lerp(0.0f, maxSunIntensity, daylight);
            sun.color     = Color.Lerp(new Color(1f, 0.65f, 0.45f), Color.white, daylight);

            // Ambient tint
            RenderSettings.ambientLight = Color.Lerp(nightAmbient, dayAmbient, daylight);
        }

        // Optional: adjust skybox exposure if supported
        if (runtimeSky != null && runtimeSky.HasProperty(exposureProperty))
        {
            // Use the same daylight metric for exposure
            float daylight = (sun != null)
                ? Mathf.Clamp01(Vector3.Dot(sun.transform.forward, Vector3.down))
                : Mathf.InverseLerp(22f, 10f, timeOfDay); // simple fallback
            
            sun.enabled = daylight > 0.02f;   // after you compute daylight
            runtimeSky.SetFloat(exposureProperty, Mathf.Lerp(nightExposure, dayExposure, daylight));
        }

        DynamicGI.UpdateEnvironment();
    }
}
