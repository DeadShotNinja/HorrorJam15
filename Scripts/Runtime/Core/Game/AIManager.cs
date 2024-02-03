using System;
using HJ.Runtime.States;
using HJ.Tools;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace HJ.Runtime
{
    public class AIManager : Singleton<AIManager>, ISaveable
    {
        public enum NoiseLevel { Green, Amber, Red }
        
        [Header("Monster Settings")]
        [SerializeField] private NPCStateMachine _monsterFSM;
        [SerializeField] private Transform _monsterSpawnPoint;
        
        [Header("Noise Settings")]
        [SerializeField] private MinMax _noiseRange = new (0f, 100f);
        [SerializeField] private float _monsterSpawnThreshold = 50f;
        [SerializeField] private float _survivalNoiseReduction = 20f;
        [SerializeField] private float _overtimeNoiseReduction = 2f;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private float _currentNoiseAmount = 0f;
        [field: SerializeField, ReadOnly]
        public NoiseLevel NoiseLevelEnum { get; private set; } = NoiseLevel.Green;
        
        private float _previousNoiseAmount;
        private bool _monsterIsActive;
        
        private CanvasGroup _noisePanelGroup;
        private Slider _noiseSlider;
        private Image _noiseFillImage;

        public float CurrentNoiseAmount => _currentNoiseAmount;
        public Vector3 MonsterStartPosition => _monsterSpawnPoint.position;
        public Vector3 LastNoiseLocation { get; private set; }
        
        public event Action<float> OnNoiseUpdated;

        private void Start()
        {
            GameManager gameManager = GameManager.Instance;

            var behaviours = gameManager.GraphicReferences.Value["Noise"];
            _noisePanelGroup = (CanvasGroup)behaviours[0];
            _noiseSlider = (Slider)behaviours[1];
            _noiseSlider.minValue = _noiseRange.Min;
            _noiseSlider.maxValue = _noiseRange.Max;
            _noiseSlider.value = 0f;
            _noiseFillImage = (Image)behaviours[2];
            _noiseFillImage.color = Color.green;

            _currentNoiseAmount = 0f;
        }

        private void Update()
        {
            if (_currentNoiseAmount > 0f && !_monsterIsActive)
            {
                float removeAmount = _overtimeNoiseReduction * Time.deltaTime;
                RemoveNoise(removeAmount);
            }            
        }

        public void AddNoise(Vector3 location, float amount)
        {
            LastNoiseLocation = location;
            UpdateNoise(amount);
        }
        
        public void RemoveNoise(float amount)
        {
            UpdateNoise(-amount);
        }
        
        private void UpdateNoise(float amount)
        {
            _previousNoiseAmount = _currentNoiseAmount;
            _currentNoiseAmount += amount;

            _currentNoiseAmount = Mathf.Clamp(_currentNoiseAmount, _noiseRange.Min, _noiseRange.Max);

            if (_currentNoiseAmount >= _monsterSpawnThreshold)
            {
                NoiseLevelEnum = NoiseLevel.Red;
                _noiseFillImage.color = Color.red;
                AudioManager.SetAudioState(AudioState.PlayerInDanager);
                AudioManager.PostAudioEvent(AudioPlayer.Play_Player_Scared, gameObject);
            }
            else
            {
                NoiseLevelEnum = NoiseLevel.Green;
                _noiseFillImage.color = Color.green;
                AudioManager.SetAudioState(AudioState.PlayerNotInDanger);
                AudioManager.PostAudioEvent(AudioPlayer.Stop_Player_Scared, gameObject);
            }
            
            if (_currentNoiseAmount >= _monsterSpawnThreshold 
                && _previousNoiseAmount < _monsterSpawnThreshold
                && !_monsterIsActive)
            {
                SpawnMonster();
            }
            else if (_currentNoiseAmount >= _monsterSpawnThreshold && !_monsterFSM.IsCurrent("Chase"))
            {
                if (_monsterIsActive) MonsterInvestigate();
                else SpawnMonster();
            }
            
            if (_currentNoiseAmount >= 1f && _previousNoiseAmount < 1f)
                CanvasGroupFader.StartFadeInstance(_noisePanelGroup, true, 5f);
            else if (_currentNoiseAmount < 1f && _previousNoiseAmount >= 1f)
                CanvasGroupFader.StartFadeInstance(_noisePanelGroup, false, 5f);

            _noiseSlider.value = _currentNoiseAmount;
            
            OnNoiseUpdated?.Invoke(_currentNoiseAmount);
        }
        
        private void SpawnMonster()
        {
            _monsterIsActive = true;
            _monsterFSM.transform.position = _monsterSpawnPoint.position;
            _monsterFSM.gameObject.SetActive(true);
            _monsterFSM.ChangeState<MonsterSummonedState>();
        }
        
        private void MonsterInvestigate()
        {
            _monsterFSM.ChangeState<MonsterSummonedState>();
        }
        
        public void DeSpawnMonster()
        {
            _monsterIsActive = false;
            _monsterFSM.gameObject.SetActive(false);
            UpdateNoise(-_survivalNoiseReduction);
        }

        public StorableCollection OnSave()
        {
            StorableCollection data = new() { { "noiseAmount", _currentNoiseAmount } };
            return data;
        }

        public void OnLoad(JToken data)
        {
            _currentNoiseAmount = data["noiseAmount"].ToObject<float>();
        }
    }
}
