using UnityEngine;
using HJ.Scriptable;
using HJ.Tools;

namespace HJ.Runtime.States
{
    public class MonsterChaseState : AIStateAsset
    {
        public float RunSpeed = 3f;
        public float ChaseStoppingDistance = 1.5f;

        [Header("Chase")]
        public float LostPlayerPatrolTime = 5f;
        public float LostPlayerPredictTime = 1f;
        public float VeryClosePlayerDetection = 1.5f;
        public float UnstuckCheckingTime = 5f;

        [Header("Attack")]
        public float AttackFOV = 30f;
        public float AttackDistance = 2f;

        public override FSMAIState InitState(NPCStateMachine machine, AIStatesGroup group)
        {
            return new ChaseState(machine, group, this);
        }

        public override string GetStateKey() => ToString();

        public override string ToString() => "Chase";

        public class ChaseState : FSMAIState
        {
            private readonly MonsterStateGroup _group;
            private readonly MonsterChaseState _state;

            private bool _isChaseStarted;
            private bool _isPatrolPending;
            private bool _resetParameters;

            private float _waitTime;
            private float _predictTime;
            private bool _playerDied;

            private Vector3 _previousPosition;
            private float _notMovedTime;
            private bool _notMoving;
            private bool _isStuck;

            public ChaseState(NPCStateMachine machine, AIStatesGroup group, AIStateAsset state) : base(machine)
            {
                _group = (MonsterStateGroup)group;
                _state = (MonsterChaseState)state;

                machine.CatchMessage("Attack", () => AttackPlayer());
            }

            public override Transition[] OnGetTransitions()
            {
                return new Transition[]
                {
                    //Transition.To<MonsterPatrolState>(() => _waitTime > _state.LostPlayerPatrolTime || _playerDied)
                    Transition.To<MonsterSearchState>(() => _waitTime > _state.LostPlayerPatrolTime || _isStuck || _playerDied)
                };
            }

            public override void OnStateEnter()
            {
                _group.ResetAnimatorPrameters(_animator);
                _agent.speed = _state.RunSpeed;
                _agent.stoppingDistance = _state.ChaseStoppingDistance;
                _machine.RotateAgentManually = true;
                _machine.AIManager.AddNoise(_machine.Player.transform.position, 999f);
            }

            public override void OnStateExit()
            {
                _machine.RotateAgentManually = false;
                _isChaseStarted = false;
                _isPatrolPending = false;
                _resetParameters = false;
                _waitTime = 0f;

                _notMoving = false;
                _previousPosition = Vector3.zero;
                _isStuck = false;
            }

            public override void OnPlayerDeath()
            {
                _animator.ResetTrigger(_group.AttackTrigger);
                _playerDied = true;
            }

            public override void OnStateUpdate()
            {
                if (CheckForStuck()) return;
                _previousPosition = _agent.transform.position;
                
                if (PlayerInSights())
                {
                    if (!_resetParameters)
                    {
                        _group.ResetAnimatorPrameters(_animator);
                        _animator.SetBool(_group.RunParameter, true);
                        _resetParameters = true;
                    }

                    Chasing();
                    SetDestination(PlayerPosition);
                    _predictTime = _state.LostPlayerPredictTime;

                    if (PathDistanceCompleted())
                    {
                        _agent.isStopped = true;
                        _agent.velocity = Vector3.zero;
                        _animator.SetBool(_group.RunParameter, false);
                        _animator.SetBool(_group.IdleParameter, true);
                    }
                    else
                    {
                        _agent.isStopped = false;
                        _animator.SetBool(_group.RunParameter, true);
                        _animator.SetBool(_group.IdleParameter, false);
                        _animator.ResetTrigger(_group.AttackTrigger);
                    }

                    _isPatrolPending = false;
                    _isChaseStarted = true;
                    _waitTime = 0f;
                }
                else if(_predictTime > 0f)
                {
                    SetDestination(PlayerPosition);
                    _predictTime -= Time.deltaTime;
                }
                else
                {
                    if (!PathCompleted())
                        return;

                    if (!_isPatrolPending)
                    {
                        _group.ResetAnimatorPrameters(_animator);
                        _animator.SetBool(_group.PatrolParameter, true);
                        _agent.velocity = Vector3.zero;
                        _agent.isStopped = true;

                        _resetParameters = false;
                        _isPatrolPending = true;
                        _isChaseStarted = false;
                    }
                    else
                    {
                        _waitTime += Time.deltaTime;
                    }
                }
            }

            private void Chasing()
            {
                bool isAttacking = IsAnimation(1, _group.AttackState);
                //Debug.Log(InPlayerDistance(_state.AttackDistance) + " " + IsObjectInSights(_state.AttackFOV, PlayerPosition) + " " + !isAttacking + " " + !_playerHealth.IsDead);
                if(InPlayerDistance(_state.AttackDistance) && IsObjectInFOV(_state.AttackFOV, PlayerPosition) && !isAttacking && !_playerHealth.IsDead)
                {
                    Debug.Log("Should be attacking");
                    if (!_playerHealth.IsDead) _playerHealth.OnApplyDamage(999, _machine.transform);
                    _animator.SetTrigger(_group.AttackTrigger);
                }
            }

            private bool PlayerInSights()
            {
                if (_playerHealth.IsDead)
                    return false;

                if (!_isChaseStarted || _isPatrolPending)
                    return SeesPlayerOrClose(_state.VeryClosePlayerDetection);

                return SeesObject(_machine.SightsDistance, PlayerHead);
            }

            private void AttackPlayer()
            {
                if (!InPlayerDistance(_state.AttackDistance))
                    return;

                Debug.Log("Attacking player");
                int damage = _group.DamageRange.Random();
                _playerHealth.OnApplyDamage(damage, _machine.transform);
            }
            
            private bool CheckForStuck()
            {
                if (_notMoving && (Time.time > _notMovedTime + _state.UnstuckCheckingTime))
                {
                    _notMoving = false;
                    _isStuck = true;
                    //ResetNPC();
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
            
            // private void ResetNPC()
            // {
            //     _agent.Warp(_machine.AIManager.MonsterStartPosition);
            //     _agent.ResetPath();
            //     _agent.SetDestination(_machine.AIManager.LastNoiseLocation);
            // }
        }
    }
}