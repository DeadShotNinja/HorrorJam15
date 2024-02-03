using UnityEngine;
using HJ.Scriptable;
using HJ.Input;

namespace HJ.Runtime.States
{
    public class JumpingStateAsset : BasicStateAsset
    {
        public override FSMPlayerState InitState(PlayerStateMachine machine, PlayerStatesGroup group)
        {
            return new JumpingPlayerState(machine, group);
        }

        public override string GetStateKey() => PlayerStateMachine.JUMP_STATE;

        public override string ToString() => "Jumping";

        public class JumpingPlayerState : BasicPlayerState
        {
            public JumpingPlayerState(PlayerStateMachine machine, PlayerStatesGroup group) : base(machine, group)
            {
            }

            public override void OnStateEnter()
            {
                _movementSpeed = _machine.Motion.magnitude;
                _machine.Motion.y = Mathf.Sqrt(_machine.PlayerBasicSettings.JumpHeight * -2f * GravityForce());

                if (_machine.PlayerFeatures.EnableStamina)
                {
                    float stamina = _machine.Stamina.Value;
                    stamina -= _machine.PlayerStamina.JumpExhaustion * 0.01f;
                    _machine.Stamina.OnNext(stamina);
                }
                
                if (AIManager.Instance != null)
                    AIManager.Instance.AddNoise(_machine.transform.position, 20f);
            }

            public override Transition[] OnGetTransitions()
            {
                return new Transition[]
                {
                    Transition.To<IdleStateAsset>(() => IsGrounded),
                    Transition.To<CrouchingStateAsset>(() =>
                    {
                        if(IsGrounded)
                        {
                            if (_machine.PlayerFeatures.CrouchToggle)
                            {
                                return InputManager.ReadButtonToggle("Crouch", Controls.CROUCH);
                            }

                            return InputManager.ReadButton(Controls.CROUCH);
                        }

                        return false;
                    }),
                    Transition.To<DeathStateAsset>(() => IsDead)
                };
            }
        }
    }
}
