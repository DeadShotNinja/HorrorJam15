using System;
using System.Collections;
using UnityEngine;
using Cinemachine;
using HJ.Tools;
using Sirenix.OdinInspector;
using UnityEngine.Rendering;

namespace HJ.Runtime
{
    public class PlayerPresenceManager : Singleton<PlayerPresenceManager>
    {
        public enum UnlockType 
        {
            /// <summary>
            /// Player will be unlocked at the start or after the game state is loaded.
            /// </summary>
            Automatically,

            /// <summary>
            /// Player will be unlocked after calling the <b>UnlockPlayer()</b> function.
            /// </summary>
            Manually
        }

        [Tooltip("Determines when the player will be unlocked.")]
        [SerializeField] private UnlockType _playerUnlockType = UnlockType.Automatically;
        [Tooltip("Reference to the Player GameObject.")]
        [SerializeField] private GameObject _player;

        [Tooltip("Time to wait before starting the fade-out effect.")]
        [SerializeField] private float _waitFadeOutTime = 0.5f;
        [Tooltip("Speed at which the background will fade out.")]
        [SerializeField] private float _fadeOutSpeed = 3f;

        [SerializeField, ReadOnly] private bool _playerIsUnlocked;
        [SerializeField, ReadOnly] private bool _gameStateIsLoaded;
        [SerializeField, ReadOnly] private bool _isCameraSwitched;
        
        private PlayerComponent[] _playerComponents;
        private PlayerManager _playerManager;
        private PlayerStateMachine _stateMachine;
        private LookController _lookController;
        private GameManager _gameManager;
        private GameObject _activeCamera;

        private bool _isBackgroundFadedOut;
        
        public PlayerManager PlayerManager
        {
            get
            {
                if (_playerManager == null)
                    _playerManager = _player.GetComponent<PlayerManager>();

                return _playerManager;
            }
        }
        
        public PlayerStateMachine StateMachine
        {
            get
            {
                if (_stateMachine == null)
                    _stateMachine = _player.GetComponent<PlayerStateMachine>();

                return _stateMachine;
            }
        }
        
        public LookController LookController
        {
            get
            {
                if (_lookController == null)
                    _lookController = _player.GetComponentInChildren<LookController>();

                return _lookController;
            }
        }

        /// <summary>
        /// Check if player is unlocked and the active camera is player camera.
        /// </summary>
        public bool IsUnlockedAndCamera => _playerIsUnlocked && !_isCameraSwitched;

        public UnlockType PlayerUnlockType => _playerUnlockType;
        public Camera PlayerCamera => PlayerManager.MainCamera;
        public CinemachineVirtualCamera PlayerVirtualCamera => PlayerManager.MainVirtualCamera;
        public bool PlayerIsUnlocked => _playerIsUnlocked;
        public GameObject Player
        {
            get => _player;
            set => _player = value;
        }

        private void OnEnable()
        {
            SaveGameManager.Instance.OnGameLoaded += (state) =>
            {
                if (!state) 
                    return;

                UnlockPlayer();
                _gameStateIsLoaded = true;
            };
        }

        private void Awake()
        {
            _gameManager = GetComponent<GameManager>();
            _playerComponents = _player.GetComponentsInChildren<PlayerComponent>(true);

            // keep player frozen at start
            FreezePlayer(true);
        }

        private void Start()
        {
            if (!SaveGameManager.IsGameJustLoaded || !SaveGameManager.GameStateExist)
            {
                Vector3 rotation = _player.transform.eulerAngles;
                _player.transform.rotation = Quaternion.identity;
                LookController.Rotation.x = rotation.y;

                if(_playerUnlockType == UnlockType.Automatically)
                    UnlockPlayer();
            }
        }

        public T Component<T>()
        {
            return _player.GetComponentInChildren<T>(true);
        }

        public T[] Components<T>()
        {
            return _player.GetComponentsInChildren<T>(true);
        }

        public void FreezeMovement(bool freeze)
        {
            StateMachine.SetEnabled(!freeze);
        }

        public void FreezeLook(bool freeze, bool showCursor = false)
        {
            GameTools.ShowCursor(!showCursor, showCursor);
            LookController.SetEnabled(!freeze);
        }

        public void FreezePlayer(bool freeze, bool showCursor = false)
        {
            GameTools.ShowCursor(!showCursor, showCursor);

            foreach (var component in _playerComponents)
            {
                component.SetEnabled(!freeze);
            }
        }

        public void FadeBackground(bool fadeOut, Action onBackgroundFade)
        {
            StartCoroutine(StartFadeBackground(fadeOut, onBackgroundFade));
        }

        IEnumerator StartFadeBackground(bool fadeOut, Action onBackgroundFade)
        {
            yield return _gameManager.StartBackgroundFade(fadeOut, _waitFadeOutTime, _fadeOutSpeed);
            _isBackgroundFadedOut = fadeOut;
            onBackgroundFade?.Invoke();
        }

        public void UnlockPlayer()
        {
            StartCoroutine(DoUnlockPlayer());
        }

        private IEnumerator DoUnlockPlayer()
        {
            if(!_isBackgroundFadedOut)
                yield return _gameManager.StartBackgroundFade(true, _waitFadeOutTime, _fadeOutSpeed);

            FreezePlayer(false);
            _playerIsUnlocked = true;
        }

        public (Vector3 position, Vector2 rotation) GetPlayerTransform()
        {
            return (_player.transform.position, LookController.Rotation);
        }

        public void SetPlayerTransform(Vector3 position, Vector2 rotation)
        {
            _player.transform.SetPositionAndRotation(position, Quaternion.identity);
            LookController.Rotation = rotation;
            Physics.SyncTransforms(); // sync position to character controller
        }

        public void SwitchActiveCamera(GameObject virtualCameraObj, float fadeSpeed, Action onBackgroundFade)
        {
            StartCoroutine(SwitchCamera(virtualCameraObj, fadeSpeed, onBackgroundFade));
            _isCameraSwitched = true;
        }

        public void SwitchToPlayerCamera(float fadeSpeed, Action onBackgroundFade)
        {
            StartCoroutine(SwitchCamera(null, fadeSpeed, onBackgroundFade));
        }

        public IEnumerator SwitchCamera(GameObject cameraObj, float fadeSpeed)
        {
            yield return _gameManager.StartBackgroundFade(false, fadeSpeed: fadeSpeed);
            _playerManager.MainVirtualCamera.gameObject.SetActive(cameraObj == null);
            
            if(cameraObj != null) _playerManager.PlayerItems.DeactivateCurrentItem();
            else _playerManager.PlayerItems.ActivatePreviouslyDeactivatedItem();

            if (_activeCamera != null) _activeCamera.SetActive(false);
            if (cameraObj != null) cameraObj.SetActive(cameraObj != null);
            _activeCamera = cameraObj;

            yield return new WaitForEndOfFrame();
            yield return _gameManager.StartBackgroundFade(true, fadeSpeed: fadeSpeed);

            _isCameraSwitched = cameraObj != null; // check if camera switched to player camera
        }

        private IEnumerator SwitchCamera(GameObject cameraObj, float fadeSpeed, Action onBackgroundFade)
        {
            yield return _gameManager.StartBackgroundFade(false, fadeSpeed: fadeSpeed);
            _playerManager.MainVirtualCamera.gameObject.SetActive(cameraObj == null);

            if (cameraObj != null) _playerManager.PlayerItems.DeactivateCurrentItem();
            else _playerManager.PlayerItems.ActivatePreviouslyDeactivatedItem();

            if (_activeCamera != null) _activeCamera.SetActive(false);
            if(cameraObj != null) cameraObj.SetActive(cameraObj != null);
            _activeCamera = cameraObj;

            onBackgroundFade?.Invoke();

            yield return new WaitForEndOfFrame();
            yield return _gameManager.StartBackgroundFade(true, fadeSpeed: fadeSpeed);

            _isCameraSwitched = cameraObj != null; // check if camera switched to player camera
        }
        
        // public IEnumerator SwitchCameraIntroCutscene(GameObject cameraObj, float fadeSpeed)
        // {
        //     //yield return _gameManager.StartBackgroundFade(false, fadeSpeed: fadeSpeed);
        //     _playerManager.MainVirtualCamera.gameObject.SetActive(cameraObj == null);
        //     
        //     if(cameraObj != null) _playerManager.PlayerItems.DeactivateCurrentItem();
        //     else _playerManager.PlayerItems.ActivatePreviouslyDeactivatedItem();
        //
        //     if (_activeCamera != null) _activeCamera.SetActive(false);
        //     if (cameraObj != null) cameraObj.SetActive(cameraObj != null);
        //     _activeCamera = cameraObj;
        //
        //     yield return new WaitForEndOfFrame();
        //     yield return _gameManager.StartBackgroundFade(true, fadeSpeed: fadeSpeed);
        //
        //     _isCameraSwitched = cameraObj != null; // check if camera switched to player camera
        // }
        
        public IEnumerator SwitchCameraIntroCutscene(GameObject cameraObj, float fadeSpeed)
        {
            //yield return _gameManager.StartBackgroundFade(false, fadeSpeed: fadeSpeed);
            _playerManager.MainVirtualCamera.gameObject.SetActive(cameraObj == null);
            
            if(cameraObj != null) _playerManager.PlayerItems.DeactivateCurrentItem();
            else _playerManager.PlayerItems.ActivatePreviouslyDeactivatedItem();

            if (_activeCamera != null) _activeCamera.SetActive(false);
            if (cameraObj != null) cameraObj.SetActive(cameraObj != null);
            _activeCamera = cameraObj;

            yield return new WaitForEndOfFrame();
            yield return _gameManager.StartBackgroundFade(true, fadeSpeed: fadeSpeed);

            _isCameraSwitched = cameraObj != null; // check if camera switched to player camera
        }
    }
}
