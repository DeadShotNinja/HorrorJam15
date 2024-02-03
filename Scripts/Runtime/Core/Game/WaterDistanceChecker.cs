using System;
using UnityEngine;

namespace HJ.Runtime
{
    public class WaterDistanceChecker : MonoBehaviour
    {
        [SerializeField] private float _acceptableDistance = 1.0f;
        [SerializeField] private GameObject _water;

        private bool _isNearWater;
        
        private void Start()
        {
            InvokeRepeating(nameof(CheckIfNearWater), 0.5f, 0.5f);
        }
        
        private void CheckIfNearWater()
        {
            float waterSurfaceY = _water.transform.position.y;
            float objectY = transform.position.y;

            float distanceToWaterSurface = Mathf.Abs(waterSurfaceY - objectY);

            bool isCurrentlyNearWater = distanceToWaterSurface <= _acceptableDistance;
            if (isCurrentlyNearWater != _isNearWater)
            {
                _isNearWater = isCurrentlyNearWater;

                if (_isNearWater) { OnNearWater(); }
                else { OnAwayFromWater(); }
            }
        }
        
        private void OnNearWater()
        {
            //AudioManager.SetAudioState(AudioState.Lakeside);
            //Debug.Log("Audio State Set to Lakeside");
        }
        
        private void OnAwayFromWater()
        {
            //AudioManager.SetAudioState(AudioState.Forest);
            //Debug.Log("Audio State Set to Forest");
        }
    }
}
