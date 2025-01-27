using Sirenix.Serialization;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Pathing
{
    [System.Serializable]
    public class AiNavigator : IAiTarget
    {
        [OdinSerialize] private IAiTarget _aiTarget;
        [HideInInspector] public Transform self;
        private int _prevWaypoint;
        private int _nextWaypoint;
        [SerializeField] private float _alwaysUpdateTollerance = 0.5f;
        [SerializeField] private float _pathRefreshInterval = 3.0f;
        [SerializeField] private NavPathUpdateMode _updateMode = NavPathUpdateMode.Always;
        [SerializeField] private float _waypointSmoothing = 2.0f;
        [SerializeField] private bool _sphereRaycastTarget = true;
        [SerializeField] private LayerMask _raycastLayermask;

        public UnityEvent<Vector3, int> OnReachedWaypoint = new UnityEvent<Vector3, int>();
        public UnityEvent<Vector3> OnReachedTarget = new UnityEvent<Vector3>();
        public UnityEvent<List<int>> OnRecalculatedPath = new UnityEvent<List<int>>();

        [SerializeField] private bool _debugLines = true;
        private bool initialized;
        private AiWaypoint _closestWaypointToTarget;
        private float _pathRefreshTimer;
        private IAiTarget _prevTarget;
        private Vector3 _prevTargetPosition;
        [SerializeField] private List<int> _pathFound = new List<int>();
        private Vector3 _targetPosition;

        public void Initialize(float offset, Transform setSelf)
        {
            if (!Application.isPlaying) return;
            _pathRefreshTimer = offset;
            self = setSelf;
            initialized = true;
        }

        public void SetTarget(IAiTarget newTarget)
        {
            if (_updateMode == NavPathUpdateMode.TargetChanged)
            {
                if (newTarget != _aiTarget)
                {
                    _aiTarget = newTarget;
                    RecalculatePath();
                }
            }
            _aiTarget = newTarget;
            if (_aiTarget == null)
            {
                _prevWaypoint = -1;
                _nextWaypoint = 1;
                _targetPosition = _navPosition;
            }
            else
            {
                _targetPosition = _aiTarget.GetPosition();
            }

            _closestWaypointToTarget = WaypointManager.Instance.Closest(_targetPosition);
        }

        public void SetSelfTransform(Transform newTransform)
        {
            self = newTransform;
        }

        public void Update(float delta)
        {
            if (!initialized) return;
            _pathRefreshTimer += delta;
            if (_pathRefreshTimer >= _pathRefreshInterval)
            {
                _pathRefreshTimer = 0;
                RecalculateCheck(delta);
            }
        }

        private Vector3 _navPosition = Vector3.zero;
        private Vector3 _moveTarget = Vector3.zero;
        public Vector3 MoveTarget { get => _moveTarget; }

        public void SetCurrentNavPosition(Vector3 pos)
        {
            _navPosition = pos;
            if (_moveTarget == Vector3.zero) _moveTarget = pos;
        }

        public Vector3 Navigate(float moveDelta)
        {
            bool getNext = false;
            if (_nextWaypoint < 0) getNext = true;
            if (Vector3.Distance(_navPosition, _moveTarget) <= _waypointSmoothing) getNext = true;

            if (getNext)
            {
                OnReachedWaypoint.Invoke(_moveTarget, _nextWaypoint);
                _moveTarget = GetNextNavPosition(_nextWaypoint);
                _prevWaypoint = _nextWaypoint;
                _nextWaypoint++;
            }

            _navPosition = Vector3.MoveTowards(_navPosition, _moveTarget, moveDelta);

            if (_hasReachedTarget == false)
            {
                if (HasReachedTarget())
                {
                    OnReachedTarget.Invoke(_targetPosition);
                }
            }

            return _navPosition;
        }

        private Vector3 GetNextNavPosition(int currentIndex)
        {
            int findPos = currentIndex + 1;
            if (_pathFound == null || findPos >= _pathFound.Count)
            {
                return _targetPosition;
            }
            else
            {
                AiWaypoint waypoint = WaypointManager.Instance.GetWaypoint(GetWaypointFromIndex(findPos));
                return waypoint.Position;
            }
        }

        [SerializeField] private bool _hasReachedTarget;
        public bool HasReachedTarget()
        {
            _hasReachedTarget = _navPosition.Approximately(_targetPosition);
            return _hasReachedTarget;
        }

        private void RecalculateCheck(float delta)
        {
            if (!initialized)
            {
                Debug.LogWarning("Not initialized!");
                return;
            }

            switch (_updateMode)
            {
                case NavPathUpdateMode.Manual:
                    break;
                case NavPathUpdateMode.TargetChanged:
                    if (_prevTarget != _aiTarget && _aiTarget != null)
                    {
                        RecalculatePath(_aiTarget.GetPosition());
                    }
                    break;

                case NavPathUpdateMode.TargetPositionChanged:
                    if (_aiTarget != null)
                    {
                        if (_prevTargetPosition != _aiTarget.GetPosition())
                        {
                            RecalculatePath();
                        }
                    }
                    break;

                case NavPathUpdateMode.Always:
                    {
                        if (_aiTarget != null && self != null)
                        {
                            float distToTarget = self.Distance(_aiTarget.GetPosition());
                            if (distToTarget > _alwaysUpdateTollerance)
                            {
                                AiWaypoint newWaypoint = WaypointManager.Instance.Closest(_aiTarget.GetPosition());
                                if (newWaypoint != null)
                                {
                                    if (_closestWaypointToTarget == null)
                                    {
                                        RecalculatePath();
                                    }
                                    else if (newWaypoint.Id != _closestWaypointToTarget.Id)
                                    {
                                        RecalculatePath();
                                    }

                                    _closestWaypointToTarget = newWaypoint;
                                }
                            }
                        }
                    }
                    break;
            }
        }

        public bool HasTarget()
        {
            return _aiTarget != null;
        }

        public void RecalculatePath()
        {
            if (_aiTarget == null)
            {
            }
            else
            {
                _prevTargetPosition = _aiTarget.GetPosition();
                RecalculatePath(_prevTargetPosition);
            }

            _moveTarget = _navPosition;
            _prevWaypoint = -1;
            _nextWaypoint = -1;
        }

        public void RecalculatePath(Vector3 end)
        {
            if (!WaypointManager.IsInitialised)
            {
                return;
            }

            if (!initialized)
            {
                Debug.LogWarning("Not initialized!");
                return;
            }

            if (self == null)
            {
                Debug.LogWarning("Self transform is NULL!");
                return;
            }

            _pathFound = null;
            _prevTarget = _aiTarget;

            if (_sphereRaycastTarget)
            {
                RaycastHit hit;
                if (Physics.Raycast(end.normalized * 1000, -end.normalized, out hit, 1000, _raycastLayermask))
                {
                    if (_debugLines && WaypointManager.Instance.LineDebugOpacity > 0.0f)
                        Debug.DrawLine(end.normalized * 1000, hit.point,
                            Color.yellow * WaypointManager.Instance.LineDebugOpacity, _pathRefreshInterval);
                    end = hit.point;
                }
            }

            if (_aiTarget != null)
            {
                _prevTargetPosition = end;
                _pathFound = WaypointManager.Instance.GetPath(self.position, end);
                if (_pathFound == null)
                {
                    Debug.LogWarning($"Could not find path from '{self.position}' to '{end}'!");
                    return;
                }

                _prevWaypoint = -1;
                _nextWaypoint = 1;

            }

            OnRecalculatedPath.Invoke(_pathFound);
        }

        internal Vector3 GetTargetPos()
        {
            return _targetPosition;
        }

        public Vector3 GetNavPosition()
        {
            return _navPosition;
        }

        public int GetWaypointFromIndex(int index)
        {
            if (index == -1 || index >= _pathFound.Count || _pathFound.Count == 0) return 0;
            return _pathFound[index];
        }

        void IAiTarget.SetPosition(Vector3 pos)
        {
            SetCurrentNavPosition(pos);
        }

        Vector3 IAiTarget.GetPosition()
        {
            return _navPosition;
        }
    }

    [System.Serializable]
    public enum NavPathUpdateMode
    {
        //  Recalculate is called manually.
        Manual,

        //  Recalculate when the target changes.
        TargetChanged,

        //  Recalculate when the target's position changes.
        TargetPositionChanged,

        //  Recalculate every cycle.
        Always
    }
}
