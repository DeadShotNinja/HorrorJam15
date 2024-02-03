using System;
using UnityEngine;

namespace HJ.Runtime.States
{
    [Serializable]
    public sealed class DeathStateData
    {
        [Header("Setup")]
        [SerializeField] private Vector3 _deathCameraPosition;
        [SerializeField] private Vector3 _deathCameraRotation;
        [SerializeField] private float _rotationChangeStart = 0.7f;
        [SerializeField] private float _deathChangeTime = 0.3f;
        
        public Vector3 DeathCameraPosition => _deathCameraPosition;
        public Vector3 DeathCameraRotation => _deathCameraRotation;
        public float RotationChangeStart => _rotationChangeStart;
        public float DeathChangeTime => _deathChangeTime;
    }
}
