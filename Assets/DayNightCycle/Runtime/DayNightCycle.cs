using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using Tick;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Time/Day Night Cycle", fileName = "New Day Night Cycle")]
public class DayNightCycle : SerializedScriptableObject, IManualUpdate
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

    [SerializeField] private bool _autoTick = true;
    [SerializeField, Min(1)] private int _ticksPerCycle = 1000;
    [SerializeField] private List<DayNightCycleFrameLength> _frames = new List<DayNightCycleFrameLength>();
    [SerializeField, HideInInspector] private List<DayNightCycleFrameLength> _loopedFrames = new List<DayNightCycleFrameLength>();
    private int _currentTick = 0;
    private DayNightFrame.DayNightFrameData _currentData;
    private float _prevLerpVal = -1;
    private float _currentLerpVal = 0;

    [Space(50)]
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
    [SerializeField] private bool _applyTestData;
    [SerializeField] private Texture2D _lerpedTexture;
    [Button]
    private void GenerateSkyGradient()
    {
        int prevTicksCount = _ticksPerCycle;

        _ticksPerCycle = 512;
        int width = _ticksPerCycle;
        int height = _ticksPerCycle / 8;
        int segHeight = height / 4;
        
        _lerpedTexture = new Texture2D(width, height);

        for (int x = 0; x < width; x++)
        {
            _lerpedData = LerpedDayNightFrameDataForTick(width / width * x, false);
            int seg = 0;
            for (int y = segHeight * seg; y < (height / 4) * (seg + 1); y++)
            {
                _lerpedTexture.SetPixel(x, y, _lerpedData.SkyColor);
            }
            seg++;
            for (int y = segHeight * seg; y < (height / 4) * (seg + 1); y++)
            {
                _lerpedTexture.SetPixel(x, y, _lerpedData.HorizonColor);
            }
            seg++;
            for (int y = segHeight * seg; y < (height / 4) * (seg + 1); y++)
            {
                _lerpedTexture.SetPixel(x, y, _lerpedData.LightColor);
            }
            seg++;
            for (int y = segHeight * seg; y < (height / 4) * (seg + 1); y++)
            {
                _lerpedTexture.SetPixel(x, y, _lerpedData.ShadowColor);
            }
        }
        _lerpedTexture.Apply();

        _ticksPerCycle = prevTicksCount;
    }

    [SerializeField] private bool _onValidate;

    public int TicksPerCycle { get => _ticksPerCycle; }
    public bool AutoTick { get => _autoTick; set => _autoTick = value; }

    private void OnValidate()
    {
        if (!_onValidate) return;

        _lerpedData = LerpedDayNightFrameDataForTick(_testLerp, false);

        if(_applyTestData)
        {
            _currentTick = _testLerp;
            Tick();
        }
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

    public DayNightFrame.DayNightFrameData LerpedDayNightFrameDataForTick(int tick, bool setLerpVal)
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

        if (setLerpVal)
        {
            _prevLerpVal = _currentLerpVal;
            _currentLerpVal = lerpVal;
        }

        return new(firstFrame.Frame.FrameData, secondFrame.Frame.FrameData, lerpVal);
    }

    public void SetTick(int newTick)
    {
        _currentTick = newTick;
        Tick();
    }

    public void Tick()
    {
        if(_currentTick >= _ticksPerCycle) _currentTick = 0;
        _currentData = LerpedDayNightFrameDataForTick(_currentTick, true);
        if(!Mathf.Approximately(_prevLerpVal, _currentLerpVal))
        {
            _currentData.ApplyDataToWorld();
        }
        _currentTick++;
    }

    UpdateManager IManualUpdate.GetUpdateManager()
    {
        return GameManager.Instance.UpdateManager;
    }

    void IManualUpdate.OnInitialise()
    {
    }

    void IManualUpdate.OnManualUpdate(float delta)
    {
    }

    void IManualUpdate.OnTick(float delta)
    {
        if (!_autoTick) return;
        Tick();
    }

    void IManualUpdate.OnManualFixedUpdate(float delta)
    {
    }

    bool IManualUpdate.IsEnabled()
    {
        return true;
    }
}
