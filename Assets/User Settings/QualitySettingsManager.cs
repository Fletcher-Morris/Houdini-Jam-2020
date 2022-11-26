using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quality
{
    public interface IUpdateQuality
    {
        void UpdateQualitySettings(UserQualitySettings settings);
    }

    [CreateAssetMenu(fileName = "Quality Settings Manager", menuName = "Scriptables/Quality Settings/Quality Settings Manager")]
    public class QualitySettingsManager : SerializedScriptableObject
    {
        [SerializeField] private UserQualitySettings[] _settingsArray = new UserQualitySettings[0];
        [SerializeField] private IUpdateQuality[] _updateQualities = new IUpdateQuality[0];
        [SerializeField, ReadOnly] private int _currentSettingsIndex = 0;

        public static QualitySettingsManager Instance => GameManager.Instance.QualityManager;

        public UserQualitySettings CurrentSettings => _settingsArray[_currentSettingsIndex];

        public void ApplySettings(int index)
        {
            _settingsArray[index].ApplySettings();

            foreach (IUpdateQuality item in _updateQualities)
            {
                item.UpdateQualitySettings(_settingsArray[index]);
            }
        }

        public void UpdateCurrentSettingsIndex(int index)
        {
            _currentSettingsIndex = index;
        }

        private void OnValidate()
        {
            for (int i = 0; i < _settingsArray.Length; i++)
            {
                UserQualitySettings item = _settingsArray[i];
                item.SetSettingsManager(this);
                item.SetIndex(i);
            }
        }

        public static UserQualitySettings.Settings Current => Instance.CurrentSettings.QualitySettings;
    }
}