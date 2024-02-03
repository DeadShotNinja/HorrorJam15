using System;
using Cinemachine;
using UnityEngine;

namespace HJ.Runtime
{
    [Serializable]
    public class OptionCameraFOV : OptionObserverType
    {
        [SerializeField] private CinemachineVirtualCamera _virtualCamera;

        public override string Name => "Camera FOV";

        public override void OptionUpdate(object value)
        {
            if (value == null || _virtualCamera == null)
                return;

            _virtualCamera.m_Lens.FieldOfView = (float)value;
        }
    }
}
