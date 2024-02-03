using UnityEngine;
using HJ.Scriptable;
using HJ.Tools;
using static HJ.Scriptable.SurfaceDetailsAsset;

namespace HJ.Runtime
{
    public class FootstepsSystem : PlayerComponent
    {
        public enum FootstepStyleEnum { Timed, HeadBob, Animation }

        [SerializeField] private SurfaceDetailsAsset _surfaceDetails;
        [SerializeField] private FootstepStyleEnum _footstepStyle;
        [SerializeField] private SurfaceDetectionEnum _surfaceDetection;
        [SerializeField] private LayerMask _footstepsMask;

        [SerializeField] private float _stepPlayerVelocity = 0.1f;
        [SerializeField] private float _jumpStepAirTime = 0.1f;

        [SerializeField] private float _walkStepTime = 1f;
        [SerializeField] private float _runStepTime = 1f;
        [SerializeField] private float _landStepTime = 1f;
        [SerializeField, Range(-1f, 1f)] private float _headBobStepWave = -0.9f;

        [SerializeField, Range(0, 1)] private float _walkingVolume = 1f;
        [SerializeField, Range(0, 1)] private float _runningVolume = 1f;
        [SerializeField, Range(0, 1)] private float _landVolume = 1f;

        private Collider _surfaceUnder;

        private bool _isWalking;
        private bool _isRunning;

        private int _lastStep;
        private int _lastLandStep;

        private float _stepTime;
        private bool _waveStep;

        private float _airTime;
        private bool _wasInAir;

        private string _currentSurfaceName;

        public FootstepStyleEnum FootstepStyle => _footstepStyle;

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            _surfaceUnder = _footstepsMask.CompareLayer(hit.gameObject.layer)
                ? hit.collider : null;
        }

        private void Update()
        {
            if(_stepTime > 0f) _stepTime -= Time.deltaTime;

            if (PlayerStateMachine.IsGrounded)
            {
                if (_surfaceUnder != null)
                {
                    SurfaceDetails? surfaceDetails = _surfaceDetails.GetSurfaceDetails(_surfaceUnder.gameObject, transform.position, _surfaceDetection);
                    if (_footstepStyle != FootstepStyleEnum.Animation && surfaceDetails.HasValue)
                        EvaluateFootsteps(surfaceDetails.Value);
                }
            }
            else
            {
                _airTime += Time.deltaTime;
                _wasInAir = true;
            }
        }

        private void EvaluateFootsteps(SurfaceDetails surfaceDetails)
        {
            float playerVelocity = PlayerCollider.velocity.magnitude;
            bool isCrouching = PlayerStateMachine.IsCurrent(PlayerStateMachine.CROUCH_STATE);
            _isWalking = PlayerStateMachine.IsCurrent(PlayerStateMachine.WALK_STATE);
            _isRunning = PlayerStateMachine.IsCurrent(PlayerStateMachine.RUN_STATE);

            if (isCrouching) return;

            if (_footstepStyle == FootstepStyleEnum.Timed)
            {
                if(_wasInAir)
                {
                    if(_airTime >= _landStepTime) 
                        PlayFootstep(surfaceDetails, true);

                    _airTime = 0;
                    _wasInAir = false;
                }
                else if(playerVelocity > _stepPlayerVelocity && _stepTime <= 0)
                {
                    PlayFootstep(surfaceDetails, false);
                    _stepTime = _isWalking ? _walkStepTime : _isRunning ? _runStepTime : 0;
                }
            }
            else if (_footstepStyle == FootstepStyleEnum.HeadBob)
            {
                if (_wasInAir)
                {
                    if (_airTime >= _landStepTime)
                        PlayFootstep(surfaceDetails, true);

                    _airTime = 0;
                    _wasInAir = false;
                }
                else if (playerVelocity > _stepPlayerVelocity)
                {
                    float yWave = PlayerManager.MotionController.BobWave;
                    if (yWave < _headBobStepWave && !_waveStep)
                    {
                        PlayFootstep(surfaceDetails, false);
                        _waveStep = true;
                    }
                    else if (yWave > _headBobStepWave && _waveStep)
                    {
                        _waveStep = false;
                    }
                }
            }
        }

        private void PlayFootstep(SurfaceDetails surfaceDetails, bool isLand)
        {            
            var footsteps = surfaceDetails.FootstepProperties;

            if (!isLand && /*_currentSurfaceName != surfaceDetails.SurfaceName &&*/ footsteps.SurfaceFootstep != null)
            {
                _currentSurfaceName = surfaceDetails.SurfaceName;
                footsteps.SurfaceFootstep.SetValue(gameObject);
                AudioManager.PostAudioEvent(AudioPlayer.PlayerFootstep, gameObject);

                //_lastStep = GameTools.RandomUnique(0, footsteps.SurfaceFootsteps.Length, _lastStep);
                //AudioClip footstep = footsteps.SurfaceFootsteps[_lastStep];
                //float multiplier = multipliers.FootstepsMultiplier;
                //float volumeScale = (_isWalking ? _walkingVolume : _isRunning ? _runningVolume : 0f) * multiplier;
                //_audioSource.PlayOneShot(footstep, volumeScale);
            }
            else if (footsteps.SurfaceLandStep != null)
            {
                footsteps.SurfaceLandStep.Post(gameObject);

                //_lastLandStep = GameTools.RandomUnique(0, footsteps.SurfaceLandSteps.Length, _lastLandStep);
                //AudioClip landStep = footsteps.SurfaceLandSteps[_lastLandStep];
                //float multiplier = multipliers.LandStepsMultiplier;
                //float volumeScale = _landVolume * multiplier;
                //_audioSource.PlayOneShot(landStep, volumeScale);
            }
        }

        public void PlayFootstep(bool runningStep)
        {
            if (_surfaceUnder == null)
                return;

            SurfaceDetails? surfaceDetails = _surfaceDetails.GetSurfaceDetails(_surfaceUnder.gameObject, transform.position, _surfaceDetection);
            if (surfaceDetails.HasValue)
            {
                _isWalking = !runningStep;
                _isRunning = runningStep;
                PlayFootstep(surfaceDetails.Value, false);
            }
        }

        public void PlayLandSteps()
        {
            if (_surfaceUnder == null)
                return;

            SurfaceDetails? surfaceDetails = _surfaceDetails.GetSurfaceDetails(_surfaceUnder.gameObject, transform.position, _surfaceDetection);
            if (surfaceDetails.HasValue)
            {
                PlayFootstep(surfaceDetails.Value, true);
            }
        }
    }
}