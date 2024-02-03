using System;
using UnityEngine;

namespace HJ.Runtime.States
{
    [Serializable]
    public sealed class PushingStateData
    {
        [Header("Controls")]
        [SerializeField] private ControlsContext _controlExit;
        
        [Header("Pushing Setup")]
        [SerializeField] private float _toMovableTime = 0.3f;
        [SerializeField] private float _pushingSpeed = 10f;
        
        public ControlsContext ControlExit => _controlExit;
        public float ToMovableTime => _toMovableTime;
        public float PushingSpeed => _pushingSpeed;
    }
}
