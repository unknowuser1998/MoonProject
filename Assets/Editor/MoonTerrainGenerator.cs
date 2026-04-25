using UnityEngine;
using UnityEditor;
using System.IO;

public class MoonTerrainGenerator : EditorWindow
{
    [MenuItem("ChillRover/Generate Lofi Moon Terrain")]
    public static void GenerateTerrain()
    {
        // 1. URP MATERIAL CREATION
        string matFolder = "Assets/Materials";
        if (!AssetDatabase.IsValidFolder(matFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Materials");
        }

        string matPath = matFolder + "/Mat_LofiMoon.mat";
        Material lofiMoonMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

        if (lofiMoonMat == null)
        {
            lofiMoonMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(lofiMoonMat, matPath);
        }

        Color baseColor;
        ColorUtility.TryParseHtmlString("#1A1A2E", out baseColor);
        lofiMoonMat.SetColor("_BaseColor", baseColor);
        lofiMoonMat.SetFloat("_Smoothness", 0.15f);
        lofiMoonMat.SetFloat("_Metallic", 0.0f);
        
        // Enable specular highlights (usually on by default, but let's ensure it)
        lofiMoonMat.SetFloat("_SpecularHighlights", 1f); 
        
        EditorUtility.SetDirty(lofiMoonMat);
        AssetDatabase.SaveAssets();

        // 2. CLEANUP & TERRAIN CREATION
        GameObject existingTerrain = GameObject.Find("StylizedMoonTerrain");
        if (existingTerrain != null)
        {
            DestroyImmediate(existingTerrain);
        }

        // Create TerrainData
        TerrainData terrainData = new TerrainData();
        terrainData.heightmapResolution = 513; // Must be power of 2 + 1
        terrainData.size = new Vector3(500, 50, 500);

        // 3. PROCEDURAL "SKATEPARK" GENERATION
        int res = terrainData.heightmapResolution;
        float[,] heights = new float[res, res];

        float scale1 = 0.003f; // Wide rolling hills (low frequency)
        float scale2 = 0.015f; // Slight unevenness
        
        float offsetX = Random.Range(0f, 9999f);
        float offsetY = Random.Range(0f, 9999f);

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float xCoord1 = (x + offsetX) * scale1;
                float yCoord1 = (y + offsetY) * scale1;
                float noise1 = Mathf.PerlinNoise(xCoord1, yCoord1);

                float xCoord2 = (x + offsetX) * scale2;
                float yCoord2 = (y + offsetY) * scale2;
                float noise2 = Mathf.PerlinNoise(xCoord2, yCoord2);

                // Combine 2 layers of Perlin noise
                float combinedHeight = (noise1 * 0.85f) + (noise2 * 0.15f);
                
                // Use a power curve to make valleys flatter and hills smoother, like a skatepark
                combinedHeight = Mathf.Pow(combinedHeight, 1.8f);

                // We want to flatten the center area a bit so the rover spawns safely
                float cx = res / 2f;
                float cy = res / 2f;
                float distFromCenter = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy)) / (res / 2f);
                
                // Smoothly blend the height out from the center
                float flattenFactor = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((distFromCenter - 0.05f) * 4f));
                
                // Apply a max amplitude so the hills aren't too crazy
                heights[y, x] = Mathf.Lerp(0.05f, combinedHeight * 0.4f, flattenFactor);
            }
        }

        terrainData.SetHeights(0, 0, heights);

        // Save TerrainData asset
        string terrainDataPath = "Assets/Materials/StylizedMoonTerrainData.asset";
        TerrainData existingData = AssetDatabase.LoadAssetAtPath<TerrainData>(terrainDataPath);
        if (existingData != null)
        {
            // If it exists, we can't easily overwrite the asset file without causing reference issues, 
            // so we delete it first or just update it.
            // Let's delete the old one to be safe.
            AssetDatabase.DeleteAsset(terrainDataPath);
        }
        AssetDatabase.CreateAsset(terrainData, terrainDataPath);

        // Create Terrain GameObject
        GameObject terrainGo = Terrain.CreateTerrainGameObject(terrainData);
        terrainGo.name = "StylizedMoonTerrain";
        terrainGo.transform.position = new Vector3(-250, 0, -250);
        
        // Set to Ground layer (assuming Layer 8 based on previous scripts)
        terrainGo.layer = LayerMask.NameToLayer("Ground");
        if (terrainGo.layer == -1) terrainGo.layer = 8;

        Terrain terrain = terrainGo.GetComponent<Terrain>();
        terrain.materialTemplate = lofiMoonMat;

        TerrainCollider col = terrainGo.GetComponent<TerrainCollider>();
        col.terrainData = terrainData;

        Debug.Log("Lofi Moon Terrain Generated Successfully!");
    }
}
