using HJ.Scriptable;
using HJ.Input;

namespace HJ.Runtime.States
{
    public class IdleStateAsset : BasicStateAsset
    {
        public override FSMPlayerState InitState(PlayerStateMachine machine, PlayerStatesGroup group)
        {
            return new IdlePlayerState(machine, group);
        }
        
        public override string GetStateKey() => PlayerStateMachine.IDLE_STATE;
        
        public override string ToString() => "Idle";
        
        public class IdlePlayerState : BasicPlayerState
        {
            public override bool CanTransitionWhenDisabled => true;

            public IdlePlayerState(PlayerStateMachine machine, PlayerStatesGroup group) : base(machine, group) { }

            public override void OnStateEnter()
            {
                _movementSpeed = 0f;
                _controllerState = _machine.StandingState;
                InputManager.ResetToggledButton("Run", Controls.SPRINT);
            }

            public override Transition[] OnGetTransitions()
            {
                return new Transition[]
                {
                    Transition.To<WalkingStateAsset>(() =>
                    {
                        return InputMagnitude > 0;
                    }),
                    Transition.To<JumpingStateAsset>(() =>
                    {
                        bool jumpPressed = InputManager.ReadButtonOnce("Jump", Controls.JUMP);
                        return jumpPressed && (!StaminaEnabled || _machine.Stamina.Value > 0f);
                    }),
                    Transition.To<RunningStateAsset>(() =>
                    {
                        if(InputMagnitude > 0)
                        {
                            if (_machine.PlayerFeatures.RunToggle)
                            {
                                bool runToggle = InputManager.ReadButtonToggle("Run", Controls.SPRINT);
                                return runToggle && (!StaminaEnabled || _machine.Stamina.Value > 0f);
                            }

                            bool runPressed = InputManager.ReadButton(Controls.SPRINT);
                            return runPressed && (!StaminaEnabled || _machine.Stamina.Value > 0f);
                        }

                        return false;
                    }),
                    Transition.To<CrouchingStateAsset>(() =>
                    {
                        if (_machine.PlayerFeatures.CrouchToggle)
                        {
                            return InputManager.ReadButtonToggle("Crouch", Controls.CROUCH);
                        }

                        return InputManager.ReadButton(Controls.CROUCH);
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
        }
    }
}
