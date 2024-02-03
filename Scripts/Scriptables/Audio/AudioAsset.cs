using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace HJ.Scriptable
{
    [CreateAssetMenu(fileName = "AudioAsset", menuName = "HJ/Audio Asset")]
    public class AudioAsset : ScriptableObject
    {
        // IMPORTANT: all enums for this asset have to follow this naming convention "Audio+Type" (ex. AudioAmbience)
        // IMPORTANT: all properties for this asset need to have the type part of the enum as its name!!
        
        // EVENTS
        [field: SerializeField]
        public List<AudioEventData<AudioAmbience>> Ambience { get; private set; } = new();

        [field: SerializeField]
        public List<AudioEventData<AudioEnemies>> Enemies { get; private set; } = new();
        
        [field: SerializeField]
        public List<AudioEventData<AudioEnvironment>> Environment { get; private set; } = new();
        
        [field: SerializeField]
        public List<AudioEventData<AudioItems>> Items { get; private set; } = new();
        
        [field: SerializeField]
        public List<AudioEventData<AudioMusic>> Music { get; private set; } = new();
        
        [field: SerializeField]
        public List<AudioEventData<AudioPlayer>> Player { get; private set; } = new();
        
        [field: SerializeField]
        public List<AudioEventData<AudioUI>> UI { get; private set; } = new();
        
        [field: SerializeField]
        public List<AudioEventData<AudioDialog>> Dialog { get; private set; } = new();

        // STATES
        [field: SerializeField]
        public List<AudioStateData> State { get; private set; } = new();

        public AK.Wwise.Event GetEvent<T>(T type) where T : Enum
        {
            string enumName = typeof(T).Name;
            string propertyName = enumName.StartsWith("Audio") ? enumName.Substring(5) : enumName;
            string listName = $"<{propertyName}>k__BackingField";

            FieldInfo listField = GetType().GetField(listName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (listField == null)
            {
                Debug.LogError("[AudioAsset] INCORRECT NAMING CONVENTION USED!!! READ TOP OF SCRIPT!!!!");
                return null;
            }

            if (listField.GetValue(this) is not List<AudioEventData<T>> list) return null;

            AudioEventData<T> data = list.Find(x => EqualityComparer<T>.Default.Equals(x.Type, type));
            return data.Equals(default(AudioEventData<T>)) ? null : data.WwiseEvent;
        }

        public AK.Wwise.State GetState(AudioState type)
        {
            AudioStateData stateData = State.Find(x => x.Type == type);
            return stateData.WwiseState;
        }
    }
}