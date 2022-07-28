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
}
