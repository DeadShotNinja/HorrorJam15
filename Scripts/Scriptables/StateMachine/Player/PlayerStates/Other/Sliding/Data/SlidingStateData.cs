using System;
using UnityEngine;

namespace HJ.Runtime.States
{
    [Serializable]
    public sealed class SlidingStateData
    {
        [Header("Setup")]
        [SerializeField] private float _slidingFriction = 2f;
        [SerializeField] private float _speedChange = 2f;
        [SerializeField] private float _motionChange = 2f;
        [SerializeField] private float _slideControlChange = 2f;
        [SerializeField] private bool _slideControl = true;
        
        public float SlidingFriction => _slidingFriction;
        public float SpeedChange => _speedChange;
        public float MotionChange => _motionChange;
        public float SlideControlChange => _slideControlChange;
        public bool SlideControl => _slideControl;
    }
}
