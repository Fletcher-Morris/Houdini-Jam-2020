using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quality
{
    [CreateAssetMenu(fileName = "New User Quality Settings", menuName = "Scriptables/Quality Settings/User Quality Settings")]
    public class UserQualitySettings : ScriptableObject
    {
        [SerializeField, ReadOnly] private QualitySettingsManager _qualitySettingsManager;

        [System.Serializable]
        public struct Settings
        {
            [ReadOnly] public string Name;
            [ReadOnly] public int Index;

            [Space]
            [Min(0)] public int UnitySettingsLevel;

            [Space, Header("Sheep")]
            [Min(5)] public float SheepDetailDistance;

            [Space, Header("Grass")]
            [Range(1, 16)] public int GrassPerVert;
        }

        public Settings QualitySettings;

        [Button]
        public void ApplySettings()
        {
            UnityEngine.QualitySettings.SetQualityLevel(QualitySettings.UnitySettingsLevel, true);

            _qualitySettingsManager.UpdateCurrentSettingsIndex(QualitySettings.Index);
        }

        private void OnValidate()
        {
            QualitySettings.Name = name;
        }

        public void SetSettingsManager(QualitySettingsManager manager)
        {
            _qualitySettingsManager = manager;
        }

        public void SetIndex(int index)
        {
            QualitySettings.Index = index;
        }
    }
}
