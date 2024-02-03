using System;
using System.Collections.Generic;
using UnityEngine;

namespace HJ.Runtime
{
    public class WwiseTester : MonoBehaviour
    {
        [Serializable]
        public struct MyStruct
        {
            public int Number;
            public string Name;
        }

        public MyStruct[] something;

        [SerializeField] private AK.Wwise.Event _mainAmbienceEvent;
        
        [SerializeField] private string _rtpcNameMasterVolume = "Master_Volume";
        [SerializeField] private string _rtpcNameMusicVolume = "Music_Volume";
        [SerializeField] private string _rtpcNameSFXVolume = "SFX_Volume";
        
        [SerializeField, Range(0f, 100f)] private float _masterVolume = 100f;
        [SerializeField, Range(0f, 100f)] private float _musicVolume = 100f;
        [SerializeField, Range(0f, 100f)] private float _sfxVolume = 100f;

        private void Start()
        {
            //AudioManager.PostAudioEvent(AudioAmbience.PlayMainAmbience, gameObject);
        }

        private void Update()
        {
            SetRTPCValue(_rtpcNameMasterVolume, _masterVolume);
            SetRTPCValue(_rtpcNameMusicVolume, _musicVolume);
            SetRTPCValue(_rtpcNameSFXVolume, _sfxVolume);
        }
        
        private void SetRTPCValue(string rtpcName, float value)
        {
            AkSoundEngine.SetRTPCValue(rtpcName, value);
        }
    }
}
