using System;
using UnityEngine;

public class GameSpeedController : MonoBehaviour
{
    public static GameSpeedController Instance { get; private set; }

    [SerializeField] private float normalSpeed = 1f;
    [SerializeField] private float fastSpeed = 2f;
    [SerializeField] private float turboSpeed = 3f;
    [SerializeField] private float ultraSpeed = 4f;
    [SerializeField] private float hyperSpeed = 5f;

    public float CurrentSpeed { get; private set; } = 1f;
    public bool IsNormal => Mathf.Approximately(CurrentSpeed, normalSpeed);
    public bool IsFast => Mathf.Approximately(CurrentSpeed, fastSpeed);
    public bool IsTurbo => Mathf.Approximately(CurrentSpeed, turboSpeed);
    public bool IsUltra => Mathf.Approximately(CurrentSpeed, ultraSpeed);
    public bool IsHyper => Mathf.Approximately(CurrentSpeed, hyperSpeed);

    public event Action<float> OnSpeedChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ApplySpeed(normalSpeed);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void SetNormalSpeed() => ApplySpeed(normalSpeed);

    public void SetFastSpeed() => ApplySpeed(fastSpeed);

    public void SetTurboSpeed() => ApplySpeed(turboSpeed);

    public void SetUltraSpeed() => ApplySpeed(ultraSpeed);

    public void SetHyperSpeed() => ApplySpeed(hyperSpeed);

    public void RestoreCurrentSpeed()
    {
        ApplySpeed(CurrentSpeed);
    }

    public void ToggleSpeed()
    {
        if (IsFast)
            SetNormalSpeed();
        else
            SetFastSpeed();
    }

    private void ApplySpeed(float speed)
    {
        if (IsGameOver())
            return;

        CurrentSpeed = speed;
        Time.timeScale = speed;
        OnSpeedChanged?.Invoke(speed);
    }

    private static bool IsGameOver()
    {
        return (FieldEnemyTracker.Instance != null && FieldEnemyTracker.Instance.IsGameOver)
            || (LeakTracker.Instance != null && LeakTracker.Instance.IsGameOver);
    }
}
