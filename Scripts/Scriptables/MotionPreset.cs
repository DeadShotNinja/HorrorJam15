using System.Collections.Generic;
using UnityEngine;
using HJ.Runtime;

namespace HJ.Scriptable
{
    [CreateAssetMenu(fileName = "Motion Preset", menuName = "HJ/Game/Motion Preset")]
    public class MotionPreset : ScriptableObject
    {
        public List<StateMotionData> StateMotions = new();
        public Dictionary<string, object> RuntimeParameters = new();
        
        public object this[string key]
        {
            get
            {
                if (RuntimeParameters.TryGetValue(key, out object value))
                    return value;

                return null;
            }
            set 
            {
                RuntimeParameters[key] = value;
            }
        }

        public bool TryGetValue(string key, out object value)
        {
            if (RuntimeParameters.TryGetValue(key, out object val))
            {
                value = val;
                return true;
            }

            value = null;
            return false;
        }

        public void Initialize(PlayerComponent component, Transform motionTransform)
        {
            foreach (var state in StateMotions)
            {
                foreach (var motion in state.Motions)
                {
                    motion.Initialize(new MotionSettings()
                    {
                        Preset = this,
                        Component = component,
                        MotionTransform = motionTransform,
                        MotionState = state.StateID
                    });
                }
            }
        }

        public void Reset()
        {
            foreach (var state in StateMotions)
            {
                foreach (var motion in state.Motions)
                {
                    motion.Reset();
                }
            }
        }
    }
}
