using System;
using UnityEngine;

namespace HJ.Runtime
{
    [Serializable]
    public class OptionAudioVolume : OptionObserverType
    {
        // TODO: this works, need to switch to Wwise implem.
        //public AudioSource AudioSource;
        private float _audioSourceVolume;

        public override string Name => "Audio Volume";

        public override void OnStart()
        {
            //_audioSourceVolume = AudioSource.volume;
        }

        public override void OptionUpdate(object value)
        {
            //if (value == null || AudioSource == null)
            //    return;

            //AudioSource.volume = _audioSourceVolume * (float)value;
        }
    }
}
