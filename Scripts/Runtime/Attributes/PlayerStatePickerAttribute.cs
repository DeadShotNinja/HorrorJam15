using System;
using UnityEngine;

namespace HJ.Attributes
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class PlayerStatePickerAttribute : PropertyAttribute
    {
        public bool IncludeDefault;

        public PlayerStatePickerAttribute(bool includeDefault = false)
        {
            IncludeDefault = includeDefault;
        }
    }
}
