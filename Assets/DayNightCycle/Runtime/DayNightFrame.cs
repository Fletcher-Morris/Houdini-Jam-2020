using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Time/Day Night Frame", fileName = "New Day Night Frame")]
public class DayNightFrame : ScriptableObject
{
    static int SkyColorHash = Shader.PropertyToID("SKY_COLOR");
    static int HorizonColorHash = Shader.PropertyToID("HORIZON_COLOR");
    static int LightColorHash = Shader.PropertyToID("LIGHT_COLOR");
    static int ShadowColorHash = Shader.PropertyToID("SHADOW_COLOR");

    [System.Serializable]
    public struct DayNightFrameData
    {
        public Color SkyColor;
        public Color HorizonColor;
        public Color LightColor;
        public Color ShadowColor;

        public DayNightFrameData(bool v)
        {
            SkyColor = Color.blue;
            HorizonColor = Color.blue;
            LightColor = Color.white;
            ShadowColor = Color.grey;
        }

        public DayNightFrameData(DayNightFrameData a, DayNightFrameData b, float t)
        {
            SkyColor = Color.Lerp(a.SkyColor, b.SkyColor, t);
            HorizonColor = Color.Lerp(a.HorizonColor, b.HorizonColor, t);
            LightColor = Color.Lerp(a.LightColor, b.LightColor, t);
            ShadowColor = Color.Lerp(a.ShadowColor, b.ShadowColor, t);
        }

        public void ApplyDataToWorld()
        {
            Shader.SetGlobalColor(SkyColorHash, SkyColor);
            Shader.SetGlobalColor(HorizonColorHash, HorizonColor);
            Shader.SetGlobalColor(LightColorHash, LightColor);
            Shader.SetGlobalColor(ShadowColorHash, ShadowColor);
        }
    }

    [SerializeField] private DayNightFrameData _frameData = new DayNightFrameData(true);

    public DayNightFrameData FrameData { get => _frameData; set => _frameData = value; }
}
