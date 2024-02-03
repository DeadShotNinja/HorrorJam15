using HJ.Scriptable;
using UnityEngine;

namespace HJ.Runtime.States
{
    public class MonsterSummonedState : AIStateAsset
    {
        [Header("Settings")]
        public float VeryClosePlayerDetection = 1f;
        public float MoveSpeed = 3f;
        public float StoppingDistance = 1f;
        public float UnstuckCheckingTime = 5f;
        
        public override FSMAIState InitState(NPCStateMachine machine, AIStatesGroup group)
        {
            return new SummonedState(machine, group, this);
        }
        
        public override string GetStateKey() => ToString();

        public override string ToString() => "Summoned";

        public class SummonedState : FSMAIState
        {
            private readonly MonsterStateGroup _group;
            private readonly MonsterSummonedState _state;
            
            private Vector3 _previousPosition;
            private float _notMovedTime;
            private bool _notMoving;
            private bool _isStuck;
            
            public SummonedState(NPCStateMachine machine, AIStatesGroup group, AIStateAsset state) : base(machine)
            {
                _group = (MonsterStateGroup)group;
                _state = (MonsterSummonedState)state;
            }
            
            public override Transition[] OnGetTransitions()
            {
                return new Transition[]
                {
                    Transition.To<MonsterChaseState>(() => (SeesPlayer() || InDistance(_state.VeryClosePlayerDetection, PlayerPosition)) && !_isPlayerDead),
                    Transition.To<MonsterSearchState>(() => PathCompleted() || _isStuck)
                };
            }
            
            public override void OnStateEnter()
            {
                _group.ResetAnimatorPrameters(_animator);
                _animator.SetBool(_group.RunParameter, true);
                _agent.speed = _state.MoveSpeed;
                _agent.stoppingDistance = _state.StoppingDistance;

                SetDestination(_machine.AIManager.LastNoiseLocation);
            }
            
            public override void OnStateUpdate()
            {
                if (CheckForStuck()) return;
                _previousPosition = _agent.transform.position;
            }

            public override void OnStateExit()
            {
                _previousPosition = Vector3.zero;
                _notMoving = false;
                _isStuck = false;
            }
            
            private bool CheckForStuck()
            {
                if (_notMoving && (Time.time > _notMovedTime + _state.UnstuckCheckingTime))
                {
                    _notMoving = false;
                    _isStuck = true;
                    return true;
                }
                
                if (_previousPosition != _agent.transform.position)
                {
                    _notMoving = false;
                }
                else if (!_notMoving)
                {
                    _notMovedTime = Time.time;
                    _notMoving = true;
                }

                return false;
            }
        }
    }
}