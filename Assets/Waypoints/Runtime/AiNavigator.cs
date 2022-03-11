using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AiNavigator
{
	public Transform target;
	[HideInInspector] public Transform self;
	public int prevWaypoint;
	public int nextWaypoint;
	public float waypointTollerance = 0.1f;
	public float pathRefreshInterval = 3.0f;
	public NavPathUpdateMode updateMode = NavPathUpdateMode.Always;

	public bool sphereRaycastTarget = true;
	public LayerMask raycastLayermask;

	[SerializeField] private bool debugLines = true;
	private bool initialized;
	[SerializeField] private AiWaypoint m_closestWaypointToTarget;
	private float m_pathRefreshTimer;
	private Transform m_prevTarget;
	private Vector3 m_prevTargetPosition;
	public List<ushort> pathFound = new List<ushort>();

	[HideInInspector] private Vector3 targetPosition;

	public void Initialize(float offset, Transform setSelf)
	{
		if (!Application.isPlaying) return;
		m_pathRefreshTimer = offset;
		self               = setSelf;
		initialized        = true;
	}

	public void SetTarget(Transform newTarget)
	{
		if (updateMode == NavPathUpdateMode.TargetChanged) RecalculatePath();
		target = newTarget;
		if (pathFound == null) return;
		prevWaypoint = -1;
		nextWaypoint = 1;
	}

	public void SetSelfTransform(Transform newTransform)
	{
		self = newTransform;
	}

	public void Update(float delta)
	{
		if (!initialized) return;
		m_pathRefreshTimer -= delta;
		if (m_pathRefreshTimer <= 0.0f)
		{
			m_pathRefreshTimer = pathRefreshInterval;
			RecalculateCheck(delta);
		}
	}

	private void RecalculateCheck(float delta)
	{
		if (!initialized)
		{
			Debug.LogWarning("Not initialized!");
			return;
		}

		switch (updateMode)
		{
			case NavPathUpdateMode.Manual:
				break;
			case NavPathUpdateMode.TargetChanged:
				if (m_prevTarget != target && target != null) RecalculatePath(target.position);
				break;
			case NavPathUpdateMode.TargetPositionChanged:
				if (target != null)
					if (m_prevTargetPosition != target.position)
						RecalculatePath();
				break;
			case NavPathUpdateMode.Always:
			{
				if (target != null && self != null)
				{
					var distToTarget = self.Distance(target);
					if (distToTarget > waypointTollerance)
					{
						var newWaypoint = WaypointManager.Closest(target.position);
						if (newWaypoint != null)
						{
							if (m_closestWaypointToTarget == null)
								RecalculatePath();
							else if (newWaypoint.Id != m_closestWaypointToTarget.Id) RecalculatePath();
							m_closestWaypointToTarget = newWaypoint;
						}
					}
				}
			}
				break;
		}
	}

	public void RecalculatePath()
	{
		if (target == null)
		{
			Debug.LogWarning("TARGET IS NULL!");
			return;
		}

		m_prevTargetPosition = target.transform.position;
		RecalculatePath(target.transform.position);
	}

	public void RecalculatePath(Vector3 end)
	{
		if (!initialized)
		{
			Debug.LogWarning("Not initialized!");
			return;
		}

		if (self == null)
		{
			Debug.LogWarning("Self transform is NULL!");
			return;
		}

		pathFound    = null;
		m_prevTarget = target;

		if (sphereRaycastTarget)
		{
			RaycastHit hit;
			if (Physics.Raycast(end.normalized * 1000, -end.normalized, out hit, 1000, raycastLayermask))
			{
				if (debugLines && WaypointManager.Instance.LineDebugOpacity > 0.0f)
					Debug.DrawLine(end.normalized * 1000, hit.point,
						Color.yellow * WaypointManager.Instance.LineDebugOpacity, pathRefreshInterval);
				end = hit.point;
			}
		}

		if (target != null)
		{
			m_prevTargetPosition = end;
			pathFound = WaypointManager.GetPath(self.position, end);
			if (pathFound == null)
			{
				Debug.LogWarning($"Could not find path from '{self.position}' to '{end}'!");
				return;
			}

			prevWaypoint = -1;
			nextWaypoint = 1;
		}
	}

	public ushort GetWaypointFromIndex(int index)
	{
		if (index == -1 || index >= pathFound.Count || pathFound.Count == 0) return 0;
		return pathFound[index];
	}

	public void DrawLines(Vector3 start, float delta)
	{
		if (!initialized) return;
		if (pathFound == null) return;
		if (WaypointManager.Instance.LineDebugOpacity <= 0.0f) return;
		if (pathFound.Count > 0)
		{
            Color lineCol = Color.white;
			lineCol.a = WaypointManager.Instance.LineDebugOpacity * 5;
            ushort n = GetWaypointFromIndex(nextWaypoint);
			if (n != 0 && debugLines) Debug.DrawLine(start, WaypointManager.GetWaypoint(n).Position, lineCol);
			for (int i = 0; i < pathFound.Count - 1; i++)
			{
				if (pathFound[i] != 0)
                {
                    Debug.DrawLine(WaypointManager.GetWaypoint(pathFound[i]).Position, WaypointManager.GetWaypoint(pathFound[i + 1]).Position, lineCol);
                }
            }
		}
	}
}

[Serializable]
public enum NavPathUpdateMode
{
	//  Recalculate is called manually.
	Manual,

	//  Recalculate when the target changes.
	TargetChanged,

	//  Recalculate when the target's position changes.
	TargetPositionChanged,

	//  Recalculate every cycle.
	Always
}