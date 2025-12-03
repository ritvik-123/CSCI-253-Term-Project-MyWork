using System.IO;
using UnityEngine;

public class MetricsCsvExporter : MonoBehaviour
{
    public static MetricsCsvExporter Instance;

    [Header("Participant Info")]
    public string participantName = "P01";   // <-- set this in Inspector

    public string fileName = "visual_metrics.csv";

    string path;

    private void Awake()
    {
        Instance = this;
        path = Path.Combine(Application.persistentDataPath, fileName);
    }

    private void Start()
    {
        EnsureHeader();
    }

    private void EnsureHeader()
    {
        if (!File.Exists(path))
        {
            using (var writer = new StreamWriter(path, false))
            {
                writer.WriteLine("Participant,TimeTaken,IncorrectGrabs,PlacementAccuracy,TimeToFirstFixation,TotalFixationOnTarget");
            }
        }
    }

    public void ExportRow()
    {
        float timeTaken        = TaskTimer.lastTaskTime;
        int incorrect          = IncorrectGrabCounter.incorrectGrabs;
        float placementAcc     = SimpleGoalChecker.lastPlacementAccuracy;
        float timeToFirstFix   = EyeMetrics.timeToFirstFixation;
        float totalFixOnTarget = EyeMetrics.totalTargetFixation;

        using (var writer = new StreamWriter(path, true)) // append
        {
            writer.WriteLine(
                $"{participantName}," +
                $"{timeTaken:F3}," +
                $"{incorrect}," +
                $"{placementAcc:F4}," +
                $"{timeToFirstFix:F3}," +
                $"{totalFixOnTarget:F3}"
            );
        }

        Debug.Log("Metrics appended to: " + path);
    }
}
