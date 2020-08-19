using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GrassScatter : MonoBehaviour
{
    [System.Serializable]
    public class ScatterObject
    {
        public string objectName;
        public int number = 5000;
        public GameObject prefab;
        public float minSpawnHeight;
        public float maxSpawnHeight;
    }

    public bool scatter = false;
    public bool delete = false;
    public List<GameObject> createdGrass = new List<GameObject>();
    public Transform grassParent;
    public LayerMask mask;
    public float sinkValue = 2.0f;
    public float randomScatter = 5.0f;
    public List<ScatterObject> objects = new List<ScatterObject>();

    void Update()
    {
        if (delete) DeleteGrass();
        if (scatter) Scatter();
    }

    void DeleteGrass()
    {
        delete = false;
        if (grassParent != null) DestroyImmediate(grassParent.gameObject);
    }


    void Scatter()
    {
        scatter = false;

        DeleteGrass();

        grassParent = new GameObject("GRASS_PARENT").transform;
        grassParent.position = Vector3.zero;


        objects.ForEach(o =>
        {
            List<Vector3> points = new List<Vector3>();
            int samples = o.number;
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

                pos.x += Random.Range(-randomScatter, randomScatter);
                pos.y += Random.Range(-randomScatter, randomScatter);
                pos.z += Random.Range(-randomScatter, randomScatter);

                Ray ray = new Ray(pos, -pos);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
                {
                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                    {
                        float h = hit.point.magnitude;
                        if (h >= o.minSpawnHeight && h <= o.maxSpawnHeight)
                        {
                            //Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.cyan, 10.0f);
                            GameObject newGrass = Instantiate(o.prefab, hit.point - (hit.point.normalized * sinkValue), Quaternion.identity, grassParent);
                            newGrass.transform.LookAt(Vector3.zero);
                            newGrass.transform.rotation = newGrass.transform.rotation * Quaternion.Euler(-90, 0, 0);
                            newGrass.isStatic = true;
                            createdGrass.Add(newGrass);
                        }
                    }
                }
            });
        });

        grassParent.gameObject.isStatic = true;
    }

}
