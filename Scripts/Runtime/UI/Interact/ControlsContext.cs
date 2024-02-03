using System;
using System.Collections.Generic;
using UnityEngine;
using HJ.Attributes;
using static HJ.Input.InputManager;

namespace HJ.Runtime
{
    [Serializable]
    public sealed class ControlsContext
    {
        public InputReference InputAction;
        public GString InteractName;

        public void SubscribeGloc()
        {
            InteractName.SubscribeGloc();
        }
    }
}
