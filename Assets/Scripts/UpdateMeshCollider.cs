using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class UpdateMeshCollider : MonoBehaviour
{
	public void UpdateCollider()
	{
		Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
		if(mesh == null) return;
		MeshCollider collider = GetComponent<MeshCollider>();
		if(collider == null) collider = gameObject.AddComponent<MeshCollider>();
		collider.sharedMesh = mesh;
		Debug.Log($"Updated mesh collider on '{gameObject.name}'!");
	}
}
