using System.Collections.Generic;
using UnityEngine;

namespace Shadows
{
    [System.Serializable]
    public struct ShadowCamSettings
    {
        public LayerMask ShadowCastLayers;
        [Range(0.01f, 100)] public float NearClip;
        [Range(2f, 200)] public float FarClip;
    }

    public class ShadowCameraController : MonoBehaviour
    {
        private readonly int ShadowMinHeight_ID = Shader.PropertyToID("SHADOW_MIN_HEIGHT");
        private readonly int ShadowMaxHeight_ID = Shader.PropertyToID("SHADOW_MAX_HEIGHT");

        [SerializeField] private ShadowCamSettings _camSettings;
        [SerializeField] private bool _drawGizmos;

        private List<Camera> _cameras = new List<Camera>();

        private void Start()
        {
            UpdateCameraProperties();
        }

        private void OnValidate()
        {
            _camSettings.FarClip = Mathf.Max(_camSettings.FarClip, 1);
            _camSettings.NearClip = Mathf.Min(_camSettings.NearClip, _camSettings.FarClip - 1);

            if (_cameras.Count < 6)
            {
                _cameras = GetComponentsInChildren<Camera>().ToList();
            }

            UpdateCameraProperties();
        }

        private void UpdateCameraProperties()
        {
            foreach (Camera cam in _cameras)
            {
                cam.cullingMask = _camSettings.ShadowCastLayers;
                cam.nearClipPlane = _camSettings.NearClip;
                cam.farClipPlane = _camSettings.FarClip;
            }

            Shader.SetGlobalFloat(ShadowMinHeight_ID, _camSettings.NearClip);
            Shader.SetGlobalFloat(ShadowMaxHeight_ID, _camSettings.FarClip);
        }

        private void OnDrawGizmos()
        {
            if (_drawGizmos)
            {
                for (int camIndex = 0; camIndex < _cameras.Count; camIndex++)
                {
                    Gizmos.color = Color.HSVToRGB(camIndex / (float)_cameras.Count, 1, 1);
                    Camera cam = _cameras[camIndex];
                    Gizmos.matrix = cam.transform.localToWorldMatrix;
                    Gizmos.DrawFrustum(transform.position, 90, _camSettings.FarClip, _camSettings.NearClip, 1);
                }
            }
        }
    }
}