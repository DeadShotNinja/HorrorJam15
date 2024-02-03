using UnityEngine;
using Newtonsoft.Json.Linq;

namespace HJ.Runtime
{
    public class DynamicBrokenFix : MonoBehaviour, IDynamicUnlock, IInventorySelector, ISaveable
    {
        [SerializeField] private MeshRenderer _disabledRenderer;
        [SerializeField] private ItemGuid _fixableItem;

        [Header("Hint Text")]
        [SerializeField] private bool _showHintText;
        [SerializeField] private GString _noFitHintText;
        [SerializeField] private float _hintTime = 2f;

        private DynamicObject _dynamicObject;
        private bool _isFixed;

        private GameManager _gameManager;

        private void Awake()
        {
            _gameManager = GameManager.Instance;
        }

        private void Start()
        {
            _noFitHintText.SubscribeGloc();
        }

        public void OnTryUnlock(DynamicObject dynamicObject)
        {
            Inventory.Instance.OpenItemSelector(this);
            this._dynamicObject = dynamicObject;
        }

        public void OnInventoryItemSelect(Inventory inventory, InventoryItem selectedItem)
        {
            if (selectedItem.ItemGuid == _fixableItem)
            {
                _disabledRenderer.enabled = true;
                inventory.RemoveItem(selectedItem);
                _dynamicObject.TryUnlockResult(true);
                _isFixed = true;
            }
            else if(_showHintText)
            {
                _gameManager.ShowHintMessage(_noFitHintText, _hintTime);
            }
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { nameof(_isFixed), _isFixed }
            };
        }

        public void OnLoad(JToken data)
        {
            _isFixed = (bool)data[nameof(_isFixed)];
            if(_isFixed) _disabledRenderer.enabled = true;
        }
    }
}