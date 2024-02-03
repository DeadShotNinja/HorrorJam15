using System;
using System.Reflection;
using UnityEngine;

namespace HJ.Runtime
{
    /// <summary>
    /// A generic reflection field allows you to get or set a generic (object) value for a property, field, or method.
    /// </summary>
    [Serializable]
    public sealed class GenericReflectionField
    {
        public enum ReflectionType { Field, Property, Method };

        [SerializeField] private ReflectionType _reflectionType;
        [SerializeField] private MonoBehaviour _instance;
        [SerializeField] private string _reflectName;
        [SerializeField] private bool _reflectDerived;
        
        private FieldInfo _fieldInfo = null;
        private PropertyInfo _propertyInfo = null;
        private MethodInfo _methodInfo = null;
        
        public bool IsSet => _instance != null;
        
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

        public object Value
        {
            get => _reflectionType switch
            {
                ReflectionType.Field => FieldInfo.GetValue(_instance),
                ReflectionType.Property => PropertyInfo.GetValue(_instance),
                ReflectionType.Method => MethodInfo.Invoke(_instance, new object[0]),
                _ => throw new NullReferenceException()
            };

            set
            {
                try
                {
                    if (_reflectionType == ReflectionType.Field)
                    {
                        FieldInfo.SetValue(_instance, value);
                    }
                    else if (_reflectionType == ReflectionType.Property)
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
