using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using HJ.Runtime;

namespace HJ.Editors
{
    public class ModulesDropdown : AdvancedDropdown
    {
        private readonly IEnumerable<ModulePair> _modules;
        public Action<Type> OnItemPressed;

        private class ModuleElement : AdvancedDropdownItem
        {
            public Type ModuleType;

            public ModuleElement(string displayName, Type moduleType) : base(displayName)
            {
                ModuleType = moduleType;
            }
        }

        public ModulesDropdown(AdvancedDropdownState state, IEnumerable<ModulePair> modules) : base(state)
        {
            _modules = modules;
            minimumSize = new Vector2(minimumSize.x, 270f);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Motion Modules");
            var groupMap = new Dictionary<string, AdvancedDropdownItem>();

            foreach (var module in _modules)
            {
                Type type = module.ModuleType;
                string name = module.ModuleName;

                // Split the name into groups
                string[] groups = name.Split('/');

                // Create or find the groups
                AdvancedDropdownItem parent = root;
                for (int i = 0; i < groups.Length - 1; i++)
                {
                    string groupPath = string.Join("/", groups.Take(i + 1));
                    if (!groupMap.ContainsKey(groupPath))
                    {
                        var newGroup = new AdvancedDropdownItem(groups[i]);
                        parent.AddChild(newGroup);
                        groupMap[groupPath] = newGroup;
                    }
                    parent = groupMap[groupPath];
                }

                // Create the item and add it to the last group
                ModuleElement item = new ModuleElement(groups.Last(), type);

                item.icon = MotionListDrawer.MotionIcon;
                parent.AddChild(item);
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            ModuleElement element = (ModuleElement)item;
            OnItemPressed?.Invoke(element.ModuleType);
        }
    }
}
