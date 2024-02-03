using System;
using System.Linq;
using System.Reactive.Subjects;
using HJ.Input;
using HJ.Scriptable;
using UnityEngine;
using HJ.Tools;
using Sirenix.OdinInspector;

namespace HJ.Runtime
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerStateMachine : PlayerComponent
    {
        public enum PositionOffset { Ground, Feet, Center, Head }
        
        [Header("Dependencies")]
        public PlayerStatesGroup StatesAsset;
        [HideInInspector] public PlayerStatesGroup StatesAssetRuntime;

        [Header("Setup")]
        [SerializeField] private BasicSettings _playerBasicSettings;
        [SerializeField] private ControllerFeatures _playerFeatures;
        [SerializeField] private StaminaSettings _playerStamina;
        [SerializeField] private ControllerSettings _playerControllerSettings;

        [Header("Controller States")]
        [SerializeField] private ControllerState _standingState;
        [SerializeField] private ControllerState _crouchingState;
        
        [Header("Controller")]
        [SerializeField] private LayerMask _surfaceMask;
        [SerializeField] private PositionOffset _controllerOffset;

        [Header("Debugging")]
        [ReadOnly] public Vector3 Motion;
        [SerializeField] private bool _drawPlayerGizmos = true;
        [ShowIf(nameof(_drawPlayerGizmos))]
        [SerializeField] private bool _drawPlayerFrame;
        [ShowIf(nameof(_drawPlayerGizmos))]
        [SerializeField] private float _scaleOffset;
        [ShowIf(nameof(_drawPlayerGizmos))]
        [SerializeField] private Color _gizmosColor;
        
        private MultiKeyDictionary<string, Type, State> _playerStates;
        private State? _currentState;
        private State? _previousState;
        private bool _hasEnteredState;
        private float _staminaRegenTime;
        private Mesh _playerGizmos;

        #region Properties

        public BasicSettings PlayerBasicSettings => _playerBasicSettings;
        public ControllerFeatures PlayerFeatures => _playerFeatures;
        public StaminaSettings PlayerStamina => _playerStamina;
        public ControllerSettings PlayerControllerSettings => _playerControllerSettings;

        public ControllerState StandingState => _standingState;
        public ControllerState CrouchingState => _crouchingState;

        public LayerMask SurfaceMask => _surfaceMask;
        
        public ControllerColliderHit ControllerHit { get; private set; }
        public BehaviorSubject<float> Stamina { get; set; } = new(1f);
        public bool IsGrounded { get; private set; }
        public bool IsPlayerDead { get; private set; }
        public Vector2 Input { get; private set; }
        
        public State? CurrentState => _currentState;
        public State? PreviousState => _previousState;
        public string CurrentStateKey => CurrentState?.StateData.StateAsset.GetStateKey();
        
        public Vector3 FeetOffset
        {
            get
            {
                float height = PlayerCollider.height;
                float skinWidth = PlayerCollider.skinWidth;
                float center = height / 2;

                return _controllerOffset switch
                {
                    PositionOffset.Ground => new Vector3(0, skinWidth, 0),
                    PositionOffset.Feet => new Vector3(0, 0, 0),
                    PositionOffset.Center => new Vector3(0, -center, 0),
                    PositionOffset.Head => new Vector3(0, -center * 2, 0),
                    _ => PlayerCollider.center
                };
            }
        }
        
        public Vector3 ControllerCenter
        {
            get
            {
                Vector3 position = transform.position;
                return position += PlayerCollider.center;
            }
        }
        
        public Vector3 ControllerFeet
        {
            get
            {
                Vector3 position = transform.position;
                return position + FeetOffset;
            }
        }
        
        /// <summary>
        /// Check that the player is not airborne by checking the ground status and the current status of the player.
        /// </summary>
        public bool StateGrounded
        {
            get => IsGrounded 
                   || IsCurrent(SLIDING_STATE) 
                   || IsCurrent(LADDER_STATE) 
                   || IsCurrent(PUSHING_STATE);
        }
        
        /// <summary>
        /// The name of the current active state.
        /// </summary>
        public string StateName
        {
            get
            {
                if (_currentState.HasValue)
                    return _currentState?.StateData.StateAsset.GetStateKey();

                return "None";
            }
        }
        
        #endregion
        
        public const string IDLE_STATE = "Idle";
        public const string WALK_STATE = "Walk";
        public const string RUN_STATE = "Run";
        public const string CROUCH_STATE = "Crouch";
        public const string JUMP_STATE = "Jump";
        public const string DEATH_STATE = "Death";
        
        public const string LADDER_STATE = "Ladder";
        public const string SLIDING_STATE = "Sliding";
        public const string PUSHING_STATE = "Pushing";
        
        /// <summary>
        /// The name of the current active state as observable.
        /// </summary>
        public BehaviorSubject<string> ObservableState = new ("None");

        private void Awake()
        {
            _playerStates = new MultiKeyDictionary<string, Type, State>();
            StatesAssetRuntime = Instantiate(StatesAsset);

            if (StatesAsset != null)
            {
                // Initialize all states
                foreach (State playerState in StatesAssetRuntime.GetStates(this))
                {
                    Type stateType = playerState.StateData.StateAsset.GetType();
                    string stateKey = playerState.StateData.StateAsset.GetStateKey();
                    _playerStates.Add(stateKey, stateType, playerState);
                }

                // Select initial player state
                if (_playerStates.Count > 0)
                {
                    _hasEnteredState = false;
                    ChangeState(_playerStates.SubDictionary.Keys.First());
                }
            }
        }

        private void Update()
        {
            if (_isEnabled) GetInput();
            else Input = Vector2.zero;
            
            // player death event
            if (_currentState != null && !IsPlayerDead && PlayerManager.PlayerHealth.IsDead)
            {
                _currentState?.FSMState.OnPlayerDeath();
                IsPlayerDead = true;
            }
            
            if (!_hasEnteredState)
            {
                // enter state
                _currentState?.FSMState.OnStateEnter();
                string stateName = _currentState?.StateData.StateAsset.GetStateKey();
                ObservableState.OnNext(stateName);
                _hasEnteredState = true;
            }
            else if (_currentState != null)
            {
                // Update State
                _currentState?.FSMState.OnStateUpdate();
                
                // Check state transitions
                if (_currentState.Value.FSMState.Transitions != null)
                {
                    foreach (Transition transition in _currentState.Value.FSMState.Transitions)
                    {
                        if (transition.Value && _currentState.GetType() != transition.NextState)
                        {
                            if (transition.NextState == null) ChangeToPreviousState();
                            else ChangeState(transition.NextState);
                            break;
                        }
                    }
                }
            }
            
            // regenerate player stamina
            if (PlayerFeatures.EnableStamina)
            {
                bool runHold = InputManager.ReadButton(Controls.SPRINT);
                if (IsCurrent(RUN_STATE) || IsCurrent(JUMP_STATE) || runHold)
                {
                    _staminaRegenTime = PlayerStamina.RegenerateAfter;
                }
                else if(_staminaRegenTime > 0f)
                {
                    _staminaRegenTime -= Time.deltaTime;
                }
                else if(Stamina.Value < 1f)
                {
                    float stamina = Stamina.Value;
                    stamina = Mathf.MoveTowards(stamina, 1f, Time.deltaTime * PlayerStamina.StaminaRegenSpeed);
                    Stamina.OnNext(stamina);
                    _staminaRegenTime = 0f;
                }
            }
            
            // apply movement direction
            if (PlayerCollider.enabled)
                IsGrounded = (PlayerCollider.Move(Motion * Time.deltaTime) & CollisionFlags.Below) != 0;
        }

        private void FixedUpdate()
        {
            if (_currentState != null)
            {
                // Update state
                _currentState?.FSMState.OnStateFixedUpdate();
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (!IsGrounded)
            {
                Vector3 normal = hit.normal;
                if (normal.y > 0) return;

                Vector3 ricochet = Vector3.Reflect(Motion, normal);
                ricochet.y = Motion.y;
                
                float ricochetDot = Mathf.Clamp01(Vector3.Dot(ricochet.normalized, Motion.normalized));
                float wallDot = Mathf.Clamp01(Vector3.Dot(Motion.normalized, -normal));

                float ricochetMul = Mathf.Lerp(1f, _playerControllerSettings.WallRicochet, wallDot);
                ricochet *= ricochetMul;

                Vector3 newMotion = Vector3.Lerp(ricochet, Motion, ricochetDot);
                newMotion.y = Motion.y;

                Motion = newMotion;
            }

            ControllerHit = hit;
        }

        /// <summary>
        /// Calculate movement input vector.
        /// </summary>
        private void GetInput()
        {
            Input = Vector2.zero;
            if (InputManager.ReadInput(Controls.MOVEMENT, out Vector2 rawInput))
            {
                // TODO: Check if we want to normalize input at this point.
                rawInput.y = rawInput.y > 0.1f ? 1f : rawInput.y < -0.1f ? -1f : 0f;
                rawInput.x = rawInput.x > 0.1f ? 1f : rawInput.x < -0.1f ? -1f : 0f;

                Input = rawInput;
            }
        }
        
        /// <summary>
        /// Change player FSM state.
        /// </summary>
        public void ChangeState(Type nextState)
        {
            if (_playerStates.TryGetValue(nextState, out State state))
            {
                if (!_isEnabled && !state.FSMState.CanTransitionWhenDisabled)
                    return;

                if ((_currentState == null || !_currentState.Value.Equals(state)) && state.StateData.IsEnabled)
                {
                    _currentState?.FSMState.OnStateExit();
                    if (_currentState.HasValue) _previousState = _currentState;
                    _currentState = state;
                    _hasEnteredState = false;
                }
                return;
            }

            throw new MissingReferenceException($"Could not find a state with type '{nextState.Name}'");
        }
        
        /// <summary>
        /// Change player FSM state and set the state data.
        /// </summary>
        public void ChangeState(string nextState, StorableCollection stateData)
        {
            if (_playerStates.TryGetValue(nextState, out State state))
            {
                if (!_isEnabled && !state.FSMState.CanTransitionWhenDisabled)
                    return;

                if ((_currentState == null || !_currentState.Value.Equals(state)) && state.StateData.IsEnabled)
                {
                    _currentState?.FSMState.OnStateExit();
                    if (_currentState.HasValue) _previousState = _currentState;
                    state.FSMState.StateData = stateData;
                    _currentState = state;
                    _hasEnteredState = false;
                }
                return;
            }

            throw new MissingReferenceException($"Could not find a state with key '{nextState}'");
        }
        
        /// <summary>
        /// Change player FSM state to previous state.
        /// </summary>
        public void ChangeToPreviousState()
        {
            if (_previousState != null && !_currentState.Value.Equals(_previousState) && _previousState.Value.StateData.IsEnabled)
            {
                if (!_isEnabled && !_previousState.Value.FSMState.CanTransitionWhenDisabled)
                    return;
                
                _currentState?.FSMState.OnStateExit();
                State temp = _currentState.Value;
                _currentState = _previousState;
                _previousState = temp;
                _hasEnteredState = false;
            }
        }
        
        /// <summary>
        /// Check if current state matches the specified state key.
        /// </summary>
        public bool IsCurrent(string stateKey)
        {
            return _currentState.Value.StateData.StateAsset.GetStateKey() == stateKey;
        }
        
        /// <summary>
        /// Set player controller state.
        /// </summary>
        public Vector3 SetControllerState(ControllerState state)
        {
            float height = state.ControllerHeight;
            float skinWidth = PlayerCollider.skinWidth;
            float center = height / 2;

            Vector3 controllerCenter = _controllerOffset switch
            {
                PositionOffset.Ground => new Vector3(0, center + skinWidth, 0),
                PositionOffset.Feet => new Vector3(0, center, 0),
                PositionOffset.Center => new Vector3(0, 0, 0),
                PositionOffset.Head => new Vector3(0, -center, 0),
                _ => PlayerCollider.center
            };

            PlayerCollider.height = height;
            PlayerCollider.center = controllerCenter;

            Vector3 cameraTop = state.CameraOffset;
            cameraTop.y += center + controllerCenter.y;

            return cameraTop;
        }

        private void OnDrawGizmos()
        {
            if (!_drawPlayerGizmos) return;
            
            if (_playerGizmos == null)
                _playerGizmos = Resources.Load<Mesh>("Gizmos/Player");
            else
                CharacterGizmos.DrawGizmos(PlayerCollider, LookController, _playerGizmos, _scaleOffset, _gizmosColor, _drawPlayerFrame);
        }
    }
}
