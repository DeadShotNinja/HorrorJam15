using System.Collections;
using UnityEngine;
using HJ.Input;
using HJ.Tools;
using QFSW.QC.Actions;
using Sirenix.OdinInspector;
using Action = System.Action;

namespace HJ.Runtime
{
    public class LookController : PlayerComponent
    {
        [Header("General")]
        [Tooltip("If true, cursor will be locked during normal gameplay.")]
        [SerializeField] private bool _shouldLockCursor = true;

        [Header("Smoothing")]
        [Tooltip("Enable or disable look smoothing.")]
        [SerializeField] private bool _smoothLook;
        [ShowIf(nameof(_smoothLook))]
        [Tooltip("Time taken for the look to smooth.")]
        [SerializeField] private float _smoothTime = 5f;
        [ShowIf(nameof(_smoothLook))]
        [Tooltip("Multiplier for the smoothing effect.")]
        [SerializeField] private float _smoothMultiplier = 2f;

        [Header("Sensitivity")]
        [Tooltip("Sensitivity for horizontal movement.")]
        [SerializeField] private float _sensitivityX = 1f;
        [Tooltip("Sensitivity for vertical movement.")]
        [SerializeField] private float _sensitivityY = 1f;

        [Header("Look Limits")]
        [Tooltip("Limits for horizontal rotation.")]
        [SerializeField] private MinMax _horizontalLimits = new(-360, 360);
        [Tooltip("Limits for vertical rotation.")]
        [SerializeField] private MinMax _verticalLimits = new(-80, 90);

        [Header("Offset")]
        [Tooltip("Offset values for the look.")]
        [SerializeField] private Vector2 _offset;
        
        [Header("Debugging")]
        [Tooltip("Current rotation values for debugging.")]
        [ReadOnly] public Vector2 Rotation;
        
        private bool _blockLook;
        private MinMax _horizontalLimitsOrig;
        private MinMax _verticalLimitsOrig;
        
        private Vector2 _targetLook;
        private Vector2 _startingLook;
        private bool _customLerp;

        #region Properties

        public MinMax VerticalLimits => _verticalLimits;
        public Vector2 DeltaInput { get; set; }
        public Quaternion RotationX { get; private set; }
        public Quaternion RotationY { get; private set; }
        public Quaternion RotationFinal { get; private set; }
        public bool LookLocked
        {
            get => _blockLook;
            set => _blockLook = value;
        }
        public float SensitivityX
        {
            get => _sensitivityX;
            set => _sensitivityX = value;
        }

        #endregion
        
        private void Start()
        {
            _verticalLimitsOrig = _verticalLimits;
            _horizontalLimitsOrig = _horizontalLimits;
            if (_shouldLockCursor) GameTools.ShowCursor(true, false);

            OptionsManager.ObserveOption("sensitivity", (obj) =>
            {
                _sensitivityX = (float)obj;
                _sensitivityY = (float)obj;
            });
            
            OptionsManager.ObserveOption("smoothing", (obj) => _smoothLook = (bool)obj);
            OptionsManager.ObserveOption("smoothing_speed", (obj) => _smoothTime = (float)obj);
        }

        private void Update()
        {
            if (Cursor.lockState != CursorLockMode.None && !_blockLook && _isEnabled)
            {
                DeltaInput = InputManager.ReadInput<Vector2>(Controls.LOOK);
            }
            else
            {
                DeltaInput = Vector2.zero;
            }

            Rotation.x += DeltaInput.x * _sensitivityX / 30 * MainCamera.fieldOfView + _offset.x;
            Rotation.y += DeltaInput.y * _sensitivityY / 30 * MainCamera.fieldOfView + _offset.y;

            Rotation.x = ClampAngle(Rotation.x, _horizontalLimits.RealMin, _horizontalLimits.RealMax);
            Rotation.y = ClampAngle(Rotation.y, _verticalLimits.RealMin, _verticalLimits.RealMax);

            RotationX = Quaternion.AngleAxis(Rotation.x, Vector3.up);
            RotationY = Quaternion.AngleAxis(Rotation.y, Vector3.left);
            RotationFinal = RotationX * RotationY;

            transform.localRotation = _smoothLook ? Quaternion.Slerp(transform.localRotation, RotationFinal, 
                _smoothTime * _smoothMultiplier * Time.deltaTime) : RotationFinal;

            _offset.y = 0f;
            _offset.x = 0f;
        }
        
        /// <summary>
        /// Lerp look rotation to a specific target rotation.
        /// </summary>
        public void LerpRotation(Vector2 target, float duration = 0.5f)
        {
            target.x = ClampAngle(target.x);
            target.y = ClampAngle(target.y);

            float xDiff = FixDiff(target.x - Rotation.x);
            float yDiff = FixDiff(target.y - Rotation.y);

            StartCoroutine(DoLerpRotation(new Vector2(xDiff, yDiff), null, duration));
        }

        /// <summary>
        /// Lerp look rotation to a specific target rotation.
        /// </summary>
        public void LerpRotation(Vector2 target, Action onLerpComplete, float duration = 0.5f)
        {
            target.x = ClampAngle(target.x);
            target.y = ClampAngle(target.y);

            float xDiff = FixDiff(target.x - Rotation.x);
            float yDiff = FixDiff(target.y - Rotation.y);

            StartCoroutine(DoLerpRotation(new Vector2(xDiff, yDiff), onLerpComplete, duration));
        }
        
        /// <summary>
        /// Lerp look rotation to a specific target transform.
        /// </summary>
        public void LerpRotation(Transform target, float duration = 0.5f, bool keepLookLocked = false)
        {
            Vector3 directionToTarget = target.position - transform.position;
            Quaternion rotationToTarget = Quaternion.LookRotation(directionToTarget);

            Vector3 eulerRotation = rotationToTarget.eulerAngles;
            Vector2 targetRotation = new Vector2(eulerRotation.y, eulerRotation.x);

            // Clamp the target rotation angles.
            targetRotation.x = ClampAngle(targetRotation.x);
            targetRotation.y = ClampAngle(-targetRotation.y);

            // Calculate the differences in each axis.
            float xDiff = FixDiff(targetRotation.x - Rotation.x);
            float yDiff = FixDiff(targetRotation.y - Rotation.y);

            // Start the lerp process.
            StartCoroutine(DoLerpRotation(new Vector2(xDiff, yDiff), null, duration, keepLookLocked));
        }
        
        /// <summary>
        /// Lerp look rotation to a specific target transform.
        /// </summary>
        public void LerpRotation(Transform target, Action onLerpComplete, float duration = 0.5f, bool keepLookLocked = false)
        {
            Vector3 directionToTarget = target.position - transform.position;
            Quaternion rotationToTarget = Quaternion.LookRotation(directionToTarget);

            Vector3 eulerRotation = rotationToTarget.eulerAngles;
            Vector2 targetRotation = new Vector2(eulerRotation.y, eulerRotation.x);

            // Clamp the target rotation angles.
            targetRotation.x = ClampAngle(targetRotation.x);
            targetRotation.y = ClampAngle(targetRotation.y);

            // Calculate the differences in each axis.
            float xDiff = FixDiff(targetRotation.x - Rotation.x);
            float yDiff = FixDiff(targetRotation.y - Rotation.y);

            // Start the lerp process.
            StartCoroutine(DoLerpRotation(new Vector2(xDiff, yDiff), onLerpComplete, duration, keepLookLocked));
        }
        
        /// <summary>
        /// Lerp the look rotation and clamp the look rotation within limits relative to the rotation.
        /// </summary>
        /// <param name="relative">Relative target rotation.</param>
        /// <param name="vLimits">Vertical Limits [Up, Down]</param>
        /// <param name="hLimits">Horizontal Limits [Left, Right]</param>
        public void LerpClampRotation(Vector3 relative, MinMax vLimits, MinMax hLimits, float duration = 0.5f)
        {
            float toAngle = ClampAngle(relative.y);
            float remainder = FixDiff(toAngle - Rotation.x);

            float targetAngle = Rotation.x + remainder;
            float min = targetAngle - Mathf.Abs(hLimits.RealMin);
            float max = targetAngle + Mathf.Abs(hLimits.RealMax);

            if (min < -360)
            {
                min += 360;
                max += 360;
            }
            else if (max > 360)
            {
                min -= 360;
                max -= 360;
            }

            if (Mathf.Abs(targetAngle - Rotation.x) > 180)
            {
                if (Rotation.x > 0) Rotation.x -= 360;
                else if (Rotation.x < 0) Rotation.x += 360;
            }

            hLimits = new MinMax(min, max);
            StartCoroutine(DoLerpClampRotation(targetAngle, vLimits, hLimits, duration));
        }
        
        /// <summary>
        /// Lerp the look rotation manually. This function should only be used in the Update() function.
        /// </summary>
        public void CustomLerp(Vector2 target, float t)
        {
            if (!_customLerp)
            {
                _targetLook.x = ClampAngle(target.x);
                _targetLook.y = ClampAngle(target.y);
                _startingLook = Rotation;
                _customLerp = true;
                _blockLook = true;
            }

            if ((t = Mathf.Clamp01(t)) < 1)
            {
                Rotation.x = Mathf.LerpAngle(_startingLook.x, _targetLook.x, t);
                Rotation.y = Mathf.LerpAngle(_startingLook.y, _targetLook.y, t);
            }
        }
        
        /// <summary>
        /// Get remainder to relative rotation.
        /// </summary>
        /// <param name="relative">Relative target rotation.</param>
        /// <returns></returns>
        public float GetLookRemainder(Vector3 relative)
        {
            float toAngle = ClampAngle(relative.y);
            float remainder = FixDiff(toAngle - Rotation.x);
            return Rotation.x + remainder;
        }

        /// <summary>
        /// Reset lerp parameters.
        /// </summary>
        public void ResetCustomLerp()
        {
            StopAllCoroutines();
            _targetLook = Vector2.zero;
            _startingLook = Vector2.zero;
            _customLerp = false;
            _blockLook = false;
        }
        
        /// <summary>
        /// Set look rotation limits.
        /// </summary>
        /// <param name="relative">Relative target rotation.</param>
        /// <param name="vLimits">Vertical Limits [Up, Down]</param>
        /// <param name="hLimits">Horizontal Limits [Left, Right]</param>
        public void SetLookLimits(Vector3 relative, MinMax vLimits, MinMax hLimits)
        {
            if (hLimits.HasValue)
            {
                float toAngle = ClampAngle(relative.y);
                float remainder = FixDiff(toAngle - Rotation.x);

                float targetAngle = Rotation.x + remainder;
                float min = targetAngle - Mathf.Abs(hLimits.RealMin);
                float max = targetAngle + Mathf.Abs(hLimits.RealMax);

                if (min < -360)
                {
                    min += 360;
                    max += 360;
                }
                else if (max > 360)
                {
                    min -= 360;
                    max -= 360;
                }

                if (Mathf.Abs(targetAngle - Rotation.x) > 180)
                {
                    if (Rotation.x > 0) Rotation.x -= 360;
                    else if (Rotation.x < 0) Rotation.x += 360;
                }

                hLimits = new MinMax(min, max);
                _horizontalLimits = hLimits;
            }

            _verticalLimits = vLimits;
        }
        
        /// <summary>
        /// Set vertical look rotation limits.
        /// </summary>
        /// <param name="vLimits">Vertical Limits [Up, Down]</param>
        public void SetVerticalLimits(MinMax vLimits)
        {
            _verticalLimits = vLimits;
        }

        /// <summary>
        /// Set horizontal look rotation limits.
        /// </summary>
        /// <param name="relative">Relative target rotation.</param>
        /// <param name="hLimits">Horizontal Limits [Left, Right]</param>
        public void SetHorizontalLimits(Vector3 relative, MinMax hLimits)
        {
            float toAngle = ClampAngle(relative.y);
            float remainder = FixDiff(toAngle - Rotation.x);

            float targetAngle = Rotation.x + remainder;
            float min = targetAngle - Mathf.Abs(hLimits.RealMin);
            float max = targetAngle + Mathf.Abs(hLimits.RealMax);

            if (min < -360)
            {
                min += 360;
                max += 360;
            }
            else if (max > 360)
            {
                min -= 360;
                max -= 360;
            }

            if (Mathf.Abs(targetAngle - Rotation.x) > 180)
            {
                if (Rotation.x > 0) Rotation.x -= 360;
                else if (Rotation.x < 0) Rotation.x += 360;
            }

            hLimits = new MinMax(min, max);
            _horizontalLimits = hLimits;
        }
        
        /// <summary>
        /// Reset look rotation to default limits.
        /// </summary>
        public void ResetLookLimits()
        {
            StopAllCoroutines();
            _horizontalLimits = _horizontalLimitsOrig;
            _verticalLimits = _verticalLimitsOrig;
        }

        /// <summary>
        /// Get the current rotation from the transform and apply its rotation to the controller.
        /// </summary>
        /// <remarks>Good to use from the timeline to set the look rotation from animation uisng the SignalEmitter.</remarks>
        public void ApplyLookFromTransform()
        {
            Vector3 eulerAngles = transform.localEulerAngles;
            Rotation.x = ClampAngle(eulerAngles.y);
            Rotation.y = ClampAngle(eulerAngles.x);
        }
        
        private IEnumerator DoLerpRotation(Vector2 target, Action onLerpComplete, float duration, bool keepLookLocked = false)
        {
            _blockLook = true;

            target = new Vector2(Rotation.x + target.x, Rotation.y + target.y);
            Vector2 current = Rotation;
            float elapsedTime = 0;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);

                Rotation.x = Mathf.LerpAngle(current.x, target.x, t);
                Rotation.y = Mathf.LerpAngle(current.y, target.y, t);

                yield return null;
            }

            Rotation = target;
            onLerpComplete?.Invoke();

            _blockLook = keepLookLocked;
        }
        
        private IEnumerator DoLerpClampRotation(float newX, Vector2 vLimit, Vector2 hLimit, float duration, bool keepLookLocked = false)
        {
            _blockLook = true;

            float newY = Rotation.y < vLimit.x
                ? vLimit.x : Rotation.y > vLimit.y
                    ? vLimit.y : Rotation.y;

            Vector2 target = new Vector2(newX, newY);
            Vector2 current = Rotation;
            float elapsedTime = 0;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);

                Rotation.x = Mathf.LerpAngle(current.x, target.x, t);
                Rotation.y = Mathf.LerpAngle(current.y, target.y, t);

                yield return null;
            }

            Rotation = target;
            _horizontalLimits = hLimit;
            _verticalLimits = vLimit;

            _blockLook = keepLookLocked;
        }
        
        private float ClampAngle(float angle, float min, float max)
        {
            float newAngle = angle.FixAngle();
            return Mathf.Clamp(newAngle, min, max);
        }

        private float ClampAngle(float angle)
        {
            angle %= 360f;
            if (angle < 0f)
                angle += 360f;
            return angle;
        }
        
        private float FixDiff(float angleDiff)
        {
            if (angleDiff > 180f)
            {
                angleDiff -= 360f;
            }
            else if (angleDiff < -180f)
            {
                angleDiff += 360f;
            }

            return angleDiff;
        }
    }
}
