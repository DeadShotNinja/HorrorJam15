using System;
using UnityEngine;

namespace HJ.Runtime
{
    [Serializable]
    public class OptionVolumeActive : OptionObserverType
    {
        [SerializeField] private VolumeComponentReferecne _volumeComponent = new();

        public override string Name => "Volume Active";

        public override void OptionUpdate(object value)
        {
            if (value == null || _volumeComponent.Volume == null)
                return;

            _volumeComponent.Volume.profile.components[_volumeComponent.ComponentIndex].active = (bool)value;
        }
    }
}
