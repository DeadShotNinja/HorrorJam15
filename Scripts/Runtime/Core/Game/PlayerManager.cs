using UnityEngine;
using Cinemachine;
using Newtonsoft.Json.Linq;

namespace HJ.Runtime
{
    public class PlayerManager : MonoBehaviour, ISaveableCustom
    {
        [Header("Player References")]
        [SerializeField] private Transform _cameraHolder;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private CinemachineVirtualCamera _mainVirtualCamera;

        private PlayerHealth _playerHealth;
        private PlayerItemsManager _playerItems;
        private MotionController _motionController;
        private InteractController _interactController;
        
        #region Properties
        
        public Transform CameraHolder => _cameraHolder;
        public Camera MainCamera
        {
            get => _mainCamera;
            set => _mainCamera = value;
        }

        public CinemachineVirtualCamera MainVirtualCamera => _mainVirtualCamera;
        
        public PlayerHealth PlayerHealth
        {
            get
            {
                if (_playerHealth == null)
                    _playerHealth = GetComponent<PlayerHealth>();

                return _playerHealth;
            }
        }
        
        public PlayerItemsManager PlayerItems
        {
            get
            {
                if (_playerItems == null)
                    _playerItems = GetComponentInChildren<PlayerItemsManager>();

                return _playerItems;
            }
        }
        
        public MotionController MotionController
        {
            get
            {
                if (_motionController == null)
                    _motionController = GetComponentInChildren<MotionController>();

                return _motionController;
            }
        }
        
        public InteractController InteractController
        {
            get
            {
                if (_interactController == null)
                    _interactController = GetComponentInChildren<InteractController>();

                return _interactController;
            }
        }
        
        #endregion
        
        /// <summary>
        /// This function is used to collect all local player data to be saved.
        /// </summary>
        public StorableCollection OnCustomSave()
        {
            StorableCollection data = new StorableCollection();
            data.Add("health", PlayerHealth.EntityHealth);

            StorableCollection playerItemsData = new StorableCollection();
            for (int i = 0; i < PlayerItems.PlayerItems.Count; i++)
            {
                var playerItem = PlayerItems.PlayerItems[i];
                var itemData = (playerItem as ISaveableCustom).OnCustomSave();
                playerItemsData.Add("playerItem_" + i, itemData);
            }

            data.Add("playerItems", playerItemsData);
            return data;
        }
        
        /// <summary>
        /// This function is used to load all stored local player data.
        /// </summary>
        public void OnCustomLoad(JToken data)
        {
            PlayerHealth.StartHealth = data["health"].ToObject<uint>();
            PlayerHealth.InitHealth();

            for (int i = 0; i < PlayerItems.PlayerItems.Count; i++)
            {
                var playerItem = PlayerItems.PlayerItems[i];
                var itemData = data["playerItems"]["playerItem_" + i];
                (playerItem as ISaveableCustom).OnCustomLoad(itemData);
            }
        }
    }
}
