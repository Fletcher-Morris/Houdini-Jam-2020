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
    private struct DayNightCycleFrameLength
    {
        public DayNightFrame Frame;
        [Range(0, 1)] public float Length;
        [HideInInspector] public float StartTime;
        [HideInInspector] public float MiddleTime;
        [HideInInspector] public float EndTime;
        [HideInInspector] public int ListIndex;
        public AnimationCurve Bias;
    }

    [SerializeField, Min(1)] private int _ticksPerCycle = 1000;
    [SerializeField] private List<DayNightCycleFrameLength> _frames = new List<DayNightCycleFrameLength>();
    [SerializeField] private List<DayNightCycleFrameLength> _loopedFrames = new List<DayNightCycleFrameLength>();
    [SerializeField] private bool _alwaysNormaliseFrameTimes = false;
    [Button]
    private void NormaliseFrameTimes()
    {
        float total = 0;
        for (int i = 0; i < _frames.Count; i++)
        {
            total += _frames[i].Length;
        }
        float multiplier = 1.0f / total;
        for (int i = 0; i < _frames.Count; i++)
        {
            DayNightCycleFrameLength frame = _frames[i];
            frame.Length *= multiplier;
            _frames[i] = frame;
        }

        float totalTime = 0;
        for (int i = 0; i < _frames.Count; i++)
        {
            DayNightCycleFrameLength frame = _frames[i];
            frame.StartTime = totalTime;
            totalTime += frame.Length;
            frame.EndTime = totalTime;
            frame.MiddleTime = Mathf.Lerp(frame.StartTime, frame.EndTime, 0.5f);
            _frames[i] = frame;
        }

        Debug.Log("Normalised Frame Times");
    }

    [Button]
    private void LoopFrames()
    {
        _loopedFrames.Clear();
        for (int loop = 0; loop < 3; loop++)
        {
            for (int i = 0; i < _frames.Count; i++)
            {
                DayNightCycleFrameLength frame = _frames[i];
                DayNightCycleFrameLength loopFrame = frame;
                loopFrame.StartTime += (float)loop;
                loopFrame.MiddleTime += (float)loop;
                loopFrame.EndTime += (float)loop;
                _loopedFrames.Add(loopFrame);
            }
        }
        for (int i = 0; i < _loopedFrames.Count; i++)
        {
            DayNightCycleFrameLength loopFrame = _loopedFrames[i];
            loopFrame.ListIndex = i;
            _loopedFrames[i] = loopFrame;
        }
    }

    [Space(50.0f)]
    [Range(0, 1000), SerializeField] private int _testLerp = 0;
    [SerializeField] private bool _useBias;
    [SerializeField] private DayNightFrame.DayNightFrameData _lerpedData;

    [SerializeField] private Texture2D _lerpedTexture;
    [Button]
    private void GenerateSkyGradient()
    {
        if (_lerpedTexture == null || _lerpedTexture.width != _ticksPerCycle || _lerpedTexture.height != _ticksPerCycle / 8) ;
        {
            _lerpedTexture = new Texture2D(_ticksPerCycle, _ticksPerCycle / 8);
        }

        for (int x = 0; x < _lerpedTexture.width; x++)
        {
            _lerpedData = LerpedDayNightFrameDataForTick(_ticksPerCycle / _lerpedTexture.width * x);
            for (int y = 0; y < _lerpedTexture.height; y++)
            {
                _lerpedTexture.SetPixel(x, y, _lerpedData.SkyColor);
            }
        }
        _lerpedTexture.Apply();
    }

    [SerializeField] private bool _onValidate;
    private void OnValidate()
    {
        if (!_onValidate) return;
        _lerpedData = LerpedDayNightFrameDataForTick(_testLerp);
    }

    private DayNightCycleFrameLength DayNightFrameLengthForTick(int tick)
    {
        if (_loopedFrames.Count == 0) return default;
        return _loopedFrames.Find(frame => tick >= frame.StartTime * _ticksPerCycle && tick <= frame.EndTime * _ticksPerCycle);
    }

    private DayNightCycleFrameLength GetPreviousFrameLength(DayNightCycleFrameLength frameLength)
    {
        if (_loopedFrames.Count == 0) return default;
        if (frameLength.ListIndex <= 0) return _loopedFrames[_frames.Count - 1];
        else return _loopedFrames[frameLength.ListIndex - 1];
    }

    private DayNightCycleFrameLength GetNextFrameLength(DayNightCycleFrameLength frameLength)
    {
        if (_loopedFrames.Count == 0) return default;
        if (frameLength.ListIndex >= _loopedFrames.Count - 1) return _frames[0];
        else return _loopedFrames[frameLength.ListIndex + 1];
    }

    public DayNightFrame.DayNightFrameData LerpedDayNightFrameDataForTick(int tick)
    {
        if (_loopedFrames == null) return default;
        if (_loopedFrames.Count == 0) return default;

        if (tick <= 0) tick = 0;
        else if (tick >= _ticksPerCycle) tick = _ticksPerCycle;

        tick += _ticksPerCycle;

        DayNightCycleFrameLength frame = DayNightFrameLengthForTick(tick);
        DayNightCycleFrameLength firstFrame = default;
        DayNightCycleFrameLength secondFrame = default;

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

        float firstVal = (firstFrame.MiddleTime) * _ticksPerCycle;
        float secondVal = (secondFrame.MiddleTime) * _ticksPerCycle;
        float lerpVal = Mathf.InverseLerp(firstVal, secondVal, tick);

        if (_useBias)
        {
            lerpVal = secondFrame.Bias.Evaluate(lerpVal);
        }

        return new(firstFrame.Frame.FrameData, secondFrame.Frame.FrameData, lerpVal);
    }
}
