using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using HJ.Tools;
using static UnityEngine.Object;
using static HJ.Runtime.NPCStateMachine;

namespace HJ.Runtime
{
    public class FSMAIState : FSMState
    {
        public Transition[] Transitions { get; private set; }
        public StorableCollection StateData { get; set; }

        public Vector3 PlayerPosition => _playerMachine.transform.position;
        public Vector3 PlayerHead => _playerManager.CameraHolder.transform.position;

        protected NPCStateMachine _machine;
        protected PlayerStateMachine _playerMachine;
        protected PlayerHealth _playerHealth;
        protected PlayerManager _playerManager;
        protected Animator _animator;
        protected NavMeshAgent _agent;

        /// <summary>
        /// Check if the player has died.
        /// </summary>
        protected bool _isPlayerDead => _machine.IsPlayerDead;

        private bool _reachedDistance;
        private Vector3 _lastPossibleDestination;

        public FSMAIState(NPCStateMachine machine)
        {
            _machine = machine;
            _playerMachine = machine.Player;
            _playerHealth = machine.PlayerHealth;
            _playerManager = machine.PlayerManager;
            _animator = machine.Animator;
            _agent = machine.Agent;
            Transitions = OnGetTransitions();
        }

        /// <summary>
        /// Get AI state transitions.
        /// </summary>
        public virtual Transition[] OnGetTransitions()
        {
            return new Transition[0];
        }

        /// <summary>
        /// Set destination of the agent.
        /// </summary>
        public bool SetDestination(Vector3 destination)
        {
            if (_agent.SetDestination(destination))
            {
                if (_agent.pathStatus != NavMeshPathStatus.PathPartial || _agent.pathStatus != NavMeshPathStatus.PathInvalid)
                {
                    _lastPossibleDestination = destination;
                    return true;
                }
            }

            if (_lastPossibleDestination != Vector3.zero)
                _agent.SetDestination(_lastPossibleDestination);

            return false;
        }

        /// <summary>
        /// Is the agent's path completed?
        /// </summary>
        public bool PathCompleted()
        {
            return _agent.remainingDistance <= _agent.stoppingDistance && _agent.velocity.sqrMagnitude <= 0.1f && !_agent.pathPending;
        }

        /// <summary>
        /// Is the agent's remaining distance less than the stopping distance?
        /// </summary>
        public bool PathDistanceCompleted()
        {
            if (_agent.remainingDistance <= _agent.stoppingDistance && !_reachedDistance) 
            {
                _reachedDistance = true;
                return true;
            }
            else if (_reachedDistance && _agent.remainingDistance < (_agent.stoppingDistance + 0.5f))
            {
                return true;
            }

            _reachedDistance = false;
            return false;
        }

        /// <summary>
        /// Can AI reach the destination?
        /// </summary>
        public bool IsPathPossible(Vector3 destination)
        {
            NavMeshPath path = new();
            _agent.CalculatePath(destination, path);
            return path.status != NavMeshPathStatus.PathPartial && path.status != NavMeshPathStatus.PathInvalid;
        }

        /// <summary>
        /// Does the AI see the object from the head position?
        /// </summary>
        public bool SeesObject(float distance, Vector3 position)
        {
            if (Vector3.Distance(_machine.transform.position, position) <= distance)
            {
                Vector3 headPos = _machine.HeadBone.position;
                return !Physics.Linecast(headPos, position, _machine.SightsMask, QueryTriggerInteraction.Collide);
            }

            return false;
        }

        /// <summary>
        /// Is the object in the AI field of view?
        /// </summary>
        public bool IsObjectInFOV(float FOV, Vector3 position)
        {
            // TODO: FINISH THIS!
            //Vector3 dir = position - _machine.transform.position;
            Vector3 dir = (position + new Vector3(0f, 1f, 0f)) - _machine.transform.position;
            
            //Debug.DrawRay(_machine.transform.position, _machine.transform.forward * 5f, Color.blue);
            //Debug.DrawRay(_machine.transform.position, dir * 5f, Color.red);
            
            return Vector3.Angle(_machine.transform.forward, dir) <= FOV * 0.5;
        }

        /// <summary>
        /// Is the object in the distance?
        /// </summary>
        public bool InDistance(float distance, Vector3 position)
        {
            return DistanceOf(position) <= distance;
        }

        /// <summary>
        /// Is the player in the distance?
        /// </summary>
        public bool InPlayerDistance(float distance)
        {
            return InDistance(distance, PlayerPosition);
        }

        /// <summary>
        /// Distance from AI to target.
        /// </summary>
        public float DistanceOf(Vector3 target)
        {
            return Vector3.Distance(_machine.transform.position, target);
        }

        /// <summary>
        /// Does the AI see the player from the head position using all the sights?
        /// </summary>
        public bool SeesPlayer()
        {
            bool isInvisible = (_machine.NPCType == NPCTypeEnum.Enemy && _playerHealth.IsInvisibleToEnemies)
                || (_machine.NPCType == NPCTypeEnum.Ally && _playerHealth.IsInvisibleToAllies);

            if (_playerHealth.IsDead || isInvisible)
                return false;

            bool seesPlayer = SeesObject(_machine.SightsDistance, PlayerHead);
            bool isPlayerInFOV = IsObjectInFOV(_machine.SightsFOV, PlayerPosition);
            return seesPlayer && isPlayerInFOV;
        }

        /// <summary>
        /// Does the AI see the player or sense the player's proximity?
        /// </summary>
        public bool SeesPlayerOrClose(float closeDistance)
        {
            return SeesPlayer() || InPlayerDistance(closeDistance);
        }

        /// <summary>
        /// Event when a player dies.
        /// </summary>
        public virtual void OnPlayerDeath() { }

        /// <summary>
        /// Check if the animation state is playing.
        /// </summary>
        public bool IsAnimation(int layerIndex, string stateName)
        {
            AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(layerIndex);
            return info.IsName(stateName);
        }

        /// <summary>
        /// Find closest waypoints group and waypoint.
        /// </summary>
        public Pair<AIWaypointsGroup, AIWaypoint> FindClosestWaypointsGroup()
        {
            AIWaypointsGroup[] allGroups = FindObjectsOfType<AIWaypointsGroup>();
            AIWaypointsGroup closestGroup = null;
            AIWaypoint closestWaypoint = null;
            float distance = Mathf.Infinity;

            foreach (var group in allGroups)
            {
                foreach (var waypoint in group.Waypoints)
                {
                    if(waypoint == null) 
                        continue;

                    Vector3 pointPos = waypoint.transform.position;
                    float waypointDistance = DistanceOf(pointPos);

                    if(waypointDistance < distance)
                    {
                        closestGroup = group;
                        closestWaypoint = waypoint;
                    }
                }
            }

            return new(closestGroup, closestWaypoint);
        }

        /// <summary>
        /// Retrieve unreserved waypoints from a group of waypoints.
        /// </summary>
        public AIWaypoint[] GetFreeWaypoints(AIWaypointsGroup group)
        {
            if (group == null || group.Waypoints.Count == 0)
                return null;

            return group.Waypoints.Where(x => x.ReservedBy == null).ToArray();
        }
    }
}