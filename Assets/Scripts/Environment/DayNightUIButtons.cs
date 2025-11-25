using UnityEngine;
using UnityEngine.UI;

public class DayNightUIButtons : MonoBehaviour
{
    [Header("Refs")]
    public DayNightControllerLite controller;

    [Header("UI")]
    public Button gearButton;
    public GameObject panel;          // toggled flyout
    public Button morningBtn;         // 06:00
    public Button afternoonBtn;       // 14:00
    public Button eveningBtn;         // 18:00
    public Button nightBtn;           // 22:00
    public Toggle autoToggle;         // optional

    void Start()
    {
        if (panel) panel.SetActive(false);

        if (gearButton)
            gearButton.onClick.AddListener(() => { if (panel) panel.SetActive(!panel.activeSelf); });

        if (morningBtn)   morningBtn.onClick.AddListener(() => SetTime(6f));
        if (afternoonBtn) afternoonBtn.onClick.AddListener(() => SetTime(14f));
        if (eveningBtn)   eveningBtn.onClick.AddListener(() => SetTime(18f));
        if (nightBtn)     nightBtn.onClick.AddListener(() => SetTime(22f));

        if (autoToggle)
        {
            autoToggle.isOn = controller != null && controller.auto;
            autoToggle.onValueChanged.AddListener(on =>
            {
                if (controller) controller.SetAuto(on);
            });
        }
    }

    void SetTime(float hour)
    {
        if (!controller) return;
        controller.QuickSet(hour);
        if (autoToggle) autoToggle.isOn = false;
    }
}
