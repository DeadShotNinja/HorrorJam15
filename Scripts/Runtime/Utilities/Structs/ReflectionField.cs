using System;
using System.Reflection;
using UnityEngine;

namespace HJ.Runtime
{
    /// <summary>
    /// Reflection field allows you to get or set a bool value for a property, field, or method.
    /// </summary>
    [Serializable]
    public sealed class ReflectionField
    {
        public enum ReflectionType { Field, Property, Method };

        [SerializeField] private ReflectionType _reflectType;
        [SerializeField] private MonoBehaviour _instance;
        [SerializeField] private string _reflectName;
        [SerializeField] private bool _reflectDerived;

        public bool IsSet => _instance != null;
        
        private FieldInfo _fieldInfo = null;
        private PropertyInfo _propertyInfo = null;
        private MethodInfo _methodInfo = null;
        
        private FieldInfo FieldInfo
        {
            get
            {
                if (_fieldInfo == null)
                    _fieldInfo = _instance.GetType().GetField(_reflectName, BindingFlags.Public | BindingFlags.Instance);

                return _fieldInfo;
            }
        }
        
        private PropertyInfo PropertyInfo
        {
            get
            {
                if (_propertyInfo == null)
                    _propertyInfo = _instance.GetType().GetProperty(_reflectName, BindingFlags.Public | BindingFlags.Instance);

                return _propertyInfo;
            }
        }
        
        private MethodInfo MethodInfo
        {
            get
            {
                if (_methodInfo == null)
                    _methodInfo = _instance.GetType().GetMethod(_reflectName, BindingFlags.Public | BindingFlags.Instance);

                return _methodInfo;
            }
        }

        public bool Value
        {
            get
            {
                object value = _reflectType switch
                {
                    ReflectionType.Field => FieldInfo.GetValue(_instance),
                    ReflectionType.Property => PropertyInfo.GetValue(_instance),
                    ReflectionType.Method => MethodInfo.Invoke(_instance, new object[0]),
                    _ => throw new NullReferenceException()
                };

                if (value is bool _value)
                    return _value;

                Debug.LogError($"Reflection Error: The specified reflection type '{_reflectName}' is not a bool type!");
                return false;
            }

            set
            {
                try
                {
                    if (_reflectType == ReflectionType.Field)
                    {
                        FieldInfo.SetValue(_instance, value);
                    }
                    else if (_reflectType == ReflectionType.Property)
                    {
                        PropertyInfo.SetValue(_instance, value);
                    }
                    else
                    {
                        MethodInfo.Invoke(_instance, new object[] { value });
                    }
                }
                catch (Exception exception)
                {
                    throw exception;
                }
            }
        }
    }
}