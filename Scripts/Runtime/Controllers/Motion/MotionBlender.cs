using HJ.Scriptable;
using UnityEngine;

namespace HJ.Runtime
{
    public class MotionBlender
    {
        public const string Default = "Default";

        public MotionPreset Preset;
        public MotionPreset Instance;
        public bool IsInitialized;

        private float weight = 1f;
        public float Weight
        {
            get => weight;
            set => weight = Mathf.Clamp01(value);
        }
        
        public void Init(MotionPreset preset, Transform transform, PlayerComponent component)
        {
            Preset = preset;
            Instance = Object.Instantiate(preset);
            Instance.Initialize(component, transform);
            IsInitialized = true;
        }

        public void Dispose()
        {
            IsInitialized = false;
            if(Preset) Preset.Reset();
            Object.Destroy(Instance);
        }

        public void BlendMotions(float deltaTime, out Vector3 position, out Quaternion rotation)
        {
            Vector3 targetPos = Vector3.zero;
            Quaternion targetRot = Quaternion.identity;

            foreach (var state in Instance.StateMotions)
            {
                foreach (var motion in state.Motions)
                {
                    motion.MotionUpdate(deltaTime);
                    targetPos += targetRot * motion.GetPosition(deltaTime);
                    targetRot *= motion.GetRotation(deltaTime);
                }
            }

            position = Vector3.Lerp(Vector3.zero, targetPos, Weight);
            rotation = Quaternion.Slerp(Quaternion.identity, targetRot, Weight);
        }
    }
}
