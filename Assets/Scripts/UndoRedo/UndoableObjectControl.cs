using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class UndoableObjectControl : MonoBehaviour
{
    public ManipulationControl undoableObject;

    // Initialize object state
    void Start()
    {
        if (undoableObject == null)
        {
            undoableObject = GetComponent<ManipulationControl>();
        }
    }
}