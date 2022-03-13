using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "Grass Scatter", menuName = "Scriptables/Grass/Grass Scatter")]
public class GrassScatter : ScriptableObject
{
    private static GrassScatter _instance;
    public static GrassScatter Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameManager.Instance.GrassScatterer;
            }
            return _instance;
        }
    }

    [SerializeField] private bool _scatter;
    [SerializeField] private bool _delete;
    [HideInInspector] public List<GameObject> _createdGrass = new List<GameObject>();
    private GameObject _grassParent;
    [SerializeField] private LayerMask _placementMask;
    [SerializeField] private float _sinkValue = 2.0f;
    [SerializeField] private float _randomScatter = 5.0f;
    [SerializeField] private List<ScatterObject> _spawnableObjects = new List<ScatterObject>();

    private void OnValidate()
    {
        if (_delete) DeleteGrass();
    }

    public void FixedUpdate()
    {
        if (_scatter) Scatter();
    }

    private void DeleteGrass()
    {
        _delete = false;
        if (_grassParent != null) Destroy(_grassParent);
        _createdGrass = new List<GameObject>();
    }

    public void Scatter()
    {
        _scatter = false;

        DeleteGrass();

        _grassParent = new GameObject("GRASS_PARENT");

        _spawnableObjects.ForEach(o =>
        {
            var points = new List<Vector3>();
            var samples = o.number;
            var phi = Mathf.PI * (3.0f - Mathf.Sqrt(5.0f));
            for (var i = 0; i < samples; i++)
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
                var pos = p.normalized * 100;

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
                        if (h >= o.minSpawnHeight && h <= o.maxSpawnHeight)
                        {
                            //Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.cyan, 10.0f);
                            var newGrass = Instantiate(o.prefab, hit.point - hit.point.normalized * _sinkValue,
                                Quaternion.identity, _grassParent.transform);
                            newGrass.transform.LookAt(Vector3.zero);
                            newGrass.transform.rotation = newGrass.transform.rotation * Quaternion.Euler(-90, 0, 0);
                            newGrass.transform.Rotate(Vector3.up, Random.Range(0.0f, 360.0f));
                            newGrass.isStatic = true;
                            _createdGrass.Add(newGrass);
                        }
                    }
                }
            });
        });

        _grassParent.isStatic = true;
    }

    [Serializable]
    public class ScatterObject
    {
        public string objectName;
        public int number = 5000;
        public GameObject prefab;
        public float minSpawnHeight;
        public float maxSpawnHeight;
    }
}