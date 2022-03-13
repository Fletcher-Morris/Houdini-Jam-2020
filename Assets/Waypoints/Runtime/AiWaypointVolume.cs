using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AiWaypointVolume : MonoBehaviour
{
    [SerializeField] private bool _refresh = false;
    [SerializeField] float _waypointDensity = 2.0f;
    [SerializeField] private List<Vector3> _points = new List<Vector3>();
    private Vector3 _storedPos;
    private Vector3 _storedScale;
    private float _storedDensity;

    public float WaypointDensity { get => _waypointDensity; set => _waypointDensity = value; }
    public List<Vector3> Points { get => _points; set => _points = value; }

    private void Update()
    {
        if (_storedPos != transform.position) _refresh = true;
        if (_storedScale != transform.localScale) _refresh = true;
        if (_storedDensity != _waypointDensity) _refresh = true;

        if (_refresh) GenPoints();
        DrawLines();
    }

    private void DrawLines()
    {
        Color col = Color.green;
        for(int i = 0; i < _points.Count - 1; i++)
        {
            Debug.DrawLine(_points[i], _points[i+1], col);
        }
    }

    public void GenPoints()
    {
        _refresh = false;

        _points = new List<Vector3>();
        int xPoints = (transform.localScale.x * _waypointDensity).RoundToInt();
        int yPoints = (transform.localScale.y * _waypointDensity).RoundToInt();
        int zPoints = (transform.localScale.z * _waypointDensity).RoundToInt();
        for(int x = 0; x < xPoints; x++)
        {
            for(int y = 0; y < yPoints; y++)
            {
                for (int z = 0; z < zPoints; z++)
                {
                    float xPos = (x / _waypointDensity);
                    float yPos = (y / _waypointDensity);
                    float zPos = (z / _waypointDensity);
                    Vector3 pos = new Vector3(xPos, yPos, zPos) + transform.position;
                    pos = pos.normalized;
                    pos = pos * Vector3.Distance(transform.position, Vector3.zero);
                    _points.Add(pos);
                }
            }
        }

        _storedPos = transform.position;
        _storedScale = transform.localScale;
        _storedDensity = _waypointDensity;
    }
}
