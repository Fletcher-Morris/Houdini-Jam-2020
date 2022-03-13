using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RockScatter : MonoBehaviour
{
    public static RockScatter Instance;
    public bool scatter;
    public bool delete;
    [HideInInspector] public List<GameObject> createdRocks = new List<GameObject>();
    public Transform rockParent;
    public LayerMask mask;

    public int rockCount = 100;
    public float sinkValue = 2.0f;
    public float randomScatter = 5.0f;
    public float minScale = 0.25f;
    public float maxScale = 1.0f;
    public List<GameObject> prefabs = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (delete) DeleteRocks();
        if (scatter) Scatter();
    }

    private void DeleteRocks()
    {
        delete = false;
        if (rockParent != null) DestroyImmediate(rockParent.gameObject);
        createdRocks = new List<GameObject>();
    }

    public void Scatter()
    {
        scatter = false;

        DeleteRocks();

        rockParent = new GameObject("ROCKS_PARENT").transform;
        rockParent.position = Vector3.zero;

        var points = Extensions.FibonacciPoints(rockCount);

        points.ForEach(p =>
        {
            var pos = p.normalized * 100;

            pos.x += Random.Range(-randomScatter, randomScatter);
            pos.y += Random.Range(-randomScatter, randomScatter);
            pos.z += Random.Range(-randomScatter, randomScatter);

            var ray = new Ray(pos, -pos);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    var newRock = Instantiate(prefabs.RandomItem(), hit.point - hit.point.normalized * sinkValue,
                        Quaternion.identity, rockParent);
                    newRock.transform.eulerAngles = Extensions.RandomVec3(-360, 360);
                    newRock.transform.localScale *= Random.Range(minScale, maxScale);
                    createdRocks.Add(newRock);
                }
        });

        rockParent.gameObject.isStatic = true;
    }
}