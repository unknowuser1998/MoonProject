using UnityEngine;
using Cinemachine;

public class CinemachineInputHandler : MonoBehaviour
{
    void Start()
    {
        // Override Cinemachine's input to use the New Input System directly
        CinemachineCore.GetInputAxis = GetAxisCustom;
    }

    private float GetAxisCustom(string axisName)
    {
        if (UnityEngine.InputSystem.Mouse.current == null)
            return 0f;

        if (axisName == "Mouse X")
        {
            return UnityEngine.InputSystem.Mouse.current.delta.x.ReadValue() * 0.05f; // Scale down raw delta
        }
        else if (axisName == "Mouse Y")
        {
            return UnityEngine.InputSystem.Mouse.current.delta.y.ReadValue() * 0.05f; // Scale down raw delta
        }

        return 0f;
    }
}
