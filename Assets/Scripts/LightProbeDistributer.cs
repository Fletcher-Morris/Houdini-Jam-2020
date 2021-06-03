using UnityEngine;

[ExecuteInEditMode, RequireComponent(typeof(LightProbeGroup))]
public class LightProbeDistributer : MonoBehaviour
{
	public bool distribute;

	[SerializeField] private int m_probeCount = 500;
	[SerializeField] private float m_altitude = 50;

	private LightProbeGroup m_probeGroup;

	private void Update()
	{
		if (distribute) Distribute();
	}

	public void Distribute()
	{
		distribute   =   false;
		m_probeGroup ??= GetComponent<LightProbeGroup>();
		var positions = Extensions.FibonacciPoints(m_probeCount);
		//m_probeGroup.probePositions = positions.ToArray();
		LightProbes.Tetrahedralize();
	}
}