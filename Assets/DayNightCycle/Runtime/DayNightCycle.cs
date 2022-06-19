using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Time/Day Night Cycle", fileName = "New Day Night Cycle")]
public class DayNightCycle : SerializedScriptableObject
{
    [System.Serializable]
    private class DayNightCycleFrameLength
    {
        public DayNightFrame Frame;
        [Range(0, 1)] public float Length;
        [Range(0, 1)] public float StartTime;
        [Range(0, 1)] public float MiddleTime;
        [Range(0, 1)] public float EndTime;
        public int ListIndex;
    }

    [SerializeField] private ushort _ticksPerCycle = 10000;
    [SerializeField] private List<DayNightCycleFrameLength> _frames = new List<DayNightCycleFrameLength>();
    [SerializeField] private bool _alwaysNormaliseFrameTimes = false;
    [Button]
    private void NormaliseFrameTimes()
    {
        float total = 0;
        _frames.ForEach(frame => total += frame.Length);
        float multiplier = 1.0f / total;
        _frames.ForEach(frame => frame.Length *= multiplier);

        float totalTime = 0;
        _frames.ForEach(frame =>
        {
            frame.StartTime = totalTime;
            totalTime += frame.Length;
            frame.EndTime = totalTime;
            frame.MiddleTime = Mathf.Lerp(frame.StartTime, frame.EndTime, 0.5f);
        });
    }

    [Space(50.0f)]
    [Range(0, 10000), SerializeField] private ushort _testLerp = 5000;
    [SerializeField] private DayNightFrame.DayNightFrameData _lerpedData;

    public ushort TicksPerCycle { get => _ticksPerCycle; }
    private List<DayNightCycleFrameLength> Frames { get => _frames; set => _frames = value; }

    private void OnValidate()
    {
        if (_alwaysNormaliseFrameTimes)
        {
            NormaliseFrameTimes();
        }

        for(int i = 0; i < _frames.Count; i++)
        {
            _frames[i].ListIndex = i;
        }

        _lerpedData = LerpedDayNightFrameDataForTick(_testLerp);
    }

    private DayNightCycleFrameLength DayNightFrameLengthForTick(ushort tick)
    {
        return _frames.Find(frame => tick >= frame.StartTime * _ticksPerCycle && tick <= frame.EndTime * _ticksPerCycle);
    }

    private DayNightCycleFrameLength GetPreviousFrameLength(DayNightCycleFrameLength frameLength)
    {
        if (frameLength.ListIndex <= 0) return _frames[_frames.Count - 1];
        else return _frames[frameLength.ListIndex - 1];
    }

    private DayNightCycleFrameLength GetNextFrameLength(DayNightCycleFrameLength frameLength)
    {
        if (frameLength.ListIndex >= _frames.Count - 1) return _frames[0];
        else return _frames[frameLength.ListIndex + 1];
    }

    public DayNightFrame.DayNightFrameData LerpedDayNightFrameDataForTick(ushort tick)
    {
        DayNightCycleFrameLength frame = DayNightFrameLengthForTick(tick);
        DayNightCycleFrameLength firstFrame = null;
        DayNightCycleFrameLength secondFrame = null;

        if (tick < frame.MiddleTime * _ticksPerCycle)
        {
            firstFrame = GetPreviousFrameLength(frame);
            secondFrame = frame;
        }
        else
        {
            firstFrame = frame;
            secondFrame = GetNextFrameLength(frame);
        }

        float tickLerp = Mathf.InverseLerp(firstFrame.MiddleTime * _ticksPerCycle, secondFrame.MiddleTime * _ticksPerCycle, tick);
        return new(firstFrame.Frame.FrameData, secondFrame.Frame.FrameData, tickLerp);
    }
}
