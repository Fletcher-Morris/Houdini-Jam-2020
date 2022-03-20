using Sirenix.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Pathing
{
    [System.Serializable]
    public class AiNavigator
    {
        [OdinSerialize] private IAiTarget _aiTarget;
        [HideInInspector] public Transform self;
        public int _prevWaypoint;
        public int _nextWaypoint;
        public float waypointTollerance = 0.1f;
        public float pathRefreshInterval = 3.0f;
        public NavPathUpdateMode updateMode = NavPathUpdateMode.Always;
        [SerializeField] private float _waypointSmoothing = 2.0f;
        public bool sphereRaycastTarget = true;
        public LayerMask raycastLayermask;

        [SerializeField] private bool debugLines = true;
        private bool initialized;
        [SerializeField] private AiWaypoint m_closestWaypointToTarget;
        private float m_pathRefreshTimer;
        private IAiTarget m_prevTarget;
        private Vector3 m_prevTargetPosition;
        public List<ushort> pathFound = new List<ushort>();

        [SerializeField] private Vector3 _targetPosition;

        public void Initialize(float offset, Transform setSelf)
        {
            if (!Application.isPlaying) return;
            m_pathRefreshTimer = offset;
            self = setSelf;
            initialized = true;
        }

        public void SetTarget(IAiTarget newTarget)
        {
            if (updateMode == NavPathUpdateMode.TargetChanged)
            {
                if(newTarget != _aiTarget)
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

            m_closestWaypointToTarget = WaypointManager.Instance.Closest(_targetPosition);
        }

        public void SetSelfTransform(Transform newTransform)
        {
            self = newTransform;
        }

        public void Update(float delta)
        {
            if (!initialized) return;
            m_pathRefreshTimer += delta;
            if (m_pathRefreshTimer >= pathRefreshInterval)
            {
                m_pathRefreshTimer = 0;
                RecalculateCheck(delta);
            }
        }

        [SerializeField] private Vector3 _navPosition = Vector3.zero;
        [SerializeField] private Vector3 _moveTarget = Vector3.zero;
        public Vector3 MoveTarget { get => _moveTarget; }

        public void SetCurrentNavPosition(Vector3 pos)
        {
            _navPosition = pos;
        }

        public Vector3 Navigate(float moveDelta)
        {
            bool getNext = false;
            if (_nextWaypoint < 0) getNext = true;
            if (Vector3.Distance(_navPosition, _moveTarget) <= _waypointSmoothing) getNext = true;

            if (getNext)
            {
                _moveTarget = GetNextNavPosition(_nextWaypoint);
                _prevWaypoint = _nextWaypoint;
                _nextWaypoint++;
            }

            _navPosition = Vector3.MoveTowards(_navPosition, _moveTarget, moveDelta);

            return _navPosition;
        }

        private Vector3 GetNextNavPosition(int currentIndex)
        {
            int findPos = currentIndex + 1;
            if(findPos >= pathFound.Count)
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

            switch (updateMode)
            {
                case NavPathUpdateMode.Manual:
                    break;
                case NavPathUpdateMode.TargetChanged:
                    if (m_prevTarget != _aiTarget && _aiTarget != null)
                    {
                        RecalculatePath(_aiTarget.GetPosition());
                    }
                    break;

                case NavPathUpdateMode.TargetPositionChanged:
                    if (_aiTarget != null)
                    {
                        if (m_prevTargetPosition != _aiTarget.GetPosition())
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
                            if (distToTarget > waypointTollerance)
                            {
                                var newWaypoint = WaypointManager.Instance.Closest(_aiTarget.GetPosition());
                                if (newWaypoint != null)
                                {
                                    if (m_closestWaypointToTarget == null)
                                    {
                                        RecalculatePath();
                                    }
                                    else if (newWaypoint.Id != m_closestWaypointToTarget.Id)
                                    {
                                        RecalculatePath();
                                    }

                                    m_closestWaypointToTarget = newWaypoint;
                                }
                            }
                        }
                    }
                    break;
            }
        }

        public void RecalculatePath()
        {
            if (_aiTarget == null)
            {
                Debug.LogWarning("TARGET IS NULL!");
            }
            else
            {
                m_prevTargetPosition = _aiTarget.GetPosition();
                RecalculatePath(m_prevTargetPosition);
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

            pathFound = null;
            m_prevTarget = _aiTarget;

            if (sphereRaycastTarget)
            {
                RaycastHit hit;
                if (Physics.Raycast(end.normalized * 1000, -end.normalized, out hit, 1000, raycastLayermask))
                {
                    if (debugLines && WaypointManager.Instance.LineDebugOpacity > 0.0f)
                        Debug.DrawLine(end.normalized * 1000, hit.point,
                            Color.yellow * WaypointManager.Instance.LineDebugOpacity, pathRefreshInterval);
                    end = hit.point;
                }
            }

            if (_aiTarget != null)
            {
                m_prevTargetPosition = end;
                pathFound = WaypointManager.Instance.GetPath(self.position, end);
                if (pathFound == null)
                {
                    Debug.LogWarning($"Could not find path from '{self.position}' to '{end}'!");
                    return;
                }

                _prevWaypoint = -1;
                _nextWaypoint = 1;
            }
        }

        public ushort GetWaypointFromIndex(int index)
        {
            if (index == -1 || index >= pathFound.Count || pathFound.Count == 0) return 0;
            return pathFound[index];
        }

        public void DrawLines(Vector3 start, float duration)
        {
            if (!initialized) return;
            if (pathFound == null) return;
            if (WaypointManager.Instance.LineDebugOpacity <= 0.0f) return;
            if (pathFound.Count > 0)
            {
                Color lineCol = Color.white;
                lineCol.a = WaypointManager.Instance.LineDebugOpacity * 5;
                ushort n = GetWaypointFromIndex(_nextWaypoint);
                if (n != 0 && debugLines) Debug.DrawLine(start, WaypointManager.Instance.GetWaypoint(n).Position, lineCol, duration);
                for (int i = 0; i < pathFound.Count - 1; i++)
                {
                    if (pathFound[i] != 0)
                    {
                        Debug.DrawLine(WaypointManager.Instance.GetWaypoint(pathFound[i]).Position, WaypointManager.Instance.GetWaypoint(pathFound[i + 1]).Position, lineCol, duration);
                    }
                }
            }
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
