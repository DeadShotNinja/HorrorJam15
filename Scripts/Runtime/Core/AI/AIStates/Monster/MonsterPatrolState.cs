using System;
using System.Linq;
using UnityEngine;
using HJ.Tools;
using HJ.Scriptable;

namespace HJ.Runtime.States
{
    public class MonsterPatrolState : AIStateAsset
    {
        public enum WaypointPatrolEnum { InOrder, Random }
        public enum PatrolTypeEnum { None, WaitTime }

        public WaypointPatrolEnum Patrol = WaypointPatrolEnum.InOrder;
        public PatrolTypeEnum PatrolType = PatrolTypeEnum.None;

        [Header("Settings")]
        public float PatrolTime = 3f;
        public float WalkSpeed = 0.5f;
        public float PatrolStoppingDistance = 1f;
        public float VeryClosePlayerDetection = 1f;

        public override FSMAIState InitState(NPCStateMachine machine, AIStatesGroup group)
        {
            return new PatrolState(machine, group, this);
        }

        public override string GetStateKey() => ToString();

        public override string ToString() => "Patrol";

        public class PatrolState : FSMAIState
        {
            private readonly MonsterStateGroup _group;
            private readonly MonsterPatrolState _state;

            private AIWaypointsGroup _waypointsGroup;
            private AIWaypoint _currWaypoint;
            private AIWaypoint _prevWaypoint;

            private float _waitTime;
            private bool _isWaypointSet;
            private bool _isPatrolPending;

            public PatrolState(NPCStateMachine machine, AIStatesGroup group, AIStateAsset state) : base(machine) 
            {
                _group = (MonsterStateGroup)group;
                _state = (MonsterPatrolState)state;
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
                var closestWaypointsGroup = FindClosestWaypointsGroup();
                _waypointsGroup = closestWaypointsGroup.Key;

                _agent.speed = _state.WalkSpeed;
                _agent.stoppingDistance = _state.PatrolStoppingDistance;
                _group.ResetAnimatorPrameters(_animator);
            }

            public override void OnStateExit()
            {
                _waitTime = 0f;
                _isWaypointSet = false;
                _isPatrolPending = false;

                if (_currWaypoint != null)
                    _currWaypoint.ReservedBy = null;
            }

            public override void OnStateUpdate()
            {
                if (_waypointsGroup == null)
                    return;

                if (!_isWaypointSet)
                {
                    SetNextWaypoint();
                    if(_currWaypoint != null)
                    {
                        Vector3 waypointPos = _currWaypoint.transform.position;
                        _agent.isStopped = false;
                        _agent.SetDestination(waypointPos);
                        _animator.SetBool(_group.WalkParameter, true);
                        _currWaypoint.ReservedBy = _machine.gameObject;
                    }

                    _isWaypointSet = true;
                }
                else
                {
                    if (!PathCompleted() && !_isPatrolPending)
                        return;

                    if (_state.PatrolType == PatrolTypeEnum.None)
                    {
                        _isWaypointSet = false;
                        _group.ResetAnimatorPrameters(_animator);
                    }
                    else if (_state.PatrolType == PatrolTypeEnum.WaitTime)
                    {
                        if (!_isPatrolPending)
                        {
                            _group.ResetAnimatorPrameters(_animator);
                            _animator.SetBool(_group.PatrolParameter, true);
                            _agent.velocity = Vector3.zero;
                            _agent.isStopped = true;
                            _isPatrolPending = true;
                        }
                        else
                        {
                            _waitTime += Time.deltaTime;

                            if (_waitTime > _state.PatrolTime)
                            {
                                _waitTime = 0f;
                                _isPatrolPending = false;
                                _isWaypointSet = false;
                                _group.ResetAnimatorPrameters(_animator);
                            }
                        }
                    }
                }
            }

            private void SetNextWaypoint()
            {
                _prevWaypoint = _currWaypoint;
                if(_prevWaypoint != null) 
                    _prevWaypoint.ReservedBy = null;

                var freeWaypoints = GetFreeWaypoints(_waypointsGroup);
                if(_state.Patrol == WaypointPatrolEnum.InOrder)
                {
                    if (_currWaypoint == null) _currWaypoint = freeWaypoints[0];
                    else
                    {
                        int currIndex = Array.IndexOf(freeWaypoints, _currWaypoint);
                        int nextIndex = currIndex + 1 >= freeWaypoints.Length ? 0 : currIndex + 1;
                        _currWaypoint = freeWaypoints[nextIndex];
                    }
                }
                else if(_state.Patrol == WaypointPatrolEnum.Random)
                {
                    freeWaypoints = freeWaypoints.Except(new[] { _prevWaypoint }).ToArray();
                    _currWaypoint = freeWaypoints.Random();
                }
            }
        }
    }
}