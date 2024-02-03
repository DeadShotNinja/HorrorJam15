using System;
using System.Collections.Generic;
using UnityEngine;
using HJ.Tools;
using HJ.Scriptable;

namespace HJ.Runtime
{
    [Serializable]
    public abstract class MotionModule
    {
        [Range(0f, 1f)]
        public float Weight = 1f;

        [NonSerialized] protected MotionPreset _preset;
        [NonSerialized] protected PlayerComponent _component;
        [NonSerialized] protected Transform _transform;
        [NonSerialized] protected string _state;

        [NonSerialized] protected CharacterController _controller;
        [NonSerialized] protected PlayerStateMachine _player;
        [NonSerialized] protected LookController _look;
        
        /// <summary>
        /// Runtime motion parameters.
        /// </summary>
        protected Dictionary<string, object> Parameters => _preset.RuntimeParameters;
        
        /// <summary>
        /// Check whether the module is updatable.
        /// </summary>
        protected bool IsUpdatable
        {
            get
            {
                string currentState = _player.StateName;
                return _state == MotionBlender.Default || _state == currentState;
            }
        }
        
        public virtual void Initialize(MotionSettings motionSettings)
        {
            _preset = motionSettings.Preset;
            _component = motionSettings.Component;
            _transform = motionSettings.MotionTransform;
            _state = motionSettings.MotionState;

            _controller = _component.PlayerCollider;
            _player = _component.PlayerStateMachine;
            _look = _component.LookController;
            _player.ObservableState.Subscribe(OnStateChange).HandleDisposable();
        }
        
        public abstract string Name { get; }

        public abstract void MotionUpdate(float deltaTime);
        public abstract Vector3 GetPosition(float deltaTime);
        public abstract Quaternion GetRotation(float deltaTime);

        public virtual void Reset() { }

        public virtual void OnStateChange(string state) { }

        protected abstract void SetTargetPosition(Vector3 target);
        protected abstract void SetTargetRotation(Vector3 target);

        protected abstract void SetTargetPosition(Vector3 target, float multiplier = 1f);
        protected abstract void SetTargetRotation(Vector3 target, float multiplier = 1f);
    }
}
