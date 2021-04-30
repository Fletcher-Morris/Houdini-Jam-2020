using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(LightProbeGroup))]
public class LightProbeDistributer : MonoBehaviour
{
    public bool distribute = false;

    private LightProbeGroup m_probeGroup = default;

    [SerializeField] private int m_probeCount = 500;
    [SerializeField] private float m_altitude = 50;
    private void Update()
    {
        if (distribute) Distribute();
    }

    public void Distribute()
    {
        distribute = false;
        m_probeGroup ??= GetComponent<LightProbeGroup>();
        List<Vector3> positions = Extensions.FibonacciPoints(m_probeCount);
        m_probeGroup.probePositions = positions.ToArray();
        LightProbes.Tetrahedralize();
    }
}
