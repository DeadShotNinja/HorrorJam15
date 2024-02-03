using System;
using UnityEngine;
using UnityEngine.Events;

namespace HJ.Runtime
{
    [Serializable]
    public sealed class Hotspot
    {
        public Transform HotspotTransform;
        [Tooltip("To show hotspot, keep this value true.")]
        public bool Enabled = true;
        [Tooltip("If this option is enabled, the hotspot action will be called when the examined item is put back.")]
        public bool ResetHotspot = false;
        [Space] public UnityEvent HotspotAction;
    }
}
