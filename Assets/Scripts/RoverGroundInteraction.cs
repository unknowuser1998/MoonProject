using UnityEngine;

public class RoverGroundInteraction : MonoBehaviour
{
    [Header("References")]
    public Rigidbody rb;
    public TrailRenderer tireTracks;
    public ParticleSystem dustParticles;

    [Header("Settings")]
    public LayerMask groundLayer;
    public float raycastDistance = 0.5f;
    public float minSpeedForEffects = 0.2f;

    private Transform tireTracksTransform;
    private Transform visualBody;
    private Vector3 tireTracksLocalPos;

    private Transform dustTransform;
    private Vector3 dustLocalPos;

    private void Start()
    {
        if (tireTracks != null)
        {
            tireTracksTransform = tireTracks.transform;
            visualBody = tireTracksTransform.parent;
            tireTracksLocalPos = tireTracksTransform.localPosition;
            tireTracksTransform.SetParent(null); // Detach to prevent twisting
        }

        if (dustParticles != null)
        {
            dustTransform = dustParticles.transform;
            if (visualBody == null) visualBody = dustTransform.parent;
            dustLocalPos = dustTransform.localPosition;
            dustTransform.SetParent(null);
        }
    }

    private void Update()
    {
        if (rb == null) return;

        bool isGrounded = CheckGrounded();
        float currentSpeed = rb.linearVelocity.magnitude;
        bool isMoving = currentSpeed > minSpeedForEffects;

        bool shouldEmit = isGrounded && isMoving;

        // Handle Tire Tracks
        if (tireTracks != null)
        {
            // Keep emitting as long as we are grounded to prevent broken trail segments
            tireTracks.emitting = isGrounded;
        }

        // Handle Dust Particles
        if (dustParticles != null)
        {
            var emission = dustParticles.emission;
            if (shouldEmit)
            {
                emission.rateOverDistance = Mathf.Clamp(currentSpeed * 2f, 2f, 10f); // More speed = more dust
            }
            else
            {
                // Stop emitting when not moving or not grounded
                emission.rateOverDistance = 0f;
            }
        }
    }

    private void LateUpdate()
    {
        // Update positions without inheriting rotation
        if (visualBody != null)
        {
            if (tireTracksTransform != null)
            {
                tireTracksTransform.position = visualBody.TransformPoint(tireTracksLocalPos);
            }
            if (dustTransform != null)
            {
                dustTransform.position = visualBody.TransformPoint(dustLocalPos);
            }
        }
    }

    private bool CheckGrounded()
    {
        // Raycast down from the center of the physics body to detect ground
        // Adding a slight offset up to ensure the raycast starts inside the collider
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        
        return Physics.Raycast(origin, Vector3.down, raycastDistance, groundLayer);
    }
}