using UnityEngine;
using UnityEngine.Events;

namespace HJ.Runtime
{
    public class GLocText : MonoBehaviour
    {
        [SerializeField] private GString _glocKey;
        [SerializeField] private bool _observeMany;
        [SerializeField] private UnityEvent<string> _onUpdateText;

        public GString GlocKey => _glocKey;
        public UnityEvent<string> OnUpdateText => _onUpdateText;

        private void Start()
        {
            if (!GameLocalization.HasReference)
                return;

            if (!_observeMany) _glocKey.SubscribeGloc(text => _onUpdateText?.Invoke(text));
            else _glocKey.SubscribeGlocMany(text => _onUpdateText?.Invoke(text));
        }
    }
}