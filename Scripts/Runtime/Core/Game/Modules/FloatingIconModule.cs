using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace HJ.Runtime
{
    [System.Serializable]
    public class FloatingIconModule : ManagerModule
    {
        public sealed class FloatingIconData
        {
            public FloatingIcon FloatingIcon;
            public Transform IconTranform;
            public GameObject TargetObject;
            public Vector3 LastPosition;
            public bool WasDisabled;

            public void UpdateLastPosition()
            {
                if(TargetObject != null)
                    LastPosition = TargetObject.transform.position;
            }
        }

        public override string Name => "FloatingIcon";

        public GameObject FloatingIconPrefab;

        [Header("Settings")]
        [SerializeField] private LayerMask _cullLayers;
        [SerializeField] private float _distanceShow = 4;
        [SerializeField] private float _distanceHide = 4;
        [SerializeField] private float _fadeInTime = 0.2f;
        [SerializeField] private float _fadeOutTime = 0.05f;

        private readonly List<FloatingIconData> _uiFloatingIcons = new List<FloatingIconData>();
        private List<GameObject> _worldFloatingIcons = new List<GameObject>();

        public override void OnAwake()
        {
            _worldFloatingIcons = (from interactable in Object.FindObjectsOfType<InteractableItem>()
                                 where interactable.ShowFloatingIcon
                                 select interactable.gameObject).ToList();

            var customIcons = Object.FindObjectsOfType<FloatingIconObject>().Select(x => x.gameObject);
            _worldFloatingIcons.AddRange(customIcons);
        }

        /// <summary>
        /// Add object to floating icons list.
        /// </summary>
        public void AddFloatingIcon(GameObject gameObject)
        {
            _worldFloatingIcons.Add(gameObject);
        }

        /// <summary>
        /// Remove object from floating icons list.
        /// </summary>
        public void RemoveFloatingIcon(GameObject gameObject)
        {
            _worldFloatingIcons.Remove(gameObject);
        }

        public override void OnUpdate()
        {
            for (int i = 0; i < _worldFloatingIcons.Count; i++)
            {
                GameObject obj = _worldFloatingIcons[i];

                if(obj == null)
                {
                    _worldFloatingIcons.RemoveAt(i);
                    continue;
                }

                if (Vector3.Distance(_playerPresence.PlayerCamera.transform.position, obj.transform.position) <= _distanceShow)
                {
                    if (!_uiFloatingIcons.Any(x => x.TargetObject == obj) && VisibleByCamera(obj) && IsIconUpdatable(obj))
                    {
                        Vector3 screenPoint = _playerPresence.PlayerCamera.WorldToScreenPoint(obj.transform.position);
                        GameObject floatingIconObj = Object.Instantiate(FloatingIconPrefab, screenPoint, Quaternion.identity, GameManager.FloatingIcons);
                        FloatingIcon icon = floatingIconObj.AddComponent<FloatingIcon>();

                        _uiFloatingIcons.Add(new FloatingIconData()
                        {
                            FloatingIcon = icon,
                            IconTranform = floatingIconObj.transform,
                            TargetObject = obj,
                            LastPosition = obj.transform.position
                        });

                        icon.FadeIn(_fadeInTime);
                    }
                }
            }

            for (int i = 0; i < _uiFloatingIcons.Count; i++)
            {
                FloatingIconData item = _uiFloatingIcons[i];
 
                if (item.IconTranform == null)
                {
                    _uiFloatingIcons.RemoveAt(i);
                    continue;
                }

                if (IsIconUpdatable(item.TargetObject))
                {
                    // update last object position
                    item.UpdateLastPosition();

                    // update distance
                    float distance = Vector3.Distance(_playerPresence.PlayerCamera.transform.position, item.LastPosition);

                    // set point position
                    Vector3 screenPoint = _playerPresence.PlayerCamera.WorldToScreenPoint(item.LastPosition);
                    item.IconTranform.position = screenPoint;

                    if (item.TargetObject == null)
                    {
                        // destroy the floating icon if the target object is removed
                        Object.Destroy(item.IconTranform.gameObject);
                        _uiFloatingIcons.RemoveAt(i);
                    }
                    else if (distance > _distanceHide)
                    {
                        // destroy and remove the item if it is out of distance
                        item.FloatingIcon.FadeOut(_fadeOutTime);
                    }
                    else if (!VisibleByCamera(item.TargetObject))
                    {
                        // disable an item if it is behind an object
                        item.IconTranform.gameObject.SetActive(false);
                        item.WasDisabled = true;
                    }
                    else if (item.WasDisabled)
                    {
                        // enable an object if it is visible when it has been disabled
                        item.FloatingIcon.FadeIn(_fadeInTime);
                        item.IconTranform.gameObject.SetActive(true);
                        item.WasDisabled = false;
                    }
                }
                else
                {
                    // destroy the floating icon if the target object is disabled
                    Object.Destroy(item.IconTranform.gameObject);
                    _uiFloatingIcons.RemoveAt(i);
                }
            }
        }

        private bool VisibleByCamera(GameObject obj)
        {
            if (obj != null)
            {
                bool linecastResult = Physics.Linecast(_playerPresence.PlayerCamera.transform.position, obj.transform.position, out RaycastHit hit, _cullLayers);

                if (!linecastResult || linecastResult && hit.collider.gameObject == obj)
                {
                    Vector3 screenPoint = _playerPresence.PlayerCamera.WorldToViewportPoint(obj.transform.position);
                    return screenPoint.x >= 0 && screenPoint.x <= 1 && screenPoint.y >= 0 && screenPoint.y <= 1 && screenPoint.z > 0;
                }
            }

            return false;
        }

        private bool IsIconUpdatable(GameObject targetObj)
        {
            return targetObj != null && (targetObj.activeSelf || targetObj.activeSelf && targetObj.TryGetComponent(out Renderer renderer) && renderer.enabled);
        }
    }
}