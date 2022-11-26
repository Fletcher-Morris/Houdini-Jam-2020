using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Pathing;
using Tick;

public class Sheep : MonoBehaviour, IManualUpdate, IFoodEater
{
    [SerializeField, Required] private UpdateManager _updateManager;

    [Space, Header("Movement")]
    [SerializeField] private bool _enableMovement = true;
    [SerializeField] private float _moveSpeed = 3.0f;
    [SerializeField] private float _rotationSpeed = 10.0f;
    [SerializeField] private float _bounceHeight = 1.0f;
    [SerializeField] private float _bounceSpeed = 4.0f;
    [SerializeField] private AnimationCurve _bounceAnimCurve;
    [SerializeField] private AnimationCurve _legAnimCurve;
    [SerializeField] private Transform _visual;
    [SerializeField] private Transform _legs;
    [SerializeField] private readonly float bounceTilt = 10.0f;
    [SerializeField] private readonly float legBounceAngle = 30.0f;
    private Vector3 _gravityDirection;
    [SerializeField] private bool _updateRotation = true;
    private Vector3 _movementVector;
    private Rigidbody _rigidBody;
    private float _bounceTimer;
    private float _bounceValue;
    private Vector3 _lastPosition;
    private Vector3 _lastTargetPosition;
    private float _legValue;
    private float _bounceDirection = 1.0f;

    [Space, Header("Audio")]
    [SerializeField] private List<AudioClip> m_bleats = new List<AudioClip>();
    [SerializeField] private float bleatChance = 0.5f;
    [SerializeField] private float m_bleatInterval = 5.0f;
    private float _bleatTimer = 5.0f;
    private AudioSource _audioSource;
    private float _bleatPitch = 1.0f;

    [Space, Header("Navigation")]
    [SerializeField] private Pathing.AiNavigator _navigator = new AiNavigator();
    [SerializeField] private float _updateWaypointInterval = 4.0f;
    [SerializeField] private LineRenderer _pathLineRenderer;
    [SerializeField] private BasicAiTarget _navTargetOverride;

    [Space, Header("Hunger")]
    [SerializeField] private bool _enableHunger = true;
    [SerializeField] private HungerState _hungerState;
    public enum HungerState { Default, Sated, SearchingForFood, MovingToFood, Eating}
    [SerializeField] private float _currentHungerValue = 1.0f;
    [SerializeField] private float _hungerDeleptionRate = 0.01f;
    [SerializeField] private Food _targetFood = null;
    [SerializeField] private float _eatTime = 1.0f;
    private float _eatTimer = 0;

    [Space, Header("Eyes")]
    [SerializeField] private EyePosition _eyes;
    [SerializeField] private EyePreset _movingEyesPreset;
    [SerializeField] private EyePreset _eatingEyesPreset;

    private void Awake()
    {
        _updateManager.AddToUpdateList(this);
    }

    private void Bouncing(float delta)
    {
        if (_movementVector.sqrMagnitude > 0 && _enableMovement)
        {
            _bounceTimer += delta * _bounceSpeed;
        }
        else
        {
            if (_bounceTimer + (delta * _bounceSpeed) <_bounceTimer.CeilToInt())
            {
                _bounceTimer += delta * _bounceSpeed;
            }
            else
            {
                _bounceTimer = 0;
            }
        }

        _bounceDirection = Mathf.Lerp(-1, 1, _bounceTimer.FloorToInt().IsEven().ToInt());
        _bounceValue = _bounceAnimCurve.Evaluate(_bounceTimer);
        _legValue = _legAnimCurve.Evaluate(_bounceTimer);
        _visual.transform.localPosition = new Vector3(0, _bounceValue * _bounceHeight, 0);
        _legs.localEulerAngles = new Vector3(Mathf.LerpAngle(0, legBounceAngle, _legValue), 0, 0);
        _visual.localEulerAngles = new Vector3(0, 0, Mathf.LerpAngle(0.0f, bounceTilt * _bounceDirection, _bounceValue));
    }

    private void Bleating(float delta)
    {
        if ((_bleatTimer -= delta) <= 0.0f)
        {
            _bleatTimer = m_bleatInterval;

            if (Random.Range(0.0f, 1.0f) <= bleatChance)
            {
                _audioSource.pitch = _bleatPitch;
                _audioSource.PlayOneShot(m_bleats.RandomItem());
            }
        }
    }

    private void SetHungerState(HungerState state)
    {
        switch (state)
        {
            case HungerState.Default:
                _eyes.ApplyAyePreset(_movingEyesPreset);
                break;
            case HungerState.Sated:
                _eyes.ApplyAyePreset(_movingEyesPreset);
                _targetFood = null;
                break;
            case HungerState.SearchingForFood:
                _eyes.ApplyAyePreset(_movingEyesPreset);
                break;
            case HungerState.MovingToFood:
                _eyes.ApplyAyePreset(_movingEyesPreset);
                break;
            case HungerState.Eating:
                _eyes.ApplyAyePreset(_eatingEyesPreset);
                break;
        }

        _hungerState = state;
    }

    private void Hunger(float delta)
    {
        if (_hungerState != HungerState.Eating)
        {
            _currentHungerValue -= delta * _hungerDeleptionRate;
            _currentHungerValue = Mathf.Max(0, _currentHungerValue);
        }

        switch (_hungerState)
        {
            case HungerState.Default:
                SetHungerState(HungerState.Sated);
                break;

            case HungerState.Sated:
                if (_currentHungerValue <= 0)
                {
                    SetHungerState(HungerState.SearchingForFood);
                }
                break;

            case HungerState.SearchingForFood:
                if (_enableHunger)
                {
                    _targetFood = null;
                    float closestDist = Mathf.Infinity;
                    foreach (Food food in FindObjectsOfType<Food>())
                    {
                        if (food.RemainingFood() >= 1 && food.BeingEatenBy().Count == 0)
                        {
                            float dist = Vector3.Distance(transform.position, food.transform.position);
                            if (dist < closestDist)
                            {
                                _targetFood = food;
                                closestDist = dist;
                            }
                        }
                    }
                    if (_targetFood != null)
                    {
                        _targetFood.AddEater(this);                        
                        _navigator.SetCurrentNavPosition(transform.position);
                        _navigator.SetTarget(_targetFood);
                        SetHungerState(HungerState.MovingToFood);
                    }
                }
                else
                {
                    SetHungerState(HungerState.Sated);
                }
                break;

            case HungerState.MovingToFood:
                if(_targetFood.BeingEatenBy().Count >= 2)
                {
                    _targetFood = null;
                    SetHungerState(HungerState.SearchingForFood);
                }
                else if (_navigator.HasReachedTarget())
                {
                    SetHungerState(HungerState.Eating);
                }
                break;

            case HungerState.Eating:
                if (_enableHunger)
                {
                    if ((_eatTimer -= delta) <= 0)
                    {
                        _eatTimer = _eatTime;
                        _targetFood.EatFromFood();
                        _currentHungerValue += 0.1f;

                        if (_currentHungerValue >= 1)
                        {
                            _targetFood.RemoveFoodEater(this);
                            _targetFood = null;
                            _navigator.SetTarget(null);
                            SetHungerState(HungerState.Sated);
                        }
                        else if (_targetFood.RemainingFood() <= 0)
                        {
                            _targetFood.RemoveFoodEater(this);
                            _targetFood = null;
                            _navigator.SetTarget(null);
                            SetHungerState(HungerState.SearchingForFood);
                        }
                    }
                }
                else
                {
                    SetHungerState(HungerState.Sated);
                }

                break;
        }
    }

    private void Movement(float delta)
    {
        if (_enableMovement == false) return;
        if (_navigator.HasReachedTarget())
        {

        }
        else if(_navigator.HasTarget())
        {
            Vector3 newPos = _navigator.Navigate(_moveSpeed * delta);
            transform.position = newPos;
        }

        if (_pathLineRenderer.positionCount > 0)
        {
            _pathLineRenderer.SetPosition(0, _navigator.GetNavPosition());
        }

        _movementVector = transform.position - _lastPosition;
        _lastPosition = transform.position;
    }

    private void Rotation(float delta)
    {
        if (_updateRotation && !_navigator.HasReachedTarget())
        {
            Vector3 lookAt;
            if (!_navigator.MoveTarget.Approximately(Vector3.zero))
            {
                lookAt = _navigator.MoveTarget;
                _lastTargetPosition = _navigator.MoveTarget;
            }
            else if (_lastTargetPosition != Vector3.zero && _lastTargetPosition != transform.position)
            {
                lookAt = _lastTargetPosition;
            }
            else
            {
                lookAt = Random.onUnitSphere.normalized;
                _lastTargetPosition = lookAt;
            }

            Vector3 forwardsVec = lookAt - ((-_gravityDirection) * Vector3.Dot(lookAt, -_gravityDirection));
            Quaternion newRotation = Quaternion.LookRotation(forwardsVec, -_gravityDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, _rotationSpeed * delta);
        }
    }

    public void CollectSheep()
    {
        GameManager.CollectSheep(this);
    }

    private void  OnReachedTarget(Vector3 vec)
    {
        _eyes.SetLookTarget(vec);
    }

    private List<Vector3> _navPoints = new List<Vector3>();

    public HungerState CurrentHungerState { get => _hungerState; }
    public float CurrentHungerValue { get => _currentHungerValue; }

    private void OnReachedWaypoint(Vector3 vec, int id)
    {
        int positionCount = Mathf.Max(0, (_navPoints.Count - id));
        _pathLineRenderer.positionCount = positionCount;
        int point = 1;
        for(int i = 0; i < _navPoints.Count; i++)
        {
            if (i < _navPoints.Count - 1)
            {
                _eyes.SetLookTarget(_navPoints[i]);
            }

            if (id < i)
            {
                _pathLineRenderer.SetPosition(point, _navPoints[i]);
                point++;
            }
        }

        if (_pathLineRenderer.positionCount > 0)
        {
            _pathLineRenderer.SetPosition(positionCount - 1, _navigator.GetTargetPos());
        }
    }

    private void OnRecalculatedPath(List<int> path)
    {
        _navPoints = new List<Vector3>();
        for (int i = 0; i < path.Count; i++)
        {
            AiWaypoint aiWaypoint = WaypointManager.Instance.GetWaypoint(path[i]);
            _navPoints.Add(aiWaypoint.Position);
        }
        OnReachedWaypoint(_navigator.GetNavPosition(), 0);
    }

    bool IManualUpdate.OnInitialise()
    {
        _audioSource = GetComponent<AudioSource>();
        _bleatPitch = Random.Range(0.8f, 1.2f);
        _rigidBody = GetComponent<Rigidbody>();
        GameManager.AddSheep(this);
        _navigator.Initialize(Random.Range(0.0f, _updateWaypointInterval), transform);
        _navigator.OnReachedTarget.AddListener(OnReachedTarget);
        _navigator.OnReachedWaypoint.AddListener(OnReachedWaypoint);
        _navigator.OnRecalculatedPath.AddListener(OnRecalculatedPath);
        _navigator.SetTarget(null);
        _navigator.SetCurrentNavPosition(WaypointManager.Instance.Closest(transform.position).Position);
        _currentHungerValue = 0;

        return true;
    }

    void IManualUpdate.OnManualUpdate(float delta)
    {
        Bouncing(delta);
        Bleating(delta);
        Movement(delta);
        _eyes.UpdateEyes(delta);

        if (WaypointManager.Instance.ShowNavigatorPaths != _pathLineRenderer.enabled)
        {
            _pathLineRenderer.enabled = WaypointManager.Instance.ShowNavigatorPaths;
        }
    }

    void IManualUpdate.OnTick(float delta)
    {
        Rotation(delta);
        Hunger(delta);

        if (_navTargetOverride != null)
        {
            _navigator.SetTarget(_navTargetOverride);
        }

        _navigator.Update(delta);
    }

    void IManualUpdate.OnManualFixedUpdate(float delta)
    {
        _gravityDirection = -transform.position.normalized;
        _rigidBody.AddForce(_gravityDirection * 10.0f);
    }

    UpdateManager IManualUpdate.GetUpdateManager()
    {
        return _updateManager;
    }

    public bool IsEnabled()
    {
        return gameObject.activeInHierarchy;
    }

    void IManualUpdate.OnApplicationQuit()
    {
    }
}