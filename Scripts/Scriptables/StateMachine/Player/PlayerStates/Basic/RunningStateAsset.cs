using HJ.Scriptable;
using HJ.Input;
using UnityEngine;

namespace HJ.Runtime.States
{
    public class RunningStateAsset : BasicStateAsset
    {
        [field: SerializeField]
        public float NoiseGeneration { get; private set; } = 3f;

        public override FSMPlayerState InitState(PlayerStateMachine machine, PlayerStatesGroup group)
        {
            return new RunningPlayerState(machine, group, this);
        }

        public override string GetStateKey() => PlayerStateMachine.RUN_STATE;

        public override string ToString() => "Running";

        public class RunningPlayerState : BasicPlayerState
        {
            private readonly RunningStateAsset _asset;

            public RunningPlayerState(PlayerStateMachine machine, PlayerStatesGroup group, RunningStateAsset asset) : base(machine, group) 
            {
                _asset = asset;
            }

            public override void OnStateEnter()
            {
                _movementSpeed = _machine.PlayerBasicSettings.RunSpeed;
                _controllerState = _machine.StandingState;
            }

            public override void OnStateUpdate()
            {
                base.OnStateUpdate();

                // fix backwards running speed
                bool runSpeed = _machine.Input.y > 0 || _machine.Input is { y: > 0, x: > 0 };
                _movementSpeed = runSpeed ? _machine.PlayerBasicSettings.RunSpeed
                    : _machine.PlayerBasicSettings.RunSpeed * 0.5f;

                // generate AI noise
                if (AIManager.Instance != null)
                    AIManager.Instance.AddNoise(_machine.transform.position, _asset.NoiseGeneration * Time.deltaTime);

                if (StaminaEnabled)
                {
                    float stamina = _machine.Stamina.Value;
                    float exhaustionSpeed = runSpeed ? _machine.PlayerStamina.RunExhaustionSpeed : _machine.PlayerStamina.RunExhaustionSpeed * 0.5f;
                    stamina = Mathf.MoveTowards(stamina, 0f, Time.deltaTime * exhaustionSpeed);
                    _machine.Stamina.OnNext(stamina);
                }
            }

            public override Transition[] OnGetTransitions()
            {
                return new Transition[]
                {
                    Transition.To<IdleStateAsset>(() =>
                    {
                        return InputMagnitude <= 0;
                    }),
                    Transition.To<JumpingStateAsset>(() =>
                    {
                        bool jumpPressed = InputManager.ReadButtonOnce("Jump", Controls.JUMP);
                        return jumpPressed && (!StaminaEnabled || _machine.Stamina.Value > 0f);
                    }),
                    Transition.To<WalkingStateAsset>(() =>
                    {
                        if(InputMagnitude > 0)
                        {
                            if (_machine.PlayerFeatures.RunToggle)
                            {
                                bool runToggle = !InputManager.ReadButtonToggle("Run", Controls.SPRINT);
                                return runToggle || (StaminaEnabled && _machine.Stamina.Value <= 0f);
                            }

                            bool runUnPressed = !InputManager.ReadButton(Controls.SPRINT);
                            return runUnPressed || (StaminaEnabled && _machine.Stamina.Value <= 0f);
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
