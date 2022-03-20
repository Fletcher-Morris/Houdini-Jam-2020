using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Pathing;
using Tick;

public class Sheep : MonoBehaviour, IManualUpdate
{
    [SerializeField, Required] private UpdateManager _updateManager;

    [Space]
    [Header("Movement")]
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

    [Space]
    [Header("Audio")]
    [SerializeField] private List<AudioClip> m_bleats = new List<AudioClip>();
    [SerializeField] private float bleatChance = 0.5f;
    [SerializeField] private float m_bleatInterval = 5.0f;
    private float _bleatTimer = 5.0f;
    private AudioSource _audioSource;
    private float _bleatPitch = 1.0f;

    [Space]
    [Header("Navigation")]
    [SerializeField] private Pathing.AiNavigator _navigator = new AiNavigator();
    [SerializeField] private float _updateWaypointInterval = 4.0f;

    [Space]
    [Header("Hunger")]
    [SerializeField] private HungerState _hungerState;
    public enum HungerState { Sated, SearchingForFood, MovingToFood, Eating}
    [SerializeField] private float _currentHungerValue = 1.0f;
    [SerializeField] private float _hungerDeleptionRate = 0.01f;
    [SerializeField] private Food _targetFood = null;
    [SerializeField] private float _eatTime = 1.0f;
    private float _eatTimer = 0;

    private void Awake()
    {
        _updateManager.AddToUpdateList(this);
    }

    private void Bouncing(float delta)
    {
        if (_movementVector.magnitude > 0)
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

    private void Hunger(float delta)
    {
        if (_hungerState != HungerState.Eating)
        {
            _currentHungerValue -= delta * _hungerDeleptionRate;
            _currentHungerValue = Mathf.Max(0, _currentHungerValue);
        }

        switch (_hungerState)
        {
            case HungerState.Sated:
                _targetFood = null;
                if(_currentHungerValue <= 0)
                {
                    _hungerState = HungerState.SearchingForFood;
                }
                break;

            case HungerState.SearchingForFood:
                _targetFood = null;
                float closestDist = Mathf.Infinity;
                foreach (Food food in FindObjectsOfType<Food>())
                {
                    if (food.RemainingFood() >= 1)
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
                    _hungerState = HungerState.MovingToFood;
                    _navigator.SetCurrentNavPosition(transform.position);
                    _navigator.SetTarget(_targetFood);
                }

                break;

            case HungerState.MovingToFood:
                if (_navigator.HasReachedTarget()) _hungerState = HungerState.Eating;
                break;

            case HungerState.Eating:
                if ((_eatTimer -= delta) <= 0)
                {
                    _eatTimer = _eatTime;
                    _targetFood.EatFromFood();
                    _currentHungerValue += 0.1f;

                    if (_currentHungerValue >= 1)
                    {
                        _targetFood = null;
                        _navigator.SetTarget(null);
                        _hungerState = HungerState.Sated;
                    }
                    else if (_targetFood.RemainingFood() <= 0)
                    {
                        _targetFood = null;
                        _navigator.SetTarget(null);
                        _hungerState = HungerState.SearchingForFood;
                    }
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
        else
        {
            Vector3 newPos = _navigator.Navigate(_moveSpeed * delta);
            transform.position = newPos;
            Physics.SyncTransforms();
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

            Vector3 forwardsVec = -Vector3.Cross(-_gravityDirection, Quaternion.AngleAxis(90.0f, -_gravityDirection) * lookAt).normalized;
            Quaternion newRotation = Quaternion.LookRotation(forwardsVec, -_gravityDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, _rotationSpeed * delta);
        }
    }

    public void CollectSheep()
    {
        GameManager.CollectSheep(this);
    }

    void IManualUpdate.OnInitialise()
    {
        _audioSource = GetComponent<AudioSource>();
        _bleatPitch = Random.Range(0.8f, 1.2f);
        _rigidBody = GetComponent<Rigidbody>();
        GameManager.AddSheep(this);
        _navigator.Initialize(Random.Range(0.0f, _updateWaypointInterval), transform);
        _navigator.SetTarget(null);
        _navigator.SetCurrentNavPosition(WaypointManager.Instance.Closest(transform.position).Position);
    }

    void IManualUpdate.OnManualUpdate(float delta)
    {
        Bouncing(delta);
        Bleating(delta);
        Movement(delta);
        Rotation(delta);
    }

    void IManualUpdate.OnTick(float delta)
    {
        Hunger(delta);

        _navigator.Update(delta);
        _navigator.DrawLines(transform.position, delta);
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
}