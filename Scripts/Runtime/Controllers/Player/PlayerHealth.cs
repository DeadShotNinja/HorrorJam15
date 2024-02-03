using UnityEngine;
using HJ.Tools;
using HJ.Rendering;
using static HJ.Runtime.GameManager;

namespace HJ.Runtime
{
    public class PlayerHealth : BaseHealthEntity
    {
        [Tooltip("Maximum health the player can have.")]
        [SerializeField] private uint _maxHealth = 100;
        [Tooltip("Health the player starts with.")]
        public uint StartHealth = 100;

        [Tooltip("Enable or disable the heartbeat effect.")]
        [SerializeField] private bool _useHearthbeat = true;
        [Tooltip("Pulse rate when health is low.")]
        [SerializeField] private float _lowHealthPulse = 3f;
        [Tooltip("Time taken for the health to fade.")]
        [SerializeField] private float _healthFadeTime = 0.1f;

        [Tooltip("Health threshold below which the blood effect starts.")]
        [SerializeField] private uint _minHealthFade = 20;
        [Tooltip("Duration of the blood effect.")]
        [SerializeField] private float _bloodDuration = 2f;
        [Tooltip("Speed at which the blood effect fades in.")]
        [SerializeField] private float _bloodFadeInSpeed = 15f;
        [Tooltip("Speed at which the blood effect fades out.")]
        [SerializeField] private float _bloodFadeOutSpeed = 2f;
        
        [Tooltip("Time taken to close the eyes upon death.")]
        [SerializeField] private float _closeEyesTime = 2f;
        [Tooltip("Speed at which the eyes close upon death.")]
        [SerializeField] private float _closeEyesSpeed = 2f;
        
        [Tooltip("Is the player invisible to enemies?")]
        [SerializeField] private bool _isInvisibleToEnemies;
        [Tooltip("Is the player invisible to allies?")]
        [SerializeField] private bool _isInvisibleToAllies;

        private GameManager _gameManager;
        private EyeBlink _eyeBlink;

        private float _targetHealth;
        private float _healthVelocity;

        private float _bloodWeight;
        private float _targetBlood;
        private float _bloodTime;
        private float _eyesTime;

        #region Properties

        public uint MaxHealth => _maxHealth;
        public bool IsInvisibleToEnemies => _isInvisibleToEnemies;
        public bool IsInvisibleToAllies => _isInvisibleToAllies;

        #endregion

        private void Awake()
        {
            _gameManager = GameManager.Instance;
            _gameManager.HealthPPVolume.profile.TryGet(out _eyeBlink);

            if (!SaveGameManager.IsGameJustLoaded || !SaveGameManager.GameStateExist)
                InitHealth();
        }

        public void InitHealth()
        {
            InitializeHealth((int)StartHealth, (int)_maxHealth);

            if (StartHealth <= _minHealthFade)
            {
                _targetBlood = 1f;
                _bloodTime = _bloodDuration;
            }
        }

        public override void OnHealthChanged(int oldHealth, int newHealth)
        {
            _gameManager.HealthPercent.text = newHealth.ToString();
            _targetHealth = (float)newHealth / _maxHealth;

            if (_useHearthbeat)
            {
                Material hearthbeatMat = _gameManager.Hearthbeat.material;

                if (newHealth <= 0)
                {
                    hearthbeatMat.EnableKeyword("ZERO_PULSE");
                }
                else
                {
                    float pulse = GameTools.Remap(0f, _maxHealth, _lowHealthPulse, 1f, newHealth);
                    hearthbeatMat.SetFloat("_PulseMultiplier", pulse);
                    hearthbeatMat.DisableKeyword("ZERO_PULSE");
                }
            }
        }

        public override void OnApplyDamage(int damage, Transform sender = null)
        {
            if (IsDead) return;

            base.OnApplyDamage(damage, sender);

            AudioManager.PostAudioEvent(AudioPlayer.PlayerTakeDamage, gameObject);

            _targetBlood = 1f;
            _bloodTime = _bloodDuration;
        }

        public override void OnApplyHeal(int healAmount)
        {
            base.OnApplyHeal(healAmount);
            if(EntityHealth > _minHealthFade)
                _bloodTime = _bloodDuration;
        }

        public override void OnHealthZero()
        {
            _gameManager.ShowPanel(PanelType.DeadPanel);
            _gameManager.PlayerPresence.FreezePlayer(true, true);
            _gameManager.PlayerPresence.PlayerManager.PlayerItems.DeactivateCurrentItem();
            _targetBlood = 1f;
        }

        private void Update()
        {
            float healthValue = _gameManager.HealthBar.value;
            healthValue = Mathf.SmoothDamp(healthValue, _targetHealth, ref _healthVelocity, _healthFadeTime);
            _gameManager.HealthBar.value = healthValue;

            if (EntityHealth > _minHealthFade)
            {
                if (_bloodTime > 0f) _bloodTime -= Time.deltaTime;
                else
                {
                    _targetBlood = 0f;
                    _bloodTime = 0f;
                }
            }

            _bloodWeight = Mathf.MoveTowards(_bloodWeight, _targetBlood, Time.deltaTime * (_bloodTime > 0 ? _bloodFadeInSpeed : _bloodFadeOutSpeed));
            _gameManager.HealthPPVolume.weight = _bloodWeight;

            if (IsDead && _eyeBlink != null)
            {
                if (_eyesTime < _closeEyesTime)
                {
                    _eyesTime += Time.deltaTime;
                }
                else
                {
                    float blinkValue = _eyeBlink.Blink.value;
                    _eyeBlink.Blink.value = Mathf.MoveTowards(blinkValue, 1f, Time.deltaTime * _closeEyesSpeed);
                }
            }
        }
    }
}
