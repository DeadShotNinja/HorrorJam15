using System.Collections.Generic;
using UnityEngine;
using HJ.Input;
using Cinemachine;

namespace HJ.Runtime
{
    public abstract class PuzzleBase : MonoBehaviour, IInteractStart
    {
        [SerializeField] protected CinemachineVirtualCamera _puzzleCamera;
        [SerializeField] protected float _switchCameraFadeSpeed = 5;
        [SerializeField] protected ControlsContext[] _controlsContexts;

        [SerializeField] protected LayerMask _cullLayers;
        [SerializeField] protected Layer _interactLayer;
        [SerializeField] protected Layer _disabledLayer;
        [SerializeField] protected bool _enablePointer;

        [SerializeField] protected List<Collider> _collidersEnable = new List<Collider>();
        [SerializeField] protected List<Collider> _collidersDisable = new List<Collider>();

        protected PlayerPresenceManager _playerPresence;
        protected PlayerManager _playerManager;
        protected GameManager _gameManager;

        /// <summary>
        /// Specifies when the camera is switched to a puzzle or normal camera. [true = puzzle, false = normal]
        /// </summary>
        protected bool isActive;

        /// <summary>
        /// Specifies when the camera can be switched back to normal camera using the default functionality.
        /// </summary>
        protected bool canManuallySwitch;

        /// <summary>
        /// Determines when the colliders switch to puzzle mode or normal mode.
        /// </summary>
        protected bool switchColliders;

        public virtual void Awake()
        {
            _playerPresence = PlayerPresenceManager.Instance;
            _playerManager = _playerPresence.PlayerManager;
            _gameManager = GameManager.Instance;

            foreach (var control in _controlsContexts)
            {
                control.SubscribeGloc();
            }
        }

        public virtual void Update()
        {
            if (InputManager.ReadButtonOnce(GetInstanceID(), Controls.EXAMINE) && isActive && canManuallySwitch)
            {
                SwitchBack();
            }
        }

        /// <summary>
        /// This function is called when player interacts with the object. It freezes the player and switches the camera to the puzzle camera.
        /// </summary>
        public virtual void InteractStart()
        {
            if (!isActive)
            {
                _playerPresence.FreezePlayer(true);
                _playerManager.PlayerItems.IsItemsUsable = false;
                _playerPresence.SwitchActiveCamera(_puzzleCamera.gameObject, _switchCameraFadeSpeed, OnBackgroundFade);
                canManuallySwitch = true;
                switchColliders = true;
                isActive = true;
            }
        }

        /// <summary>
        /// This function is called before switching to the puzzle camera, after the screen fades to black.
        /// </summary>
        public virtual void OnBackgroundFade()
        {
            if (isActive)
            {
                _gameManager.DisableAllGamePanels();
                _gameManager.ShowControlsInfo(true, _controlsContexts);

                if (_enablePointer)
                {
                    _gameManager.ShowPointer(_cullLayers, _interactLayer, (hit, interactStart) =>
                    {
                        interactStart.InteractStart();
                    });
                }

                if (switchColliders)
                {
                    _collidersEnable.ForEach(x => x.enabled = true);
                    _collidersDisable.ForEach(x => x.enabled = false);
                }
            }
            else
            {
                _playerPresence.FreezePlayer(false);
                _playerManager.PlayerItems.IsItemsUsable = true;
                _gameManager.ShowControlsInfo(false, null);
                _gameManager.ShowPanel(GameManager.PanelType.MainPanel);

                if (switchColliders)
                {
                    _collidersEnable.ForEach(x => x.enabled = false);
                    _collidersDisable.ForEach(x => x.enabled = true);
                }
            }
        }

        /// <summary>
        /// Calling this function switches the puzzle camera to the normal camera.
        /// </summary>
        protected virtual void SwitchBack()
        {
            if (isActive)
            {
                _playerPresence.SwitchToPlayerCamera(_switchCameraFadeSpeed, OnBackgroundFade);
                if (_enablePointer) _gameManager.HidePointer();
                isActive = false;
            }
        }

        /// <summary>
        /// Disable the puzzle interaction functionality. The GameObject layer will be set to Disabled Layer.
        /// </summary>
        protected void DisableInteract(bool includeChild = true)
        {
            gameObject.layer = _disabledLayer;

            if (includeChild)
            {
                foreach (Transform tr in transform)
                {
                    tr.gameObject.layer = _disabledLayer;
                }
            }
        }
    }
}