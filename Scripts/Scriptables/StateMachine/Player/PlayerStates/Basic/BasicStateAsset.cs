using UnityEngine;
using HJ.Scriptable;

namespace HJ.Runtime.States
{
    public class BasicStateAsset : PlayerStateAsset
    {
        public override FSMPlayerState InitState(PlayerStateMachine machine, PlayerStatesGroup group)
        {
            return new BasicPlayerState(machine, group);
        }
        
        public class BasicPlayerState : FSMPlayerState
        {
            protected readonly BasicMovementGroup _basicGroup;
            protected float _movementSpeed;

            public BasicPlayerState(PlayerStateMachine machine, PlayerStatesGroup group) : base(machine)
            {
                _basicGroup = (BasicMovementGroup)group;
            }

            public override void OnStateUpdate()
            {
                Vector3 wishDir = new Vector3(_machine.Input.x, 0, _machine.Input.y);
                wishDir = _cameraLook.RotationX * wishDir;

                if (_machine.IsGrounded)
                {
                    Friction(ref _machine.Motion);
                    Accelerate(ref _machine.Motion, wishDir, _movementSpeed);
                    _machine.Motion.y = -_machine.PlayerControllerSettings.AntiBumpFactor;
                }
                else
                {
                    AirAccelerate(ref _machine.Motion, wishDir, _movementSpeed);
                }

                ApplyGravity(ref _machine.Motion);
                PlayerHeightUpdate();
            }

            protected void Accelerate(ref Vector3 velocity, Vector3 wishDir, float wishSpeed)
            {
                // see if we are changing direction.
                float currentSpeed = Vector3.Dot(velocity, wishDir);

                // see how much to add.
                float addSpeed = wishSpeed - currentSpeed;

                // if not going to add any speed, done.
                if (addSpeed <= 0) return;

                // determine amount of accleration.
                float accelSpeed = _basicGroup.GroundAcceleration * wishSpeed * Time.deltaTime;

                // cap at addspeed.
                accelSpeed = Mathf.Min(accelSpeed, addSpeed);

                velocity += wishDir * accelSpeed;
            }

            protected void AirAccelerate(ref Vector3 velocity, Vector3 wishDir, float wishSpeed)
            {
                float wishspd = wishSpeed;

                // cap speed.
                wishspd = Mathf.Min(wishspd, _basicGroup.AirAccelerationCap);

                // see if we are changing direction.
                float currentSpeed = Vector3.Dot(velocity, wishDir);

                // see how much to add.
                float addSpeed = wishspd - currentSpeed;

                // if not going to add any speed, done.
                if (addSpeed <= 0) return;

                // determine amount of accleration.
                float accelSpeed = _basicGroup.AirAcceleration * wishSpeed * Time.deltaTime;

                // cap at addspeed.
                accelSpeed = Mathf.Min(accelSpeed, addSpeed);

                velocity += wishDir * accelSpeed;
            }

            protected void Friction(ref Vector3 velocity)
            {
                float speed = velocity.magnitude;

                if (speed != 0)
                {
                    float drop = speed * _basicGroup.Friction * Time.deltaTime;
                    velocity *= Mathf.Max(speed - drop, 0) / speed;
                }
            }

            protected bool SlopeCast(out Vector3 normal, out float angle)
            {
                if (Physics.SphereCast(_centerPosition, _controller.radius, Vector3.down, out RaycastHit hit, 
                        _basicGroup.SlideRayLength, _basicGroup.SlidingMask))
                {
                    normal = hit.normal;
                    angle = Vector3.Angle(hit.normal, Vector3.up);
                    return true;
                }

                normal = Vector3.zero;
                angle = 0f;
                return false;
            }
        }
    }
}
