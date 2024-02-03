using System.Collections;
using UnityEngine;
using HJ.Input;
using HJ.Tools;

namespace HJ.Runtime
{
    public class MovableObject : MonoBehaviour, IStateInteract
    {
        public enum MoveDirectionEnum { LeftRight, ForwardBackward, AllDirections }

        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Axis _forwardAxis;
        [SerializeField] private bool _drawGizmos = true;

        [SerializeField] private MoveDirectionEnum _moveDirection;
        [SerializeField] private LayerMask _collisionMask;
        [SerializeField] private Vector3 _holdOffset;
        [SerializeField] private bool _allowRotation = true;

        [SerializeField] private float _holdDistance = 2f;
        [SerializeField] private float _objectWeight = 20f;
        [SerializeField] private float _playerRadius = 0.3f;
        [SerializeField] private float _playerHeight = 1.8f;

        [SerializeField] private float _walkMultiplier = 1f;
        [SerializeField] private float _lookMultiplier = 1f;

        [Range(0f, 1f)]
        [SerializeField] private float _slideVolume = 1f;
        [SerializeField] private float _volumeFadeSpeed = 1f;

        [SerializeField] private bool _useMouseLimits;
        [SerializeField] private MinMax _mouseVerticalLimits;

        #region Properties
        
        public Axis ForwardAxis => _forwardAxis;
        public LayerMask CollisionMask => _collisionMask;
        public MoveDirectionEnum MoveDirection => _moveDirection;
        public Vector3 HoldOffset => _holdOffset;
        public bool AllowRotation => _allowRotation;
        public bool UseMouseLimits => _useMouseLimits;
        public MinMax MouseVerticalLimits => _mouseVerticalLimits;
        public float SlideVolume => _slideVolume;
        public float VolumeFadeSpeed => _volumeFadeSpeed;
        public float HoldDistance => _holdDistance;
        public float ObjectWeight  => _objectWeight;
        public float WalkMultiplier => _walkMultiplier;
        public float LookMultiplier => _lookMultiplier;

        
        public Transform RootMovable => _rigidbody.transform;

        public MeshRenderer Renderer => RootMovable.GetComponent<MeshRenderer>();
        
        #endregion

        private void Awake()
        {
            if(_rigidbody != null) _rigidbody.mass = _objectWeight;
        }

        public void FadeSoundOut()
        {
            // TODO: Play Wwise move sound?

            //StartCoroutine(FadeSound());
        }

        //IEnumerator FadeSound()
        //{
        //    while(Mathf.Approximately(_audioSource.volume, 0f))
        //    {
        //        _audioSource.volume = Mathf.MoveTowards(_audioSource.volume, 0f, Time.deltaTime * _slideVolume * 10);
        //        yield return null;
        //    }

        //    _audioSource.volume = 0f;
        //    _audioSource.Stop();
        //}

        public StateParams OnStateInteract()
        {
            if (!CheckOverlapping())
            {
                StopAllCoroutines();
                return new StateParams()
                {
                    StateKey = PlayerStateMachine.PUSHING_STATE,
                    StateData = new StorableCollection()
                    {
                        { "reference", this }
                    }
                };
            }

            return null;
        }

        private bool CheckOverlapping()
        {
            Vector3 forwardGlobal = _forwardAxis.Convert();
            float height = _playerHeight - 0.6f;

            Vector3 position = RootMovable.TransformPoint((-forwardGlobal * _holdDistance) + _holdOffset);
            Vector3 bottomPos = new(position.x, Renderer.bounds.min.y, position.z);

            Vector3 playerBottom = bottomPos;
            playerBottom.y += _playerRadius;

            Vector3 p1 = new Vector3(position.x, playerBottom.y, position.z);
            Vector3 p2 = new Vector3(position.x, playerBottom.y + height, position.z);

            return Physics.CheckCapsule(p1, p2, _playerRadius, _collisionMask);
        }

        private void OnDrawGizmosSelected()
        {
            if (!_drawGizmos) 
                return;

            Vector3 forwardGlobal = _forwardAxis.Convert();
            Vector3 forwardLocal = RootMovable.Direction(_forwardAxis);
            float radius = 0.5f;

            Vector3 position = RootMovable.TransformPoint((-forwardGlobal * _holdDistance) + _holdOffset);
            Vector3 bottomPos = new(position.x, Renderer.bounds.min.y, position.z);

            GizmosE.DrawDisc(bottomPos, radius, Color.green, Color.green.Alpha(0.01f));
            GizmosE.DrawGizmosArrow(bottomPos, forwardLocal * radius);

            float height = _playerHeight - 0.6f;
            Vector3 playerBottom = bottomPos;
            playerBottom.y += _playerRadius;

            Vector3 p1 = new Vector3(position.x, playerBottom.y, position.z);
            Vector3 p2 = new Vector3(position.x, playerBottom.y + height, position.z);

            Gizmos.color = Color.green;
            GizmosE.DrawWireCapsule(p1, p2, _playerRadius);
        }
    }
}