using System;
using UnityEngine;

namespace HJ.Runtime
{
    [Serializable]
    public sealed class ObjectReference
    {
        public string GUID;
        public GameObject Object;
    }
}