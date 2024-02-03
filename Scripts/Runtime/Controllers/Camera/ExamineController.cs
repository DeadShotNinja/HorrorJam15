using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HJ.Input;
using HJ.Tools;
using UnityEngine.UI;
using static HJ.Runtime.InteractableItem;

namespace HJ.Runtime
{
    [RequireComponent(typeof(InteractController))]
    public class ExamineController : PlayerComponent
    {
        [SerializeField] private LayerMask _focusCullLayes;
        [SerializeField] private Layer _focusLayer;
        [SerializeField] private uint _focusRenderingLayer;
        [SerializeField] private GameObject _hotspotPrefab;

        [SerializeField] private ControlsContext _controlPutBack;
        [SerializeField] private ControlsContext _controlRead;
        [SerializeField] private ControlsContext _controlTake;
        [SerializeField] private ControlsContext _controlRotate;
        [SerializeField] private ControlsContext _controlZoom;

        [SerializeField] private float _rotateTime = 0.1f;
        [SerializeField] private float _rotateMultiplier = 3f;
        [SerializeField] private float _zoomMultiplier = 0.1f;
        [SerializeField] private float _timeToExamine = 2f;

        [SerializeField] private Vector3 _dropOffset;
        [SerializeField] private Vector3 _inventoryOffset;
        [SerializeField] private bool _showLabels = true;

        [SerializeField] private AnimationCurve _pickUpCurve = new(new Keyframe(0, 0), new Keyframe(1, 0));
        [SerializeField] private float _pickUpCurveMultiplier = 1f;
        [SerializeField] private float _pickUpTime = 0.2f;

        [SerializeField] private AnimationCurve _putPositionCurve = new(new Keyframe(0, 0), new Keyframe(1, 0));
        [SerializeField] private float _putPositionCurveMultiplier = 1f;
        [SerializeField] private float _putPositionCurveTime = 0.1f;

        [SerializeField] private AnimationCurve _putRotationCurve = new(new Keyframe(0, 0), new Keyframe(1, 0));
        [SerializeField] private float _putRotationCurveMultiplier = 1f;
        [SerializeField] private float _putRotationCurveTime = 0.1f;
        
        private GameManager _gameManager;
        private InteractController _interactController;

        private readonly Stack<ExaminedObject> _examinedObjects = new();
        private ExaminedObject _currentExamine;
        private Image _examineHotspot;

        private bool _isInventoryExamine;
        private bool _isPointerShown;
        private bool _isReadingPaper;
        private bool _isHotspotPressed;

        private Vector2 _examineRotate;
        private Vector2 _rotateVelocity;
      
        public Vector3 DropPosition => transform.TransformPoint(_dropOffset);
        public Vector3 InventoryPosition => transform.TransformPoint(_inventoryOffset);

        public bool IsExamining { get; private set; }

        private void Awake()
        {
            _gameManager = GameManager.Instance;
            _interactController = GetComponent<InteractController>();
        }

        private void Start()
        {
            _controlPutBack.InteractName.SubscribeGloc();
            _controlRead.InteractName.SubscribeGloc();
            _controlTake.InteractName.SubscribeGloc();
            _controlRotate.InteractName.SubscribeGloc();
            _controlZoom.InteractName.SubscribeGloc();
        }

        private void Update()
        {
            if (_interactController.RaycastObject != null || IsExamining)
            {
                if (InputManager.ReadButtonOnce(GetInstanceID(), Controls.EXAMINE))
                {
                    if (!IsExamining)
                    {
                        GameObject raycastObj = _interactController.RaycastObject;
                        if (!raycastObj.GetComponent<ExaminePutter>())
                            StartExamine(raycastObj);
                    }
                    else
                    {
                        PopExaminedObject();
                    }
                }
            }

            if (IsExamining) ExamineHold();
        }

        public void ExamineFromInventory(GameObject obj)
        {
            _isInventoryExamine = true;
            StartExamine(obj);
        }

        private void StartExamine(GameObject obj)
        {
            if (obj.TryGetComponent(out InteractableItem interactableItem))
            {
                if (interactableItem.ExamineType == ExamineTypeEnum.None)
                    return;

                ExamineObject(interactableItem);
                _gameManager.SetBlur(true, true);
                _gameManager.FreezePlayer(true);
                _gameManager.DisableAllGamePanels();

                ShowBottomControls(interactableItem);
                IsExamining = true;
            }
        }

        private void ShowBottomControls(InteractableItem interactableItem)
        {
            List<ControlsContext> controls = new()
            {
                _controlPutBack // default put back button info
            };

            // read paper or take object info
            if (interactableItem.IsPaper) controls.Add(_controlRead);
            else if (interactableItem.TakeFromExamine) controls.Add(_controlTake);

            // rotate object info
            if (interactableItem.ExamineRotate != ExamineRotateEnum.Static)
                controls.Add(_controlRotate);

            // zoom object info
            if (interactableItem.UseExamineZooming)
                controls.Add(_controlZoom);

            _gameManager.ShowControlsInfo(true, controls.ToArray());
        }

        private void ExamineObject(InteractableItem interactableItem)
        {
            if (interactableItem == null) return;
            _currentExamine?.GameObject.SetLayerRecursively(_interactController.InteractLayer);

            Vector3 controlOffset = Quaternion.LookRotation(MainCamera.transform.forward) * interactableItem.ControlPoint;
            Vector3 holdPosition = MainCamera.transform.position + MainCamera.transform.forward * interactableItem.ExamineDistance;

            _examinedObjects.Push(_currentExamine = new ExaminedObject()
            {
                InteractableItem = interactableItem,
                PutSettings = new PutSettings(interactableItem.transform, controlOffset, new PutCurve(_putPositionCurve)
                {
                    EvalMultiply = _putPositionCurveMultiplier,
                    CurveTime = _putPositionCurveTime
                },
                new PutCurve(_putRotationCurve)
                {
                    EvalMultiply = _putRotationCurveMultiplier,
                    CurveTime = _putRotationCurveTime,
                }, 
                _examinedObjects.Count > 0),
                HoldPosition = holdPosition,
                StartPosition = interactableItem.transform.position,
                StartRotation = interactableItem.transform.rotation,
                ControlPoint = interactableItem.transform.position + controlOffset,
                ExamineDistance = interactableItem.ExamineDistance
            });

            if (interactableItem.TryGetComponent(out Rigidbody rigidbody))
            {
                rigidbody.isKinematic = true;
                rigidbody.useGravity = false;
            }

            foreach (Collider collider in interactableItem.GetComponentsInChildren<Collider>())
            {
                Physics.IgnoreCollision(collider, PlayerCollider, true);
            }

            if (interactableItem.IsCustomExamine)
            {
                foreach (var col in interactableItem.CollidersEnable)
                {
                    col.enabled = true;
                }

                foreach (var col in interactableItem.CollidersDisable)
                {
                    col.enabled = false;
                }
            }

            if (interactableItem.ShowExamineTitle)
            {
                StopAllCoroutines();
                StartCoroutine(ExamineItemAndShowInfo(interactableItem));
            }

            if (interactableItem.ExamineType == ExamineTypeEnum.CustomObject && interactableItem.ExamineHotspot.HotspotTransform != null)
            {
                // clear previous active hotspot
                if (_examineHotspot != null)
                {
                    Destroy(_examineHotspot.gameObject);
                    _examineHotspot = null;
                }

                // add new hotspot
                GameObject hotspotGo = Instantiate(_hotspotPrefab, Vector3.zero, Quaternion.identity, _gameManager.ExamineHotspots);
                Image hotspotImage = hotspotGo.GetComponent<Image>();
                hotspotImage.Alpha(0f);
                _examineHotspot = hotspotImage;
            }

            AudioManager.PostAudioEvent(AudioItems.ItemExamine, gameObject);
            interactableItem.gameObject.SetLayerRecursively(_focusLayer);
            interactableItem.gameObject.SetRenderingLayer(_focusRenderingLayer);
            interactableItem.OnExamineStartEvent?.Invoke();
            PlayerManager.PlayerItems.IsItemsUsable = false;
        }

        IEnumerator ExamineItemAndShowInfo(InteractableItem item)
        {
            bool isExamined = item.IsExamined;
            if (!isExamined)
            {
                yield return new WaitForSeconds(_timeToExamine);
                item.IsExamined = true;
            }

            string title = item.ExamineTitle;
            if (item.ExamineInventoryTitle)
            {
                Item inventoryItem = item.PickupItem.GetItem();
                title = inventoryItem.Title;
            }

            if (!isExamined)
            {
                AudioManager.PostAudioEvent(AudioItems.ItemExamineHint, gameObject);
            }

            _gameManager.ShowExamineInfo(true, false, title);
        }

        private void PopExaminedObject()
        {
            ExaminedObject obj = _examinedObjects.Pop();
            obj.InteractableItem.OnExamineEndEvent?.Invoke();

            // destroy an object if there are no other objects examined and the object is examined from the inventory
            if (_examinedObjects.Count <= 0 && _isInventoryExamine)
            {
                Destroy(obj.GameObject);
            }
            // otherwise return the object to its original location
            else
            {
                obj.GameObject.AddComponent<ExaminePutter>().Put(obj.PutSettings);
                obj.GameObject.SetRenderingLayer(_focusRenderingLayer, false);
            }

            // if the number of examined objects is greater than zero, peek the previous object
            if (_examinedObjects.Count > 0)
            {
                _currentExamine = _examinedObjects.Peek();
                _currentExamine.GameObject.SetLayerRecursively(_focusLayer);
            }
            // otherwise reset examined object and unlock player
            else
            {
                ResetExamine(obj);
                _currentExamine = null;
            }

            // if it's a custom examine, enable/disable custom colliders
            if (obj.InteractableItem.IsCustomExamine)
            {
                foreach (var col in obj.InteractableItem.CollidersEnable)
                {
                    col.enabled = false;
                }

                foreach (var col in obj.InteractableItem.CollidersDisable)
                {
                    col.enabled = true;
                }
            }

            // disable pointer
            if (_isPointerShown) _gameManager.HidePointer();
            _gameManager.ShowPaperInfo(false, true);
            _isReadingPaper = false;
            _isPointerShown = false;
        }

        private void ExamineHold()
        {
            InteractableItem currentItem = _currentExamine.InteractableItem;

            // hold position
            foreach (var obj in _examinedObjects)
            {
                Vector3 holdPos = MainCamera.transform.position + MainCamera.transform.forward * obj.ExamineDistance;
                obj.HoldPosition = Vector3.Lerp(obj.HoldPosition, holdPos, Time.deltaTime * 5);
                float speedMultiplier = _pickUpCurve.Evaluate(obj.TFactor) * _pickUpCurveMultiplier;
                obj.TFactor = Mathf.SmoothDamp(obj.TFactor, 1f, ref obj.Velocity, _pickUpTime + speedMultiplier);
                obj.InteractableItem.transform.position = VectorExtension.QuadraticBezier(obj.StartPosition, obj.HoldPosition, obj.ControlPoint, obj.TFactor);
            }

            // rotation
            if (currentItem.UseFaceRotation && _currentExamine.TFactor <= 0.99f)
            {
                Vector3 faceRotation = currentItem.FaceRotation;
                Quaternion faceRotationQ = Quaternion.LookRotation(MainCamera.transform.forward) * Quaternion.Euler(faceRotation);
                currentItem.transform.rotation = Quaternion.Slerp(_currentExamine.StartRotation, faceRotationQ, _currentExamine.TFactor);
            }
            else if (!_isPointerShown && !_isReadingPaper && InputManager.ReadButton(Controls.FIRE))
            {
                Vector2 rotateValue = InputManager.ReadInput<Vector2>(Controls.LOOK) * _rotateMultiplier;
                _examineRotate = Vector2.SmoothDamp(_examineRotate, rotateValue, ref _rotateVelocity, _rotateTime);

                switch (currentItem.ExamineRotate)
                {
                    case ExamineRotateEnum.Horizontal:
                        currentItem.transform.Rotate(MainCamera.transform.up, -_examineRotate.x, Space.World);
                        break;
                    case ExamineRotateEnum.Vertical:
                        currentItem.transform.Rotate(MainCamera.transform.right, _examineRotate.y, Space.World);
                        break;
                    case ExamineRotateEnum.Both:
                        currentItem.transform.Rotate(MainCamera.transform.up, -_examineRotate.x, Space.World);
                        currentItem.transform.Rotate(MainCamera.transform.right, _examineRotate.y, Space.World);
                        break;
                }
            }

            // examine zooming
            if (!_isReadingPaper && currentItem.UseExamineZooming)
            {
                Vector2 scroll = InputManager.ReadInput<Vector2>(Controls.SCROLL_WHEEL);
                float nextZoom = _currentExamine.ExamineDistance + scroll.normalized.y * _zoomMultiplier;
                _currentExamine.ExamineDistance = Mathf.Clamp(nextZoom, currentItem.ExamineZoomLimits.RealMin, currentItem.ExamineZoomLimits.RealMax);
            }

            // pointer
            if (!_isReadingPaper && currentItem.AllowCursorExamine && InputManager.ReadButtonOnce(GetInstanceID(), Controls.SHOW_CURSOR))
            {
                _isPointerShown = !_isPointerShown;

                if (_isPointerShown)
                {
                    _gameManager.ShowPointer(_focusCullLayes, _focusLayer, (hit, _) =>
                    {
                        if (hit.collider.gameObject.TryGetComponent(out InteractableItem interactableItem))
                        {
                            ExamineObject(interactableItem);
                            _gameManager.HidePointer();
                            _isPointerShown = false;
                        }
                    });
                }
                else
                {
                    _gameManager.HidePointer();
                }
            }

            // examine hotspots
            bool isHotspotShown = false;
            if (_examineHotspot != null && currentItem.ExamineHotspot.HotspotTransform != null)
            {
                if (currentItem.ExamineType == ExamineTypeEnum.CustomObject
                && currentItem.ExamineHotspot.HotspotTransform.gameObject.activeInHierarchy
                && _currentExamine.TFactor > 0.99f)
                {
                    var hotspot = currentItem.ExamineHotspot;
                    Vector3 mainCamera = MainCamera.transform.position;
                    Vector3 hotspotPos = currentItem.ExamineHotspot.HotspotTransform.position;

                    Vector3 screenPointPos = MainCamera.WorldToScreenPoint(hotspotPos);
                    _examineHotspot.transform.position = screenPointPos;

                    Vector3 direction = hotspotPos - mainCamera;
                    direction -= direction.normalized * 0.01f;

                    float alpha = _examineHotspot.color.a;
                    {
                        if (!Physics.Raycast(mainCamera, direction, direction.magnitude, _focusCullLayes, QueryTriggerInteraction.Ignore) && currentItem.ExamineHotspot.Enabled)
                        {
                            alpha = Mathf.MoveTowards(alpha, 1f, Time.deltaTime * 10f);
                            isHotspotShown = true;

                            if (InputManager.ReadButtonOnce(this, Controls.USE))
                            {
                                hotspot.HotspotAction?.Invoke();
                                if (hotspot.ResetHotspot)
                                    _isHotspotPressed = !_isHotspotPressed;
                            }
                        }
                        else
                        {
                            alpha = Mathf.MoveTowards(alpha, 0f, Time.deltaTime * 10f);
                        }
                    }
                    _examineHotspot.Alpha(alpha);
                }
                else
                {
                    _examineHotspot.Alpha(0f);
                }
            }

            // paper reading
            if (!isHotspotShown) // if the hotspot is not shown, you can read the paper or take the item
            {
                if (currentItem.InteractableType == InteractableTypeEnum.ExamineItem && currentItem.IsPaper && !string.IsNullOrEmpty(currentItem.PaperText))
                {
                    if (InputManager.ReadButtonOnce(GetInstanceID(), Controls.USE))
                    {
                        _isReadingPaper = !_isReadingPaper;
                        _gameManager.ShowPaperInfo(_isReadingPaper, false, currentItem.PaperText);
                    }
                }
                else if (currentItem.InteractableType == InteractableTypeEnum.InventoryItem && currentItem.TakeFromExamine)
                {
                    if (InputManager.ReadButtonOnce(GetInstanceID(), Controls.USE))
                    {
                        ResetExamine(_currentExamine, true);
                        _interactController.Interact(_currentExamine.GameObject);
                        _currentExamine = null;
                        return;
                    }
                }
            }
        }

        private void ResetExamine(ExaminedObject examine, bool examineTake = false)
        {
            _gameManager.SetBlur(false, true);
            _gameManager.FreezePlayer(false);
            _gameManager.ShowPanel(GameManager.PanelType.MainPanel);
            _gameManager.ShowControlsInfo(false, new ControlsContext[0]);
            _gameManager.ShowExamineInfo(false, true);
            PlayerManager.PlayerItems.IsItemsUsable = true;

            StopAllCoroutines();
            _examinedObjects.Clear();

            if(!_isInventoryExamine)
                examine.GameObject.SetLayerRecursively(_interactController.InteractLayer);

            if (!examineTake)
            {
                if (_examineHotspot != null)
                {
                    var hotspot = examine.InteractableItem.ExamineHotspot;
                    if (hotspot.ResetHotspot && _isHotspotPressed)
                    {
                        hotspot.HotspotAction?.Invoke();
                        _isHotspotPressed = false;
                    }

                    Destroy(_examineHotspot.gameObject);
                    _examineHotspot = null;
                }
            }
            else
            {
                if (_examineHotspot != null)
                {
                    Destroy(_examineHotspot.gameObject);
                    _examineHotspot = null;
                }

                examine.GameObject.SetActive(false);
                examine.GameObject.transform.position = examine.PutSettings.PutPosition;
                examine.GameObject.transform.rotation = examine.PutSettings.PutRotation;
            }

            _isInventoryExamine = false;
            IsExamining = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(InventoryPosition, 0.01f);
            if(_showLabels) GizmosE.DrawCenteredLabel(InventoryPosition, "Inventory Position");

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(DropPosition, 0.01f);
            if (_showLabels) GizmosE.DrawCenteredLabel(DropPosition, "Drop Position");
        }
    }
}