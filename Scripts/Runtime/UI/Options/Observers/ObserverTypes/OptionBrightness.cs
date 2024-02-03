using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HJ.Runtime
{
    [Serializable]
    public class OptionBrightness : OptionObserverType
    {
        [SerializeField] private Volume _volume;
        [SerializeField] private MinMax _exposureLimits;

        public override string Name => "Brightness";

        public override void OptionUpdate(object value)
        {
            if (value == null || _volume == null)
                return;

            if (_volume.profile.TryGet<ColorAdjustments>(out var colorAdjustments))
            {
                float exposure = Mathf.Lerp(_exposureLimits.RealMin, _exposureLimits.RealMax, (float)value);
                colorAdjustments.postExposure.value = exposure;
            }
        }
    }
}
