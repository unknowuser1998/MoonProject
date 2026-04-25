using UnityEngine;
using UnityEngine.InputSystem;

public class RoverExcavator : MonoBehaviour
{
    [Header("Tractor Beam Settings")]
    [Tooltip("Visual representation of the tractor beam (e.g., a glowing cylinder)")]
    public GameObject tractorBeamVisual;
    public float extractionRadius = 4f;
    public LayerMask relicLayer = ~0; // Default to all layers for easy prototyping

    private void Start()
    {
        if (tractorBeamVisual != null)
        {
            tractorBeamVisual.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        bool isPressingE = false;
        
        // Use New Input System
        if (Keyboard.current != null)
        {
            isPressingE = Keyboard.current.eKey.isPressed;
        }

        if (isPressingE)
        {
            if (tractorBeamVisual != null)
            {
                if (!tractorBeamVisual.activeSelf)
                {
                    tractorBeamVisual.SetActive(true);
                }
                
                // Force the beam to stay upright and point straight down from the rover center,
                // regardless of how the rover sphere rolls physically.
                tractorBeamVisual.transform.position = transform.position + (Vector3.down * 1f);
                tractorBeamVisual.transform.rotation = Quaternion.identity;
            }

            // Look for any relics in range
            Collider[] hits = Physics.OverlapSphere(transform.position, extractionRadius, relicLayer);
            foreach (var hit in hits)
            {
                RelicMound relic = hit.GetComponentInParent<RelicMound>();
                if (relic != null)
                {
                    relic.StartExtraction();
                }
            }
        }
        else
        {
            if (tractorBeamVisual != null && tractorBeamVisual.activeSelf)
            {
                tractorBeamVisual.SetActive(false);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, extractionRadius);
    }
}
