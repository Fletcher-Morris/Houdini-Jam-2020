using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CloudScatter : MonoBehaviour
{
    public bool create;
    public bool delete;
    public bool autoUpdate = false;

    public List<GameObject> cloudPrefabs = new List<GameObject>();
    public int cloudLayers = 2;
    public int cloudsPerLayer = 6;
    public float baseLayerHeight = 80;
    public float additionLayerHeight = 20;

    public int m_prevSum = -1;

    public List<GameObject> m_createdParents = new List<GameObject>();

    void Update()
    {
        if (delete) Delete();
        if (create) Create();

        if (autoUpdate)
        {
            int s = CheckSum();
            if (s != m_prevSum)
            {
                Create();
                m_prevSum = s;
            }
        }
    }

    int CheckSum()
    {
        int sum = 0;
        sum += cloudPrefabs.Count;
        sum += cloudLayers;
        sum += cloudsPerLayer;
        sum += baseLayerHeight.RoundToInt();
        sum += additionLayerHeight.RoundToInt();
        return sum;
    }

    void Create()
    {
        create = false;
        Delete();

        if (cloudPrefabs.Count <= 0) return;

        for (int layer = 0; layer < cloudLayers; layer++)
        {

            GameObject newLayer = new GameObject($"Clouds_{layer}");
            newLayer.transform.parent = transform;
            m_createdParents.Add(newLayer);
            float layerHeight = baseLayerHeight;
            if (layer > 0)
            {
                layerHeight += (additionLayerHeight * layer);
            }
            Rotator r = newLayer.AddComponent<Rotator>();
            r.axis = new Vector3(0.2f, 1.0f, 0.0f);
            r.degreesPerSecond = 3.0f / (layer + 1);
            r.rotationMode = Rotator.RotationMode.World;

            List<Vector3> points = new List<Vector3>();
            int samples = cloudsPerLayer;
            float phi = Mathf.PI * (3.0f - Mathf.Sqrt(5.0f));
            for (int i = 0; i < samples; i++)
            {
                float y = 1.0f - (i / (float) (samples - 1) * 2.0f);
                float radius = Mathf.Sqrt(1.0f - (y * y));
                float theta = phi * i;
                float x = Mathf.Cos(theta) * radius;
                float z = Mathf.Sin(theta) * radius;
                points.Add(new Vector3(x, y, z).normalized * layerHeight);
            }

            points.ForEach(p =>
            {
                GameObject newCloud = Instantiate(cloudPrefabs.RandomItem(), p, Quaternion.identity, newLayer.transform);
                float s = Random.Range(1.0f, 1.5f);
                newCloud.transform.localScale = new Vector3(s, s, s);
                newCloud.transform.LookAt(Vector3.zero);
                newCloud.transform.rotation = newCloud.transform.rotation * Quaternion.Euler(-90, 0, 0);
                newCloud.transform.localEulerAngles += new Vector3(0, Random.Range(0.0f, 360.0f), 0.0f);
            });

            newLayer.transform.eulerAngles = new Vector3(Random.Range(-90.0f, 90.0f), Random.Range(-90.0f, 90.0f), Random.Range(-90.0f, 90.0f));
        }

    }

    void Delete()
    {
        delete = false;
        m_createdParents.ForEach(p => DestroyImmediate(p));
        m_createdParents.Clear();
    }
}
