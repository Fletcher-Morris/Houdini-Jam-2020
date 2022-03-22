using Pathing;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Tick;
using UnityEngine;

public class Food : MonoBehaviour, IManualUpdate, IAiTarget
{
    [SerializeField, Required] private UpdateManager _updateManager;
    [Space]
    [Header("Food Data")]
    [SerializeField, Min(0)] private int _maxFood = 10;
    [SerializeField, Min(0)] private int _currentFood = 10;
    [SerializeField, Min(0)] private float _growthRate = 0.1f;
    [SerializeField, Min(0)] private float _growthDelay = 10.0f;

    private float _growthDelayTimer = 0;
    private float _currentGrowth;
    private byte _clusterId;

    private List<IFoodEater> _eaters = new List<IFoodEater>();
    public void AddEater(IFoodEater foodEater)
    {
        if (!_eaters.Contains(foodEater)) _eaters.Add(foodEater);
    }
    public void RemoveFoodEater(IFoodEater foodEater)
    {
        if (_eaters.Contains(foodEater)) _eaters.Remove(foodEater);
    }
    public List<IFoodEater> BeingEatenBy()
    {
        return _eaters;
    }

    public byte ClusterId { get => _clusterId; }

    private void Awake()
    {
        GetUpdateManager().AddToUpdateList(this);
    }

    public UpdateManager GetUpdateManager()
    {
        return _updateManager;
    }

    public void OnInitialise()
    {
        _maxFood = Random.Range(_maxFood / 2, _maxFood);
        _currentGrowth = _currentFood;
        Pathing.AiWaypoint closestWaypoint = Pathing.WaypointManager.Instance.Closest(transform.position);
        _clusterId = closestWaypoint.Cluster;
        _growthDelayTimer = _growthDelay;
    }

    public void OnManualFixedUpdate(float delta)
    {

    }

    public void OnManualUpdate(float delta)
    {
        if(_currentFood < _maxFood)
        {
            if((_growthDelayTimer += delta) >= _growthDelay)
            {
                if((_currentGrowth += (_growthRate * delta)) < _maxFood)
                {
                    _currentFood = _currentGrowth.FloorToInt();
                }
            }
        }
    }

    public void OnTick(float delta)
    {

    }

    public void EatFromFood()
    {
        _currentFood--;
        _currentGrowth = _currentFood;
        _growthDelayTimer = 0;
    }

    public int RemainingFood()
    {
        return _currentFood;
    }

    void IAiTarget.SetPosition(Vector3 pos)
    {
        transform.position = pos;
    }

    Vector3 IAiTarget.GetPosition()
    {
        return transform.position;
    }

    bool IManualUpdate.IsEnabled()
    {
        return gameObject.activeInHierarchy;
    }
}
