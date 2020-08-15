using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class UpdateMeshCollider : MonoBehaviour
{
	public void UpdateCollider()
	{
        GameObject holder = gameObject.GetComponentInChildren<MeshFilter>().gameObject;
        Mesh mesh = holder.GetComponent<MeshFilter>().sharedMesh;
        if (mesh == null) return;
        MeshCollider collider = holder.GetComponent<MeshCollider>();
        if (collider == null) collider = holder.AddComponent<MeshCollider>();
        collider.sharedMesh = mesh;
        Debug.Log($"Updated mesh collider on '{holder.gameObject.name}'!");
	}
}
