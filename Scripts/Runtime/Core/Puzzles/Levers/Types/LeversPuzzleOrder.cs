using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

namespace HJ.Runtime
{
    [Serializable]
    public class LeversPuzzleOrder : LeversPuzzleType
    {
        [SerializeField] private string _leversOrder = "";

        private string _currentOrder = "";
        private bool _validate = false;
        
        public string LeversOrder
        {
            get => _leversOrder;
            set => _leversOrder = value;
        }

        public override void OnLeverInteract(LeversPuzzleLever lever)
        {
            if (_validate) 
                return;

            int leverIndex = Levers.IndexOf(lever);
            _currentOrder += leverIndex;
            TryToValidate();
        }

        public override void TryToValidate()
        {
            if (_currentOrder.Length >= Levers.Count)
            {
                _validate = true;
                ValidateLevers();
            }
        }

        public override bool OnValidate()
        {
            bool result = _leversOrder.Equals(_currentOrder);

            if (result) DisableLevers();
            else _validate = false;

            _currentOrder = "";
            return result;
        }

        public override StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { nameof(_currentOrder), _currentOrder },
                { nameof(_validate), _validate },
            };
        }

        public override void OnLoad(JToken token)
        {
            _currentOrder = token[nameof(_currentOrder)].ToString();
            _validate = (bool)token[nameof(_validate)];
        }
    }
}