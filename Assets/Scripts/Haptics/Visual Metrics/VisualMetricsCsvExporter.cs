using System.IO;
using UnityEngine;

public class VisualMetricsCsvExporter : MonoBehaviour
{
    public static VisualMetricsCsvExporter Instance;

    [Header("Participant Info")]
    public string participantName = "P01";

    [Header("Condition (type manually or choose)")]
    public string condition = "Control";   
    // You can type: "Control" or "Experiment"

    [Header("CSV")]
    public string fileName = "visual_metrics.csv";

    public EyeMetrics eyeMetrics;

    private string path;

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
                writer.WriteLine(
                    "Participant," +
                    "Condition," +
                    "TimeTaken," +
                    "IncorrectGrabs," +
                    "PlacementAccuracy," +
                    "TTFF," +
                    "FixationCount," +
                    "FixationRatio," +
                    "DwellDuration"
                );
            }
        }
    }

    public void ExportRow()
    {
        TaskTimer.EndTask();

        using (var writer = new StreamWriter(path, true))
        {
            writer.WriteLine(
                $"{participantName}," +
                $"{condition}," +
                $"{TaskTimer.lastTaskTime:F3}," +
                $"{IncorrectGrabCounter.incorrectGrabs}," +
                $"{SimpleGoalChecker.lastPlacementAccuracy:F4}," +
                $"{eyeMetrics.ttff:F3}," +
                $"{eyeMetrics.fixationCount}," +
                $"{eyeMetrics.fixationRatio:F3}," +
                $"{eyeMetrics.dwellDuration:F3}"
            );
        }

        // Reset for next trial
        TaskTimer.ResetTimer();
        eyeMetrics.ResetMetrics();
    }

}
