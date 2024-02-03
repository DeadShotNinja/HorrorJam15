using HJ.Tools;
using UnityEngine;

namespace HJ.Runtime.States
{
    public class LadderController
    {
        private LadderStateAsset.LadderPlayerState _playerState;
        private PlayerStateMachine _machine;

        public LadderController(LadderStateAsset.LadderPlayerState playerState, PlayerStateMachine machine)
        {
            _playerState = playerState;
            _machine = machine;
        }
        
        public LadderPositions GetLadderPositions(StorableCollection stateData)
        {
            LadderPositions ladderPos;
            
            ladderPos.Start = (Vector3)stateData["start"];
            ladderPos.End = (Vector3)stateData["end"];
            ladderPos.Exit = (Vector3)stateData["exit"];
            ladderPos.Arc = (Vector3)stateData["arc"];

            return ladderPos;
        }

        public void UpdateLadderMotion(Vector3 centerPos)
        {
            if (!_playerState.PlayerMoved)
            {
                if (_playerState.ClimbDown || _playerState.ExitLadder)
                {
                    _playerState.BezierEval += _playerState.Data.BezierEvalSpeed * Time.deltaTime;
                    _playerState.BezierEval = Mathf.Clamp01(_playerState.BezierEval);

                    // use QuadraticBezier to move player to the desired position
                    Vector3 bezierPosition = VectorExtension.QuadraticBezier(_playerState.StartPosition, _playerState.MovePosition, 
                        _playerState.LadderPositions.Arc, _playerState.BezierEval);
                    centerPos = Vector3.MoveTowards(centerPos, bezierPosition, 
                        Time.deltaTime * _playerState.Data.BezierLadderSpeed);
                    _machine.Motion = Vector3.zero;
                }
                else
                {
                    Vector3 ladderMotion = (_playerState.MovePosition - centerPos).normalized;
                    _machine.Motion = _playerState.Data.ToLadderSpeed * ladderMotion;
                }

                if (_playerState.ExitLadder)
                {
                    _playerState.ExitState = Vector3.Distance(centerPos, _playerState.MovePosition) <= _playerState.Data.EndLadderDistance;
                }
                else
                {
                    _playerState.PlayerMoved = Vector3.Distance(centerPos, _playerState.MovePosition) <= _playerState.Data.OnLadderDistance;
                }
            }
            else
            {
                LadderClimbUpdate(centerPos);
            }
        }
        
        private void LadderClimbUpdate(Vector3 centerPos)
        {
            Vector3 playerPos = new Vector3(centerPos.x, 0, centerPos.z);
            Vector3 ladderPos = new Vector3(_playerState.LadderPositions.Start.x, 0, _playerState.LadderPositions.Start.z);
            Vector3 direction = ladderPos - playerPos;
            Vector3 ladderMotion = direction.normalized;
            int magnitude = direction.magnitude > .05f ? 1 : 0;

            // assign ladder motion
            ladderMotion.x *= _playerState.Data.ToLadderSpeed * magnitude;
            ladderMotion.z *= _playerState.Data.ToLadderSpeed * magnitude;
            ladderMotion.y = _machine.Input.y * _playerState.Data.OnLadderSpeed;

            // set ladder motion to machine
            _machine.Motion = ladderMotion;

            // check if player should climb up and exit ladder
            if (LadderUtility.LadderEval(_playerState.OffsetedCenter, _playerState.LadderPositions.Start, _playerState.LadderPositions.End) >= 1f)
            {
                _playerState.StartPosition = centerPos;
                _playerState.MovePosition = _playerState.LadderPositions.Exit;
                _playerState.PlayerMoved = false;
                _playerState.ExitLadder = true;
                _playerState.BezierEval = 0;
            }

            // check if player touches ground to exit ladder
            _playerState.ExitState = _machine.IsGrounded;

            // TODO: This was used for testing, integrate properly with Wwise. Also need to move this somewhere else
            //if (_stepTime > 0) _stepTime -= Time.deltaTime;
            //else if (_data.LadderFootsteps.Length > 0 && Mathf.Abs(_machine.Input.y) > 0)
            //{
            //    _lastStep = GameTools.RandomUnique(0, _data.LadderFootsteps.Length, _lastStep);
            //    AudioClip footstep = _data.LadderFootsteps[_lastStep];
            //    _audioSource.PlayOneShot(footstep, _data.FootstepsVolume);
            //    _stepTime = _data.LadderStepTime;
            //}
        }
    }
}
