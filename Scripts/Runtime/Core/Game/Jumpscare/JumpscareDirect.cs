using System;
using UnityEngine;

namespace HJ.Runtime
{
    public class JumpscareDirect : MonoBehaviour
    {
        [Serializable]
        public struct DirectModel
        {
            public string ModelID;
            public GameObject ModelObject;
        }

        [SerializeField] private DirectModel[] _jumpscareDirectModels;

        private GameObject _directModel;
        private float _directDuration;

        public void ShowDirectJumpscare(string modelID, float duration)
        {
            foreach (var direct in _jumpscareDirectModels)
            {
                if (direct.ModelID == modelID)
                {
                    direct.ModelObject.SetActive(true);
                    _directModel = direct.ModelObject;
                    break;
                }
            }

            if(_directModel != null) _directDuration = duration;
        }

        private void Update()
        {
            if(_directDuration > 0f)
            {
                _directDuration -= Time.deltaTime;
            }
            else if(_directModel != null)
            {
                _directModel.SetActive(false);
                _directModel = null;
                _directDuration = 0f;
            }
        }
    }
}