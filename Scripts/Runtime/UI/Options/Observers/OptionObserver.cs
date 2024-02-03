using UnityEngine;

namespace HJ.Runtime
{
    public class OptionObserver : MonoBehaviour
    {
        [Space, SerializeField] private string _optionName;
        [Space, SerializeField] private GenericReflectionField _optionAction;

        private void Start()
        {
            OptionsManager.ObserveOption(_optionName, (obj) => _optionAction.Value = obj);
        }
    }
}
