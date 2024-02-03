using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HJ.Runtime
{
    [Serializable]
    public sealed class ContainerSettings
    {
        public RectTransform ContainerObject;
        public RectTransform ContainerItems;
        public GridLayoutGroup ContainerSlots;
        public TMP_Text ContainerName;
    }
}
