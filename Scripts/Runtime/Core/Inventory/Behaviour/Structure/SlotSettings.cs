using System;
using UnityEngine;

namespace HJ.Runtime
{
    [Serializable]
    public sealed class SlotSettings
    {
        public GameObject SlotPrefab;
        public GameObject SlotItemPrefab;

        [Header("Slot Textures")]
        public Sprite NormalSlotFrame;
        public Sprite RestrictedSlotFrame;

        [Header("Slot Colors")]
        public Color ItemNormalColor = Color.white;
        public Color ItemHoverColor = Color.white;
        public Color ItemMoveColor = Color.white;
        public Color ItemErrorColor = Color.white;
        public float ColorChangeSpeed = 20f;

        [Header("Slot Quantity")]
        public Color NormalQuantityColor = Color.white;
        public Color ZeroQuantityColor = Color.red;
    }
}
