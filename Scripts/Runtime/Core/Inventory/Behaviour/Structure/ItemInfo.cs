using System;
using TMPro;
using UnityEngine;

namespace HJ.Runtime
{
    [Serializable]
    public sealed class ItemInfo
    {
        public GameObject InfoPanel;
        public TMP_Text ItemTitle;
        public TMP_Text ItemDescription;
    }
}
