using HJ.Scriptable;
using UnityEngine;

namespace HJ.Runtime.States
{
    public class MonsterSearchState : AIStateAsset
    {
        [Header("Settings")]
        public float VeryClosePlayerDetection = 1f;
        public float RotateCooldown = 1.5f;
        public float RotateSpeed = 2f;
        public float LoseInterestInterval = 1f;
        public float LoseInterestAmount = 10f;
        
        public override FSMAIState InitState(NPCStateMachine machine, AIStatesGroup group)
        {
            return new SearchState(machine, group, this);
        }
        
        public override string GetStateKey() => ToString();

        public override string ToString() => "Search";

        public class SearchState : FSMAIState
        {
            private readonly MonsterStateGroup _group;
            private readonly MonsterSearchState _state;

            private float _rotateTimer;
            private float _interestTimer;
            private bool _lostInterest;
            private Quaternion _targetRotation;
            
            public SearchState(NPCStateMachine machine, AIStatesGroup group, AIStateAsset state) : base(machine)
            {
                _group = (MonsterStateGroup)group;
                _state = (MonsterSearchState)state;
            }
            
            public override Transition[] OnGetTransitions()
            {
                //     return new Transition[]
                //     {
                //         Transition.To<MonsterChaseState>(() => (SeesPlayer() || InDistance(_state.VeryClosePlayerDetection, PlayerPosition)) && !_isPlayerDead),
                //         Transition.To<MonsterPatrolState>(() => _interestTimer + _state.LostInterestTime < Time.time)
                //     };
                
                return new Transition[]
                {
                    Transition.To<MonsterChaseState>(() => (SeesPlayer() || InDistance(_state.VeryClosePlayerDetection, PlayerPosition)) && !_isPlayerDead),
                    Transition.To<MonsterReturnState>(() => _lostInterest)
                };
            }
            
            public override void OnStateEnter()
            {
                _group.ResetAnimatorPrameters(_animator);
                _animator.SetBool(_group.IdleParameter, true);
                _rotateTimer = Time.time;
                _interestTimer = Time.time;
                SetRandomTargetRotation();
            }

            public override void OnStateUpdate()
            {
                if (_rotateTimer + _state.RotateCooldown < Time.time)
                {
                    SetRandomTargetRotation();
                    _rotateTimer = Time.time;
                }
                
                if (!_lostInterest && _interestTimer + _state.LoseInterestInterval < Time.time)
                {
                    _machine.AIManager.RemoveNoise(10f);
                    _interestTimer = Time.time;

                    if (_machine.AIManager.NoiseLevelEnum != AIManager.NoiseLevel.Red)
                    {
                        _lostInterest = true;
                    }
                }
                
                _agent.transform.rotation = Quaternion.Lerp(_agent.transform.rotation, _targetRotation, Time.deltaTime * _state.RotateSpeed);
            }

            public override void OnStateExit()
            {
                _lostInterest = false;
            }

            private void SetRandomTargetRotation()
            {
                float randomYRotation = Random.Range(0f, 360f);
                _targetRotation = Quaternion.Euler(0f, randomYRotation, 0f);
            }
        }
    }
}