using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Rotator : MonoBehaviour
{
    [System.Serializable]
    public enum RotationMode
    {
        Local,
        World
    }

    [System.Serializable]
    public enum UpdateMethod
    {
        Update,
        FixedUpdate
    }

    public Vector3 _axis = Vector3.right;
    [FormerlySerializedAs("degreesPerSecond")] public float _minDegreesPerSecond = 15.0f;
    [FormerlySerializedAs("degreesPerSecond")] public float _maxDegreesPerSecond = 15.0f;
    public UpdateMethod _updateMethod;
    public RotationMode _rotationMode;

    private void Update()
    {
        if (_updateMethod != UpdateMethod.Update) return;
        Rotate(Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (_updateMethod != UpdateMethod.FixedUpdate) return;
        Rotate(Time.fixedDeltaTime);
    }

    private void Rotate(float dt)
    {
        switch (_rotationMode)
        {
            case RotationMode.Local:
                transform.Rotate(_axis * dt * Random.Range(_minDegreesPerSecond, _maxDegreesPerSecond), Space.Self);
                break;
            case RotationMode.World:
                transform.Rotate(_axis * dt * Random.Range(_minDegreesPerSecond, _maxDegreesPerSecond), Space.World);
                break;
        }
    }
}