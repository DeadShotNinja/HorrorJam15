using UnityEngine;
using HJ.Tools;
using UnityEngine.Events;

namespace HJ.Runtime
{
    public class LevelInteract : MonoBehaviour, IInteractStart
    {
        public enum LevelType { NextLevel, WorldState, PlayerData }

        [SerializeField] private LevelType _levelLoadType = LevelType.NextLevel;
        [SerializeField] private string _nextLevelName;

        [SerializeField] private bool _customTransform;
        [SerializeField] private Transform _targetTransform;
        [SerializeField] private float _lookUpDown;

        [SerializeField] private AK.Wwise.State _nextWwiseState;

        [SerializeField] private UnityEvent _onInteract;

        public void InteractStart()
        {
            _onInteract?.Invoke();
            _nextWwiseState?.SetValue();
            
            if (_levelLoadType == LevelType.PlayerData)
            {
                SaveGameManager.SavePlayer();
                GameManager.Instance.LoadNextLevel(_nextLevelName);
            }
            else if (_customTransform)
            {
                SaveGameManager.SaveGame(_targetTransform.position, new Vector2(_targetTransform.eulerAngles.y, _lookUpDown), () =>
                {
                    if (_levelLoadType == LevelType.NextLevel)
                        GameManager.Instance.LoadNextLevel(_nextLevelName);
                    else
                        GameManager.Instance.LoadNextWorld(_nextLevelName);
                });
            }
            else
            {
                SaveGameManager.SaveGame(() =>
                {
                    if (_levelLoadType == LevelType.NextLevel)
                        GameManager.Instance.LoadNextLevel(_nextLevelName);
                    else
                        GameManager.Instance.LoadNextWorld(_nextLevelName);
                });
            }
        }

        private void OnDrawGizmos()
        {
            if(_customTransform && _targetTransform != null)
            {
#if UNITY_EDITOR
                UnityEditor.Handles.color = Color.green.Alpha(0.01f);
                UnityEditor.Handles.DrawSolidDisc(_targetTransform.position, Vector3.up, 1f);
                UnityEditor.Handles.color = Color.green;
                UnityEditor.Handles.DrawWireDisc(_targetTransform.position, Vector3.up, 1f);
#endif
                Gizmos.DrawSphere(_targetTransform.position, 0.05f);
                GizmosE.DrawGizmosArrow(_targetTransform.position, _targetTransform.forward);
            }
        }
    }
}