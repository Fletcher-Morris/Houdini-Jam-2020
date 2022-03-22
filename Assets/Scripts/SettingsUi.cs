using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Tick;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUi : MonoBehaviour, Tick.IManualUpdate
{
    [SerializeField, Required] private UpdateManager _updateManager;
    [Space]

    [Header("Quality")]
    [SerializeField] private Button _lowQButton;
    [SerializeField] private Button _medQButton;
    [SerializeField] private Button _highQButton;

    [Space]
    [Header("Performance")]
    [SerializeField] private TMP_Text _fpsText;
    private int _frameCounter = 0;
    private float _frameTimer = 0;

    [Space]
    [Header("Pathing")]
    [SerializeField] private Button _showPathsButton;

    private void Start()
    {
        _updateManager.AddToUpdateList(this);
    }

    private void SetQuality(int level)
    {
        QualitySettings.SetQualityLevel(level, true);
    }

    private void ToggleShowPaths()
    {
        Pathing.WaypointManager.Instance.ShowNavigatorPaths = !Pathing.WaypointManager.Instance.ShowNavigatorPaths;
    }

    UpdateManager IManualUpdate.GetUpdateManager()
    {
        return _updateManager;
    }

    void IManualUpdate.OnInitialise()
    {
        _lowQButton.onClick.AddListener(() => SetQuality(0));
        _medQButton.onClick.AddListener(() => SetQuality(1));
        _highQButton.onClick.AddListener(() => SetQuality(2));
        _showPathsButton.onClick.AddListener(ToggleShowPaths);
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
}
