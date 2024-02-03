using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

namespace HJ.Runtime
{
    public class CutsceneModule : ManagerModule
    {
        private PlayableDirector _currentCutscene;
        private Coroutine _cutsceneRoutine;
        private float _skipCutsceneFadeSpeed = 20f;
        private float _waitFadeTime = 0.5f;
        private float _fadeSpeed = 3f;

        public override string Name => "Cutscene";

        public void PlayCutscene(PlayableDirector cutscene, Action onCutsceneComplete)
        {
            _currentCutscene = cutscene;
            _cutsceneRoutine = GameManager.StartCoroutine(OnPlayPlayerCutscene(onCutsceneComplete));
        }

        public void PlayCutscene(bool playOnStart, PlayableDirector cutscene, GameObject cutsceneCamera, float fadeSpeed, Action onCutsceneComplete)
        {
            _currentCutscene = cutscene;
            _cutsceneRoutine = GameManager.StartCoroutine(OnPlayCameraCutscene(playOnStart, cutsceneCamera, fadeSpeed, onCutsceneComplete));
        }

        public void SkipCutscene(bool playOnStart, CutsceneTrigger.CutsceneTypeEnum cutsceneType, Action onCutsceneCompleted)
        {
            if (_currentCutscene == null) return;

            GameManager.StopCoroutine(_cutsceneRoutine);
            _currentCutscene.Stop();
            _currentCutscene.time = _currentCutscene.duration;
            _currentCutscene.Evaluate();

            if (_currentCutscene.state != PlayState.Playing)
            {
                _currentCutscene = null;

                if (cutsceneType == CutsceneTrigger.CutsceneTypeEnum.CameraCutscene)
                {
                    GameManager.StartCoroutine(OnSkipCameraCutscene(playOnStart, onCutsceneCompleted));
                }
                else
                {
                    GameManager.ShowPanel(GameManager.PanelType.MainPanel);
                    _playerPresence.FreezePlayer(false);
                    onCutsceneCompleted?.Invoke();
                }
            }
        }

        private IEnumerator OnPlayPlayerCutscene(Action onCutsceneComplete)
        {
            GameManager.DisableAllGamePanels();
            _playerPresence.FreezePlayer(true);
            _currentCutscene.Play();

            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds((float)_currentCutscene.duration);

            GameManager.ShowPanel(GameManager.PanelType.MainPanel);
            _playerPresence.FreezePlayer(false);

            _currentCutscene = null;
        }

        private IEnumerator OnPlayCameraCutscene(bool playOnStart, GameObject cutsceneCamera, float fadeSpeed, Action onCutsceneComplete)
        {
            GameManager.DisableAllGamePanels();
            _playerPresence.FreezePlayer(true);

            // if (!playOnStart)
            //     yield return _playerPresence.SwitchCamera(cutsceneCamera, fadeSpeed);
            // else
            //     yield return GameManager.StartBackgroundFade(true, _waitFadeTime, _fadeSpeed);
            //if (!playOnStart)
                yield return _playerPresence.SwitchCameraIntroCutscene(cutsceneCamera, fadeSpeed);
                //yield return GameManager.StartBackgroundFade(true, fadeSpeed: fadeSpeed);
            //else
            //    yield return GameManager.StartBackgroundFade(true, _waitFadeTime, _fadeSpeed);

            _currentCutscene.Play();

            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds((float)_currentCutscene.duration);
            yield return _playerPresence.SwitchCamera(null, fadeSpeed);

            GameManager.ShowPanel(GameManager.PanelType.MainPanel);
            
            if (playOnStart)
                _playerPresence.UnlockPlayer();
            else
                _playerPresence.FreezePlayer(false);
            
            onCutsceneComplete.Invoke();
            _currentCutscene = null;
        }

        private IEnumerator OnSkipCameraCutscene(bool playOnStart, Action onCutsceneCompleted)
        {
            yield return _playerPresence.SwitchCamera(null, _skipCutsceneFadeSpeed);

            GameManager.ShowPanel(GameManager.PanelType.MainPanel);
            
            if (playOnStart)
                _playerPresence.UnlockPlayer();
            else
                _playerPresence.FreezePlayer(false);
            
            onCutsceneCompleted?.Invoke();
            
            // TODO: Ending any cutscene sounds
            AudioManager.Instance.OnCutsceneSkipped();
        }
    }
}