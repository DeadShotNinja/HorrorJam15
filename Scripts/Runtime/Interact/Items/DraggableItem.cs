using UnityEngine;
using HJ.Tools;
using Newtonsoft.Json.Linq;

namespace HJ.Runtime
{
    [RequireComponent(typeof(Rigidbody))]
    public class DraggableItem : SaveableBehaviour
    {
        [Tooltip("Minimum and maximum distance to which the object can be zoomed.")]
        [SerializeField] private MinMax _zoomDistance;
        [Tooltip("Maximum hold distance at which the object will be out of range and will be dropped.")]
        [SerializeField] private float _maxHoldDistance = 4f;

        [SerializeField] private bool _enableImpactSound = true;
        [Tooltip("Minimum and maximum impact volume. The impact will be played if the calculated volume is greater than the minimum impact volume.")]
        [SerializeField] private MinMax _impactVolume;
        [Tooltip("Modifier that is multiplied with the impact volume. Higher value = louder impact volume")]
        [SerializeField] private float _volumeModifier;
        [Tooltip("Time at which the next impact will be detected.")]
        [SerializeField] private float _nextImpact = 0.1f;

        [SerializeField] private bool _enableSlidingSound = true;
        [Tooltip("Minimum angle between the collision and the motion at which the sliding is detected. Near 0 = sliding, More than 0 = static")]
        [SerializeField] private float _minSlidingFactor = 5f;
        [Tooltip("Velocity range at which the sliding volume is calculated. Higher value = faster movement is required to achieve volume 1")]
        [SerializeField] private float _slidingVelocityRange = 5f;
        [Tooltip("Modifier that is multiplied with the sliding volume. Higher value = louder sliding volume")]
        [SerializeField] private float _slidingVolumeModifier = 5f;
        [Tooltip("Speed at which the volume is faded when the sliding stops.")]
        [SerializeField] private float _volumeFadeOffSpeed = 5f;

        [SerializeField] private bool _collision;

        private Rigidbody _rigid;

        private float _impactTime;
        private int _lastImpact;

        public MinMax ZoomDistance => _zoomDistance;
        public float MaxHoldDistance => _maxHoldDistance;

        private bool _isDragSoundPlaying;

        private void Awake()
        {
            _rigid = GetComponent<Rigidbody>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            _collision = true;
            if (!_enableImpactSound) return;

            float newVolume = collision.relativeVelocity.magnitude / _volumeModifier;
            if (newVolume < _impactVolume.RealMin) return;

            newVolume = Mathf.Clamp(newVolume, _impactVolume.RealMin, _impactVolume.RealMax);
            if (_impactTime <= 0) OnObjectImpact(newVolume);
        }

        private void OnCollisionExit(Collision collision)
        {
            _collision = false;
        }

        private void OnCollisionStay(Collision collision)
        {
            _collision = true;
        }

        private void Update()
        {
            if (_impactTime > 0) _impactTime -= Time.deltaTime;
            if (!_enableSlidingSound) return;

            float velMagnitudeNormalized = _rigid.velocity.normalized.magnitude;
            
            switch (_collision)
            {
                case true when velMagnitudeNormalized > _minSlidingFactor && !_isDragSoundPlaying:
                    _isDragSoundPlaying = true;
                    AudioManager.PostAudioEventSpecial(AudioItems.ItemDragStart, gameObject, WwiseCallback);
                    break;
                case false when velMagnitudeNormalized <= _minSlidingFactor && _isDragSoundPlaying:
                    _isDragSoundPlaying = false;
                    AudioManager.PostAudioEvent(AudioItems.ItemDragStop, gameObject);
                    break;
            }
        }
        
        private void WwiseCallback(object inCookie, AkCallbackType inType, object inInfos)
        {
            if (inType == AkCallbackType.AK_EndOfEvent)
            {
                _isDragSoundPlaying = false;
            }
        }

        private void OnObjectImpact(float volume)
        {
            _lastImpact = GameTools.RandomUnique(0, 1, _lastImpact);
            AudioManager.PostAudioEvent(AudioEnvironment.Impact, gameObject);
            //_lastImpact = GameTools.RandomUnique(0, _impactSounds.Length, _lastImpact);
            //AudioClip audioClip = ImpactSounds[_lastImpact];
            //AudioSource.PlayClipAtPoint(audioClip, transform.position, volume);
        }

        public override StorableCollection OnSave()
        {
            return new StorableCollection().WithTransform(transform);
        }

        public override void OnLoad(JToken data)
        {
            data.LoadTransform(transform);
        }
    }
}