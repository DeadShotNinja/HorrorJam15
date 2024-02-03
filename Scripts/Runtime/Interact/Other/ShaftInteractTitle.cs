using Sirenix.OdinInspector;
using UnityEngine;

namespace HJ.Runtime
{
    public class ShaftInteractTitle : MonoBehaviour, IInteractTitle
    {
        [SerializeField]
        [Required]
        private ShaftV2 _shaft;
        
        [SerializeField] private GString SelectTheNextGear = "Select the next gear";
        
        public TitleParams InteractTitle()
        {
            return new() {
                Title = "Shaft",
                Button1 = UseTitle(),
                Button2 = ExamineTitle()
            };
        }

        private GString ExamineTitle()
        {
            if (_shaft.PlacedGear != null)
                return null;
            
            return _shaft.CountOfRequiredItemsPlayerHas >= 2 ? SelectTheNextGear : null;
        }

        private string UseTitle()
        {
            if (_shaft.PlacedGear != null)
                return "Take out the gear";
            
            if (_shaft.CountOfRequiredItemsPlayerHas == 0)
                // return _shaft.InteractTitle_WhenPlayerDoesntHaveAnyOfTheRequiredItems;
                return null;
            
            return $"Place {_shaft.CurrentlySelectedItem.Item.Title}";
        }
    }
}