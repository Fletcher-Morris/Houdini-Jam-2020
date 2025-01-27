using Quality;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Compute Grass", menuName = "Scriptables/Grass/Compute Grass Settings")]
public class ComputeGrassSettings : ScriptableObject, IUpdateQuality
{
    public ComputeGrassSettingsData SettingsData;

    private void OnValidate()
    {
        SettingsData.maxCameraDist = SettingsData.maxCameraDist.Clamp(SettingsData.minCamDist, float.MaxValue);
        UpdateChecksum();
    }

    public void UpdateChecksum()
    {
        SettingsData.SetChecksum(SettingsData.Checksum);
    }

    void IUpdateQuality.UpdateQualitySettings(UserQualitySettings settings)
    {
        SettingsData.grassPerVertex = settings.QualitySettings.GrassPerVert;

        UpdateChecksum();
    }
}

[Serializable]
public struct ComputeGrassSettingsData
{
    [Min(0)] public float grassHeight;
    [Min(0)] public float grassHeightRandom;
    [Min(0)] public float grassHeightCuttoff;
    [Min(0)] public float grassWidth;
    [Min(0)] public float grassWidthRandom;
    [Range(0, 1)] public float grassBendRandom;
    [Range(1, 4)] public int grassSegments;
    [Range(1, 32)] public int grassPerVertex;
    [Min(0)] public float randomPosition;
    [Min(0)] public float maxCameraDist;
    [Min(0)] public float minAltitude;
    [Min(0)] public float maxAltitude;
    [Min(0)] public float altitudeFade;
    [Range(-1, 1)] public float camDotCuttoff;
    [Min(0)] public float minCamDist;
    [Min(0.1f)] public float averagePlanetRadius;
    [Min(0)] public float windSpeed;
    [Min(0)] public float windScale;

    [ReadOnly, SerializeField] private int m_checksum;

    public int Checksum
    {
        get
        {
            int result = 0;
            result += (grassHeight * 7550).RoundToInt();
            result += (grassHeightCuttoff * 1044).RoundToInt();
            result += (grassWidth * 3589).RoundToInt();
            result += (grassHeightRandom * 2954).RoundToInt();
            result += (grassWidthRandom * 1232).RoundToInt();
            result += (grassBendRandom * 2845).RoundToInt();
            result += grassSegments * 242;
            result += grassPerVertex * 2942;
            result += (randomPosition * 846).RoundToInt();
            result += (maxCameraDist * 3253).RoundToInt();
            result += (minAltitude * 4593).RoundToInt();
            result += (maxAltitude * 8552).RoundToInt();
            result += (altitudeFade * 2844).RoundToInt();
            result += (camDotCuttoff * 2723).RoundToInt();
            result += (minCamDist * 3951).RoundToInt();
            result += (averagePlanetRadius * 8354).RoundToInt();
            result += (windSpeed * 723).RoundToInt();
            result += (windScale * 542).RoundToInt();
            return result;
        }
    }

    public void SetChecksum(int _value)
    {
        m_checksum = _value;
    }
}