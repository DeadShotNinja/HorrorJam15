using UnityEngine;
using HJ.Input;
using HJ.Scriptable;

namespace HJ.Runtime.States
{
    public class SlidingStateAsset : BasicStateAsset
    {
        [SerializeField] private SlidingStateData _stateData;

        public override FSMPlayerState InitState(PlayerStateMachine machine, PlayerStatesGroup group)
        {
            return new SlidingPlayerState(machine, group, _stateData);
        }

        public override string GetStateKey() => PlayerStateMachine.SLIDING_STATE;

        public override string ToString() => "Sliding";

        public class SlidingPlayerState : BasicPlayerState
        {
            private readonly SlidingStateData _data;

            private bool isSliding;
            private float slidingSpeed;

            private Vector3 enterMotion;
            private float motionToSlidingBlend;

            public SlidingPlayerState(PlayerStateMachine machine, PlayerStatesGroup group, SlidingStateData data) : base(machine, group)
            {
                _data = data;
            }

            public override void OnStateEnter()
            {
                _controllerState = _machine.StandingState;
                slidingSpeed = _machine.Motion.magnitude;
                enterMotion = _machine.Motion;
                motionToSlidingBlend = 0f;
                InputManager.ResetToggledButtons();
            }

            public override void OnStateUpdate()
            {
                bool sliding = SlopeCast(out Vector3 normal, out float angle);
                isSliding = sliding && angle > _basicGroup.SlopeLimit;

                Vector3 slidingForward = Vector3.ProjectOnPlane(Vector3.down, normal);
                Vector3 slidingRight = Vector3.Cross(normal, slidingForward);

                Vector3 slidingDirection = slidingForward;
                if (_data.SlideControl) slidingDirection += _machine.Input.x * _data.SlideControlChange * slidingRight;

                slidingSpeed = Mathf.MoveTowards(slidingSpeed, _data.SlidingFriction, Time.deltaTime * _data.SpeedChange);
                slidingDirection = slidingDirection.normalized * slidingSpeed;

                motionToSlidingBlend = Mathf.MoveTowards(motionToSlidingBlend, 1f, Time.deltaTime * _data.MotionChange);
                Vector3 finalMotion = Vector3.Lerp(enterMotion, slidingDirection, motionToSlidingBlend);

                _machine.Motion = finalMotion;
                PlayerHeightUpdate();
            }

            public override Transition[] OnGetTransitions()
            {
                return new Transition[]
                {
                    Transition.To<IdleStateAsset>(() => !isSliding),
                    Transition.To<DeathStateAsset>(() => IsDead)
                };
            }
        }
    }
}
