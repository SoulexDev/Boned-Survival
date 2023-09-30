using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WorldGenerator : MonoBehaviour
{
    public static WorldGenerator Instance;

    [SerializeField] public Material mat;
    [SerializeField] public int worldSizeInChunks = 16;
    [SerializeField] public int chunkSize = 8;
    [SerializeField] public AnimationCurve heightCurve;
    [SerializeField] public AnimationCurve oceanCurve;

    [SerializeField] public float worldHeight = 20;
    [SerializeField] public float scale = 0.5f;

    private List<WorldChunk> chunks = new List<WorldChunk>();
    [SerializeField] private int LOD;

    private void Start()
    {
        Instance = this;

        for (int y = 0; y < worldSizeInChunks; y++)
        {
            for (int x = 0; x < worldSizeInChunks; x++)
            {
                WorldChunk chunk = new WorldChunk();
                Vector3 position = new Vector3(x, 0, y);
                chunk.Initialize(position);
                chunks.Add(chunk);
            }
        }
        Generate();
    }
    private void OnEnable()
    {
        LODManager.OnUpdateLOD += LODManager_OnUpdateLOD;
    }
    private void OnDisable()
    {
        LODManager.OnUpdateLOD -= LODManager_OnUpdateLOD;
    }

    private void LODManager_OnUpdateLOD()
    {
        Generate();
    }

    [ContextMenu("Generate World")]
    public void Generate()
    {
        for (int i = 0; i < chunks.Count; i++)
        {
            chunks[i].Generate();
        }
    }
}