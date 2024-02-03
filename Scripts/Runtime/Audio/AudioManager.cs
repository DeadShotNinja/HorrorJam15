using System;
using System.Collections.Generic;
using HJ.Scriptable;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HJ.Runtime
{
    public class AudioManager : Singleton<AudioManager>
    {
        [Title("Audio Database Asset")]
        [field: SerializeField]
        public AudioAsset AudioAsset { get; private set; }

        [Title("Soundbanks to Load on Initialization")]
        [SerializeField] private List<AK.Wwise.Bank> _soundbanks;

        private readonly Dictionary<(Type, Enum), AK.Wwise.Event> _cachedWwiseEvents = new();

        private bool _musicPlaying = false;
        private bool _ambiencePlaying = false;

        private void Awake()
        {
            LoadSoundbanks();
        }

        private void Start()
        {
            SwitchStateSceneDependent();
        }
        
        private void SwitchStateSceneDependent()
        {
            Scene currentScene = SceneManager.GetActiveScene();
            string sceneName = currentScene.name;

            switch (sceneName)
            {
                case "1_MainMenu":
                    SetAudioState(AudioState.MainMenu);
                    PlayMainMusic();
                    break;
                case "2_LevelManager":
                    PostAudioEvent(AudioMusic.PlayLevelTransitionMusic, gameObject);
                    StopMainMusic();
                    break;
                case "3_Level_Outside":
                    PlayMainAmbience();
                    PlayMainMusic();
                    SetAudioState(AudioState.Lakeside);
                    SetAudioState(AudioState.GameActive);
                    break;
                case "4_Level_Inside":
                    PlayMainMusic();
                    SetAudioState(AudioState.LightHouse);
                    SetAudioState(AudioState.GameActive);
                    break;
                case "5_Credits":
                    PlayMainMusic();
                    SetAudioState(AudioState.Credits);
                    break;
            }
        }
        
        public void PlayMenuNavigateSound()
        {
            PostAudioEvent(AudioUI.UIMenuNavigate, gameObject);
        }
        
        public void PlayMenuNegativeSound()
        {
            PostAudioEvent(AudioUI.UIMenuNegative, gameObject);
        }
        
        public void PlayMenuObjectiveSound()
        {
            PostAudioEvent(AudioUI.UIMenuNavigate, gameObject);
        }
        
        public void PlayMenuPosativeSound()
        {
            PostAudioEvent(AudioUI.UIMenuNavigate, gameObject);
        }

        public static void PostAudioEvent<T>(T audioType, GameObject gameObject) where T : Enum
        {
            if (!Instance._cachedWwiseEvents.TryGetValue((typeof(T), audioType), out AK.Wwise.Event audioEvent))
            {
                audioEvent = Instance.AudioAsset.GetEvent(audioType);

                if (audioEvent != null)
                {
                    Instance._cachedWwiseEvents[(typeof(T), audioType)] = audioEvent;
                }
            }

            audioEvent?.Post(gameObject);
        }
        
        public static void PostAudioEventSpecial<T>(T audioType, GameObject gameObject, AkCallbackManager.EventCallback callback) where T : Enum
        {
            if (!Instance._cachedWwiseEvents.TryGetValue((typeof(T), audioType), out AK.Wwise.Event audioEvent))
            {
                audioEvent = Instance.AudioAsset.GetEvent(audioType);

                if (audioEvent != null)
                {
                    Instance._cachedWwiseEvents[(typeof(T), audioType)] = audioEvent;
                }
            }

            audioEvent?.Post(gameObject, (uint)AkCallbackType.AK_EndOfEvent, callback);
        }

        public static void SetAudioState(AudioState audioState)
        {
            AK.Wwise.State wwiseState = Instance.AudioAsset.GetState(audioState);
            wwiseState?.SetValue();
        }
        
        public void OnCutsceneSkipped()
        {
            PostAudioEvent(AudioDialog.StopIntroCutsceneDialog, this.gameObject);
        }

        public void PlayMainAmbience()
        {
            if (_ambiencePlaying == false)
            {
                PostAudioEvent(AudioAmbience.PlayMainAmbience, gameObject);
                _ambiencePlaying = true;
            }
        }
        public void StopAmbience()
        {
            if (_ambiencePlaying == true)
            {
                PostAudioEvent(AudioAmbience.StopMainAmbiences, gameObject);
                _ambiencePlaying = false;
            }
        }

        public void PlayMainMusic()
        {
            if (_musicPlaying == false)
            {
                PostAudioEvent(AudioMusic.PlayMainMusic, gameObject);
                _musicPlaying = true;
            }
        }

        public void StopMainMusic()
        {
            if (_musicPlaying == true)
            {
                PostAudioEvent(AudioMusic.StopMainMusic, gameObject);
                _musicPlaying = false;
            }
        }

        public void SetStateToActive()
        {
            SetAudioState(AudioState.GameActive);
        }

        private void LoadSoundbanks()
        {
            if (_soundbanks.Count > 0)
            {
                foreach (AK.Wwise.Bank bank in _soundbanks)
                {
                    bank.Load();
                }
                Debug.Log("Soundbanks Loaded!");
            }
            else
            {
                Debug.LogWarning("no soundbanks loaded! are banks assigned in audio state manager?");
            }
        }
    }
}