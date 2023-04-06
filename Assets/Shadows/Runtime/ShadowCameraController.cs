using System.Collections.Generic;
using UnityEngine;

namespace Shadows
{
    [System.Serializable]
    public struct ShadowCamSettings
    {
        public LayerMask ShadowCastLayers;
        [Range(0.01f, 100)] public float NearClip;
        [Range(1, 200)] public float FarClip;
    }

    public class ShadowCameraController : MonoBehaviour
    {
        [SerializeField] private ShadowCamSettings _camSettings;

        private List<Camera> _cameras = new List<Camera>();

        private void OnValidate()
        {
            _camSettings.NearClip = Mathf.Min(_camSettings.NearClip, _camSettings.FarClip);

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
        }

        private void OnDrawGizmos()
        {
            for (int camIndex = 0; camIndex < _cameras.Count; camIndex++)
            {
                Gizmos.color = Color.HSVToRGB((float)camIndex / (float)_cameras.Count, 1, 1);
                Camera cam = _cameras[camIndex];
                Gizmos.matrix = cam.transform.localToWorldMatrix;
                Gizmos.DrawFrustum(transform.position, 90, _camSettings.FarClip, _camSettings.NearClip, 1);
            }
        }
    }
}