using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace HJ.Runtime
{
    public class Gear : MonoBehaviour
    { 
        // AngularSpeed0 * NumberOfTeeth0
        // = AngularSpeed1 * NumberOfTeeth1
        // = ...
        // = AngularSpeedN * NumberOfTeethN
        
        [SerializeField] private UnityEvent _onStartedRotating;
        [SerializeField] private UnityEvent _onStoppedRotating;
        
        [SerializeField] private GameObject _innerObjectForRotation;
        
        [OnValueChanged("RecalculateForDebug")]
        [SerializeField] 
        [Min(3)] 
        private int _numberOfTeeth;

        [OnValueChanged("RecalculateForDebug")]
        [Header("Debug Gears Info")]
        [SerializeField]
        private bool _drawGizmos;
        
        [ReadOnly] 
        [SerializeField] 
        [Min(0)] 
        private float _rootCircle;
        
        [OnValueChanged("RecalculateForDebug")]
        [SerializeField]
        [Min(0)] 
        private float _addendumCircle;
        
        [ReadOnly] 
        [SerializeField] 
        private float _pitchCircle;

        [ReadOnly]
        [SerializeField]
        private float _module;

        public float NumberOfTeeth => _numberOfTeeth;

        private float _initialRotation;

        private void Awake()
        {
            _initialRotation = _innerObjectForRotation.transform.localRotation.eulerAngles.z;
            Assert.IsNotNull(_innerObjectForRotation);
        }

        // Intended to be called from the system
        public void OnStartedRotating()
        {
            _onStartedRotating?.Invoke();
        }
        
        public void OnStoppedRotating ()
        {
            _onStoppedRotating?.Invoke();
        }

        public void SetGearRotation(float driverGearAngleTimesTeeth)
        {
            var euler = _innerObjectForRotation.transform.localRotation.eulerAngles;
            euler.z = driverGearAngleTimesTeeth / _numberOfTeeth + _initialRotation;

            var newLocalRotation = Quaternion.identity;
            newLocalRotation.eulerAngles = euler;
            _innerObjectForRotation.transform.localRotation = newLocalRotation;
        }

        private void OnDrawGizmos()
        {
            if (!_drawGizmos) 
                return;
            
            var cachedTransform = transform.position;
            
            // NOTE(Hulvdan): There's a mistake. Diameters weren't divided by 2.
            // Was prototyping and messing with gears. 
            if (_rootCircle > 0)
                DrawCircle(cachedTransform, transform.forward, _rootCircle, Color.red);
            if (_addendumCircle > 0)
                DrawCircle(cachedTransform, transform.forward, _addendumCircle, Color.yellow);
            if (_pitchCircle > 0)
                DrawCircle(cachedTransform, transform.forward, _pitchCircle, Color.green);
        }
        
        private static void DrawCircle(Vector3 position, Vector3 forward, float radius, Color color)
        {
            var oldColor = Gizmos.color;
            Gizmos.color = color;
            var oldMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(
                position, Quaternion.FromToRotation(Vector3.up, forward), new Vector3(1, 0f, 1)
            );
            Gizmos.DrawWireSphere(Vector3.zero, radius);
            Gizmos.matrix = oldMatrix;
            Gizmos.color = oldColor;
        }

        private void RecalculateForDebug()
        {
            // if (_rootCircle <= 0 || _addendumCircle <= 0) 
            //     return;
            //
            // if (_addendumCircle <= _rootCircle)
            //     return;
            //
            // _module = (_addendumCircle - _rootCircle) / 4.5f;
            // _pitchCircle = _module * _numberOfTeeth;

            if (_addendumCircle <= 0) 
                return;
            
            // NOTE(Hulvdan): The calculation is probably correct only for
            // GOST gears with involute teeth profile.
            _module = _addendumCircle / (_numberOfTeeth + 2);
            _rootCircle = _addendumCircle - 4.5f * _module;
            
            _pitchCircle = _module * _numberOfTeeth;
        }
    }
}