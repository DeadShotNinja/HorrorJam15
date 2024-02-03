using System;
using UnityEngine;
using UnityEngine.UI;

namespace HJ.Runtime
{
    [Serializable]
    public sealed class ContextMenu
    {
        public GameObject ContextMenuGO;
        public GameObject BlockerPanel;
        public float DisabledAlpha = 0.35f;

        [Header("Context Buttons")]
        public Button ContextUse;
        public Button ContextExamine;
        public Button ContextCombine;
        public Button ContextShortcut;
        public Button ContextDrop;
        public Button ContextDiscard;
    }
}
