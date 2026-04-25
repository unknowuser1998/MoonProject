using UnityEngine;

public class RoverSonar : MonoBehaviour
{
    [Header("Components")]
    public AudioSource sonarSource;
    public AudioClip beepClip;

    [Header("Sonar Settings")]
    public float maxDetectionRange = 100f;
    public float minDistance = 5f;
    public float slowPingInterval = 2.0f;
    public float fastPingInterval = 0.15f;
    [Range(0f, 1f)]
    public float sonarVolume = 0.7f;

    private float pingTimer = 0f;
    private RelicMound[] activeRelics;
    private float relicSearchTimer = 0f;
    private float relicSearchInterval = 1f; // Cache active relics every 1 second

    private void Start()
    {
        if (sonarSource == null)
        {
            sonarSource = gameObject.AddComponent<AudioSource>();
            sonarSource.playOnAwake = false;
            sonarSource.spatialBlend = 1f; // 3D sound from the rover
            sonarSource.rolloffMode = AudioRolloffMode.Linear;
            sonarSource.minDistance = 10f;
            sonarSource.maxDistance = 50f;
        }
    }

    private void Update()
    {
        if (beepClip == null) return;

        relicSearchTimer -= Time.deltaTime;
        if (relicSearchTimer <= 0f)
        {
            relicSearchTimer = relicSearchInterval;
            activeRelics = Object.FindObjectsByType<RelicMound>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        }

        if (activeRelics == null || activeRelics.Length == 0) return;

        // Find closest relic
        float closestDistance = float.MaxValue;
        foreach (var relic in activeRelics)
        {
            if (relic != null && !relic.IsExtracted) // Assuming we don't ping extracted ones
            {
                float dist = Vector3.Distance(transform.position, relic.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                }
            }
        }

        // If no unextracted relics or out of range
        if (closestDistance > maxDetectionRange || closestDistance == float.MaxValue)
        {
            pingTimer = slowPingInterval;
            return;
        }

        // Calculate ping interval
        // Map distance [minDistance, maxDetectionRange] to interval [fastPingInterval, slowPingInterval]
        float t = Mathf.InverseLerp(minDistance, maxDetectionRange, closestDistance);
        float currentInterval = Mathf.Lerp(fastPingInterval, slowPingInterval, t);

        pingTimer -= Time.deltaTime;
        if (pingTimer <= 0f)
        {
            pingTimer = currentInterval;
            if (sonarSource != null)
            {
                sonarSource.PlayOneShot(beepClip, sonarVolume);
                
                // Optional: Slightly increase pitch when very close for more tension/excitement
                sonarSource.pitch = Mathf.Lerp(1.5f, 1.0f, t);
            }
        }
    }
}
