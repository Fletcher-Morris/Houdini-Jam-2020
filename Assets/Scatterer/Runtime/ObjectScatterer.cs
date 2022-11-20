using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scatter
{
    [CreateAssetMenu(fileName = "Object Scatterer", menuName = "Scriptables/Scatterer/Object Scatterer")]
    public class ObjectScatterer : ScriptableObject
    {
        [SerializeField] private bool _scatter;
        [SerializeField] private bool _delete;
        [HideInInspector] public List<GameObject> _createdObjects = new List<GameObject>();
        private GameObject _objectParent;
        [SerializeField] private LayerMask _placementMask;
        [SerializeField] private float _randomScatter = 5.0f;
        [SerializeField] private List<ScattererObject> _spawnableObjects = new List<ScattererObject>();

        private void OnValidate()
        {
            if (_delete) DeleteObjects();
            if (_scatter) Scatter();
        }

        private void GetObjectParent()
        {
            string parentName = (name + "_parent").ToUpper().Replace(' ', '_');
            GameObject found = GameObject.Find(parentName);
            if (found != null) _objectParent = found;
            else _objectParent = new GameObject(parentName);
        }

        private void DeleteObjects()
        {
            _delete = false;

            GetObjectParent();

            if (_objectParent != null) DestroyImmediate(_objectParent);
            _createdObjects = new List<GameObject>();
        }

        public void Scatter()
        {
            _scatter = false;

            DeleteObjects();

            GetObjectParent();

            _spawnableObjects.ForEach(o =>
            {
                List<Vector3> points = new List<Vector3>();
                int samples = o.Number;
                float phi = Mathf.PI * (3.0f - Mathf.Sqrt(5.0f));
                for (int i = 0; i < samples; i++)
                {
                    float y = 1.0f - (i / (float)(samples - 1) * 2.0f);
                    float radius = Mathf.Sqrt(1.0f - (y * y));
                    float theta = phi * i;
                    float x = Mathf.Cos(theta) * radius;
                    float z = Mathf.Sin(theta) * radius;
                    points.Add(new Vector3(x, y, z));
                }

                points.ForEach(p =>
                {
                    Vector3 pos = p.normalized * 100;

                    pos.x += Random.Range(-_randomScatter, _randomScatter);
                    pos.y += Random.Range(-_randomScatter, _randomScatter);
                    pos.z += Random.Range(-_randomScatter, _randomScatter);

                    Ray ray = new Ray(pos, -pos);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, _placementMask))
                    {
                        if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                        {
                            float h = hit.point.magnitude;
                            if (h >= o.MinSpawnHeight && h <= o.MaxSpawnHeight)
                            {
                                GameObject newObject = Instantiate(o.Prefab, hit.point - (hit.point.normalized * o.SinkValue), Quaternion.identity, _objectParent.transform);
                                newObject.transform.LookAt(Vector3.zero);
                                newObject.transform.rotation = newObject.transform.rotation * Quaternion.Euler(-90, 0, 0);
                                newObject.transform.Rotate(Vector3.up, Random.Range(0.0f, 360.0f));
                                newObject.isStatic = true;
                                _createdObjects.Add(newObject);
                            }
                        }
                    }
                });
            });
            _objectParent.isStatic = true;
        }
    }
}