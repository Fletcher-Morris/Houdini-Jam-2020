using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tick
{
    [CreateAssetMenu(fileName = "Update Manager", menuName = "Scriptables/Tick/Update Manager")]
    public class UpdateManager : SerializedScriptableObject
    {
        public UpdateManager()
        {
            _managerSettings = new UpdateManagerSettings(0.1f);
        }

        [System.Serializable]
        public struct UpdateManagerSettings
        {
            public float TickLength;

            public UpdateManagerSettings(float tickLength)
            {
                TickLength = tickLength;
            }
        }

        [SerializeField] private UpdateManagerSettings _managerSettings;
        [SerializeField] private List<IManualUpdate> _updateList = new List<IManualUpdate>();
        [SerializeField] private Queue<IManualUpdate> _initialisationQueue = new Queue<IManualUpdate>();

        private float _tickTimer;

        public void OnApplicationQuit()
        {
            _updateList = new List<IManualUpdate>();
            _initialisationQueue = new Queue<IManualUpdate>();
        }

        public void AddToUpdateList(IManualUpdate queueObject)
        {
            if (!_updateList.Contains(queueObject))
            {
                _initialisationQueue.Enqueue(queueObject);
                Debug.Log($"Added '{queueObject}' to initialisation queue.");
            }
        }
        public void RemoveFromUpdateList(IManualUpdate _object)
        {
            if (_updateList.Contains(_object)) _updateList.Remove(_object);
        }

        public void OnUpdate(float delta)
        {
            if (_initialisationQueue.TryDequeue(out IManualUpdate queuedObject))
            {
                queuedObject.OnInitialise();
                _updateList.Add(queuedObject);
                Debug.Log($"Initialised '{queuedObject}'.");
            }

            _updateList.ForEach(o => o.OnManualUpdate(delta));
        }

        public void OnFixedUpdate(float delta)
        {
            if ((_tickTimer -= delta) <= 0)
            {
                OnTickUpdate(_tickTimer + _managerSettings.TickLength);
                _tickTimer = _managerSettings.TickLength;
            }

            _updateList.ForEach(o => o.OnManualFixedUpdate(delta));
        }

        private void OnTickUpdate(float delta)
        {
            _updateList.ForEach(o => o.OnTick(delta));
        }
    }

    internal class OdinSerislizeAttribute : Attribute
    {
    }
}