using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace HJ.Runtime
{
    public class LeversPuzzleOrderLights : MonoBehaviour, ISaveable
    {
        [Serializable]
        public struct OrderLight
        {
            public Light Light;
            public RendererMaterial LightMaterial;
        }

        [SerializeField] private LeversPuzzle _leversPuzzle;
        [SerializeField] private List<OrderLight> _orderLights = new();
        [SerializeField] private string _emissionKeyword = "_EMISSION";
        [SerializeField] private int _orderIndex = 0;

        public LeversPuzzle LeversPuzzle => _leversPuzzle;

        public void OnSetLever()
        {
            if (_orderIndex < _leversPuzzle.Levers.Count)
                SetLightState(_orderLights[_orderIndex++], true);
        }

        public void ResetLights()
        {
            foreach (var item in _orderLights)
            {
                SetLightState(item, false);
            }

            _orderIndex = 0;
        }

        private void SetLightState(OrderLight light, bool state)
        {
            light.Light.enabled = state;
            if (state) light.LightMaterial.ClonedMaterial.EnableKeyword(_emissionKeyword);
            else light.LightMaterial.ClonedMaterial.DisableKeyword(_emissionKeyword);
        }

        public StorableCollection OnSave()
        {
            StorableCollection storableCollection = new StorableCollection();

            for (int i = 0; i < _orderLights.Count; i++)
            {
                string name = "light_" + i;
                bool lightState = _orderLights[i].Light.enabled;
                storableCollection.Add(name, lightState);
            }

            storableCollection.Add("orderIndex", _orderIndex);
            return storableCollection;
        }

        public void OnLoad(JToken data)
        {
            for (int i = 0; i < _orderLights.Count; i++)
            {
                string name = "light_" + i;
                bool lightState = (bool)data[name];
                SetLightState(_orderLights[i], lightState);
            }

            _orderIndex = (int)data["orderIndex"];
        }
    }
}