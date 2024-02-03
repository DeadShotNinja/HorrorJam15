using System.Collections.Generic;
using UnityEngine;
using HJ.Runtime;

namespace HJ.Scriptable
{
    [CreateAssetMenu(fileName = "ManagerModules", menuName = "HJ/Manager Modules")]
    public class ManagerModulesAsset : ScriptableObject
    {
        [SerializeReference] public List<ManagerModule> ManagerModules = new();
    }
}
