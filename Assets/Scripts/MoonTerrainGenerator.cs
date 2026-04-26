using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class MoonTerrainGenerator : MonoBehaviour
{
    [Header("Perlin Noise Settings (Unevenness)")]
    public float noiseScale = 0.02f;
    [Tooltip("Height variation in meters")]
    public float noiseHeight = 2f; 
    [Tooltip("How many layers of noise (fractal)")]
    public int noiseOctaves = 3;

    [Header("Craters Settings")]
    public int craterCount = 50;
    public float minCraterRadius = 10f;
    public float maxCraterRadius = 40f;
    [Tooltip("How deep the crater is in meters (scaled by the curve)")]
    public float craterDepth = 5f; 
    
    [Tooltip("Crater cross-section from center (0) to outer edge (1). Y<0 is hole, Y>0 is rim.")]
    public AnimationCurve craterShape = new AnimationCurve(
        new Keyframe(0f, -1f),      // Deepest at center
        new Keyframe(0.7f, 0f),     // Ground level inside
        new Keyframe(0.85f, 0.3f),  // Rim peak
        new Keyframe(1f, 0f)        // Back to ground
    );

    // To store backup in RAM
    private float[,] backupHeights;

    public void BackupTerrain()
    {
        TerrainData td = GetComponent<Terrain>().terrainData;
        backupHeights = td.GetHeights(0, 0, td.heightmapResolution, td.heightmapResolution);
        Debug.Log("Terrain backed up to RAM. (Note: Will be lost if Unity restarts)");
    }

    public void RestoreTerrain()
    {
        if (backupHeights != null)
        {
            TerrainData td = GetComponent<Terrain>().terrainData;
            td.SetHeights(0, 0, backupHeights);
            Debug.Log("Terrain restored from RAM.");
        }
        else
        {
            Debug.LogWarning("No backup found in RAM. Please backup first.");
        }
    }

    public void ApplyPerlinNoise()
    {
        TerrainData td = GetComponent<Terrain>().terrainData;
        int res = td.heightmapResolution;
        float[,] heights = td.GetHeights(0, 0, res, res);

        float[] offsetsX = new float[noiseOctaves];
        float[] offsetsY = new float[noiseOctaves];
        for (int i = 0; i < noiseOctaves; i++)
        {
            offsetsX[i] = Random.Range(-10000f, 10000f);
            offsetsY[i] = Random.Range(-10000f, 10000f);
        }

        // Convert noiseHeight from meters to 0-1 scale used by Unity Terrain
        float heightScale = noiseHeight / td.size.y;

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float amplitude = 1f;
                float frequency = 1f;
                float noiseValue = 0f;

                for (int i = 0; i < noiseOctaves; i++)
                {
                    float xCoord = (x * noiseScale * frequency) + offsetsX[i];
                    float yCoord = (y * noiseScale * frequency) + offsetsY[i];

                    // PerlinNoise returns 0 to 1. Shift to -0.5 to 0.5 to not always raise terrain
                    float perlin = Mathf.PerlinNoise(xCoord, yCoord) - 0.5f;
                    noiseValue += perlin * amplitude;

                    amplitude *= 0.5f;
                    frequency *= 2f;
                }

                heights[y, x] += noiseValue * heightScale;
            }
        }
        
        td.SetHeights(0, 0, heights);
        Debug.Log("Applied Perlin Noise to terrain.");
    }

    public void GenerateCraters()
    {
        TerrainData td = GetComponent<Terrain>().terrainData;
        int res = td.heightmapResolution;
        float[,] heights = td.GetHeights(0, 0, res, res);
        
        Vector3 size = td.size;
        float depthInMap = craterDepth / size.y;

        for (int i = 0; i < craterCount; i++)
        {
            // Random center in heightmap coordinates
            float rx = Random.Range(0f, res);
            float ry = Random.Range(0f, res);
            
            // Random radius in world meters
            float radius = Random.Range(minCraterRadius, maxCraterRadius);
            
            // Convert world radius to heightmap coordinate radius
            float radiusInMap = radius / size.x * res;
            
            int minX = Mathf.FloorToInt(Mathf.Max(0, rx - radiusInMap));
            int maxX = Mathf.CeilToInt(Mathf.Min(res - 1, rx + radiusInMap));
            int minY = Mathf.FloorToInt(Mathf.Max(0, ry - radiusInMap));
            int maxY = Mathf.CeilToInt(Mathf.Min(res - 1, ry + radiusInMap));

            // Optional: Randomize depth slightly per crater
            float currentDepth = depthInMap * Random.Range(0.8f, 1.2f);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(rx, ry));
                    float normalizedDist = dist / radiusInMap;
                    
                    if (normalizedDist <= 1f)
                    {
                        float curveVal = craterShape.Evaluate(normalizedDist);
                        heights[y, x] += curveVal * currentDepth;
                    }
                }
            }
        }
        
        td.SetHeights(0, 0, heights);
        Debug.Log($"Generated {craterCount} Craters on terrain.");
    }
}
