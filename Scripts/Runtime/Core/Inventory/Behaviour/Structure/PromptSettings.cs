using System;
using UnityEngine;

namespace HJ.Runtime
{
    [Serializable]
    public sealed class PromptSettings
    {
        public CanvasGroup PromptPanel;
        public GString ShortcutPrompt;
        public GString CombinePrompt;
    }
}
