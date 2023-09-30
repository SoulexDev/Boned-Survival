using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseManager : MonoBehaviour
{
    public static NoiseManager Instance;
    FastNoiseLite noise;

    [SerializeField] private FastNoiseLite.NoiseType noiseType;
    [SerializeField] private FastNoiseLite.FractalType fractalType;
    [SerializeField] private int octaves = 3;
    [SerializeField] private float frequency = 0.75f;
    [SerializeField] private float lacunarity = 1;

    private void Awake()
    {
        Instance = this;

        Random.InitState(System.DateTime.Now.Millisecond);

        noise = new FastNoiseLite(Random.Range(int.MinValue, int.MaxValue));

        noise.SetNoiseType(noiseType);
        noise.SetFractalType(fractalType);
        noise.SetFractalOctaves(octaves);
        noise.SetFrequency(frequency);
        noise.SetFractalLacunarity(lacunarity);
    }
    public float GetNoise(float x, float z)
    {
        return (noise.GetNoise(x, z) + 1) / 2;
    }
}