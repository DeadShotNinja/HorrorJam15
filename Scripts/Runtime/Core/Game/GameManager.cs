using System;
using System.Reactive.Disposables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using HJ.Rendering;
using HJ.Scriptable;
using HJ.Input;
using HJ.Tools;
using TMText = TMPro.TMP_Text;

namespace HJ.Runtime
{
    public class GameManager : Singleton<GameManager>
    {
        public enum PanelType { GamePanel, PausePanel, DeadPanel, MainPanel, InventoryPanel, MapPanel }

        // configuration
        [SerializeField] private ManagerModulesAsset _modules;
        [SerializeField] private Volume _globalPPVolume;
        [SerializeField] private Volume _healthPPVolume;
        [SerializeField] private BackgroundFader _backgroundFade;

        #region Panels
        // Main Panels
        [SerializeField] private CanvasGroup _gamePanel;
        [SerializeField] private CanvasGroup _pausePanel;
        [SerializeField] private CanvasGroup _deadPanel;
        // Sub Panels
        [SerializeField] private CanvasGroup _hudPanel;
        [SerializeField] private CanvasGroup _tabPanel;
        // Game Panels
        [SerializeField] private CanvasGroup _inventoryPanel;
        [SerializeField] private Transform _floatingIcons;
        #endregion

        #region UserInterface
        // reticule
        [SerializeField] private Image _reticleImage;
        [SerializeField] private Image _interactProgress;
        [SerializeField] private Slider _staminaSlider;

        // interaction
        [SerializeField] private InteractInfoPanel _interactInfoPanel;
        [SerializeField] private ControlsInfoPanel _controlsInfoPanel;

        // interact pointer
        [SerializeField] private Image _pointerImage;
        [SerializeField] private Sprite _normalPointer;
        [SerializeField] private Sprite _hoverPointer;
        [SerializeField] private Vector2 _normalPointerSize;
        [SerializeField] private Vector2 _hoverPointerSize;

        // item pickup
        [SerializeField] private Transform _itemPickupLayout;
        [SerializeField] private GameObject _itemPickup;
        [SerializeField] private float _pickupMessageTime = 2f;

        // hint message
        [SerializeField] private CanvasGroup _hintMessageGroup;
        [SerializeField] private float _hintMessageFadeSpeed = 2f;

        // header
        [SerializeField] private Slider _healthBar;
        [SerializeField] private Image _hearthbeat;
        [SerializeField] private TMText _healthPercent;

        // paper
        [SerializeField] private CanvasGroup _paperPanel;
        [SerializeField] private TMText _paperText;
        [SerializeField] private float _paperFadeSpeed;

        // examine
        [SerializeField] private CanvasGroup _examineInfoPanel;
        [SerializeField] private Transform _examineHotspots;
        [SerializeField] private TMText _examineText;
        [SerializeField] private float _examineFadeSpeed;

        // overlay
        [SerializeField] private GameObject _overlaysParent;
        #endregion

        // blur config
        [SerializeField] private bool _enableBlur = true;
        [SerializeField] private float _blurRadius = 5f;
        [SerializeField] private float _blurDuration = 0.15f;

        // graphic ref
        [SerializeField] private GraphicReference[] _graphicReferencesRaw;
        
        private bool _isInputLocked;
        private bool _showStaminaSlider;

        private bool _isPointerShown;
        private int _pointerCullLayers;
        private float _defaultBlurRadius;

        private Layer _pointerInteractLayer;
        private Action<RaycastHit, IInteractStart> _pointerInteractAction;

        private CoroutineRunner _blurCoroutine;
        private PlayerPresenceManager _playerPresence;
        private Inventory _inventory;
        
        public CompositeDisposable Disposables;

        public Image ReticleImage => _reticleImage;
        public Transform FloatingIcons => _floatingIcons;
        public ManagerModulesAsset Modules => _modules;
        public Volume HealthPPVolume => _healthPPVolume;
        public InteractInfoPanel InteractInfoPanel => _interactInfoPanel;
        public Slider HealthBar => _healthBar;
        public Image Hearthbeat => _hearthbeat;
        public TMText HealthPercent => _healthPercent;
        public Transform ExamineHotspots => _examineHotspots;
        
        public bool IsPaused { get; private set; }
        public bool IsInventoryShown { get; private set; }
        
        public Inventory Inventory
        {
            get
            {
                if (_inventory == null)
                    _inventory = GetComponent<Inventory>();

                return _inventory;
            }
        }
        
        public PlayerPresenceManager PlayerPresence
        {
            get
            {
                if (_playerPresence == null)
                    _playerPresence = GetComponent<PlayerPresenceManager>();

                return _playerPresence;
            }
        }

        public bool PlayerDied => PlayerPresence.PlayerManager.PlayerHealth.IsDead;

        /// <summary>
        /// Get Custom Graphic References
        /// </summary>
        public Lazy<IDictionary<string, Behaviour[]>> GraphicReferences { get; } = new(() => 
        {
            Dictionary<string, Behaviour[]> referencesDict = new();

            foreach (var reference in Instance._graphicReferencesRaw)
            {
                if (string.IsNullOrEmpty(reference.Name) || referencesDict.ContainsKey(reference.Name)) 
                    continue;

                referencesDict.Add(reference.Name, reference.Graphics);
            }

            return referencesDict;
        });

        private void Awake()
        {
            Disposables = new CompositeDisposable();
            InputManager.Performed(Controls.PAUSE, OnPause);
            InputManager.Performed(Controls.INVENTORY, OnInventory);

            // update stamina slider value
            if (PlayerPresence.StateMachine.PlayerFeatures.EnableStamina)
            {
                PlayerPresence.StateMachine.Stamina.Subscribe(value =>
                {
                    _staminaSlider.value = value;
                    _showStaminaSlider = value < 1f;
                })
                .AddTo(Disposables);
            }

            foreach (var module in _modules.ManagerModules)
            {
                if (module == null) continue;
                module.GameManager = this;
                module.OnAwake();
            }

            DualKawaseBlur blur = GetStack<DualKawaseBlur>();
            if (blur != null) _defaultBlurRadius = blur.BlurRadius.value;
        }

        private void Start()
        {
            foreach (var module in _modules.ManagerModules)
            {
                if (module == null) continue;
                module.OnStart();
            }
        }

        private void OnDestroy()
        {
            Disposables?.Dispose();
        }

        private void Update()
        {
            if (_isPointerShown && _pointerInteractAction != null)
            {
                Vector2 pointerDelta = InputManager.ReadInput<Vector2>(Controls.POINTER_DELTA);
                Image pointerImage = _pointerImage;

                Vector3 pointerPos = pointerImage.transform.position;
                pointerPos.x = Mathf.Clamp(pointerPos.x + pointerDelta.x, 0, Screen.width);
                pointerPos.y = Mathf.Clamp(pointerPos.y + pointerDelta.y, 0, Screen.height);
                pointerImage.transform.position = pointerPos;

                Ray pointerRay = PlayerPresence.PlayerCamera.ScreenPointToRay(pointerPos);
                if (GameTools.Raycast(pointerRay, out RaycastHit hit, 5, _pointerCullLayers, _pointerInteractLayer) && hit.collider.TryGetComponent(out IInteractStart interactStart))
                {
                    pointerImage.sprite = _hoverPointer;
                    pointerImage.rectTransform.sizeDelta = _hoverPointerSize;

                    if (InputManager.ReadButtonOnce(GetInstanceID(), Controls.LEFT_BUTTON))
                    {
                        _pointerInteractAction?.Invoke(hit, interactStart);
                    }
                }
                else
                {
                    pointerImage.sprite = _normalPointer;
                    pointerImage.rectTransform.sizeDelta = _normalPointerSize;
                }
            }

            // update manager modules
            foreach (var module in _modules.ManagerModules)
            {
                if (module == null) 
                    continue;

                module.OnUpdate();
            }

            // update stamina slider alpha
            if (PlayerPresence.StateMachine.PlayerFeatures.EnableStamina)
            {
                CanvasGroup staminaGroup = _staminaSlider.GetComponent<CanvasGroup>();
                staminaGroup.alpha = Mathf.MoveTowards(staminaGroup.alpha, _showStaminaSlider ? 1f : 0f, Time.deltaTime * 3f);
            }
        }

        /// <summary>
        /// Start background fade.
        /// </summary>
        public IEnumerator StartBackgroundFade(bool fadeOut, float waitTime = 0, float fadeSpeed = 3) 
            => _backgroundFade.StartBackgroundFade(fadeOut, waitTime, fadeSpeed);

        /// <summary>
        /// Get GameManager Module.
        /// </summary>
        public static T Module<T>() where T : ManagerModule
        {
            foreach (var module in Instance._modules.ManagerModules)
            {
                if (module.GetType() == typeof(T))
                    return (T)module;
            }

            return default;
        }

        /// <summary>
        /// When the next level is loaded, only the player's data is loaded.
        /// </summary>
        public void LoadNextLevel(string sceneName)
        {
            StartCoroutine(LoadNext(sceneName, false));
        }

        /// <summary>
        /// When the next level is loaded, the player's data and the world state are loaded.
        /// </summary>
        public void LoadNextWorld(string sceneName)
        {
            StartCoroutine(LoadNext(sceneName, true));
        }

        /// <summary>
        /// When the next level is loaded, the player's data and the world state are loaded from the game state.
        /// </summary>
        /// <remarks>If sceneName is null, the current scene is used.</remarks>
        public void LoadGameState(string sceneName, string folderName)
        {
            if (string.IsNullOrEmpty(sceneName))
                sceneName = SceneManager.GetActiveScene().name;

            StartCoroutine(LoadGame(sceneName, folderName));
        }

        private IEnumerator LoadNext(string sceneName, bool worldState)
        {
            yield return _backgroundFade.StartBackgroundFade(false);
            yield return new WaitForEndOfFrame();
            if (worldState) SaveGameManager.SetLoadWorldState(sceneName);
            else SaveGameManager.SetLoadPlayerData(sceneName);
            SceneManager.LoadScene(SaveGameManager.LMS);
        }

        private IEnumerator LoadGame(string sceneName, string folderName)
        {
            yield return _backgroundFade.StartBackgroundFade(false);
            yield return new WaitForEndOfFrame();

            SaveGameManager.SetLoadGameState(sceneName, folderName);
            SceneManager.LoadScene(SaveGameManager.LMS);
        }

        /// <summary>
        /// Freeze Player Controls.
        /// </summary>
        public void FreezePlayer(bool state, bool showCursor = false, bool lockInput = true)
        {
            PlayerPresence.FreezePlayer(state, showCursor);
            _isInputLocked = lockInput && state;
        }

        /// <summary>
        /// Set the active state of the PostProcessing Volume.
        /// </summary>
        public void SetStack<T>(bool active) where T : VolumeComponent
        {
            if (_globalPPVolume.profile.TryGet(out T component))
            {
                component.active = active;
            }
        }

        /// <summary>
        /// Get PostProcessing Volume component.
        /// </summary>
        public T GetStack<T>() where T : VolumeComponent
        {
            if (_globalPPVolume.profile.TryGet(out T component))
                return component;

            return default;
        }

        /// <summary>
        /// Set the blur PostProcessing Volume active state.
        /// </summary>
        public void SetBlur(bool active, bool interpolate = false)
        {
            if (!_enableBlur) 
                return;

            if (!interpolate)
            {
                DualKawaseBlur blur = GetStack<DualKawaseBlur>();
                if (blur == null) return;

                blur.BlurRadius.value = _defaultBlurRadius;
                blur.active = active;
            }
            else if (active)
            {
                InterpolateBlur(_blurRadius, _blurDuration);
            }
            else
            {
                InterpolateBlur(0f, _blurDuration);
            }
        }

        /// <summary>
        /// Interpolate blur radius over time.
        /// </summary>
        public void InterpolateBlur(float blurRadius, float duration)
        {
            if (!_enableBlur) 
                return;

            DualKawaseBlur blur = GetStack<DualKawaseBlur>();
            if (blur == null) return;

            if (blurRadius > 0) blur.BlurRadius.value = 0f;
            blur.active = true;

            if (_blurCoroutine != null) _blurCoroutine.Stop();
            _blurCoroutine = CoroutineRunner.Run(gameObject, InterpolateBlur(blur, blurRadius, duration));
        }

        IEnumerator InterpolateBlur(DualKawaseBlur blur, float targetRadius, float duration)
        {
            float startBlurRadius = blur.BlurRadius.value;
            float elapsedTime = 0;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                blur.BlurRadius.value = Mathf.Lerp(startBlurRadius, targetRadius, elapsedTime / duration);
                yield return null;
            }

            blur.BlurRadius.value = targetRadius;
            if(targetRadius <= 0) blur.active = false;
        }

        /// <summary>
        /// Show timed interact progress.
        /// </summary>
        public void ShowInteractProgress(bool show, float progress)
        {
            if (show)
            {
                _interactProgress.enabled = true;
                _interactProgress.fillAmount = progress;
            }
            else
            {
                _interactProgress.fillAmount = 0f;
                _interactProgress.enabled = false;
            }
        }

        /// <summary>
        /// Block input to prevent the Inventory or Pause menu from opening when using other functions.
        /// </summary>
        public void LockInput(bool state)
        {
            _isInputLocked = state;
        }

        /// <summary>
        /// Show the helper controls panel at the bottom of the screen.
        /// </summary>
        public void ShowControlsInfo(bool show, params ControlsContext[] contexts)
        {
            if (show)
            {
                _controlsInfoPanel.ShowInfo(contexts);
            }
            else
            {
                _controlsInfoPanel.HideInfo();
            }
        }

        /// <summary>
        /// Show the panel to examine the text of the paper document.
        /// </summary>
        public void ShowPaperInfo(bool show, bool noFade, string paperText = "")
        {
            if (!noFade)
            {
                _paperText.text = paperText;
                StartCoroutine(CanvasGroupFader.StartFade(_paperPanel, show, _paperFadeSpeed, () =>
                {
                    if(!show) _paperText.text = string.Empty;
                }));
                return;
            }

            if (show)
            {
                _paperPanel.gameObject.SetActive(true);
                _paperText.text = paperText;
                _paperPanel.alpha = 1;
            }
            else
            {
                _paperPanel.gameObject.SetActive(false);
                _paperText.text = string.Empty;
                _paperPanel.alpha = 0;
            }
        }

        /// <summary>
        /// Show info about the item being examined.
        /// </summary>
        public void ShowExamineInfo(bool show, bool noFade, string examineText = "")
        {
            StopAllCoroutines();

            if (!noFade)
            {
                _examineText.text = examineText;
                StartCoroutine(CanvasGroupFader.StartFade(_examineInfoPanel, show, _examineFadeSpeed, () =>
                {
                    if (!show) _examineText.text = string.Empty;
                }));
                return;
            }

            if (show)
            {
                _examineText.text = examineText;
                _examineInfoPanel.alpha = 1;
            }
            else
            {
                _examineText.text = string.Empty;
                _examineInfoPanel.alpha = 0;
            }
        }

        /// <summary>
        /// Show the game panel.
        /// </summary>
        /// <param name="panel"></param>
        public void ShowPanel(PanelType panel)
        {
            switch (panel)
            {
                case PanelType.PausePanel:
                    SetPanelInteractable(panel);
                    _gamePanel.alpha = 0;
                    _pausePanel.alpha = 1;
                    _deadPanel.alpha = 0;
                    break;
                case PanelType.GamePanel:
                    SetPanelInteractable(panel);
                    _gamePanel.alpha = 1;
                    _pausePanel.alpha = 0;
                    _deadPanel.alpha = 0;
                    break;
                case PanelType.DeadPanel:
                    SetPanelInteractable(panel);
                    _gamePanel.alpha = 0;
                    _pausePanel.alpha = 0;
                    _deadPanel.alpha = 1;
                    break;
                case PanelType.MainPanel:
                    SetPanelInteractable(PanelType.GamePanel);
                    _gamePanel.alpha = 1;
                    _pausePanel.alpha = 0;
                    _deadPanel.alpha = 0;
                    DisableAllGamePanels();
                    _hudPanel.alpha = 1;
                    break;
                case PanelType.InventoryPanel:
                    SetPanelInteractable(PanelType.GamePanel);
                    _gamePanel.alpha = 1;
                    _pausePanel.alpha = 0;
                    _deadPanel.alpha = 0;
                    DisableAllGamePanels();
                    _inventoryPanel.alpha = 1;
                    _tabPanel.alpha = 1;
                    IsInventoryShown = true;
                    break;
            }
        }

        /// <summary>
        /// Set the panel as interactable.
        /// </summary>
        public void SetPanelInteractable(PanelType panel)
        {
            _gamePanel.interactable = panel == PanelType.GamePanel;
            _gamePanel.blocksRaycasts = panel == PanelType.GamePanel;

            _pausePanel.interactable = panel == PanelType.PausePanel;
            _pausePanel.blocksRaycasts = panel == PanelType.PausePanel;

            _deadPanel.interactable = panel == PanelType.DeadPanel;
            _deadPanel.blocksRaycasts = panel == PanelType.DeadPanel;
        }

        /// <summary>
        /// Disable all game panels. (HUD, Tab, Inventory etc.)
        /// </summary>
        public void DisableAllGamePanels()
        {
            _hudPanel.alpha = 0;
            _tabPanel.alpha = 0;
            _inventoryPanel.alpha = 0;
            IsInventoryShown = false;
        }

        /// <summary>
        /// Disable all feature panels. (Inventory etc.)
        /// </summary>
        public void DisableAllFeaturePanels()
        {
            _inventoryPanel.alpha = 0;
        }

        /// <summary>
        /// Show game inventory panel.
        /// </summary>
        public void ShowInventoryPanel(bool state)
        {
            if (PlayerPresence.IsUnlockedAndCamera && !PlayerDied && !_isInputLocked && !IsPaused)
            {
                // set inventory status
                IsInventoryShown = state;

                // freeze player functions
                PlayerPresence.FreezePlayer(IsInventoryShown, IsInventoryShown);

                // show blur
                if (IsInventoryShown) InterpolateBlur(_blurRadius, _blurDuration);
                else InterpolateBlur(0, _blurDuration);

                // set panel visibility
                if (IsInventoryShown)
                {
                    ShowPanel(PanelType.InventoryPanel);
                    ShowControlsInfo(true, Inventory.ControlsContexts);
                    _overlaysParent.SetActive(false);
                }
                else
                {
                    Inventory.OnCloseInventory();
                    ShowPanel(PanelType.MainPanel);
                    ShowControlsInfo(false, null);
                    _overlaysParent.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Show interaction/mouse pointer.
        /// </summary>
        public void ShowPointer(int cullLayers, Layer interactLayer, Action<RaycastHit, IInteractStart> interactAction)
        {
            _isPointerShown = true;
            _pointerCullLayers = cullLayers;
            _pointerInteractLayer = interactLayer;
            _pointerInteractAction = interactAction;
            _pointerImage.gameObject.SetActive(true);
        }

        /// <summary>
        /// Hide interaction/mouse pointer.
        /// </summary>
        public void HidePointer()
        {
            _isPointerShown = false;
            _pointerInteractAction = null;
            _pointerCullLayers = -1;
            _pointerInteractLayer = -1;
            _pointerImage.gameObject.SetActive(false);
            _pointerImage.rectTransform.anchoredPosition = Vector2.zero;
        }

        /// <summary>
        /// Show a message when you pickup something.
        /// </summary>
        public void ShowItemPickupMessage(string text, Sprite icon, float time)
        {
            GameObject pickupElement = Instantiate(_itemPickup, _itemPickupLayout);
            ItemPickupElement element = pickupElement.GetComponent<ItemPickupElement>();
            element.ShowItemPickup(text, icon, time);
        }

        /// <summary>
        /// Show the hint at the top of the screen.
        /// </summary>
        public void ShowHintMessage(string text, float time)
        {
            TMText tmpText = _hintMessageGroup.GetComponentInChildren<TMText>();
            tmpText.text = text;

            StopAllCoroutines();
            StartCoroutine(ShowHintMessage(time));
        }

        IEnumerator ShowHintMessage(float time)
        {
            yield return CanvasGroupFader.StartFade(_hintMessageGroup, true, _hintMessageFadeSpeed);

            yield return new WaitForSeconds(time);

            yield return CanvasGroupFader.StartFade(_hintMessageGroup, false, _hintMessageFadeSpeed, () =>
            {
                _hintMessageGroup.gameObject.SetActive(false);
            });
        }

        /// <summary>
        /// Restart the game from the last saved game.
        /// </summary>
        public void RestartGame()
        {
            string sceneName = SaveGameManager.LoadSceneName;
            string saveName = SaveGameManager.LoadFolderName;
            LoadGameState(sceneName, saveName);
        }

        public void ResumeGame()
        {
            IsPaused = false;
            ShowPanel(PanelType.GamePanel);

            // reset panels
            SetPanelInteractable(PanelType.GamePanel);

            // hide blur
            InterpolateBlur(0, _blurDuration);

            // un-freeze player functions
            if (PlayerPresence.PlayerIsUnlocked)
                PlayerPresence.FreezePlayer(false, false);
        }

        public void MainMenu()
        {
            StartCoroutine(LoadMainMenu());
        }

        private IEnumerator LoadMainMenu()
        {
            yield return _backgroundFade.StartBackgroundFade(false);
            yield return new WaitForEndOfFrame();
            SceneManager.LoadScene(SaveGameManager.MM);
        }

        private void OnPause(InputAction.CallbackContext obj)
        {
            if (obj.ReadValueAsButton() && PlayerPresence.IsUnlockedAndCamera && !PlayerDied && !_isInputLocked && !IsInventoryShown)
            {
                // set panel visibility
                IsPaused = !IsPaused;
                _gamePanel.alpha = IsPaused ? 0 : 1;
                _pausePanel.alpha = IsPaused ? 1 : 0;
                SetPanelInteractable(IsPaused ? PanelType.PausePanel : PanelType.GamePanel);

                // show blur
                if (IsPaused) InterpolateBlur(_blurRadius, _blurDuration);
                else InterpolateBlur(0, _blurDuration);

                // freeze player functions
                if (PlayerPresence.PlayerIsUnlocked)
                    PlayerPresence.FreezePlayer(IsPaused, IsPaused);
                
                // Change Wwise pause/active State
                AudioManager.SetAudioState(IsPaused ? AudioState.GamePaused : AudioState.GameActive);
            }
        }

        private void OnInventory(InputAction.CallbackContext obj)
        {
            if (obj.ReadValueAsButton()) ShowInventoryPanel(!IsInventoryShown);
        }
    }
}