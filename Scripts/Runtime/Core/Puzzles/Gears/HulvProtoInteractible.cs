using System.Collections;
using UnityEngine;
using HJ.Tools;
using UnityEngine.Events;

namespace HJ.Runtime
{
    public class HulvProtoInteractible : MonoBehaviour, IInteractStart
    {
        [SerializeField] private UnityEvent<bool> _onTriggered;

        public void InteractStart()
        {
            _onTriggered?.Invoke(true);
        }

        public void SetInteractState(bool state)
        {
        }
    }
}