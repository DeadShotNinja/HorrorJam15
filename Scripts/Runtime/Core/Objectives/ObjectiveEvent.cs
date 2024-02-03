using UnityEngine;
using UnityEngine.Events;

namespace HJ.Runtime
{
    public class ObjectiveEvent : MonoBehaviour
    {
        // TODO: Complete this class...
        public SingleObjectiveSelect Objective;
        
        public UnityEvent OnObjectiveAdded;
        public UnityEvent OnObjectiveCompleted;

        public UnityEvent OnSubObjectiveAdded;
        public UnityEvent OnSubObjectiveCompleted;
        public UnityEvent<int> OnSubObjectiveCountChanged;
    }
}