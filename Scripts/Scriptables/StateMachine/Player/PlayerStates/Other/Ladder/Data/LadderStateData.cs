using System;
using UnityEngine;

namespace HJ.Runtime.States
{
    [Serializable]
    public sealed class LadderStateData
    {
        [Header("Controls")]
        [SerializeField] private ControlsContext _ControlExit;
        
        [Header("Speed")]
        [SerializeField] private  float _onLadderSpeed = 1.5f;
        [SerializeField] private  float _toLadderSpeed = 3f;
        [SerializeField] private  float _bezierLadderSpeed = 3f;
        [SerializeField] private  float _bezierEvalSpeed = 1f;

        [Header("Distances")]
        [SerializeField] private  float _onLadderDistance = 0.1f;
        [SerializeField] private  float _endLadderDistance = 0.1f;

        [Header("Settings")]
        [SerializeField] private  float _ladderFrontAngle = 10f;
        [SerializeField] private  float _playerCenterOffset = 0.5f;
        [SerializeField] private  float _groundToLadderOffset = 0.1f;

        // TODO: These were temps for testing, need to switch to Wwise!
        // [Header("Sounds")]
        // [SerializeField, Range(0f, 1f)] private float FootstepsVolume = 1f;
        // [SerializeField] private  float LadderStepTime = 0.5f;
        // [SerializeField] private  AudioClip[] LadderFootsteps;
        
        public ControlsContext ControlExit => _ControlExit;
        
        public float OnLadderSpeed => _onLadderSpeed;
        public float ToLadderSpeed => _toLadderSpeed;
        public float BezierLadderSpeed => _bezierLadderSpeed;
        public float BezierEvalSpeed => _bezierEvalSpeed;

        public float OnLadderDistance => _onLadderDistance;
        public float EndLadderDistance => _endLadderDistance;

        public float LadderFrontAngle => _ladderFrontAngle;
        public float PlayerCenterOffset => _playerCenterOffset;
        public float GroundToLadderOffset => _groundToLadderOffset;
    }
}
