using System;
using System.Diagnostics;
using CombatSystem;
using GGMLib.ModuleSystem;
using UnityEngine;
using UnityEngine.Events;

namespace Agents
{
    public class Agent : ModuleOwner, IDamageable
    {
        public bool IsDead { get; set; }
        public UnityEvent OnHit;
        public UnityEvent OnDeath;
        
        public HealthModule Health { get; private set; }
        public ActionDataModule ActionData { get; private set; }

        protected override void InitializeModules()
        {
            EnsureEvents();
            base.InitializeModules();
            Health = GetModule<HealthModule>();
            ActionData = GetModule<ActionDataModule>();
        }

        private void EnsureEvents()
        {
            if (OnHit == null)
                OnHit = new UnityEvent();

            if (OnDeath == null)
                OnDeath = new UnityEvent();
        }

        protected override void AfterInitModules()
        {
            base.AfterInitModules();
            Health.OnDeath += HandleDeath;
        }

        protected virtual void OnDestroy()
        {
            Health.OnDeath -= HandleDeath;
        }

        private void HandleDeath()
        {
            IsDead = true;
            OnDeath?.Invoke(); //Agent의 사망을 알림.
        }

        public void ApplyDamage(DamageData damageData)
        {
            if (IsDead) return;
            if (ActionData != null)
            {
                ActionData.HitPoint = damageData.HitPoint;
                ActionData.Attacker = damageData.Attacker;
            }
            OnHit?.Invoke();
            
            Health?.ApplyDamage(damageData.DamageAmount);
        }
    }
}