using UnityEngine;

public class EyeMetrics : MonoBehaviour
{
    [Header("Gaze Source")]
    public Transform gazeOrigin;

    [Header("Tags")]
    public string targetTag = "Target";
    public string distractorTag = "Distractor";

    [Header("Settings")]
    public float maxDistance = 40f;
    public float dwellBreak = 0.25f;

    // Public metrics
    public float ttff = -1f;
    public int fixationCount = 0;
    public float totalTargetTime = 0f;
    public float totalDistractorTime = 0f;
    public float fixationRatio = 0f;
    public float dwellDuration = 0f;

    // Internals
    float trialTime = 0f;
    bool firstFix = false;
    bool fixating = false;
    float currentFix = 0f;
    float longestFix = 0f;
    float breakTimer = 0f;

    void Start()
    {
        ResetMetrics();
    }

    public void ResetMetrics()
    {
        ttff = -1f;
        fixationCount = 0;
        totalTargetTime = 0f;
        totalDistractorTime = 0f;
        fixationRatio = 0f;
        dwellDuration = 0f;

        trialTime = 0f;
        firstFix = false;
        fixating = false;
        currentFix = 0f;
        longestFix = 0f;
        breakTimer = 0f;
    }

    void Update()
    {
        if (!gazeOrigin) return;

        trialTime += Time.deltaTime;

        RaycastHit hit;
        bool didHit = Physics.Raycast(gazeOrigin.position, gazeOrigin.forward, out hit, maxDistance);
        string tag = didHit ? hit.collider.tag : "";

        if (didHit && tag == targetTag)
        {
            if (!firstFix)
            {
                firstFix = true;
                ttff = trialTime;
            }

            totalTargetTime += Time.deltaTime;

            if (!fixating)
            {
                fixating = true;
                fixationCount++;
                currentFix = 0f;
                breakTimer = 0f;
            }

            currentFix += Time.deltaTime;
        }
        else
        {
            if (didHit && tag == distractorTag)
                totalDistractorTime += Time.deltaTime;

            if (fixating)
            {
                breakTimer += Time.deltaTime;

                if (breakTimer >= dwellBreak)
                {
                    fixating = false;

                    if (currentFix > longestFix)
                        longestFix = currentFix;

                    currentFix = 0f;
                }
            }
        }

        dwellDuration = longestFix;

        fixationRatio = (totalDistractorTime > 0)
            ? totalTargetTime / totalDistractorTime
            : totalTargetTime;
    }
}