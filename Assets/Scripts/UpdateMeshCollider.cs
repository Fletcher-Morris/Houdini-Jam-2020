using UnityEngine;

[ExecuteInEditMode]
public class UpdateMeshCollider : MonoBehaviour
{
	public void UpdateCollider()
	{
		var holder = gameObject.GetComponentInChildren<MeshFilter>().gameObject;
		var mesh = holder.GetComponent<MeshFilter>().sharedMesh;
		if (mesh == null) return;
		var collider = holder.GetComponent<MeshCollider>();
		if (collider == null) collider = holder.AddComponent<MeshCollider>();
		collider.sharedMesh = mesh;
		Debug.Log($"Updated mesh collider on '{holder.gameObject.name}'!");
	}
}