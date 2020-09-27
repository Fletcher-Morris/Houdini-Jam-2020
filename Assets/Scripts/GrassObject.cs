using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GrassObject : MonoBehaviour
{
    private void OnEnable()
    {
        GrassDistortion.Instance?.AddObject(this);
    }
    private void OnDisable()
    {
        GrassDistortion.Instance?.RemoveObject(this);
    }

    public float radius = 1.0f;
    public Vector4 GetVector()
    {
        return new Vector4(transform.position.x,transform.position.y,transform.position.z,radius);
    }
}
