using UnityEngine;
using Newtonsoft.Json.Linq;

namespace HJ.Runtime
{
    public class SimpleLight : MonoBehaviour, ISaveable
    {
        [SerializeField] private Light _light;

        [Header("Settings")]
        [SerializeField] private bool _useEmission = true;
        [SerializeField] private bool _lightState = false;

        [Header("Emission")]
        [SerializeField] private RendererMaterial _lightRenderer;
        [SerializeField] private string _emissionKeyword = "_EMISSION";

        private void Awake()
        {
            if (_lightState) SetLightState(true);
        }

        public void SetLightState(bool state)
        {
            if (state)
            {
                _light.enabled = true;
                if (_useEmission) _lightRenderer.ClonedMaterial.EnableKeyword(_emissionKeyword);
            }
            else
            {
                _light.enabled = false;
                if (_useEmission) _lightRenderer.ClonedMaterial.DisableKeyword(_emissionKeyword);
            }

            _lightState = state;
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { "lightState", _lightState }
            };
        }

        public void OnLoad(JToken data)
        {
            bool lightState = (bool)data["lightState"];
            SetLightState(lightState);
        }
    }
}