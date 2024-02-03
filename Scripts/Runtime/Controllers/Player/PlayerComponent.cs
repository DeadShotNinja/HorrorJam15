using System;
using Cinemachine;
using UnityEngine;

namespace HJ.Runtime
{
    public abstract class PlayerComponent : MonoBehaviour
    {
        protected bool _isEnabled = true;

        public virtual void SetEnabled(bool isEnabled)
        { 
            _isEnabled = isEnabled;
        }

        [NonSerialized]
        private CharacterController _playerCollider;
        public CharacterController PlayerCollider
        {
            get
            {
                if (_playerCollider == null)
                    _playerCollider = transform.root.GetComponent<CharacterController>();

                return _playerCollider;
            }
        }
        
        [NonSerialized]
        private PlayerStateMachine _playerStateMachine;
        public PlayerStateMachine PlayerStateMachine
        {
            get
            {
                if (_playerStateMachine == null)
                    _playerStateMachine = transform.root.GetComponent<PlayerStateMachine>();

                return _playerStateMachine;
            }
        }
        
        [NonSerialized]
        private LookController _lookController;
        public LookController LookController
        {
            get
            {
                if (_lookController == null)
                    _lookController = transform.root.GetComponentInChildren<LookController>();

                return _lookController;
            }
        }
        
        [NonSerialized]
        private ExamineController _examineController;
        public ExamineController ExamineController
        {
            get
            {
                if (_examineController == null)
                    _examineController = transform.root.GetComponentInChildren<ExamineController>();

                return _examineController;
            }
        }
        
        [NonSerialized]
        private PlayerManager _playerManager;
        public PlayerManager PlayerManager
        {
            get
            {
                if (_playerManager == null)
                    _playerManager = transform.root.GetComponent<PlayerManager>();

                return _playerManager;
            }
        }
        
        public Camera MainCamera => PlayerManager.MainCamera;
        public CinemachineVirtualCamera VirtualCamera => PlayerManager.MainVirtualCamera;
    }
}
