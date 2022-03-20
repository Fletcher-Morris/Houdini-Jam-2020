using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scatter
{
    [CreateAssetMenu(fileName = "Scatter Object", menuName = "Scriptables/Scatterer/Scatter Object")]
    public class ScattererObject : ScriptableObject
    {
        public string ObjectName = "";
        public int Number = 5000;
        public GameObject Prefab = null;
        public float MinSpawnHeight = 40;
        public float MaxSpawnHeight = 60;
        public float SinkValue = 0.5f;

        private void OnValidate()
        {
            if (ObjectName == "") ObjectName = name;
        }
    }
}