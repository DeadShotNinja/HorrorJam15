using UnityEngine;
using HJ.Scriptable;

namespace HJ.Runtime
{
    public static class SerializationUtillity
    {
        private static SerializationAsset _serializationAsset;
        public static SerializationAsset SerializationAsset
        {
            get
            {
                if (_serializationAsset == null)
                    _serializationAsset = Resources.Load<SerializationAsset>("SerializationAsset");

                return _serializationAsset;
            }
        }
    }
}
