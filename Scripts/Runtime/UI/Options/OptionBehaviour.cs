using Newtonsoft.Json.Linq;
using UnityEngine;

namespace HJ.Runtime
{
    public abstract class OptionBehaviour : MonoBehaviour
    {
        [Header("Setup")]
        public bool IsChanged;

        public abstract object GetOptionValue();
        public abstract void SetOptionValue(object value);

        public virtual void SetOptionData(string[] data) { }
    }
}