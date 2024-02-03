using UnityEngine;
using HJ.Input;
using HJ.Scriptable;

namespace HJ.Runtime.States
{
    public class CrouchingStateAsset : BasicStateAsset
    {
        public override FSMPlayerState InitState(PlayerStateMachine machine, PlayerStatesGroup group)
        {
            return new CrouchingPlayerState(machine, group);
        }

        public override string GetStateKey() => PlayerStateMachine.CROUCH_STATE;

        public override string ToString() => "Crouching";

        public class CrouchingPlayerState : BasicPlayerState
        {
            public CrouchingPlayerState(PlayerStateMachine machine, PlayerStatesGroup group) : base(machine, group)
            {
            }

            public override void OnStateEnter()
            {
                _movementSpeed = _machine.PlayerBasicSettings.CrouchSpeed;
                _controllerState = _machine.CrouchingState;
            }

            public override Transition[] OnGetTransitions()
            {
                return new Transition[]
                {
                    Transition.To<IdleStateAsset>(() =>
                    {
                        if(_gameManager.IsInventoryShown)
                            return false;

                        if (_machine.PlayerFeatures.CrouchToggle)
                        {
                            if(InputManager.ReadButtonOnce("Jump", Controls.JUMP))
                            {
                                InputManager.ResetToggledButtons();
                                return !CheckStandObstacle();
                            }

                            if(!InputManager.ReadButtonToggle("Crouch", Controls.CROUCH))
                                return !CheckStandObstacle();

                            return false;
                        }

                        if(!InputManager.ReadButton(Controls.CROUCH))
                        {
                            InputManager.ResetToggledButtons();
                            return !CheckStandObstacle();
                        }

                        return false;
                    }),
                    Transition.To<SlidingStateAsset>(() =>
                    {
                        if(SlopeCast(out _, out float angle))
                            return angle > _basicGroup.SlopeLimit;

                        return false;
                    }),
                    Transition.To<DeathStateAsset>(() => IsDead)
                };
            }

            private bool CheckStandObstacle()
            {
                float height = _machine.StandingState.ControllerHeight + 0.1f;
                float radius = _controller.radius;
                Vector3 origin = _machine.ControllerFeet;
                Ray ray = new(origin, Vector3.up);

                return Physics.SphereCast(ray, radius, out _, height, _machine.SurfaceMask);
            }
        }
    }
}
