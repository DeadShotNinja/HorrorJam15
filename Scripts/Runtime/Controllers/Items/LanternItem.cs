using System.Collections;
using UnityEngine;
using HJ.Tools;
using Newtonsoft.Json.Linq;

namespace HJ.Runtime
{
    public class LanternItem : PlayerItemBehaviour
    {
        [System.Serializable]
        public struct HandleVariationStruct
        {
            public MinMax HandleVariation;
            public float HandleVariationSpeed;
        }

        [SerializeField] private ItemGuid _fuelInventoryItem;
        [SerializeField] private Transform _handleBone;
        [SerializeField] private Light _lanternLight;
        [SerializeField] private MeshRenderer _lanternFlame;
        [SerializeField] private MinMax _handleLimits;
        [SerializeField] private Axis _handleAxis;

        [SerializeField] private float _handleGravityTime = 0.2f;
        [SerializeField] private float _handleForwardAngle = -90f;
        [SerializeField] private float _flameChangeSpeed = 1f;
        [SerializeField] private float _flameLightIntensity = 1f;
        [SerializeField] private float _flameAlphaFadeStart = 0.2f;

        [SerializeField] private MinMax _flameFlickerLimits;
        [SerializeField] private float _flameFlickerSpeed;

        [SerializeField] private HandleVariationStruct _handleIdleVariation;
        [SerializeField] private HandleVariationStruct _handleWalkVariation;
        [SerializeField] private float _variationBlendTime;
        [SerializeField] private bool _useHandleVariation;

        [SerializeField] private bool _infiniteFuel = false;
        [SerializeField] private float _fuelReloadTime = 2f;
        [SerializeField] private ushort _fuelLife = 320;
        [SerializeField] private Percentage _fuelPercentage = 100;

        [SerializeField] private string _lanternDrawState = "LanternDraw";
        [SerializeField] private string _lanternHideState = "LanternHide";
        [SerializeField] private string _lanternReloadStartState = "Lantern_Reload_Start";
        [SerializeField] private string _lanternReloadEndState = "Lantern_Reload_End";
        [SerializeField] private string _lanternIdleState = "LanternIdle";

        [SerializeField] private string _lanternHideTrigger = "Hide";
        [SerializeField] private string _lanternReloadTrigger = "Reload";
        [SerializeField] private string _lanternReloadEndTrigger = "ReloadEnd";

        // TODO: Wwise sounds...

        private CanvasGroup _lanternPanelGroup;
        private CanvasGroup _lanternFlameGroup;

        private bool _updateHandle;
        private float _handleAngle;
        private float _handleVelocity;

        private float _flameLerp;
        private float _targetFlame;
        private float _flameIntensity;
        private float _variationBlend;
        private float _variationVelocity;
        
        private float _currentFuel;
        private bool _noFueldAudioPlayed;

        private bool _isEquipped;
        private bool _isBusy;

        public override string Name => "Lantern";

        public override bool IsBusy() => !_isEquipped || _isBusy;

        public override bool CanCombine() => _isEquipped && !_isBusy;

        public Percentage FuelPercentage => _fuelPercentage;
        public float LanternFuel { get; private set; }

        private void Awake()
        {
            GameManager gameManager = GameManager.Instance;

            var behaviours = gameManager.GraphicReferences.Value["Lantern"];
            _lanternPanelGroup = (CanvasGroup)behaviours[0];
            _lanternFlameGroup = (CanvasGroup)behaviours[1];

            if (!SaveGameManager.IsGameJustLoaded)
            {
                _currentFuel = _fuelPercentage.From(_fuelLife);
                UpdateFuel();
            }
        }

        private void LateUpdate()
        {
            if (!_updateHandle)
                return;

            float lookY = LookController.Rotation.y;
            MinMax lookLimits = LookController.VerticalLimits;
            Vector3 movement = PlayerStateMachine.PlayerCollider.velocity;

            float lookInverse1 = Mathf.InverseLerp(lookLimits.Min, 0, lookY);
            float lookInverse2 = Mathf.InverseLerp(0, lookLimits.Max, lookY);

            float lerp1 = Mathf.Lerp(_handleLimits.Min, _handleForwardAngle, lookInverse1);
            float lerp2 = Mathf.Lerp(_handleForwardAngle, _handleLimits.Max, lookInverse2);

            float targetInverse = lookInverse1 + lookInverse2;
            float targetAngle = Mathf.Lerp(lerp1, lerp2, targetInverse);

            float movementVariation = 0f;
            if (_useHandleVariation)
            {
                float idleNoise = Mathf.PerlinNoise(Time.time * _handleIdleVariation.HandleVariationSpeed, 0);
                float idleVariation = Mathf.Lerp(_handleIdleVariation.HandleVariation.RealMin, _handleIdleVariation.HandleVariation.RealMax, idleNoise);

                float walkNoise = Mathf.PerlinNoise(Time.time * _handleWalkVariation.HandleVariationSpeed, 0);
                float walkVariation = Mathf.Lerp(_handleWalkVariation.HandleVariation.RealMin, _handleWalkVariation.HandleVariation.RealMax, walkNoise);

                movement.y = 0f;
                movement = Vector3.ClampMagnitude(movement, 1f);
                _variationBlend = Mathf.SmoothDamp(_variationBlend, movement.magnitude, ref _variationVelocity, _variationBlendTime);
                movementVariation = Mathf.Lerp(idleVariation, walkVariation, _variationBlend);
            }

            _handleAngle = Mathf.SmoothDamp(_handleAngle, targetAngle, ref _handleVelocity, _handleGravityTime);
            _handleBone.localRotation = Quaternion.AngleAxis(_handleAngle + movementVariation, _handleAxis.Convert());
        }

        public override void OnUpdate()
        {
            if (!_updateHandle)
                return;

            float flicker = Mathf.PerlinNoise(Time.time * _flameFlickerSpeed, 0);
            _flameIntensity = Mathf.Lerp(_flameFlickerLimits.RealMin, _flameFlickerLimits.RealMax, flicker) * _flameLightIntensity;

            if (_isEquipped && !_isBusy && !_infiniteFuel)
            {
                // lantern fuel
                _currentFuel = _currentFuel > 0 ? _currentFuel -= Time.deltaTime : 0;

                if (_currentFuel <= 0 && !_noFueldAudioPlayed)
                {
                    _noFueldAudioPlayed = true;
                    AudioManager.PostAudioEvent(AudioDialog.Play_Estella_Phrase_CantSee, gameObject);
                }
                
                UpdateFuel();
            }

            float fuelFlameIntensity = _flameIntensity * LanternFuel;
            _flameLerp = Mathf.MoveTowards(_flameLerp, _targetFlame, Time.deltaTime * _flameChangeSpeed);
            _lanternLight.intensity = Mathf.Lerp(0f, fuelFlameIntensity, _flameLerp);
        }

        private void UpdateFuel()
        {
            LanternFuel = Mathf.InverseLerp(0, _fuelLife, _currentFuel);
            _lanternFlameGroup.alpha = LanternFuel;

            if (_lanternFlame != null)
            {
                float mappedT = Mathf.InverseLerp(0, _flameAlphaFadeStart, _currentFuel);
                float flameAlpha = Mathf.Lerp(1, 0, 1 - mappedT);
                _lanternFlame.material.SetFloat("_Fade", flameAlpha);
            }
        }

        public override void OnItemCombine(InventoryItem combineItem)
        {
            if (combineItem.ItemGuid != _fuelInventoryItem || !_isEquipped)
                return;

            Inventory.Instance.RemoveItem(combineItem, 1);
            Animator.SetTrigger(_lanternReloadTrigger);
            StartCoroutine(ReloadLantern());
            _isBusy = true;
        }

        private IEnumerator ReloadLantern()
        {
            yield return new WaitForAnimatorClip(Animator, _lanternReloadStartState);

            yield return new WaitForSeconds(_fuelReloadTime);

            Animator.SetTrigger(_lanternReloadEndTrigger);
            _currentFuel = _fuelPercentage.From(_fuelLife);
            _noFueldAudioPlayed = false;
            UpdateFuel();

            yield return new WaitForAnimatorClip(Animator, _lanternReloadEndState);

            _isBusy = false;
        }

        public override void OnItemSelect()
        {
            CanvasGroupFader.StartFadeInstance(_lanternPanelGroup, true, 5f);

            ItemObject.SetActive(true);
            StartCoroutine(SelectLantern());

            _flameLerp = 0f;
            _targetFlame = 1f;
            _updateHandle = true;
            _isEquipped = false;
            _isBusy = false;
        }

        private IEnumerator SelectLantern()
        {
            yield return new WaitForAnimatorClip(Animator, _lanternDrawState);
            _isEquipped = true;
        }

        public override void OnItemDeselect()
        {
            CanvasGroupFader.StartFadeInstance(_lanternPanelGroup, false, 5f,
                () => _lanternPanelGroup.gameObject.SetActive(false));

            StopAllCoroutines();
            StartCoroutine(HideLantern());
            Animator.SetTrigger(_lanternHideTrigger);

            _targetFlame = 0f;
            _isBusy = true;
        }

        private IEnumerator HideLantern()
        {
            yield return new WaitForAnimatorClip(Animator, _lanternHideState);
            yield return new WaitUntil(() => _flameLerp <= 0f);
            ItemObject.SetActive(false);
            _updateHandle = false;
            _isEquipped = false;
            _isBusy = false;
        }

        public override void OnItemActivate()
        {
            StopAllCoroutines();
            ItemObject.SetActive(true);
            Animator.Play(_lanternIdleState);

            _flameLerp = 1f;
            _targetFlame = 1f;

            _updateHandle = true;
            _isEquipped = true;
            _isBusy = false;
        }

        public override void OnItemDeactivate()
        {
            StopAllCoroutines();
            ItemObject.SetActive(false);
            _updateHandle = false;
            _isEquipped = false;
            _isBusy = false;
        }

        public override StorableCollection OnCustomSave()
        {
            return new StorableCollection()
            {
                { "currentFuel", _currentFuel }
            };
        }

        public override void OnCustomLoad(JToken data)
        {
            _currentFuel = _fuelPercentage.From(_fuelLife);
            UpdateFuel();
        }
    }
}