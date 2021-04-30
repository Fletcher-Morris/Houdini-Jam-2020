using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    public Vector3 axis = Vector3.right;
    public float degreesPerSecond = 15.0f;

    [System.Serializable]
    public enum UpdateMethod
    {
        Update,
        FixedUpdate
    }
    public UpdateMethod updateMethod;

    [System.Serializable]
    public enum RotationMode
    {
        Local,
        World
    }
    public RotationMode rotationMode;

    void Update()
    {
        if (updateMethod != UpdateMethod.Update) return;
        Rotate(Time.deltaTime);
    }

    void FixedUpdate()
    {
        if (updateMethod != UpdateMethod.FixedUpdate) return;
        Rotate(Time.fixedDeltaTime);
    }

    void Rotate(float dt)
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
