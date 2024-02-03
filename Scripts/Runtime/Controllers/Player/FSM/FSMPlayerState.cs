using UnityEngine;

namespace HJ.Runtime
{
    public class FSMPlayerState : FSMState
    {
        protected Transform _cameraHolder;
        protected GameManager _gameManager;
        protected PlayerStateMachine _machine;
        protected CharacterController _controller;
        protected PlayerItemsManager _playerItems;
        protected MotionController _motionController;
        protected LookController _cameraLook;
        protected ControllerState _controllerState;
        
        private Vector3 _heightVelocity;
        
        /// <summary>
        /// Character controller current position.
        /// </summary>
        protected Vector3 Position
        {
            get => _machine.transform.position;
            set => _machine.transform.position = value;
        }
        
        /// <summary>
        /// Character controller center position.
        /// </summary>
        protected Vector3 _centerPosition
        {
            get => _machine.ControllerCenter;
            set
            {
                Vector3 position = value;
                position -= _controller.center;
                _machine.transform.position = position;
            }
        }
        
        /// <summary>
        /// Character controller bottom position.
        /// </summary>
        protected Vector3 FeetPosition
        {
            get => _machine.ControllerFeet;
            set
            {
                Vector3 position = value;
                position -= _machine.ControllerFeet;
                _machine.transform.position = position;
            }
        }
        
        /// <summary>
        /// The magnitude of the movement input.
        /// </summary>
        protected float InputMagnitude => _machine.Input.magnitude;

        /// <summary>
        /// Check if the character controller is on the ground.
        /// </summary>
        protected bool IsGrounded => _machine.IsGrounded;

        /// <summary>
        /// Check if the stamina feature is enabled in the player.
        /// </summary>
        protected bool StaminaEnabled => _machine.PlayerFeatures.EnableStamina;

        /// <summary>
        /// Check if the player has died.
        /// </summary>
        protected bool IsDead => _machine.IsPlayerDead;
        
        /// <summary>
        /// Check if you can transition to this state when the transition is disabled.
        /// </summary>
        public virtual bool CanTransitionWhenDisabled => false;
        
        public Transition[] Transitions { get; private set; }
        public StorableCollection StateData { get; set; }
        
        public FSMPlayerState(PlayerStateMachine machine)
        {
            _machine = machine;
            _gameManager = GameManager.Instance;
            _controller = machine.PlayerCollider;
            _playerItems = machine.PlayerManager.PlayerItems;
            _cameraHolder = machine.PlayerManager.CameraHolder;
            _motionController = machine.PlayerManager.MotionController;
            _cameraLook = machine.LookController;
            Transitions = OnGetTransitions();
        }
        
        /// <summary>
        /// Get player state transitions.
        /// </summary>
        public virtual Transition[] OnGetTransitions()
        {
            return new Transition[0];
        }

        /// <summary>
        /// Change player controller height.
        /// </summary>
        public void PlayerHeightUpdate()
        {
            if (_controllerState != null)
            {
                Vector3 cameraPosition = _machine.SetControllerState(_controllerState);
                float changeSpeed = _machine.PlayerControllerSettings.StateChangeSpeed;

                Vector3 localPos = _machine.PlayerManager.CameraHolder.localPosition;
                localPos = Vector3.SmoothDamp(localPos, cameraPosition, ref _heightVelocity, changeSpeed);
                _machine.PlayerManager.CameraHolder.localPosition = localPos;
            }
        }

        /// <summary>
        /// Get player gravity force with weight.
        /// </summary>
        public float GravityForce()
        {
            float gravity = _machine.PlayerControllerSettings.BaseGravity;
            float weight = _machine.PlayerControllerSettings.PlayerWeight / 10f;
            return gravity - weight;
        }

        /// <summary>
        /// Apply gravity force to motion.
        /// </summary>
        public void ApplyGravity(ref Vector3 motion)
        {
            float gravityForce = GravityForce();
            motion += gravityForce * Time.deltaTime * Vector3.up;
        }

        /// <summary>
        /// Event when a player dies.
        /// </summary>
        public virtual void OnPlayerDeath() { }
    }
}
