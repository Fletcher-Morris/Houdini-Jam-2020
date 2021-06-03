using UnityEngine;

[ExecuteInEditMode]
public class GrassObject : MonoBehaviour
{
	public float radius = 1.0f;

	private void OnEnable()
	{
		GrassDistortion.Instance?.AddObject(this);
	}

	private void OnDisable()
	{
		GrassDistortion.Instance?.RemoveObject(this);
	}

	public Vector4 GetVector()
	{
		return new Vector4(transform.position.x, transform.position.y, transform.position.z, radius);
	}
}