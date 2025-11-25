using UnityEngine;

public class PoseDebug : MonoBehaviour
{
    Vector3 lastPos; Quaternion lastRot;
    void Start(){ lastPos = transform.position; lastRot = transform.rotation; }
    void Update(){
        if ((transform.position - lastPos).sqrMagnitude > 1e-6f || Quaternion.Angle(transform.rotation, lastRot) > 0.01f){
            Debug.Log($"Camera pose: {transform.position} / {transform.rotation.eulerAngles}");
            lastPos = transform.position; lastRot = transform.rotation;
        }
    }
}
