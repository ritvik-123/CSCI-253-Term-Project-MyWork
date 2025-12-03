using System.IO;
using UnityEngine;

public class AudioMetricsCsvExporter : MonoBehaviour
{
    [Header("Participant Info")]
    public string participantName = "P01";
    public string condition = "Audio-Control";

    [Header("CSV")]
    public string fileName = "audio_metrics.csv";

    private string path;

    private void Awake()
    {
        path = Path.Combine(Application.persistentDataPath, fileName);
    }

    private void Start()
    {
        EnsureHeader();
    }

    private void EnsureHeader()
    {
        if (!File.Exists(path) || new FileInfo(path).Length == 0)
        {
            using (var writer = new StreamWriter(path, false))
            {
                writer.WriteLine(
                    "Participant," +
                    "Condition," +
                    "ObjectName," +
                    "TaskCompletionTime," +
                    "WrongGrabs," +
                    "PrematureReleases," +
                    "BadPlacements," +
                    "StateMisinterpretations," +
                    "LastCueLatencyMs," +
                    "AvgCueLatencyMs"
                );
            }
        }
    }

    public void ExportAudioMetrics(AudioMetrics m)
    {
        using (var writer = new StreamWriter(path, true))
        {
            writer.WriteLine(
                $"{participantName}," +
                $"{condition}," +
                $"{m.gameObject.name}," +   // NEW: Log which sphere's trial this row came from
                $"{m.taskCompletionTime:F3}," +
                $"{m.wrongGrabCount}," +
                $"{m.prematureReleaseCount}," +
                $"{m.badPlacementCount}," +
                $"{m.stateMisinterpretationCount}," +
                $"{m.lastCueLatencyMs:F1}," +
                $"{m.avgCueLatencyMs:F1}"
            );
        }

        Debug.Log("[CSV] Exported AUDIO metrics for " + m.gameObject.name);
    }
}
