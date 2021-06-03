using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public class GrassScatter : MonoBehaviour
{
	public static GrassScatter Instance;

	public bool scatter;
	public bool delete;
	[HideInInspector] public List<GameObject> createdGrass = new List<GameObject>();
	public Transform grassParent;
	public LayerMask mask;
	public float sinkValue = 2.0f;
	public float randomScatter = 5.0f;
	public List<ScatterObject> objects = new List<ScatterObject>();

	private void Awake()
	{
		Instance = this;
	}

	private void Update()
	{
		if (delete) DeleteGrass();
		if (scatter) Scatter();
	}

	private void DeleteGrass()
	{
		delete = false;
		if (grassParent != null) DestroyImmediate(grassParent.gameObject);
		createdGrass = new List<GameObject>();
	}

	public void Scatter()
	{
		scatter = false;

		DeleteGrass();

		grassParent          = new GameObject("GRASS_PARENT").transform;
		grassParent.position = Vector3.zero;

		objects.ForEach(o =>
		{
			var points = new List<Vector3>();
			var samples = o.number;
			var phi = Mathf.PI * (3.0f - Mathf.Sqrt(5.0f));
			for (var i = 0; i < samples; i++)
			{
				var y = 1.0f - i / (float) (samples - 1) * 2.0f;
				var radius = Mathf.Sqrt(1.0f - y * y);
				var theta = phi * i;
				var x = Mathf.Cos(theta) * radius;
				var z = Mathf.Sin(theta) * radius;
				points.Add(new Vector3(x, y, z));
			}

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
						var h = hit.point.magnitude;
						if (h >= o.minSpawnHeight && h <= o.maxSpawnHeight)
						{
							//Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.cyan, 10.0f);
							var newGrass = Instantiate(o.prefab, hit.point - hit.point.normalized * sinkValue,
								Quaternion.identity,             grassParent);
							newGrass.transform.LookAt(Vector3.zero);
							newGrass.transform.rotation = newGrass.transform.rotation * Quaternion.Euler(-90, 0, 0);
							newGrass.transform.Rotate(transform.up, Random.Range(0.0f, 360.0f));
							newGrass.isStatic = true;
							createdGrass.Add(newGrass);
						}
					}
			});
		});

		grassParent.gameObject.isStatic = true;
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