using Agents;
using CombatSystem;
using CoreSystem.EffectSystem;
using UnityEngine;

public abstract class AbstractBow : MonoBehaviour
{
    [SerializeField] private float speed = 13;
    [SerializeField] private LayerMask WhatEnemy;
    [SerializeField] private HitEffectDataSO hitEffect;
    [SerializeField] private float damage = 10f;

    private CharacterController charController;

    public virtual void Awake()
    {
        charController = GetComponent<CharacterController>();
    }

    public virtual void Update()
    {
        charController.Move(transform.forward * speed * Time.deltaTime);
    }

    public virtual void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.layer != LayerMask.NameToLayer("Enemy"))
            return;

        Vector3 hitPoint = hit.point;
        HitVfxUtility.Play(hitEffect, hitPoint, Quaternion.identity);

        Agent agent = hit.gameObject.GetComponentInParent<Agent>();
        if (agent != null)
            agent.ApplyDamage(new DamageData(damage, hitPoint, null));
        else
            hit.gameObject.GetComponentInChildren<HealthModule>()?.ApplyDamage(damage);

        Free();
    }

    public abstract void New();

    public abstract void Free();
}
