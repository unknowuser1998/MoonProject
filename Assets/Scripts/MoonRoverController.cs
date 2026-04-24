using UnityEngine;

public class MoonRoverController : MonoBehaviour
{
    [Header("Components")]
    [Tooltip("The Rigidbody attached to the Physics Sphere.")]
    [SerializeField] private Rigidbody sphereRigidbody;
    [Tooltip("The visual model of the rover.")]
    [SerializeField] private Transform visualBody;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 15f;
    [SerializeField] private float turnSpeed = 90f;
    [SerializeField] private float gravityMultiplier = 0.5f; // Less than 1 for low gravity feel

    [Header("Alignment Settings")]
    [SerializeField] private float alignSpeed = 5f;
    [SerializeField] private float raycastDistance = 1.5f;
    [SerializeField] private LayerMask groundLayer;

    private float moveInput;
    private float turnInput;
    private Vector3 currentNormal = Vector3.up;

    void Start()
    {
        if (sphereRigidbody == null) 
            sphereRigidbody = GetComponent<Rigidbody>();

        // Detach visual body so it can rotate independently of the sphere's rolling
        if (visualBody != null)
        {
            visualBody.parent = null;
        }
        
        // Setup rigidbody constraints for the Sphere trick
        if (sphereRigidbody != null)
        {
            // We want the sphere to roll freely if needed, but in this implementation, 
            // the sphere acts as a sliding/rolling puck. Let's just let it act naturally.
            sphereRigidbody.useGravity = true;
        }
    }

    void Update()
    {
        // Get Input (W/S for move, A/D for turn) using New Input System
        moveInput = 0f;
        turnInput = 0f;
        
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            if (UnityEngine.InputSystem.Keyboard.current.wKey.isPressed || UnityEngine.InputSystem.Keyboard.current.upArrowKey.isPressed) moveInput += 1f;
            if (UnityEngine.InputSystem.Keyboard.current.sKey.isPressed || UnityEngine.InputSystem.Keyboard.current.downArrowKey.isPressed) moveInput -= 1f;
            
            if (UnityEngine.InputSystem.Keyboard.current.dKey.isPressed || UnityEngine.InputSystem.Keyboard.current.rightArrowKey.isPressed) turnInput += 1f;
            if (UnityEngine.InputSystem.Keyboard.current.aKey.isPressed || UnityEngine.InputSystem.Keyboard.current.leftArrowKey.isPressed) turnInput -= 1f;
        }

        // Rotate the visual body steering smoothly based on input
        if (visualBody != null)
        {
            visualBody.Rotate(0f, turnInput * turnSpeed * Time.deltaTime, 0f, Space.Self);
        }
    }

    void FixedUpdate()
    {
        if (sphereRigidbody == null || visualBody == null) return;

        // Apply Forward Movement to the Sphere
        // Force is applied in the direction the Visual Body is facing
        Vector3 forwardForce = visualBody.forward * moveInput * moveSpeed;
        sphereRigidbody.AddForce(forwardForce, ForceMode.Acceleration);

        // Custom Moon Gravity (Base gravity * multiplier)
        // Unity's default gravity is -9.81. We add extra force to adjust the feel.
        Vector3 extraGravity = Physics.gravity * (gravityMultiplier - 1f);
        sphereRigidbody.AddForce(extraGravity, ForceMode.Acceleration);

        // Alignment & Suspension Tracking
        AlignAndFollow();
    }

    private void AlignAndFollow()
    {
        // 1. Follow the sphere's position
        visualBody.position = Vector3.Lerp(visualBody.position, sphereRigidbody.position, Time.fixedDeltaTime * 20f);

        // 2. Raycast to find the ground normal for alignment
        RaycastHit hit;
        // Shoot ray downwards from the sphere
        if (Physics.Raycast(sphereRigidbody.position, Vector3.down, out hit, raycastDistance, groundLayer))
        {
            // Smoothly transition the normal to the ground normal
            currentNormal = Vector3.Lerp(currentNormal, hit.normal, Time.fixedDeltaTime * alignSpeed);
        }
        else
        {
            // If floating/falling, slowly return to upright position
            currentNormal = Vector3.Lerp(currentNormal, Vector3.up, Time.fixedDeltaTime * alignSpeed * 0.5f);
        }

        // 3. Align the visual body to the current normal while maintaining its steering direction
        Vector3 projectedForward = Vector3.ProjectOnPlane(visualBody.forward, currentNormal).normalized;
        if (projectedForward != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(projectedForward, currentNormal);
            visualBody.rotation = Quaternion.Slerp(visualBody.rotation, targetRotation, Time.fixedDeltaTime * alignSpeed);
        }
    }
}
