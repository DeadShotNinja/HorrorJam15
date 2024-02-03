using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using Cinemachine;
using Newtonsoft.Json.Linq;
using HJ.Input;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem;

namespace HJ.Runtime
{
    public class CutsceneTrigger : MonoBehaviour, ISaveable
    {
        public enum CutsceneTypeEnum { CameraCutscene, PlayerCutscene }

        public CutsceneTypeEnum CutsceneType;
        public PlayableDirector Cutscene;
        // For playOnStart, need to make sure the cinema VC is activated and player cinema VC is deactivated.
        public bool PlayOnStart;

        public CinemachineVirtualCamera CutsceneCamera;
        public float CutsceneFadeSpeed;

        public Vector3 InitialPosition;
        public Vector2 InitialLook;

        public UnityEvent OnCutsceneStart;
        public UnityEvent OnCutsceneEnd;

        private CutsceneModule _cutscene;
        private PlayerStateMachine _player;
        private bool _isPlayed;

        private void Awake()
        {
            _cutscene = GameManager.Module<CutsceneModule>();
            PlayerPresenceManager playerPresence = PlayerPresenceManager.Instance;
            _player = playerPresence.Player.GetComponent<PlayerStateMachine>();
        }

        private void Start()
        {
            InputManager.Performed(Controls.JUMP, SkipCutscene);

            if (PlayOnStart && PlayerPrefs.GetInt("IntroCutscenePlayed") == 0)
                StartCutscene();
            else
            {
                PlayerPresenceManager.Instance.UnlockPlayer();
            }
        }
        
        public void StartCutscene()
        {
            StartCoroutine(ManualCutsceneTrigger());
        }

        private IEnumerator ManualCutsceneTrigger()
        {
            yield return new WaitForEndOfFrame();
            PlayCutscene();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                PlayCutscene();
            }
        }
        
        private void PlayCutscene()
        {
            if (_isPlayed) return;
            
            if (CutsceneType == CutsceneTypeEnum.CameraCutscene)
            {
                _cutscene.PlayCutscene(PlayOnStart, Cutscene, CutsceneCamera.gameObject, CutsceneFadeSpeed, () => OnCutsceneEnd?.Invoke());
            }
            else
            {
                _player.ChangeState("Cutscene", 
                    new StorableCollection() {
                        { "position", InitialPosition },
                        { "look", InitialLook },
                        { "cutscene", Cutscene },
                        { "event", new System.Action(() => OnCutsceneEnd?.Invoke()) }
                    });
            }

            OnCutsceneStart?.Invoke();
            _isPlayed = true;
            PlayerPrefs.SetInt("IntroCutscenePlayed", 1);
        }

        private void SkipCutscene(InputAction.CallbackContext context)
        {
            if (Cutscene.state != PlayState.Playing) 
                return;
            
            _cutscene.SkipCutscene(PlayOnStart, CutsceneType, () => OnCutsceneEnd?.Invoke());
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { nameof(_isPlayed), _isPlayed }
            };
        }

        public void OnLoad(JToken data)
        {
            _isPlayed = (bool)data[nameof(_isPlayed)];
        }
    }
}