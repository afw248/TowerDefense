using Agents;
using CombatSystem;
using GGMLib.AnimatorSystem;
using System.Collections;
using UnityEngine;

public class Enemy : Agent
{
    [SerializeField] private AnimParamSO runAnim;
    [SerializeField] private AnimParamSO dieAnim;
    private IRenderer _renderer;
    private AgentTrigger _trigger;
    private float crossDuration = 0.15f;
    private bool _isDead;
    private bool _deathHandled;
    private int _lastSplineLoopCount;
    private Coroutine _dieFallbackRoutine;
    private CharacterController characterController;
    private EnemyHitFlash _hitFlash;
    public SplineMove spline { get;  private set; }

    protected override void InitializeModules()
    {
        base.InitializeModules();
        _trigger = GetModule<AgentTrigger>();
        _renderer = GetModule<IRenderer>();
        spline = GetComponent<SplineMove>();
        characterController = GetComponent<CharacterController>();
        EnemyColliderLayout.Apply(characterController, gameObject.name);
    }

    protected override void AfterInitModules()
    {
        base.AfterInitModules();

        if (spline != null)
        {
            spline.OnPathComplete += HandlePathComplete;
            spline.OnLooped += HandleSplineLooped;
            _lastSplineLoopCount = spline.LoopCount;
        }

        EnsureHitFlash();
        OnHit.AddListener(HandleHitFlash);
        OnHit.AddListener(HandleHitSound);
        if (Health != null)
            Health.OnDeath += HandleDeathSound;
        PlayRunAnimation();
    }

    private void EnsureHitFlash()
    {
        _hitFlash = GetComponent<EnemyHitFlash>();
        if (_hitFlash == null)
            _hitFlash = gameObject.AddComponent<EnemyHitFlash>();

        _hitFlash.Initialize();
    }

    private void HandleHitFlash()
    {
        if (_isDead)
            return;

        _hitFlash?.PlayFlash();
    }

    private void HandleHitSound()
    {
        if (_isDead)
            return;

        GameAudioManager.Instance?.PlayEnemyHit();
    }

    private void HandleDeathSound()
    {
        GameAudioManager.Instance?.PlayEnemyDeath();
    }

    private void PlayRunAnimation(bool forceRestart = false)
    {
        if (_isDead || _renderer == null || runAnim == null)
            return;

        if (!forceRestart && IsRunAnimationActive())
            return;

        _renderer.PlayClip(runAnim.ParamHash, 0, crossDuration);
    }

    private void HandleSplineLooped()
    {
        if (_isDead)
            return;

        PlayRunAnimation(forceRestart: true);
    }

    private void HandlePathComplete()
    {
        if (_isDead || spline == null || spline.LoopsPath)
            return;

        spline.moveSpeed = 0f;
    }

    private void Update()
    {
        if (_isDead || spline == null || runAnim == null || _renderer == null)
            return;

        if (!spline.IsMoving)
            return;

        if (spline.LoopCount != _lastSplineLoopCount)
        {
            _lastSplineLoopCount = spline.LoopCount;
            PlayRunAnimation(forceRestart: true);
            return;
        }

        if (spline.LastMoveDeltaSqr <= 0.0001f)
            return;

        if (!IsRunAnimationActive())
            PlayRunAnimation();
    }

    private bool IsRunAnimationActive()
    {
        Animator animator = _renderer?.Animator;
        if (animator == null || runAnim == null)
            return false;

        if (animator.IsInTransition(0))
        {
            AnimatorStateInfo nextState = animator.GetNextAnimatorStateInfo(0);
            return nextState.shortNameHash == runAnim.ParamHash;
        }

        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
        return currentState.shortNameHash == runAnim.ParamHash;
    }

    public void Remove()
    {
        if (_isDead)
            return;

        _isDead = true;
        UnsubscribeSplineEvents();

        if (spline != null)
        {
            spline.moveSpeed = 0f;
            spline.enabled = false;
        }

        if (characterController != null)
            characterController.enabled = false;

        if (_trigger != null)
        {
            _trigger.OnAnimationEnd -= HandleDieAnimationEnd;
            _trigger.OnAnimationEnd += HandleDieAnimationEnd;
        }

        PlayDieAnimation();

        if (_dieFallbackRoutine != null)
            StopCoroutine(_dieFallbackRoutine);
        _dieFallbackRoutine = StartCoroutine(DieFallbackRoutine());
    }

    private void PlayDieAnimation()
    {
        if (_renderer == null || dieAnim == null)
        {
            HandleDieAnimationEnd();
            return;
        }

        Animator animator = _renderer.Animator;
        if (animator != null)
            animator.speed = 1f;

        _renderer.PlayClip(dieAnim.ParamHash, 0, crossDuration);
        StartCoroutine(MaintainDieAnimationState());
    }

    private IEnumerator MaintainDieAnimationState()
    {
        if (crossDuration > 0f)
            yield return new WaitForSeconds(crossDuration + 0.05f);

        Animator animator = _renderer?.Animator;
        while (_isDead && animator != null && dieAnim != null)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.shortNameHash != dieAnim.ParamHash)
            {
                animator.Play(dieAnim.ParamHash, 0, 0f);
                animator.Update(0f);
            }
            else if (state.normalizedTime >= 0.98f)
            {
                animator.Play(dieAnim.ParamHash, 0, 1f);
                animator.speed = 0f;
                HandleDieAnimationEnd();
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator DieFallbackRoutine()
    {
        yield return new WaitForSeconds(2f);
        if (_isDead)
            HandleDieAnimationEnd();
    }

    private void UnsubscribeSplineEvents()
    {
        if (spline == null)
            return;

        spline.OnPathComplete -= HandlePathComplete;
        spline.OnLooped -= HandleSplineLooped;
    }

    private void HandleDieAnimationEnd()
    {
        if (_deathHandled)
            return;

        _deathHandled = true;

        if (_dieFallbackRoutine != null)
        {
            StopCoroutine(_dieFallbackRoutine);
            _dieFallbackRoutine = null;
        }

        if (_trigger != null)
            _trigger.OnAnimationEnd -= HandleDieAnimationEnd;

        OnDeathComplete();
    }

    protected virtual void OnDeathComplete()
    {
        GetComponent<EnemyEconomyBridge>()?.TryGrantKillReward();
        Destroy(gameObject);
    }

    protected virtual void OnDisable()
    {
        OnHit.RemoveListener(HandleHitFlash);
        OnHit.RemoveListener(HandleHitSound);
        if (Health != null)
            Health.OnDeath -= HandleDeathSound;
        UnsubscribeSplineEvents();

        if (_trigger != null)
            _trigger.OnAnimationEnd -= HandleDieAnimationEnd;

        if (_dieFallbackRoutine != null)
        {
            StopCoroutine(_dieFallbackRoutine);
            _dieFallbackRoutine = null;
        }
    }
}
