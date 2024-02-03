using System.Collections.Generic;
using UnityEngine.InputSystem;
using NameAndParameters = UnityEngine.InputSystem.Utilities.NameAndParameters;

namespace HJ.Input
{
    public class Action
    {
        public InputAction InputAction;
        public Dictionary<int, Binding> Bindings = new();

        public Action(InputAction action)
        {
            InputAction = action;

            int bindingsCount = action.bindings.Count;
            for (int bindingIndex = 0; bindingIndex < bindingsCount; bindingIndex++)
            {
                if (action.bindings[bindingIndex].isComposite)
                {
                    int firstPartIndex = bindingIndex + 1;
                    int lastPartIndex = firstPartIndex;
                    while (lastPartIndex < bindingsCount && action.bindings[lastPartIndex].isPartOfComposite)
                        ++lastPartIndex;

                    int partCount = lastPartIndex - firstPartIndex;
                    for (int i = 0; i < partCount; i++)
                    {
                        int bindingPartIndex = firstPartIndex + i;
                        InputBinding binding = action.bindings[bindingPartIndex];
                        AddBinding(binding, bindingPartIndex);
                    }

                    bindingIndex += partCount;
                }
                else
                {
                    InputBinding binding = action.bindings[bindingIndex];
                    AddBinding(binding, bindingIndex);
                }
            }

            void AddBinding(InputBinding binding, int bindingIndex)
            {
                string[] groups = binding.groups.Split(InputBinding.Separator);

                string partString = string.Empty;
                if (!string.IsNullOrEmpty(binding.name))
                {
                    NameAndParameters nameParameters = NameAndParameters.Parse(binding.name);
                    partString = nameParameters.name;
                }

                Bindings.Add(bindingIndex, new Binding()
                {
                    Name = binding.name,
                    ParentAction = action.name,
                    CompositePart = partString,
                    BindingIndex = bindingIndex,
                    Group = groups,
                    BindingPath = new BindingPath(binding.path, binding.overridePath),
                    InputBinding = binding
                });
            }
        }
    }
}
