using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Pathing
{
    [ExecuteInEditMode]
    public class WaypointModifierVolume : MonoBehaviour
    {
        [System.Serializable]
        public enum Modifier
        {
            None = 0,
            Remove = 1,
            Add = 2,
            Cluster = 3
        }
        [SerializeField] private Modifier _modifier;
        private Modifier _prevModifier;

        [SerializeField] private BoxCollider[] _volumes;
        [SerializeField, Required] private WaypointManager _waypointManager;

        [SerializeField, Range(0.1f, 4)] private float _density = 2.0f;
        private float _prevDensity = -1f;

        [SerializeField] private bool _preRaycast = true;
        [SerializeField] private List<Vector3> _points = new List<Vector3>();

        public float PointDensity { get => _density; }
        public List<Vector3> Points { get => _points; }
        public Modifier ModifierType { get => _modifier; }
        public bool PreRaycast { get => _preRaycast; }

        private void Update()
        {
            if (Application.isPlaying) return;

            if (_modifier != _prevModifier)
            {
                _prevModifier = _modifier;

                _points.Clear();

                switch (_modifier)
                {
                    case Modifier.None:
                        break;

                    case Modifier.Remove:
                        ModifierRemove();
                        break;

                    case Modifier.Add:
                        ModifierAdd();
                        break;

                    case Modifier.Cluster:
                        break;
                }
            }

            if (_density != _prevDensity)
            {
                _prevModifier = Modifier.None;
            }
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying) return;

            switch (_modifier)
            {
                case Modifier.None:
                    break;
                case Modifier.Remove:
                    break;
                case Modifier.Add:
                    foreach (Vector3 point in _points)
                    {
                        Gizmos.DrawWireSphere(point, 0.1f);
                    }
                    break;
                case Modifier.Cluster:
                    break;
            }
        }

        public void ModifierRemove() { }

        public bool PointIsInVolume(Vector3 point)
        {
            for (int v = 0; v < _volumes.Length; v++)
            {
                return _volumes[v].bounds.Contains(point);
            }

            return false;
        }

        public void ModifierAdd()
        {
            if (_waypointManager == null)
            {
                _waypointManager = Extensions.GetScriptableObjectAsset<WaypointManager>();
            }

            _density.Clamp(0, 4);

            _points.Clear();

            for (int v = 0; v < _volumes.Length; v++)
            {
                int xPoints = Mathf.Max(_volumes[v].size.x * _density, 1).RoundToInt();
                int yPoints = Mathf.Max(_volumes[v].size.y * _density, 1).RoundToInt();
                int zPoints = Mathf.Max(_volumes[v].size.z * _density, 1).RoundToInt();

                Matrix4x4 thisMatrix = _volumes[v].transform.localToWorldMatrix;

                for (int x = 0; x < xPoints; x++)
                {
                    for (int y = 0; y < yPoints; y++)
                    {
                        for (int z = 0; z < zPoints; z++)
                        {
                            float xPos = Mathf.Lerp(_volumes[v].size.x * -0.5f, _volumes[v].size.x * 0.5f, Mathf.InverseLerp(0, xPoints, x + 0.5f));
                            float yPos = Mathf.Lerp(_volumes[v].size.y * -0.5f, _volumes[v].size.y * 0.5f, Mathf.InverseLerp(0, yPoints, y + 0.5f));
                            float zPos = Mathf.Lerp(_volumes[v].size.z * -0.5f, _volumes[v].size.z * 0.5f, Mathf.InverseLerp(0, zPoints, z + 0.5f));
                            Vector3 pos = new Vector3(xPos, yPos, zPos);
                            pos += _volumes[v].center;
                            pos = thisMatrix.MultiplyPoint3x4(pos);

                            if (_preRaycast)
                            {
                                Ray ray = new Ray(pos, -pos.normalized);
                                RaycastHit hit;
                                if (Physics.Raycast(ray, out hit, Mathf.Infinity, _waypointManager.Settings.RaycastMask))
                                {
                                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                                    {
                                        pos = hit.point + (pos.normalized * _waypointManager.Settings.WaypointHeightOffset);
                                    }
                                }
                            }

                            _points.Add(pos);
                        }
                    }
                }
            }
        }
    }
}