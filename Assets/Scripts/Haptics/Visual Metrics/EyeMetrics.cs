using UnityEngine;

public class EyeMetrics : MonoBehaviour
{
    public static float timeToFirstFixation = -1f;
    public static float totalTargetFixation = 0f;

    [Header("Eye gaze source")]
    public Transform gazeOrigin;         // assign Left Eye here in Inspector

    [Header("Settings")]
    public float maxDistance = 50f;
    public string targetTag = "Target";  // your grabbable spheres

    bool hasFirstFixation = false;
    float trialTime = 0f;

    void Awake()
    {
        Debug.Log("[EyeMetrics] Awake");

        timeToFirstFixation = -1f;
        totalTargetFixation = 0f;
        hasFirstFixation = false;
        trialTime = 0f;
    }

    void Update()
    {
        // 1) Check if script is running and gaze is assigned
        if (gazeOrigin == null)
        {
            Debug.LogWarning("[EyeMetrics] gazeOrigin is NULL");
            return;
        }

        trialTime += Time.deltaTime;

        RaycastHit hit;

        // Always draw a yellow ray so you can see it in Scene view
        Debug.DrawRay(gazeOrigin.position, gazeOrigin.forward * maxDistance, Color.yellow);

        if (Physics.Raycast(gazeOrigin.position, gazeOrigin.forward, out hit, maxDistance))
        {
            // Overwrite with green if we hit something
            Debug.DrawRay(gazeOrigin.position, gazeOrigin.forward * maxDistance, Color.green);

            // TEMP: log everything we hit
            Debug.Log("[EyeMetrics] Gaze hit: " + hit.collider.name + " (tag: " + hit.collider.tag + ")");

            if (hit.collider.CompareTag(targetTag))
            {
                totalTargetFixation += Time.deltaTime;

                if (!hasFirstFixation)
                {
                    hasFirstFixation = true;
                    timeToFirstFixation = trialTime;
                    Debug.Log("[EyeMetrics] First fixation on Target at " + timeToFirstFixation + "s");
                }
            }
        }
    }
}
