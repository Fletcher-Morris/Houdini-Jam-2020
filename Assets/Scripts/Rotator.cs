using System;
using UnityEngine;

public class Rotator : MonoBehaviour
{
	[Serializable]
	public enum RotationMode
	{
		Local,
		World
	}

	[Serializable]
	public enum UpdateMethod
	{
		Update,
		FixedUpdate
	}

	public Vector3 axis = Vector3.right;
	public float degreesPerSecond = 15.0f;
	public UpdateMethod updateMethod;
	public RotationMode rotationMode;

	private void Update()
	{
		if (updateMethod != UpdateMethod.Update) return;
		Rotate(Time.deltaTime);
	}

	private void FixedUpdate()
	{
		if (updateMethod != UpdateMethod.FixedUpdate) return;
		Rotate(Time.fixedDeltaTime);
	}

	private void Rotate(float dt)
	{
		switch (rotationMode)
		{
			case RotationMode.Local:
				transform.Rotate(axis * dt * degreesPerSecond, Space.Self);
				break;
			case RotationMode.World:
				transform.Rotate(axis * dt * degreesPerSecond, Space.World);
				break;
		}
	}
}