using UnityEngine.InputSystem;

namespace HJ.Input
{
    public struct Binding
    {
        public string Name;
        public string ParentAction;
        public string CompositePart;
        public int BindingIndex;
        public string[] Group;
        public BindingPath BindingPath;
        public InputBinding InputBinding;
        
        public override string ToString()
        {
            string actionName = ParentAction;
            if (!string.IsNullOrEmpty(Name))
                actionName += "." + Name;
            return actionName;
        }
    }
}
