using Quality;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Tick;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUi : MonoBehaviour, IManualUpdate
{
    [SerializeField, Required] private UpdateManager _updateManager;
    [SerializeField, Required] private QualitySettingsManager _qualityManager;

    [Space, Header("Quality")]
    [SerializeField] private Button _lowQButton;
    [SerializeField] private Button _medQButton;
    [SerializeField] private Button _highQButton;
    [SerializeField] private Button _grassToggleButton;
    [SerializeField] private GrassComputeController _grassComputeController;

    [Space, Header("Time")]
    [SerializeField] private Slider _timeSlider;
    [SerializeField] private Toggle _timeToggle;
    [SerializeField] private DayNightCycle _dayNightCycle;

    [Space, Header("Performance")]
    [SerializeField] private TMP_Text _fpsText;
    private int _frameCounter = 0;
    private float _frameTimer = 0;

    [Space, Header("Pathing")]
    [SerializeField] private Button _showPathsButton;

    private void Start()
    {
        _updateManager.AddToUpdateList(this);
    }

    private void SetQuality(int level)
    {
        _qualityManager.ApplySettings(level);
    }

    private void ToggleShowPaths()
    {
        Pathing.WaypointManager.Instance.ShowNavigatorPaths = !Pathing.WaypointManager.Instance.ShowNavigatorPaths;
    }

    private void ToggleComputeGrass()
    {
        _grassComputeController.ToggleGrass();
    }

    private void ToggleTime(bool val)
    {
        _dayNightCycle.AutoTick = val;
    }

    private void OnTimeSliderChanged(float val)
    {
        _dayNightCycle.SetTick(val.RoundToInt());
    }

    bool IManualUpdate.OnInitialise()
    {
        _lowQButton.onClick.AddListener(() => SetQuality(0));
        _medQButton.onClick.AddListener(() => SetQuality(1));
        _highQButton.onClick.AddListener(() => SetQuality(2));
        _grassToggleButton.onClick.AddListener(() => ToggleComputeGrass());
        _showPathsButton.onClick.AddListener(ToggleShowPaths);

        _timeSlider.maxValue = _dayNightCycle.TicksPerCycle;
        _timeSlider.onValueChanged.AddListener(OnTimeSliderChanged);
        _timeToggle.onValueChanged.AddListener(ToggleTime);

        return true;
    }

    UpdateManager IManualUpdate.GetUpdateManager()
    {
        return _updateManager;
    }


    void IManualUpdate.OnManualUpdate(float delta)
    {
        if(_frameCounter < 30)
        {
            _frameCounter++;
            _frameTimer += delta;
        }
        else
        {
            _fpsText.text = (_frameCounter / _frameTimer).RoundToInt().ToString() + " FPS";
            _frameCounter = 0;
            _frameTimer = 0;
        }
    }

    void IManualUpdate.OnTick(float delta)
    {
    }

    void IManualUpdate.OnManualFixedUpdate(float delta)
    {
    }

    bool IManualUpdate.IsEnabled()
    {
        return true;
    }

    void IManualUpdate.OnApplicationQuit()
    {
    }
}
