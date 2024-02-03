using UnityEngine;

namespace HJ.Runtime
{
    public class WaterDamager : MonoBehaviour
    {
        [SerializeField] private PlayerHealth _playerHealth;

        private float _yPosAdjusted;

        private void Start()
        {
            _yPosAdjusted = transform.position.y - 2;
        }

        private void Update()
        {
            if (_playerHealth.transform.position.y < _yPosAdjusted && !_playerHealth.IsDead)
            {
                _playerHealth.OnApplyDamage(100, transform);
            }
        }
    }
}
