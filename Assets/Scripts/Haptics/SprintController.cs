using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class SprintController : MonoBehaviour
{
    [Header("References")]
    public DynamicMoveProvider moveProvider;
    public InputActionProperty rightTrigger;

    [Header("Settings")]
    public float normalSpeed = 2.5f;
    public float sprintSpeed = 5f;
    public float triggerThreshold = 0.1f;

    private void Update()
    {
        float triggerValue = rightTrigger.action.ReadValue<float>();

        moveProvider.moveSpeed = 
            triggerValue > triggerThreshold ? sprintSpeed : normalSpeed;
    }
}
