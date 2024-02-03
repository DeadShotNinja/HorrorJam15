using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace HJ.Input
{
    public class ActionMap
    {
        public string name;
        public Dictionary<string, Action> Actions = new Dictionary<string, Action>();

        public ActionMap(InputActionMap map)
        {
            name = map.name;

            foreach (var action in map.actions)
            {
                Actions.Add(action.name, new Action(action));
            }
        }
    }
}
