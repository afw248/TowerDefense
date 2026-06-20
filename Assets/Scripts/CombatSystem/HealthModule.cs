using System;
using GGMLib.ModuleSystem;
using UnityEngine;

namespace CombatSystem
{
    public class HealthModule : MonoBehaviour, IModule
    {
        [field:SerializeField] public float maxHealth { get; set; }
        [SerializeField] private float currentHealth;
        
        private ModuleOwner _owner;

        public float CurrentHealth => currentHealth;
        public float HealthRatio => maxHealth > 0f ? currentHealth / maxHealth : 0f;

        public event Action OnDeath;
        
        public void Initialize(ModuleOwner owner)
        {
            _owner = owner;
            currentHealth = maxHealth; 
        }

        public void ApplyDamage(float damageAmount)
        {
            if (currentHealth <= 0f || damageAmount <= 0f)
                return;

            currentHealth -= damageAmount;
            if (currentHealth <= 0f)
            {
                currentHealth = 0f;
                OnDeath?.Invoke();
            }
        }

        public void ApplyWaveScaling(float multiplier)
        {
            if (multiplier <= 0f)
                return;

            maxHealth *= multiplier;
            currentHealth = maxHealth;
        }

        public void ApplyStatMultiplier(float multiplier)
        {
            if (multiplier <= 0f)
                return;

            maxHealth *= multiplier;
            currentHealth = maxHealth;
        }

        public void StatUp(float multply)
        {
            maxHealth *= multply;
        }
    }
}