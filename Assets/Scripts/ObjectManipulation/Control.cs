using UnityEngine;
using UnityEngine.InputSystem;
// using UnityEngine.Events; // only needed if you create local UnityEvents

public class ManipulationControl : MonoBehaviour
{
    // Optional global: lets other scripts know "something is grabbed"
    public static bool IsGrabbedGlobal = false;

    [Tooltip("Is THIS particular object currently grabbed?")]
    public bool IsGrabbed = false;

    [Header("Grab Settings")]
    [Tooltip("Maximum distance from controller to allow a grab.")]
    public float grabRadius = 0.2f;

    [Header("Controllers")]
    public GameObject leftController;
    public GameObject rightController;

    private XRIDefaultInputActions controls;

    void Awake()
    {
        controls = new XRIDefaultInputActions();
    }

    void OnEnable()
    {
        // Subscribe input callbacks

        // LEFT
        InputAction leftGripAction = controls.XRILeftLocomotion.GrabMove;
        leftGripAction.started  += LeftGripStarted;
        leftGripAction.canceled += LeftGripCanceled;

        // RIGHT
        InputAction rightGripAction = controls.XRIRightLocomotion.GrabMove;
        rightGripAction.started  += RightGripStarted;
        rightGripAction.canceled += RightGripCanceled;

        controls.Enable();
    }

    void OnDisable()
    {
        // Unsubscribe input callbacks

        // LEFT
        InputAction leftGripAction = controls.XRILeftLocomotion.GrabMove;
        leftGripAction.started  -= LeftGripStarted;
        leftGripAction.canceled -= LeftGripCanceled;

        // RIGHT
        InputAction rightGripAction = controls.XRIRightLocomotion.GrabMove;
        rightGripAction.started  -= RightGripStarted;
        rightGripAction.canceled -= RightGripCanceled;

        controls.Disable();
    }

    // ─────────────────────────────────────────────
    // LEFT HAND
    // ─────────────────────────────────────────────

    private void LeftGripStarted(InputAction.CallbackContext ctx)
    {
        if (leftController == null) return;

        float delta = Vector3.Distance(transform.position, leftController.transform.position);
        // Debug.Log($"Grip (Left) pressed. Distance={delta:F3}");

        // Only grab if close enough AND not already parented (not grabbed)
        if (delta < grabRadius && transform.parent == null)
        {
            transform.SetParent(leftController.transform, true);
            IsGrabbedGlobal = true;
            IsGrabbed = true;

            GrabEventSystem.TriggerGrab(gameObject, "Left", delta);
            // Debug.Log($"Grabbed {name} (left hand)");
        }
    }

    private void LeftGripCanceled(InputAction.CallbackContext ctx)
    {
        // Only release if this object is currently parented to the left controller
        if (transform.parent == leftController?.transform)
        {
            transform.SetParent(null, true);
            IsGrabbedGlobal = false;
            IsGrabbed = false;

            GrabEventSystem.TriggerRelease(gameObject, "Left");
            // Debug.Log($"Released {name} (left hand)");

            // OPTIONAL: notify AudioControl on this object that we just released it
            var audio = GetComponent<AudioControl>();
            if (audio != null)
            {
                // Only needed if you added a corresponding hook like:
                // public void NotifyReleased() { /* custom logic */ }
                audio.NotifyReleased();
            }
        }
    }

    // ─────────────────────────────────────────────
    // RIGHT HAND
    // ─────────────────────────────────────────────

    private void RightGripStarted(InputAction.CallbackContext ctx)
    {
        if (rightController == null) return;

        float delta = Vector3.Distance(transform.position, rightController.transform.position);
        // Debug.Log($"Grip (Right) pressed. Distance={delta:F3}");

        if (delta < grabRadius && transform.parent == null)
        {
            transform.SetParent(rightController.transform, true);
            IsGrabbedGlobal = true;
            IsGrabbed = true;

            GrabEventSystem.TriggerGrab(gameObject, "Right", delta);
            // Debug.Log($"Grabbed {name} (right hand)");
        }
    }

    private void RightGripCanceled(InputAction.CallbackContext ctx)
    {
        // Only release if this object is currently parented to the right controller
        if (transform.parent == rightController?.transform)
        {
            transform.SetParent(null, true);
            IsGrabbedGlobal = false;
            IsGrabbed = false;

            GrabEventSystem.TriggerRelease(gameObject, "Right");
            // Debug.Log($"Released {name} (right hand)");

            // OPTIONAL: notify AudioControl on this object that we just released it
            var audio = GetComponent<AudioControl>();
            if (audio != null)
            {
                audio.NotifyReleased();
            }
        }
    }

    // Example global listeners if you want them:
    // private void OnAnyGrab(GameObject obj, string hand) { /* ... */ }
    // private void OnAnyRelease(GameObject obj, string hand) { /* ... */ }
}