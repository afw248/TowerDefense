using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Splines;

public class SplineMove : MonoBehaviour
{
    [Header("Spline")]
    [SerializeField]private SplineContainer splineContainer;

    [Header("Movement")]
    [field: SerializeField] public float moveSpeed { get; set; } = 5f;

    [Header("Rotation")]
    [SerializeField] private bool rotateAlongPath = true;
    [SerializeField] private float rotationSpeed = 10f;

    private CharacterController _controller;

    private float _progress;
    private float _splineLength;

    public void Path(SplineContainer spline)
    {
        splineContainer = spline;
    }
    private void Start()
    {
        _controller = GetComponent<CharacterController>();

        if (splineContainer == null)
        {
            Debug.LogError("SplineContainer가 없습니다.");
            enabled = false;
            return;
        }

        _splineLength = splineContainer.CalculateLength();
    }

    private void Update()
    {
        MoveAlongSpline();
    }

    private void MoveAlongSpline()
    {
        if (_splineLength <= 0f)
            return;

        _progress += (moveSpeed / _splineLength) * Time.deltaTime;

        if (_progress > 1f)
            _progress -= 1f;

        Vector3 targetPosition =
            splineContainer.EvaluatePosition(_progress);

        Vector3 moveDirection =
            targetPosition - transform.position;
        
        moveDirection.y = 0f;


        _controller.Move(moveDirection);

        if (rotateAlongPath && moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(moveDirection.normalized);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime);
        }
    }
}
