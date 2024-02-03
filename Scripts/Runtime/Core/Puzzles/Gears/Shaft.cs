using Sirenix.OdinInspector;
using UnityEngine;

namespace HJ.Runtime
{
    /// <summary>
    /// A thing where the player can place/take out a gear.
    /// </summary>
    public class Shaft : MonoBehaviour, IInteractStart
    {
        [SerializeField]
        private GearsSystem _system;

        [SerializeField]
        private int _systemGearIndex = -1;

        [SerializeField]
        [ReadOnly]
        private Gear _gear;

        private bool _placedGear;

        public bool PlacedGear => _placedGear;

        [Header("Prototyping")]
        [SerializeField]
        private GameObject _gearPrefab;

        private void OnValidate()
        {
            _gear = null;
            _placedGear = false;
            if (_systemGearIndex < 0 || _system == null)
                return;

            if (_systemGearIndex >= _system.Gears.Count)
            {
                Debug.LogWarning("Value of _systemGearIndex is higher than count of available gears in _system!");
                return;
            }

            _gear = _system.Gears[_systemGearIndex];
            _placedGear = _gear != null;
        }

        public void InteractStart()
        {
            if (_placedGear)
            {
                _system.SetGear(_systemGearIndex, null);
                Destroy(_gear.gameObject);
            }
            else
            {
                var gear = Instantiate(_gearPrefab, transform);
                _gear = (Gear)gear.GetComponent(typeof(Gear));
                _system.SetGear(_systemGearIndex, _gear);
            }

            _placedGear = !_placedGear;
        }
    }
}
