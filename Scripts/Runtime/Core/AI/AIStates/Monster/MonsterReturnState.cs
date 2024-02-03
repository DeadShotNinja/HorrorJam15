using HJ.Scriptable;
using UnityEngine;

namespace HJ.Runtime.States
{
    public class MonsterReturnState : AIStateAsset
    {
        [Header("Settings")]
        public float VeryClosePlayerDetection = 1f;
        
        public override FSMAIState InitState(NPCStateMachine machine, AIStatesGroup group)
        {
            return new ReturnState(machine, group, this);
        }
        
        public override string GetStateKey() => ToString();

        public override string ToString() => "Return";

        public class ReturnState : FSMAIState
        {
            private readonly MonsterStateGroup _group;
            private readonly MonsterReturnState _state;

            private float _resetTimer;
            private float _minResetTime = 30f;
            
            public ReturnState(NPCStateMachine machine, AIStatesGroup group, AIStateAsset state) : base(machine)
            {
                _group = (MonsterStateGroup)group;
                _state = (MonsterReturnState)state;
            }
            
            public override Transition[] OnGetTransitions()
            {
                return new Transition[]
                {
                    Transition.To<MonsterChaseState>(() => (SeesPlayer() || InDistance(_state.VeryClosePlayerDetection, PlayerPosition)) && !_isPlayerDead)
                };
            }
            
            public override void OnStateEnter()
            {
                _group.ResetAnimatorPrameters(_animator);
                _agent.isStopped = false;
                _resetTimer = Time.time;
                SetDestination(_machine.AIManager.MonsterStartPosition);
            }

            public override void OnStateUpdate()
            {
                if (PathCompleted() || (_resetTimer + _minResetTime < Time.time))
                {
                    _machine.AIManager.DeSpawnMonster();
                }
            }
        }
    }
}