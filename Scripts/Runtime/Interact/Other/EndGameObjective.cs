using System;
using HJ.Input;
using HJ.Runtime;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace HJ
{
    [RequireComponent(typeof(BoxCollider))]
    public class EndGameObjective : MonoBehaviour
    {
        [SerializeField] private ObjectiveManager _objManager;
        [SerializeField] private string _objectiveToCheckFor;
        [SerializeField] private UnityEvent _OnEndGame;
        
        private BoxCollider _boxCollider;

        private void Awake()
        {
            _boxCollider = GetComponent<BoxCollider>();
            _boxCollider.enabled = false;
        }

        private void Start()
        {
            InputManager.Performed(Controls.SKIP, TestCutscene);
            
            int endGameValue = 0;
            if (PlayerPrefs.HasKey("EndGame"))
                endGameValue = PlayerPrefs.GetInt("EndGame");
            
            if (endGameValue == 1 || _objManager.ActiveObjectives.ContainsKey(_objectiveToCheckFor))
            {
                _boxCollider.enabled = true;
            }
        }
        
        private void TestCutscene(InputAction.CallbackContext context)
        {
            _boxCollider.enabled = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _OnEndGame?.Invoke();
            }
        }
    }
}
