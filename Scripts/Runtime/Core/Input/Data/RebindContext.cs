using UnityEngine.InputSystem;

namespace HJ.Input
{
    public class RebindContext
    {
        public InputAction Action;
        public int BindingIndex;
        public string OverridePath;
        
        public RebindContext(InputAction action, int bindingIndex, string overridePath)
        {
            Action = action;
            BindingIndex = bindingIndex;
            OverridePath = overridePath;
        }

        public static bool operator ==(RebindContext left, RebindContext right)
        {
            return left.Action.name == right.Action.name && left.BindingIndex == right.BindingIndex;
        }

        public static bool operator !=(RebindContext left, RebindContext right) => !(left == right);

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if(obj is RebindContext context)
            {
                return Action.name == context.Action.name
                       && BindingIndex == context.BindingIndex;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (Action.name, bindingIndex: BindingIndex, overridePath: OverridePath).GetHashCode();
        }

        public override string ToString()
        {
            string actionName = Action.name;
            string bindingName = Action.bindings[BindingIndex].name;

            if (!string.IsNullOrEmpty(bindingName))
                actionName += "." + bindingName;

            return actionName;
        }
    }
}
