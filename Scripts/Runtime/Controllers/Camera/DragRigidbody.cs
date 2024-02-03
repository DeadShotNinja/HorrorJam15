using UnityEngine;
using HJ.Input;
using System;

namespace HJ.Runtime
{
    [RequireComponent(typeof(InteractController))]
    public class DragRigidbody : PlayerComponent, IReticleProvider
    {
        private enum HoldTypeEnum { Press, Hold }
        private enum DragTypeEnum { WeightedVelocity, FixedVelocity }

        [SerializeField] private HoldTypeEnum _holdType = HoldTypeEnum.Press;
        [SerializeField] private DragTypeEnum _dragType = DragTypeEnum.WeightedVelocity;

        [SerializeField] private ControlsContext[] _controlsContexts;

        [SerializeField] private bool _showGrabReticle = true;
        [SerializeField] private Reticle _grabHand;
        [SerializeField] private Reticle _holdHand;

        [SerializeField] private RigidbodyInterpolation _interpolate = RigidbodyInterpolation.Interpolate;
        [SerializeField] private CollisionDetectionMode _collisionDetection = CollisionDetectionMode.ContinuousDynamic;
        [SerializeField] private bool _freezeRotation = false;

        [SerializeField] private float _dragStrength = 10f;
        [SerializeField] private float _throwStrength = 10f;
        [SerializeField] private float _rotateSpeed = 1f;
        [SerializeField] private float _zoomSpeed = 1f;

        [SerializeField] private bool _hitpointOffset = true;
        [SerializeField] private bool _playerCollision = false;
        [SerializeField] private bool _objectZooming = true;
        [SerializeField] private bool _objectRotating = true;
        [SerializeField] private bool _objectThrowing = true;

        private GameManager _gameManager;
        private InteractController _interactController;
        private DraggableItem _currentDraggable;
        private Rigidbody _draggableRigidbody;
        private GameObject _raycastObject;

        private Transform _camTransform;
        private RigidbodyInterpolation _defInterpolate;
        private CollisionDetectionMode _defCollisionDetection;
        private bool _defFreezeRotation;
        private bool _defUseGravity;
        private bool _defIsKinematic;

        private Vector3 _holdOffset;
        private GameObject _holdPoint;
        private GameObject _holdRotatePoint;
        private float _holdDistance;

        private bool _isDragging;
        private bool _isRotating;
        private bool _isThrown;
        
        private void Awake()
        {
            _gameManager = GameManager.Instance;
            _interactController = GetComponent<InteractController>();
            _camTransform = PlayerManager.MainVirtualCamera.transform;
        }

        private void Start()
        {
            foreach (var control in _controlsContexts)
            {
                control.SubscribeGloc();
            }
        }

        private void Update()
        {
            _raycastObject = _interactController.RaycastObject;
            if (_raycastObject != null || _isDragging)
            {
                if(_holdType == HoldTypeEnum.Press)
                {
                    if (InputManager.ReadButtonOnce(GetInstanceID(), Controls.USE))
                    {
                        if (!_isDragging)
                        {
                            GrabObject();
                        }
                        else
                        {
                            DropObject();
                            _isDragging = false;
                        }
                    }
                }
                else
                {
                    if (InputManager.ReadButton(Controls.USE))
                    {
                        if (!_isDragging && !_isThrown)
                        {
                            GrabObject();
                        }
                    }
                    else if (_isDragging)
                    {
                        DropObject();
                        _isDragging = false;
                    }
                    else
                    {
                        _isThrown = false;
                    }
                }
            }

            if (_isDragging) HoldUpdate();
        }

        private void FixedUpdate()
        {
            if (_isDragging) FixedHoldUpdate();
        }

        private void GrabObject()
        {
            if (_raycastObject == null) return;
            if (!_raycastObject.TryGetComponent(out _currentDraggable)) return;
            if (!_raycastObject.TryGetComponent(out _draggableRigidbody)) return;

            _defUseGravity = _draggableRigidbody.useGravity;
            _defIsKinematic = _draggableRigidbody.isKinematic;

            _defInterpolate = _draggableRigidbody.interpolation;
            _defCollisionDetection = _draggableRigidbody.collisionDetectionMode;
            _defFreezeRotation = _draggableRigidbody.freezeRotation;

            _draggableRigidbody.interpolation = _interpolate;
            _draggableRigidbody.collisionDetectionMode = _collisionDetection;
            _draggableRigidbody.freezeRotation = _freezeRotation;
            Physics.IgnoreCollision(_raycastObject.GetComponent<Collider>(), PlayerCollider, !_playerCollision);

            if (_dragType == DragTypeEnum.FixedVelocity)
            {
                float distance = Vector3.Distance(MainCamera.transform.position, _raycastObject.transform.position);
                _holdDistance = Mathf.Clamp(distance, _currentDraggable.ZoomDistance.RealMin, _currentDraggable.ZoomDistance.RealMax);

                _holdRotatePoint = new GameObject("RotatePoint");
                _holdRotatePoint.transform.SetParent(VirtualCamera.transform);
                _holdRotatePoint.transform.position = Vector3.zero;
                _holdRotatePoint.transform.eulerAngles = _raycastObject.transform.eulerAngles;

                _draggableRigidbody.velocity = Vector3.zero;
                _draggableRigidbody.useGravity = false;
                _draggableRigidbody.isKinematic = false;
            }
            else
            {
                _holdPoint = new GameObject("HoldPoint");
                _holdPoint.transform.SetParent(VirtualCamera.transform);
                _holdPoint.transform.position = VirtualCamera.transform.position + VirtualCamera.transform.forward * _holdDistance;

                if (_hitpointOffset)
                {
                    _holdRotatePoint = new GameObject("RotatePoint");
                    _holdRotatePoint.transform.SetParent(_holdPoint.transform);
                    _holdRotatePoint.transform.position = Vector3.zero;
                    _holdRotatePoint.transform.eulerAngles = _raycastObject.transform.eulerAngles;
                }
                else
                {
                    _holdPoint.transform.eulerAngles = _raycastObject.transform.eulerAngles;
                }

                Vector3 localHitpoint = _interactController.LocalHitpoint;
                Vector3 worldHitpoint = _raycastObject.transform.TransformPoint(localHitpoint);
                _holdOffset = worldHitpoint - _raycastObject.transform.position;

                float distance = Vector3.Distance(MainCamera.transform.position, worldHitpoint);
                _holdDistance = Mathf.Clamp(distance, _currentDraggable.ZoomDistance.RealMin, _currentDraggable.ZoomDistance.RealMax);

                _draggableRigidbody.useGravity = true;
                _draggableRigidbody.isKinematic = false;
            }

            foreach (var dragStart in _raycastObject.GetComponents<IOnDragStart>())
            {
                dragStart.OnDragStart();
            }

            PlayerManager.PlayerItems.IsItemsUsable = false;
            _interactController.EnableInteractInfo(false);
            _gameManager.ShowControlsInfo(true, _controlsContexts);
            _isDragging = true;
        }

        private void HoldUpdate()
        {
            if (_objectZooming && InputManager.ReadInput(Controls.SCROLL_WHEEL, out Vector2 scroll))
            {
                _holdDistance = Mathf.Clamp(_holdDistance + scroll.y * _zoomSpeed * 0.001f, _currentDraggable.ZoomDistance.RealMin, _currentDraggable.ZoomDistance.RealMax);
            }

            if (_objectRotating && InputManager.ReadButton(Controls.RELOAD))
            {
                InputManager.ReadInput(Controls.POINTER_DELTA, out Vector2 delta);
                delta = delta.normalized * _rotateSpeed;

                if (_dragType == DragTypeEnum.WeightedVelocity && (_holdPoint != null || _holdRotatePoint != null))
                {
                    Transform rotateTransform = _hitpointOffset ? _holdRotatePoint.transform : _holdPoint.transform;
                    rotateTransform.Rotate(VirtualCamera.transform.up, delta.x, Space.World);
                    rotateTransform.Rotate(VirtualCamera.transform.right, delta.y, Space.World);
                }
                else if(_dragType == DragTypeEnum.FixedVelocity && _holdRotatePoint != null)
                {
                    _holdRotatePoint.transform.Rotate(VirtualCamera.transform.up, delta.x, Space.World);
                    _holdRotatePoint.transform.Rotate(VirtualCamera.transform.right, delta.y, Space.World);
                }

                LookController.SetEnabled(false);
                _isRotating = true;
            }
            else if (_isRotating)
            {
                LookController.SetEnabled(true);
                _isRotating = false;
            }

            if(_objectThrowing && InputManager.ReadButtonOnce("Fire", Controls.FIRE))
            {
                ThrowObject();
                _isThrown = true;
            }
        }

        private void FixedHoldUpdate()
        {
            Vector3 grabPos = VirtualCamera.transform.position + VirtualCamera.transform.forward * _holdDistance;
            Vector3 currPos = _currentDraggable.transform.position;

            if (_hitpointOffset && _holdPoint != null)
            {
                Vector3 offsetDirection = _holdPoint.transform.TransformDirection(_holdOffset);
                grabPos -= offsetDirection;
            }

            Vector3 targetVelocity = grabPos - currPos;

            if (_dragType == DragTypeEnum.WeightedVelocity)
            {
                _holdPoint.transform.position = grabPos;
                targetVelocity.Normalize();

                float massFactor = 1f / _draggableRigidbody.mass;
                float distanceFactor = Mathf.Clamp01(Vector3.Distance(grabPos, currPos));
                Transform rotateTransform = _hitpointOffset ? _holdRotatePoint.transform : _holdPoint.transform;

                _draggableRigidbody.velocity = Vector3.Lerp(_draggableRigidbody.velocity, distanceFactor * _dragStrength * massFactor * targetVelocity, 0.3f);
                _draggableRigidbody.rotation = Quaternion.Slerp(_draggableRigidbody.rotation, rotateTransform.rotation, 0.3f);
                _draggableRigidbody.angularVelocity = Vector3.zero;
            }
            else
            {
                _draggableRigidbody.velocity = targetVelocity * _dragStrength;
                _draggableRigidbody.rotation = Quaternion.Slerp(_draggableRigidbody.rotation, _holdRotatePoint.transform.rotation, 0.3f);
                _draggableRigidbody.angularVelocity = Vector3.zero;
                //draggableRigidbody.angularVelocity = Vector3.Lerp(draggableRigidbody.angularVelocity, Vector3.zero, 0.3f);
            }

            foreach (var dragUpdate in _currentDraggable.GetComponents<IOnDragUpdate>())
            {
                dragUpdate.OnDragUpdate(targetVelocity);
            }

            if(Vector3.Distance(currPos, MainCamera.transform.position) > _currentDraggable.MaxHoldDistance)
            {
                DropObject();
            }
        }

        private void ThrowObject()
        {
            _draggableRigidbody.AddForce(10 * _throwStrength * MainCamera.transform.forward, ForceMode.Force);
            DropObject();
        }

        private void DropObject()
        {
            _draggableRigidbody.useGravity = _defUseGravity;
            _draggableRigidbody.isKinematic = _defIsKinematic;

            _draggableRigidbody.interpolation = _defInterpolate;
            _draggableRigidbody.collisionDetectionMode = _defCollisionDetection;
            _draggableRigidbody.freezeRotation = _defFreezeRotation;
            Physics.IgnoreCollision(_currentDraggable.GetComponent<Collider>(), PlayerCollider, false);

            if (_isRotating)
            {
                LookController.SetEnabled(true);
                _isRotating = false;
            }

            foreach (var dragEnd in _currentDraggable.GetComponents<IOnDragEnd>())
            {
                dragEnd.OnDragEnd();
            }

            Destroy(_holdRotatePoint);
            Destroy(_holdPoint);
            _interactController.EnableInteractInfo(true);
            _gameManager.ShowControlsInfo(false, new ControlsContext[0]);
            PlayerManager.PlayerItems.IsItemsUsable = true;

            _holdOffset = Vector3.zero;
            _holdDistance = 0;

            _draggableRigidbody = null;
            _currentDraggable = null;
            _isDragging = false;
        }

        public (Type, Reticle, bool) OnProvideReticle()
        {
            Reticle reticle = _isDragging ? _holdHand : _grabHand;
            if (_showGrabReticle) return (typeof(DraggableItem), reticle, _isDragging);
            else return (null, null, false);
        }
    }
}