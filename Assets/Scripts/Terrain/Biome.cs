using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Biome
{
    public string name;

    public AnimationCurve biomeHeightMask;
    public float noiseAmplitude, noiseFrequency;
    public Texture2D biomeGroundTexture;

    [Header("Flora")]
    public float floraDensityFrequency, plantFrequency;
    [Range(0.7f, 1f)]
    public float plantThreshold = 1f;
    public GameObject plantPrefab;
}
