using UnityEngine;

namespace HJ
{
    public class GlobalGameSettingsManager : MonoBehaviour
    {
        private void Start()
        {
            GlobalGameSettings.ApplySettingsOnStart();
        }
    }
}
