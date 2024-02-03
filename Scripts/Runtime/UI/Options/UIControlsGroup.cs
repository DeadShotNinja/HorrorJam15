using UnityEngine;
using HJ.Input;

namespace HJ.Runtime
{
    public class UIControlsGroup : MonoBehaviour
    {
        public void ResetBindings()
        {
            InputManager.ResetInputsToDefaults();
        }
    }
}