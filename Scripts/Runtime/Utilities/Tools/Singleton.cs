using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T _reference;
    
    public static T Instance
    {
        get
        {
            if (_reference == null)
            {
                if ((_reference = FindObjectOfType<T>()) == null)
                {
                    throw new MissingReferenceException($"The singleton reference to a {typeof(T).Name} is not found!");
                }
            }

            return _reference;
        }
    }
    
    public static bool HasReference
    {
        get
        {
            if (_reference == null)
            {
                return (_reference = FindObjectOfType<T>()) != null;
            }

            return true;
        }
    }
    
    protected void Reset()
    {
        #if UNITY_EDITOR
        if (FindObjectsOfType<T>().Length > 1)
        {
            EditorUtility.DisplayDialog("Singleton Error", $"There should never be more than 1 reference of {typeof(T).Name}!", "OK");
            DestroyImmediate(this);
        }
        #endif
    }
}
