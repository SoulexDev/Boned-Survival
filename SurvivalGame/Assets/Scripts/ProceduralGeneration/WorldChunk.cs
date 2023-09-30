using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class WorldChunk
{
    private WorldGenerator worldGenerator => WorldGenerator.Instance;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider col;
    private Mesh mesh;

    private int[] triangles;
    private Vector3[] vertices;

    public GameObject chunkObj;
    private int previousLOD;
    private Vector3 initPosition;

    public void Initialize(Vector3 position)
    {
        initPosition = position;
        chunkObj = new GameObject($"Chunk {position.x} {position.z}");
        meshFilter = chunkObj.AddComponent<MeshFilter>();
        meshRenderer = chunkObj.AddComponent<MeshRenderer>();
        col = chunkObj.AddComponent<MeshCollider>();

        mesh = new Mesh();
        Material[] mats = new Material[]
        {
            worldGenerator.mat
        };
        meshRenderer.sharedMaterials = mats;

        meshFilter.mesh = mesh;
    }
    public void Generate()
    {
        Vector3 position = initPosition;
        int LOD = GetLOD();

        if (LOD == previousLOD)
            return;
        previousLOD = LOD;

        int newChunkSize = worldGenerator.chunkSize / LOD;

        position *= worldGenerator.chunkSize;

        chunkObj.transform.position = position;

        triangles = new int[newChunkSize * newChunkSize * 6];
        vertices = new Vector3[(newChunkSize + 1) * (newChunkSize + 1)];

        float halfWorldSize = (worldGenerator.worldSizeInChunks * worldGenerator.chunkSize)/2;

        int i = 0;
        for (float z = 0; z <= worldGenerator.chunkSize; z+= LOD)
        {
            for (float x = 0; x <= worldGenerator.chunkSize; x+= LOD)
            {
                float translatedX = x + position.x;
                float translatedZ = z + position.z;

                float worldXCurve = 1 - Mathf.Abs((translatedX - halfWorldSize)/halfWorldSize);
                float worldZCurve = 1 - Mathf.Abs((translatedZ - halfWorldSize)/halfWorldSize);

                float worldCurve = worldGenerator.oceanCurve.Evaluate(Mathf.Min(worldXCurve, worldZCurve));

                float y = worldCurve * worldGenerator.heightCurve.Evaluate(
                    NoiseManager.Instance.GetNoise(translatedX / worldGenerator.scale, translatedZ / worldGenerator.scale)) 
                    * worldGenerator.worldHeight;

                vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }

        int tris = 0;
        int verts = 0;

        for (int z = 0; z < newChunkSize; z++)
        {
            for (int x = 0; x < newChunkSize; x++)
            {
                triangles[tris + 0] = verts + 0;
                triangles[tris + 1] = verts + newChunkSize + 1;
                triangles[tris + 2] = verts + 1;

                triangles[tris + 3] = verts + 1;
                triangles[tris + 4] = verts + newChunkSize + 1;
                triangles[tris + 5] = verts + newChunkSize + 2;

                verts++;
                tris += 6;
            }
            verts++;
        }
        UpdateMesh();
    }
    int GetLOD()
    {
        float distance = (chunkObj.transform.position - LODManager.Instance.transform.position).sqrMagnitude;
        float radiusSquared = LODManager.Instance.LODRadiusInterval * LODManager.Instance.LODRadiusInterval;

        if (distance >= radiusSquared * 3)
        {
            return 8;
        }
        else if (distance >= radiusSquared * 2)
        {
            return 4;
        }
        else if (distance >= radiusSquared)
        {
            return 2;
        }
        else
            return 1;
    }
    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        col.sharedMesh = mesh;
    }
}