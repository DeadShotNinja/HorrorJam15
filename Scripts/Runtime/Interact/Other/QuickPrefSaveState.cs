using UnityEngine;

namespace HJ.Runtime
{
    public class QuickPrefSaveState : MonoBehaviour
    {
        public void SaveEndGameState()
        {
            PlayerPrefs.SetInt("EndGame", 1);
        }
    }
}
