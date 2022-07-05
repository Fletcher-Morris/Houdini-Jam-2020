using Sirenix.OdinInspector;
using UnityEngine;

[ExecuteInEditMode]
public class GrassObject : MonoBehaviour
{
    [SerializeField] private float _radius = 1.0f;
    [SerializeField, Required] private GrassComputeController _grassComputeController;

    public float Radius { get => _radius; }

    private void OnEnable()
    {
        _grassComputeController.AddDistortionObject(this);
    }

    private void OnDisable()
    {
        _grassComputeController.RemoveDistortionObject(this);
    }

    public Vector4 GetVector()
    {
        return new Vector4(transform.position.x, transform.position.y, transform.position.z, _radius);
    }
}