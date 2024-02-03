using Newtonsoft.Json.Linq;
using UnityEngine;

namespace HJ.Runtime
{
    public class ObjectiveTrigger : MonoBehaviour, IInteractStart, ISaveable
    {
        public enum TriggerType { Trigger, Interact, Event }
        public enum ObjectiveType { New, Complete, NewAndComplete }

        [SerializeField] private TriggerType _triggerType = TriggerType.Trigger;
        [SerializeField] private ObjectiveType _objectiveType = ObjectiveType.New;

        [SerializeField] private ObjectiveSelect _objectiveToAdd;
        [SerializeField] private ObjectiveSelect _objectiveToComplete;

        private ObjectiveManager _objectiveManager;
        private bool _isTriggered;

        private void Awake()
        {
            _objectiveManager = ObjectiveManager.Instance;
        }

        public void InteractStart()
        {
            if (_triggerType != TriggerType.Interact || _triggerType == TriggerType.Event || _isTriggered)
                return;

            TriggerObjective();
            _isTriggered = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_triggerType != TriggerType.Trigger || _triggerType == TriggerType.Event || _isTriggered)
                return;

            if (other.CompareTag("Player"))
            {
                TriggerObjective();
                _isTriggered = true;
            }
        }

        public void TriggerObjective()
        {
            if (_objectiveType == ObjectiveType.New)
            {
                _objectiveManager.AddObjective(_objectiveToAdd.ObjectiveKey, _objectiveToAdd.SubObjectives);
            }
            else if (_objectiveType == ObjectiveType.Complete)
            {
                _objectiveManager.CompleteObjective(_objectiveToComplete.ObjectiveKey, _objectiveToComplete.SubObjectives);
            }
            else if(_objectiveType == ObjectiveType.NewAndComplete)
            {
                _objectiveManager.AddObjective(_objectiveToAdd.ObjectiveKey, _objectiveToAdd.SubObjectives);
                _objectiveManager.CompleteObjective(_objectiveToComplete.ObjectiveKey, _objectiveToComplete.SubObjectives);
            }
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { nameof(_isTriggered), _isTriggered }
            };
        }

        public void OnLoad(JToken data)
        {
            _isTriggered = (bool)data[nameof(_isTriggered)];
        }
    }
}