using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Time/Day Night Frame", fileName = "New Day Night Frame")]
public class DayNightFrame : ScriptableObject
{
    [System.Serializable]
    public struct DayNightFrameData
    {
        public Color SkyColor;

        public DayNightFrameData(bool v)
        {
            SkyColor = Color.blue;
        }

        public DayNightFrameData(DayNightFrameData a, DayNightFrameData b, float t)
        {
            SkyColor = Color.Lerp(a.SkyColor, b.SkyColor, t);
        }
    }

    [SerializeField] private DayNightFrameData _frameData = new DayNightFrameData(true);

    public DayNightFrameData FrameData { get => _frameData; set => _frameData = value; }
}
