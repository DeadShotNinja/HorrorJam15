using System;
using UnityEngine;

namespace HJ.Runtime
{
    /// <summary>
    /// Represents the base class for entity health functions.
    /// </summary>
    public abstract class BaseHealthEntity : MonoBehaviour, IHealthEntity
    {
        private int _health;
        
        public int EntityHealth
        {
            get => _health;
            set 
            {
                OnHealthChanged(_health, value);
                _health = value;

                if (_health <= 0 && !IsDead)
                {
                    OnHealthZero();
                    IsDead = true;
                    AudioManager.SetAudioState(AudioState.PlayerDead);
                }
                else if (_health > 0 && IsDead)
                {
                    IsDead = false;
                    AudioManager.SetAudioState(AudioState.GameActive);
                }

                if (_health >= MaxEntityHealth) OnHealthMax();
            }
        }

        public int MaxEntityHealth { get; set; }
        public bool IsDead = false;

        public void InitializeHealth(int health, int maxHealth = 100)
        {
            _health = health;
            MaxEntityHealth = maxHealth;
            OnHealthChanged(0, health);
            IsDead = false;
        }

        public virtual void OnApplyDamage(int damage, Transform sender = null)
        {
            if (IsDead) return;
            EntityHealth = Math.Clamp(EntityHealth - damage, 0, MaxEntityHealth);
        }

        public virtual void ApplyDamageMax(Transform sender = null)
        {
            if (IsDead) return;
            EntityHealth = 0;
        }

        public virtual void OnApplyHeal(int healAmount)
        {
            if (IsDead) return;
            EntityHealth = Math.Clamp(EntityHealth + healAmount, 0, MaxEntityHealth);
        }

        public virtual void ApplyHealMax()
        {
            if (IsDead) return;
            EntityHealth = MaxEntityHealth;
        }

        /// <summary>
        /// Override this method to define custom behavior when the entity health is changed.
        /// </summary>
        public virtual void OnHealthChanged(int oldHealth, int newHealth) { }

        /// <summary>
        /// Override this method to define custom behavior when the entity health is zero.
        /// </summary>
        public virtual void OnHealthZero() { }

        /// <summary>
        /// Override this method to define custom behavior when the entity health is maximum.
        /// </summary>
        public virtual void OnHealthMax() { }
    }
}
