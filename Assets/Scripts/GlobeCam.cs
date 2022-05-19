using UnityEngine;
using UnityEngine.UI;

public class GlobeCam : MonoBehaviour
{
    [SerializeField] private bool _enableMovement;
    [SerializeField] private float _minRotateSpeed = 15.0f;
    [SerializeField] private float _maxRotateSpeed = 45.0f;
    [SerializeField] private float _rotSpeed = 45.0f;
    [SerializeField] private float _scrollZoomSpeed = 4.0f;
    [SerializeField] private float _maxZoomSpeed = 10.0f;
    [SerializeField] private float _closeUpZoomStart = 70.0f;
    [SerializeField] private float _minZoom = 60.0f;
    [SerializeField] private float _maxZoom = 150.0f;
    [SerializeField] private float _defaultZoom = 100.0f;
    [SerializeField] private float _zoomValue = 100.0f;
    [SerializeField] private Sheep _focusSheep;
    [SerializeField] private bool _followFocusTarget;
    [SerializeField] private float _followRotationLerpSpeed = 15.0f;
    [SerializeField] private float _rotationDragLerp = 2.0f;
    [SerializeField] private Joystick _joystick;

    private float _rotDragX;
    private float _rotDragY;
    private int _selectedSheep;
    private Vector2 _targetCamRotation;
    private Transform _xAxis;
    private Transform _yAxis;

    private void Start()
    {
        if (_yAxis == null)
        {
            _yAxis = new GameObject("GlobeCam_Y").transform;
            _yAxis.transform.position = Vector3.zero;
            _yAxis.transform.rotation = Quaternion.identity;

            _xAxis = new GameObject("GlobeCam_X").transform;
            _xAxis.parent = _yAxis;
            _xAxis.transform.position = Vector3.zero;
            _xAxis.transform.rotation = Quaternion.identity;

            transform.parent = _xAxis;
            transform.position = -_xAxis.forward * _defaultZoom;
        }
    }

    private void Update()
    {
        SelectTargetSheep();

        if (!_enableMovement) return;

        var inDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (_joystick != null)
        {
            var jDir = _joystick.Direction;
            if (jDir != Vector2.zero) inDir = jDir;
        }

        if (inDir.magnitude >= 0.05f) _followFocusTarget = false;

        _rotSpeed = Mathf.Lerp(_minRotateSpeed, _maxRotateSpeed, Mathf.InverseLerp(_minZoom, _maxZoom, _zoomValue));

        if (_followFocusTarget)
        {
            //  Follow the predefined target transform.
            _targetCamRotation = CalcAxisRotations(_focusSheep.transform.position);
            var followLerpSpeed = _followRotationLerpSpeed;
            _yAxis.localEulerAngles = new Vector3(0,
                Mathf.LerpAngle(_yAxis.localEulerAngles.y, _targetCamRotation.y, followLerpSpeed * Time.deltaTime),
                0);
            _xAxis.localEulerAngles =
                new Vector3(
                    Mathf.LerpAngle(_xAxis.localEulerAngles.x, _targetCamRotation.x,
                        followLerpSpeed * Time.deltaTime), 0, 0);
        }
        else
        {
            //  Follow via input axis.
            _rotDragX += inDir.x;
            _rotDragY += inDir.y;
            _rotDragX = _rotDragX.Clamp(-1.0f, 1.0f);
            _rotDragY = _rotDragY.Clamp(-1.0f, 1.0f);
            _rotDragX = Mathf.Lerp(_rotDragX, 0.0f, _rotationDragLerp * Time.deltaTime);
            _rotDragY = Mathf.Lerp(_rotDragY, 0.0f, _rotationDragLerp * Time.deltaTime);
            _yAxis.Rotate(-Vector3.up, _rotSpeed * Time.deltaTime * _rotDragX);
            _xAxis.Rotate(Vector3.right, _rotSpeed * Time.deltaTime * _rotDragY);
            _xAxis.localEulerAngles = new Vector3(_xAxis.transform.localEulerAngles.x.ClampAngle(0.0f, 89.0f), 0, 0);
        }

        float prevZoomValue = _zoomValue;
        _zoomValue -= Input.mouseScrollDelta.y * _scrollZoomSpeed;
        _zoomValue += (Input.GetKey(KeyCode.Q).ToFloat() - Input.GetKey(KeyCode.E).ToFloat()) * 0.25f;
        _zoomValue = Mathf.Clamp(_zoomValue, prevZoomValue - _maxZoomSpeed, prevZoomValue + _maxZoomSpeed);
        _zoomValue = _zoomValue.Clamp(_minZoom, _maxZoom);

        var closeUpLerp = Mathf.InverseLerp(_closeUpZoomStart, _minZoom, _zoomValue).Clamp(0.0f, 1.0f);
        transform.localPosition = Vector3.Lerp(new Vector3(0, 0, -1) * _zoomValue,
            new Vector3(0, 0, -1) * _zoomValue + new Vector3(0, -7.5f, 0), closeUpLerp);
        transform.localEulerAngles = Vector3.Lerp(Vector3.zero, new Vector3(-60.0f, 0, 0), closeUpLerp);
    }

    private void SelectTargetSheep()
    {
        if (Input.GetKeyDown(KeyCode.RightBracket))
            SelectNextSheep();
        else if (Input.GetKeyDown(KeyCode.LeftBracket)) SelectPrevSheep();
    }

    public void SelectNextSheep()
    {
        _selectedSheep++;
        _selectedSheep = _selectedSheep.Loop(0, GameManager.Instance.SheepList.Count - 1);
        SetFocusTarget(GameManager.Instance.SheepList[_selectedSheep]);
    }

    public void SelectPrevSheep()
    {
        _selectedSheep--;
        _selectedSheep = _selectedSheep.Loop(0, GameManager.Instance.SheepList.Count - 1);
        SetFocusTarget(GameManager.Instance.SheepList[_selectedSheep]);
    }

    public void SetFocusTarget(Sheep targ)
    {
        _focusSheep = targ;
        _followFocusTarget = false;
        if (_focusSheep != null) _followFocusTarget = true;
    }

    public void SetZoomFromSlider(Slider _slider)
    {
        _zoomValue = Mathf.Lerp(_minZoom, _maxZoom, _slider.value);
    }

    public Vector2 CalcAxisRotations(Vector3 pos)
    {
        return new Vector2(Mathf.Asin(pos.normalized.y) * Mathf.Rad2Deg,
            180 + pos.ToTopDownVec2().ToAngle());
    }
}