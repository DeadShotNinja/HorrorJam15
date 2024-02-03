using DG.Tweening;
using UnityEngine;

namespace HJ.Runtime
{
    [RequireComponent(typeof(Light))]
    public class LightFlicker : MonoBehaviour
    {
        [Header("Flicker Settings")]
        [SerializeField] private MinMax _intensityRange = new(2f, 3f);
        [SerializeField] private MinMax _speedRange = new(0.1f, 0.4f);

        private Light _light;
        private Tween _flickerTween;

        private void Awake() => Init();

        private void Init()
        {
            _light = GetComponent<Light>();
            StartFlickering();
        }

        private void StartFlickering()
        {
            float randIntensity = Random.Range(_intensityRange.Min, _intensityRange.Max);
            float randSpeed = Random.Range(_speedRange.Min, _speedRange.Max);

            _flickerTween?.Kill();

            _flickerTween = _light.DOIntensity(randIntensity, randSpeed).OnComplete(StartFlickering);
        }

        private void OnDisable()
        {
            _flickerTween?.Kill();
        }
    }
}
