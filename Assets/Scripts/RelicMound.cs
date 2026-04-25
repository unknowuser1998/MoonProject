using UnityEngine;
using System.Collections;

public class RelicMound : MonoBehaviour
{
    [Header("Extraction Settings")]
    public float extractionHeight = 3f;
    public float extractionDuration = 4f;
    public float rotationSpeed = 90f;
    
    [Header("VFX")]
    [Tooltip("Particle system to hide the ground intersection during extraction")]
    public ParticleSystem dustSwirlVFX;

    private bool isExtracting = false;
    public bool IsExtracted { get; private set; } = false;

    private void Start()
    {
        // Force stop on awake in case the prefab had Play On Awake enabled
        if (dustSwirlVFX != null)
        {
            dustSwirlVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    public void StartExtraction()
    {
        if (isExtracting || IsExtracted) return;
        
        isExtracting = true;
        
        if (dustSwirlVFX != null)
        {
            dustSwirlVFX.Play();
        }

        StartCoroutine(ExtractionRoutine());
    }

    private IEnumerator ExtractionRoutine()
    {
        Vector3 startPos = transform.localPosition;
        Vector3 endPos = startPos + Vector3.up * extractionHeight;
        float elapsed = 0f;

        while (elapsed < extractionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / extractionDuration;
            
            // Smooth step for a nicer, floaty ease-in/ease-out
            t = t * t * (3f - 2f * t);

            transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

            yield return null;
        }

        transform.localPosition = endPos;
        isExtracting = false;
        IsExtracted = true;
        
        if (dustSwirlVFX != null)
        {
            dustSwirlVFX.Stop();
        }
        
        Debug.Log($"[RelicMound] Relic Extracted: {gameObject.name}");
    }
    
    private void Update()
    {
        if (IsExtracted)
        {
            // Keep rotating slowly in the air after extraction
            transform.Rotate(Vector3.up, rotationSpeed * 0.5f * Time.deltaTime, Space.World);
        }
    }
}
