using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class GravTether : MonoBehaviour
{
    [Header("Tether Origin Settings")]
    [Tooltip("The radar dish or arm on top of the rover that shoots the beam.")]
    public Transform radarDish;
    [Tooltip("The actual point where the beam originates (usually a child of radarDish).")]
    public Transform tetherPoint;
    public float radarDishTurnSpeed = 10f;
    
    [Header("Aiming Settings")]
    public float maxAimDistance = 20f;
    public float maxTetherDistance = 25f;
    public float aimSphereRadius = 2.5f;
    [Tooltip("Layers that can be tethered. Ensure relics have Rigidbodies.")]
    public LayerMask tetherableLayer = ~0;
    public Color highlightColor = new Color(0f, 1f, 1f, 1f); // Cyan highlight

    [Header("Visual Settings")]
    public int lineResolution = 20;
    [Tooltip("How much the magnetic arc bends upwards or downwards")]
    public float curveHeight = 1.5f;
    [Tooltip("Intensity of the energetic wiggle")]
    public float noiseAmount = 0.2f;
    public float noiseSpeed = 5f;

    [Header("Physics Settings")]
    public float springForce = 50f;
    public float springDamper = 10f;
    public float winchScrollSpeed = 2f;
    public float minWinchLength = 2f;
    public float maxWinchLength = 15f;
    
    [Header("VFX Settings")]
    [Tooltip("Prefab for dust particle system when dragging objects on the ground")]
    public ParticleSystem dustVfxPrefab;
    [Tooltip("Layer mask for the ground to detect dragging")]
    public LayerMask groundLayer = 1; // Default to 1 (Default layer)

    private LineRenderer lineRenderer;
    private SpringJoint currentJoint;
    private Rigidbody tetheredBody;
    private Rigidbody roverRigidbody;
    private Camera mainCamera;

    private bool isTethering = false;
    
    // Highlighting state
    private Renderer currentlyHighlightedRenderer;
    private MaterialPropertyBlock propBlock;
    
    // VFX state
    private ParticleSystem currentDustVfx;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
        lineRenderer.positionCount = lineResolution;
        lineRenderer.useWorldSpace = true;
        
        roverRigidbody = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        propBlock = new MaterialPropertyBlock();

        // Auto-create a tether point if none assigned
        if (tetherPoint == null)
        {
            GameObject tp = new GameObject("TetherPoint_Arm");
            if (radarDish != null) tp.transform.SetParent(radarDish);
            else tp.transform.SetParent(transform);
            tp.transform.localPosition = new Vector3(0, 0, 0.5f);
            tetherPoint = tp.transform;
        }
    }

    private void Update()
    {
        if (Keyboard.current == null || Mouse.current == null || mainCamera == null) return;

        bool holdingRightClick = Mouse.current.rightButton.isPressed;
        
        // Aiming Logic
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 lookTarget = ray.GetPoint(maxAimDistance);
        
        if (holdingRightClick)
        {
            if (!isTethering)
            {
                TryStartTether(ray, ref lookTarget);
            }
            else
            {
                UpdateTetherLogic();
                HandleWinchInput();
                lookTarget = tetheredBody.position;
            }
        }
        else
        {
            if (isTethering)
            {
                BreakTether();
            }
            else
            {
                // Highlight logic when just looking around (not holding right click)
                HandleHighlighting(ray, ref lookTarget);
            }
            
            if (lineRenderer.enabled)
            {
                lineRenderer.enabled = false;
            }
        }

        // Radar Dish visual rotation
        UpdateRadarDishRotation(lookTarget);
        
        // Update VFX
        UpdateDragVFX();
    }

    private void TryStartTether(Ray ray, ref Vector3 lookTarget)
    {
        // Try SphereCast for generous aiming
        if (Physics.SphereCast(ray, aimSphereRadius, out RaycastHit hit, maxAimDistance, tetherableLayer))
        {
            Rigidbody targetRb = hit.collider.attachedRigidbody;
            if (targetRb != null && targetRb != roverRigidbody)
            {
                // Start tethering
                isTethering = true;
                tetheredBody = targetRb;

                // Create SpringJoint on the Rover
                currentJoint = gameObject.AddComponent<SpringJoint>();
                currentJoint.connectedBody = tetheredBody;
                
                // Keep the anchor at the center of the sphere so rolling doesn't yank the relic around
                currentJoint.autoConfigureConnectedAnchor = false;
                currentJoint.anchor = Vector3.zero; 
                currentJoint.connectedAnchor = Vector3.zero;

                // Configure physics
                currentJoint.spring = springForce;
                currentJoint.damper = springDamper;
                
                // Initial length based on current distance
                float initialDist = Vector3.Distance(transform.position, tetheredBody.position);
                currentJoint.maxDistance = Mathf.Clamp(initialDist, minWinchLength, maxWinchLength);
                currentJoint.minDistance = Mathf.Max(0f, currentJoint.maxDistance - 2f);

                lineRenderer.enabled = true;
                lookTarget = tetheredBody.position;
                
                // Keep it highlighted while tethered
                ApplyHighlight(tetheredBody.GetComponentInChildren<Renderer>());
            }
        }
        else
        {
            // Just looking around, show aiming laser
            if (Physics.Raycast(ray, out hit, maxAimDistance))
            {
                lookTarget = hit.point;
            }
            UpdateAimingLaser(lookTarget);
            RemoveHighlight();
        }
    }

    private void HandleHighlighting(Ray ray, ref Vector3 lookTarget)
    {
        if (Physics.SphereCast(ray, aimSphereRadius, out RaycastHit hit, maxAimDistance, tetherableLayer))
        {
            Rigidbody targetRb = hit.collider.attachedRigidbody;
            if (targetRb != null && targetRb != roverRigidbody)
            {
                ApplyHighlight(targetRb.GetComponentInChildren<Renderer>());
                lookTarget = targetRb.position;
                return;
            }
        }
        
        RemoveHighlight();
    }

    private void ApplyHighlight(Renderer renderer)
    {
        if (renderer == null) return;
        if (currentlyHighlightedRenderer == renderer) return; // Already highlighted
        
        RemoveHighlight(); // Remove previous if any
        
        currentlyHighlightedRenderer = renderer;
        renderer.GetPropertyBlock(propBlock);
        
        // Use MaterialPropertyBlock to add emission safely without instantiating materials
        propBlock.SetColor("_EmissionColor", highlightColor * 2f); 
        propBlock.SetColor("_Color", highlightColor); // Fallback for standard/URP BaseColor
        propBlock.SetColor("_BaseColor", highlightColor); 
        
        renderer.SetPropertyBlock(propBlock);
    }

    private void RemoveHighlight()
    {
        if (currentlyHighlightedRenderer != null)
        {
            // Remove the property block override, returning the material to its original state
            currentlyHighlightedRenderer.SetPropertyBlock(null); 
            currentlyHighlightedRenderer = null;
        }
    }

    private void HandleWinchInput()
    {
        if (currentJoint == null) return;
        
        float scrollAmount = Mouse.current.scroll.ReadValue().y;
        if (scrollAmount != 0)
        {
            // scroll.y is usually a large number (like 120 or -120), so we normalize the sign
            float scrollDir = Mathf.Sign(scrollAmount);
            // Scrolling up (positive) = Reel IN (reduce distance)
            // Scrolling down (negative) = Reel OUT (increase distance)
            float newDistance = currentJoint.maxDistance - (scrollDir * winchScrollSpeed);
            
            currentJoint.maxDistance = Mathf.Clamp(newDistance, minWinchLength, maxWinchLength);
            currentJoint.minDistance = Mathf.Max(0f, currentJoint.maxDistance - 2f);
        }
    }

    private void DrawCurvedTether(Vector3 start, Vector3 end)
    {
        if (lineRenderer.positionCount != lineResolution) 
            lineRenderer.positionCount = lineResolution;

        float distance = Vector3.Distance(start, end);
        
        // Midpoint
        Vector3 mid = Vector3.Lerp(start, end, 0.5f);
        
        // Control point: offset upwards to create a sagging or arcing magnetic line
        float time = Time.time * noiseSpeed;
        Vector3 noiseOffset = new Vector3(Mathf.Sin(time), Mathf.Cos(time * 1.3f), Mathf.Sin(time * 0.7f)) * noiseAmount;
        Vector3 controlPoint = mid + Vector3.up * (curveHeight * distance * 0.1f) + noiseOffset;
        
        for (int i = 0; i < lineResolution; i++)
        {
            float t = i / (float)(lineResolution - 1);
            Vector3 pos = CalculateBezierPoint(t, start, controlPoint, end);
            lineRenderer.SetPosition(i, pos);
        }
    }

    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector3 p = uu * p0; // (1-t)^2 * P0
        p += 2 * u * t * p1; // 2(1-t)t * P1
        p += tt * p2;        // t^2 * P2
        return p;
    }

    private void UpdateAimingLaser(Vector3 targetPoint)
    {
        lineRenderer.enabled = true;
        Vector3 start = tetherPoint != null ? tetherPoint.position : transform.position;
        DrawCurvedTether(start, targetPoint);
    }

    private void UpdateTetherLogic()
    {
        if (tetheredBody == null || currentJoint == null)
        {
            BreakTether();
            return;
        }

        // Distance check (Anti-Frustration snapping)
        float distance = Vector3.Distance(transform.position, tetheredBody.position);
        if (distance > maxTetherDistance)
        {
            BreakTether();
            return;
        }

        // Update visuals
        Vector3 start = tetherPoint != null ? tetherPoint.position : transform.position;
        DrawCurvedTether(start, tetheredBody.position);
    }

    private void UpdateRadarDishRotation(Vector3 targetPoint)
    {
        if (radarDish == null) return;

        Vector3 direction = targetPoint - radarDish.position;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, transform.up); // Ensure it respects up vector
            radarDish.rotation = Quaternion.Slerp(radarDish.rotation, targetRotation, Time.deltaTime * radarDishTurnSpeed);
        }
    }

    private void UpdateDragVFX()
    {
        if (dustVfxPrefab == null) return;

        if (isTethering && tetheredBody != null)
        {
            // Check if moving fast enough
            if (tetheredBody.linearVelocity.magnitude > 0.5f)
            {
                // Check if touching ground (raycast down from relic center)
                if (Physics.Raycast(tetheredBody.position, Vector3.down, out RaycastHit hit, 2f, groundLayer))
                {
                    if (currentDustVfx == null)
                    {
                        // Spawn VFX at the contact point
                        currentDustVfx = Instantiate(dustVfxPrefab, hit.point, Quaternion.identity);
                    }
                    else
                    {
                        // Move existing VFX to current ground point
                        currentDustVfx.transform.position = hit.point;
                        if (!currentDustVfx.isPlaying) currentDustVfx.Play();
                    }
                    return; // VFX is active, don't stop it
                }
            }
        }

        // If we reach here, we shouldn't show VFX
        if (currentDustVfx != null && currentDustVfx.isPlaying)
        {
            currentDustVfx.Stop();
            // Let it fade out naturally, destroy after a few seconds
            Destroy(currentDustVfx.gameObject, 3f); 
            currentDustVfx = null;
        }
    }

    private void BreakTether()
    {
        isTethering = false;
        tetheredBody = null;
        RemoveHighlight();
        
        if (currentJoint != null)
        {
            Destroy(currentJoint);
            currentJoint = null;
        }

        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }
}
