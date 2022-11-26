using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

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
        [SerializeField] private IManualUpdate[] _criticalOrder = new IManualUpdate[0];
        private int _criticalIndex = 0;
        [SerializeField, ReadOnly] private List<IManualUpdate> _updateList = new List<IManualUpdate>();
        [SerializeField, ReadOnly] private Queue<IManualUpdate> _initialisationQueue = new Queue<IManualUpdate>();

        private float _tickTimer;

        public void OnApplicationQuit()
        {
            while(_initialisationQueue.Count > 0)
            {
                _initialisationQueue.Dequeue().OnApplicationQuit();
            }
            _updateList.ForEach(o => o.OnApplicationQuit());

            _updateList.Clear();
            _initialisationQueue.Clear();
        }

        public void AddToUpdateList(IManualUpdate queueObject)
        {
            if (!_updateList.Contains(queueObject))
            {
                _initialisationQueue.Enqueue(queueObject);
                Debug.Log($"Added '{queueObject}' to initialisation queue.");
            }
            else
            {
                Debug.LogWarning($"'{queueObject}' is already added to the initialisation queue!");
            }
        }
        public void RemoveFromUpdateList(IManualUpdate _object)
        {
            if (_updateList.Contains(_object)) _updateList.Remove(_object);
        }

        public void Start()
        {
            _criticalIndex = 0;
        }

        private IManualUpdate _queuedObject = null;
        public void OnUpdate(float delta)
        {
            _queuedObject = null;

            if (_criticalIndex < _criticalOrder.Length)
            {
                _queuedObject = _criticalOrder[_criticalIndex];
                if (_queuedObject.OnInitialise())
                {
                    _updateList.Add(_queuedObject);
                    _criticalIndex++;
                    //Debug.Log($"Initialised '{_queuedObject}'.");
                }

                return;
            }

            if (_initialisationQueue.Count > 0)
            {
                _queuedObject = _initialisationQueue.Peek();
            }

            if (_queuedObject != null)
            {
                if (_queuedObject.OnInitialise())
                {
                    _initialisationQueue.Dequeue();
                    _updateList.Add(_queuedObject);
                    //Debug.Log($"Initialised '{_queuedObject}'.");
                }
            }
            else
            {
                foreach (IManualUpdate o in _updateList)
                {
                    if (o.IsEnabled())
                    {
                        o.OnManualUpdate(delta);
                    }
                }
            }

            if ((_tickTimer -= delta) <= 0)
            {
                OnTickUpdate(_tickTimer + _managerSettings.TickLength);
                _tickTimer = _managerSettings.TickLength;
            }
        }

        public void OnFixedUpdate(float delta)
        {
            foreach (IManualUpdate o in _updateList)
            {
                if(o.IsEnabled())
                {
                    o.OnManualFixedUpdate(delta);
                }
            }
        }

        private void OnTickUpdate(float delta)
        {
            foreach (IManualUpdate o in _updateList)
            {
                if (o.IsEnabled())
                {
                    o.OnTick(delta);
                }
            }
        }
    }

    internal class OdinSerislizeAttribute : Attribute
    {
    }
}