using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEditor.IMGUI.Controls;

namespace HJ.Editors
{
    public class InputPicker : AdvancedDropdown
    {
        private class InputElement : AdvancedDropdownItem
        {
            public string ActionName;
            public int BindingIndex;
            public bool IsNone;

            public InputElement(string displayName, string actionName, int bindingIndex) : base(displayName)
            {
                ActionName = actionName;
                BindingIndex = bindingIndex;
            }

            public InputElement(string displayName) : base(displayName) { }

            public InputElement() : base("Empty")
            {
                IsNone = true;
            }
        }

        public string SelectedKey;
        public Action<string, int> OnItemPressed;

        private readonly InputActionAsset inputAsset;

        public InputPicker(AdvancedDropdownState state, InputActionAsset inputAsset) : base(state)
        {
            this.inputAsset = inputAsset;
            minimumSize = new Vector2(200f, 250f);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Input Action Selector");
            root.AddChild(new InputElement()); // none selector

            foreach (var map in inputAsset.actionMaps)
            {
                InputElement section = new(map.name);

                foreach (var action in map.actions)
                {
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
                                AddBinding(section, binding, bindingPartIndex);
                            }

                            bindingIndex += partCount;
                        }
                        else
                        {
                            InputBinding binding = action.bindings[bindingIndex];
                            AddBinding(section, binding, bindingIndex);
                        }
                    }
                }

                root.AddChild(section);
            }

            return root;
        }

        void AddBinding(InputElement section, InputBinding binding, int bindingIndex)
        {
            string partString = string.Empty;
            if (!string.IsNullOrEmpty(binding.name))
            {
                NameAndParameters nameParameters = NameAndParameters.Parse(binding.name);
                partString = nameParameters.name;
            }

            string name = binding.action;
            if (!string.IsNullOrEmpty(partString))
                name += $" ({partString})";

            name += $" [{bindingIndex}]";
            InputElement inputAction = new(name, binding.action, bindingIndex);
            inputAction.icon = InputReferenceDrawer.InputActionIcon;

            section.AddChild(inputAction);
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            var element = item as InputElement;
            if (!element.IsNone) OnItemPressed?.Invoke(element.ActionName, element.BindingIndex);
            else OnItemPressed?.Invoke(null, -1);
        }
    }
}
