using UnityEngine;
using UnityEngine.Events;

namespace HJ.Runtime
{
    public class CustomInteract : MonoBehaviour, IInteractStart
    {
        public UnityEvent OnInteract;
        
        public void InteractStart()
        {
            OnInteract?.Invoke();
        }
    }
}
