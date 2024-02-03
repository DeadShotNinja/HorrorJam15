using System;
using UnityEngine;

namespace HJ.Runtime
{
    [Serializable]
    public class OptionListenerVolume : OptionObserverType
    {
        public override string Name => "Audio Listener Volume";

        public override void OptionUpdate(object value)
        {
            //if (value == null)
            //    return;

            // TODO: Works, need to redo for Wwise
            //AudioListener.volume = (float)value;
        }
    }
}
