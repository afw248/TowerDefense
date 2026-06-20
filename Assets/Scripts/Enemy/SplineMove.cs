using System;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(CharacterController))]
public class SplineMove : MonoBehaviour
{
    public event Action OnPathComplete;
    public event Action OnLooped;

    [Header("Spline")]
    [SerializeField] private SplineContainer splineContainer;

    [Header("Movement")]
    [field: SerializeField] public float moveSpeed { get; set; } = 5f;
    [SerializeField] private bool loopPath = true;

    [Header("Rotation")]
    [SerializeField] private bool rotateAlongPath = true;
    [SerializeField] private float rotationSpeed = 10f;

    private CharacterController _controller;
    private float _progress;
    private float _splineLength;
    private bool _completed;
    private float _groundY;
    private int _loopCount;
    private float _lastMoveDeltaSqr;

    public bool IsMoving => enabled && (loopPath || !_completed) && moveSpeed > 0f;
    public bool LoopsPath => loopPath;
    public int LoopCount => _loopCount;
    public float LastMoveDeltaSqr => _lastMoveDeltaSqr;

    public Vector3 GetEstimatedWorldVelocity()
    {
        if (!IsMoving || splineContainer == null || _splineLength <= 0f)
            return Vector3.zero;

        float normalizedProgress = loopPath
            ? _progress - Mathf.Floor(_progress)
            : Mathf.Clamp01(_progress);

        Vector3 tangent = splineContainer.EvaluateTangent(normalizedProgress);
        tangent.y = 0f;

        if (tangent.sqrMagnitude < 0.0001f)
            return Vector3.zero;

        return tangent.normalized * moveSpeed;
    }

    public void Path(SplineContainer spline)
    {
        splineContainer = spline;
        Initialize();
    }

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _groundY = transform.position.y;
    }

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        _controller ??= GetComponent<CharacterController>();

        if (splineContainer == null)
            return;

        _splineLength = splineContainer.CalculateLength();
        _progress = 0f;
        _loopCount = 0;
        _completed = false;
        _lastMoveDeltaSqr = 0f;
        _groundY = transform.position.y;
        enabled = _splineLength > 0f;
    }

    private void Update()
    {
        if (_splineLength <= 0f || splineContainer == null)
            return;

        _progress += (moveSpeed / _splineLength) * Time.deltaTime;
        _lastMoveDeltaSqr = 0f;

        float normalizedProgress;

        if (loopPath)
        {
            int lapIndex = Mathf.FloorToInt(_progress);
            if (lapIndex > _loopCount)
            {
                _loopCount = lapIndex;
                OnLooped?.Invoke();
            }

            normalizedProgress = _progress - lapIndex;
        }
        else
        {
            if (_progress >= 1f)
            {
                _progress = 1f;
                normalizedProgress = 1f;

                if (!_completed)
                {
                    _completed = true;
                    OnPathComplete?.Invoke();
                }

                enabled = false;
                return;
            }

            normalizedProgress = _progress;
        }
        Vector3 targetPosition = splineContainer.EvaluatePosition(normalizedProgress);
        Vector3 desiredPosition = new Vector3(targetPosition.x, _groundY, targetPosition.z);
        Vector3 delta = desiredPosition - transform.position;
        _lastMoveDeltaSqr = delta.sqrMagnitude;

        if (_lastMoveDeltaSqr > 0.0001f)
            _controller.Move(delta);

        if (rotateAlongPath)
        {
            Vector3 tangent = splineContainer.EvaluateTangent(normalizedProgress);
            tangent.y = 0f;

            if (tangent.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(tangent.normalized);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime);
            }
        }
    }
}
