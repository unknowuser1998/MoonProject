using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MoonTerrainGenerator))]
public class MoonTerrainGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MoonTerrainGenerator generator = (MoonTerrainGenerator)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Terrain Operations", EditorStyles.boldLabel);

        if (GUILayout.Button("1. Backup Terrain to RAM"))
        {
            generator.BackupTerrain();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("2. Apply Perlin Noise (Unevenness)"))
        {
            Undo.RegisterCompleteObjectUndo(generator.GetComponent<Terrain>().terrainData, "Apply Perlin Noise");
            generator.ApplyPerlinNoise();
        }

        if (GUILayout.Button("3. Generate Craters"))
        {
            Undo.RegisterCompleteObjectUndo(generator.GetComponent<Terrain>().terrainData, "Generate Craters");
            generator.GenerateCraters();
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("If you don't like the result, you can restore from RAM or use Edit -> Undo (Ctrl+Z).", MessageType.Info);

        if (GUILayout.Button("Restore Terrain from Backup"))
        {
            Undo.RegisterCompleteObjectUndo(generator.GetComponent<Terrain>().terrainData, "Restore Terrain");
            generator.RestoreTerrain();
        }
    }
}
