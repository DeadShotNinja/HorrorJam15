using HJ.Scriptable;
using UnityEngine;

namespace HJ.Runtime
{
    public struct MotionSettings
    {
        public MotionPreset Preset;
        public PlayerComponent Component;
        public Transform MotionTransform;
        public string MotionState;
    }
}
