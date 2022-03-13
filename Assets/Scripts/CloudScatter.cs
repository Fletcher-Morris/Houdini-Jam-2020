using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CloudScatter : MonoBehaviour
{
    public bool create;
    public bool delete;
    public bool autoUpdate;

    public List<GameObject> cloudPrefabs = new List<GameObject>();
    public int cloudLayers = 2;
    public int cloudsPerLayer = 6;
    public float baseLayerHeight = 80;
    public float additionLayerHeight = 20;

    public int m_prevSum = -1;

    public List<GameObject> m_createdParents = new List<GameObject>();

    private void Update()
    {
        if (delete) Delete();
        if (create) Create();

        if (autoUpdate)
        {
            var s = CheckSum();
            if (s != m_prevSum)
            {
                Create();
                m_prevSum = s;
            }
        }
    }

    private int CheckSum()
    {
        var sum = 0;
        sum += cloudPrefabs.Count;
        sum += cloudLayers;
        sum += cloudsPerLayer;
        sum += baseLayerHeight.RoundToInt();
        sum += additionLayerHeight.RoundToInt();
        return sum;
    }

    private void Create()
    {
        create = false;
        Delete();

        if (cloudPrefabs.Count <= 0) return;

        for (var layer = 0; layer < cloudLayers; layer++)
        {
            var newLayer = new GameObject($"Clouds_{layer}");
            newLayer.transform.parent = transform;
            m_createdParents.Add(newLayer);
            var layerHeight = baseLayerHeight;
            if (layer > 0) layerHeight += additionLayerHeight * layer;
            var r = newLayer.AddComponent<Rotator>();
            r._axis = new Vector3(0.2f, 1.0f, 0.0f);
            r._minDegreesPerSecond = 3.0f / (layer + 1);
            r._rotationMode = Rotator.RotationMode.World;

            var points = new List<Vector3>();
            var samples = cloudsPerLayer;
            var phi = Mathf.PI * (3.0f - Mathf.Sqrt(5.0f));
            for (var i = 0; i < samples; i++)
            {
                var y = 1.0f - i / (float)(samples - 1) * 2.0f;
                var radius = Mathf.Sqrt(1.0f - y * y);
                var theta = phi * i;
                var x = Mathf.Cos(theta) * radius;
                var z = Mathf.Sin(theta) * radius;
                points.Add(new Vector3(x, y, z).normalized * layerHeight);
            }

            points.ForEach(p =>
            {
                var newCloud = Instantiate(cloudPrefabs.RandomItem(), p, Quaternion.identity, newLayer.transform);
                var s = Random.Range(1.0f, 1.5f);
                newCloud.transform.localScale = new Vector3(s, s, s);
                newCloud.transform.LookAt(Vector3.zero);
                newCloud.transform.rotation = newCloud.transform.rotation * Quaternion.Euler(-90, 0, 0);
                newCloud.transform.localEulerAngles += new Vector3(0, Random.Range(0.0f, 360.0f), 0.0f);
            });

            newLayer.transform.eulerAngles = new Vector3(Random.Range(-90.0f, 90.0f), Random.Range(-90.0f, 90.0f),
                Random.Range(-90.0f, 90.0f));
        }
    }

    private void Delete()
    {
        delete = false;
        m_createdParents.ForEach(p => DestroyImmediate(p));
        m_createdParents.Clear();
    }
}