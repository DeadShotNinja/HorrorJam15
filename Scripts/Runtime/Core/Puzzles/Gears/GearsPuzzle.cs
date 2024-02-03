using Newtonsoft.Json.Linq;
using UnityEngine;

namespace HJ.Runtime
{
    public class GearsPuzzle : PuzzleBase, ISaveable
    {
        [SerializeField] private GearsSystem _system;

        public void OnCompleted()
        {
            DisableInteract();
        }
        
        public StorableCollection OnSave()
        {
            StorableCollection storableCollection = new StorableCollection();
            return storableCollection;
        }

        public void OnLoad(JToken data)
        {
        }
    }
}