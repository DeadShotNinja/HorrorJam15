using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Disposables;
using UnityEngine;
using HJ.Tools;
using HJ.Scriptable;
using UnityEngine.AI;

namespace HJ.Runtime
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NPCStateMachine : MonoBehaviour
    {
        #region Getters / Setters
        private NavMeshAgent _navMeshAgent;
        public NavMeshAgent Agent
        {
            get
            {
                if (_navMeshAgent == null)
                    _navMeshAgent = GetComponent<NavMeshAgent>();

                return _navMeshAgent;
            }
        }

        private PlayerStateMachine _player;
        public PlayerStateMachine Player
        {
            get
            {
                if (_player == null)
                    _player = PlayerPresenceManager.Instance.Player.GetComponent<PlayerStateMachine>();

                return _player;
            }
        }

        private PlayerHealth _playerHealth;
        public PlayerHealth PlayerHealth
        {
            get
            {
                if (_playerHealth == null)
                    _playerHealth = Player.GetComponent<PlayerHealth>();

                return _playerHealth;
            }
        }

        private PlayerManager _playerManager;
        public PlayerManager PlayerManager
        {
            get
            {
                if (_playerManager == null)
                    _playerManager = Player.GetComponent<PlayerManager>();

                return _playerManager;
            }
        }

        private AIManager _aiManager;
        public AIManager AIManager
        {
            get
            {
                if (_aiManager == null)
                    _aiManager = AIManager.Instance;

                return _aiManager;
            }
        }

        public State? CurrentState => _currentState;

        public State? PreviousState => _previousState;

        public string CurrentStateKey => CurrentState?.StateData.StateAsset.GetStateKey();

        public bool RotateAgentManually { get; set; }
        #endregion

        public enum NPCTypeEnum { Enemy, Ally }

        public struct State
        {
            public AIStateData StateData;
            public FSMAIState FSMState;
        }

        public AIStatesGroup StatesAsset;
        public AIStatesGroup StatesAssetRuntime;

        public Animator Animator;
        public Transform HeadBone;
        public LayerMask SightsMask;
        public NPCTypeEnum NPCType;

        [Range(0, 179)] public float SightsFOV = 110;
        public float SightsDistance = 15;
        public float SteeringSpeed = 6f;

        public bool ShowDestination;
        public bool ShowSights;

        private MultiKeyDictionary<string, Type, State> _aiStates;
        private readonly Subject<string> _messages = new();
        private readonly CompositeDisposable _disposables = new();

        private State? _currentState;
        private State? _previousState;
        private bool _stateEntered;

        public bool IsPlayerDead { get; private set; }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }

        private void Awake()
        {
            _aiStates = new MultiKeyDictionary<string, Type, State>();
            StatesAssetRuntime = Instantiate(StatesAsset);

            if (StatesAsset != null)
            {
                // initialize all states
                foreach (var state in StatesAssetRuntime.GetStates(this))
                {
                    Type stateType = state.StateData.StateAsset.GetType();
                    string stateKey = state.StateData.StateAsset.GetStateKey();
                    _aiStates.Add(stateKey, stateType, state);
                }

                // select initial ai state
                if (_aiStates.Count > 0)
                {
                    _stateEntered = false;
                    ChangeState(_aiStates.SubDictionary.Keys.First());
                }
            }
        }

        private void Update()
        {
            if (!_stateEntered)
            {
                // enter state
                _currentState?.FSMState.OnStateEnter();
                _stateEntered = true;
            }
            else if (_currentState != null)
            {
                // update state
                _currentState?.FSMState.OnStateUpdate();

                // check state transitions
                if (_currentState.Value.FSMState.Transitions != null)
                {
                    foreach (var transition in _currentState.Value.FSMState.Transitions)
                    {
                        if (transition.Value && _currentState.GetType() != transition.NextState)
                        {
                            ChangeState(transition.NextState);
                            break;
                        }
                    }
                }
            }

            // player death event
            if(_currentState != null && !IsPlayerDead && PlayerHealth.IsDead)
            {
                _currentState.Value.FSMState.OnPlayerDeath();
                IsPlayerDead = true;
            }

            // agent rotation
            if (RotateAgentManually)
            {
                Agent.updateRotation = false;
                RotateManually();
            }
            else
            {
                Agent.updateRotation = true;
            }
        }

        /// <summary>
        /// Rotate agent manually.
        /// </summary>
        private void RotateManually()
        {
            Vector3 target = Agent.steeringTarget;
            Vector3 direction = (target - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.SlerpUnclamped(transform.rotation, lookRotation, Time.deltaTime * SteeringSpeed);
        }

        /// <summary>
        /// Send a message to the state machine so that you can catch it in the state.
        /// </summary>
        public void SendAnimationMessage(string message)
        {
            _messages.OnNext(message);
        }

        /// <summary>
        /// Catch animation messages to perform actions.
        /// </summary>
        public void CatchMessage(string message, Action action)
        {
            _disposables.Add(_messages.Where(msg => msg == message).Subscribe(_ => action?.Invoke()));
        }

        /// <summary>
        /// Change AI FSM state.
        /// </summary>
        public void ChangeState<TState>() where TState : AIStateAsset
        {
            if (_aiStates.TryGetValue(typeof(TState), out State state))
            {
                if ((_currentState == null || !_currentState.Value.Equals(state)) && state.StateData.IsEnabled)
                {
                    _currentState?.FSMState.OnStateExit();
                    if (_currentState.HasValue) _previousState = _currentState;
                    _currentState = state;
                    _stateEntered = false;
                }
                return;
            }

            throw new MissingReferenceException($"Could not find a state with type '{typeof(TState).Name}'");
        }

        /// <summary>
        /// Change AI FSM state.
        /// </summary>
        public void ChangeState(Type nextState)
        {
            if (_aiStates.TryGetValue(nextState, out State state))
            {
                if ((_currentState == null || !_currentState.Value.Equals(state)) && state.StateData.IsEnabled)
                {
                    _currentState?.FSMState.OnStateExit();
                    if (_currentState.HasValue) _previousState = _currentState;
                    _currentState = state;
                    _stateEntered = false;
                }
                return;
            }

            throw new MissingReferenceException($"Could not find a state with type '{nextState.Name}'");
        }

        /// <summary>
        /// Change AI FSM state.
        /// </summary>
        public void ChangeState(string nextState)
        {
            if (_aiStates.TryGetValue(nextState, out State state))
            {
                if ((_currentState == null || !_currentState.Value.Equals(state)) && state.StateData.IsEnabled)
                {
                    _currentState?.FSMState.OnStateExit();
                    if (_currentState.HasValue) _previousState = _currentState;
                    _currentState = state;
                    _stateEntered = false;
                }
                return;
            }

            throw new MissingReferenceException($"Could not find a state with key '{nextState}'");
        }

        /// <summary>
        /// Change AI FSM state and set the state data.
        /// </summary>
        public void ChangeState(string nextState, StorableCollection stateData)
        {
            if (_aiStates.TryGetValue(nextState, out State state))
            {
                if ((_currentState == null || !_currentState.Value.Equals(state)) && state.StateData.IsEnabled)
                {
                    _currentState?.FSMState.OnStateExit();
                    if (_currentState.HasValue) _previousState = _currentState;
                    state.FSMState.StateData = stateData;
                    _currentState = state;
                    _stateEntered = false;
                }
                return;
            }

            throw new MissingReferenceException($"Could not find a state with key '{nextState}'");
        }

        /// <summary>
        /// Check if current state is of the specified type.
        /// </summary>
        public bool IsCurrent(Type stateType)
        {
            if (!_currentState.HasValue)
                return false;
            
            return _currentState.Value.StateData.StateAsset.GetType() == stateType;
        }

        /// <summary>
        /// Check if current state matches the specified state key.
        /// </summary>
        public bool IsCurrent(string stateKey)
        {
            if (!_currentState.HasValue)
                return false;
            
            return _currentState.Value.StateData.StateAsset.GetStateKey() == stateKey;
        }

        private void OnDrawGizmosSelected()
        {
            if (!ShowSights)
                return;

            Vector3 fovLeftDir = Quaternion.AngleAxis(-SightsFOV / 2, Vector3.up) * transform.forward;
            Vector3 fovRightDir = Quaternion.AngleAxis(SightsFOV / 2, Vector3.up) * transform.forward;

            Gizmos.DrawRay(transform.position, fovLeftDir * SightsDistance);
            Gizmos.DrawRay(transform.position, fovRightDir * SightsDistance);
        }

        private void OnDrawGizmos()
        {
            if (!ShowDestination || !Application.isPlaying) 
                return;

            Vector3 targetPosition = Agent.destination;
            Color outerColor = Color.green;
            Color innerColor = Color.green.Alpha(0.01f);
            GizmosE.DrawDisc(targetPosition, 0.5f, outerColor, innerColor);
        }
    }
}