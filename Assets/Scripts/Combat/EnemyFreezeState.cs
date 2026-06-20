using UnityEngine;

[DisallowMultipleComponent]
public class EnemyFreezeState : MonoBehaviour
{
    private SplineMove _spline;
    private float _freezeUntilUnscaled;
    private float _savedSpeed;
    private bool _isFrozen;

    public bool IsFrozen => _isFrozen;

    public void ApplyFreeze(float durationSeconds)
    {
        if (durationSeconds <= 0f)
            return;

        _spline ??= GetComponent<SplineMove>();
        if (_spline == null)
            return;

        float until = Time.unscaledTime + durationSeconds;
        if (!_isFrozen)
        {
            _savedSpeed = _spline.moveSpeed;
            _isFrozen = true;
        }

        _freezeUntilUnscaled = Mathf.Max(_freezeUntilUnscaled, until);
        _spline.moveSpeed = 0f;
    }

    private void Update()
    {
        if (!_isFrozen || _spline == null)
            return;

        if (Time.unscaledTime < _freezeUntilUnscaled)
            return;

        _spline.moveSpeed = _savedSpeed;
        _isFrozen = false;
    }

    private void OnDisable()
    {
        if (!_isFrozen || _spline == null)
            return;

        _spline.moveSpeed = _savedSpeed;
        _isFrozen = false;
    }
}
