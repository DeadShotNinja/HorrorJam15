using System;
using System.Collections.Generic;
using UnityEngine;
using HJ.Attributes;

namespace HJ.Runtime
{
    [Serializable]
    public sealed class StateMotionData
    {
        [PlayerStatePicker(IncludeDefault = true)]
        public string StateID = "Default";

        [SerializeReference]
        public List<MotionModule> Motions = new();
    }
}
