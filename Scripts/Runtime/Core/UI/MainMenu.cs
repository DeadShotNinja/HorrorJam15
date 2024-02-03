using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace HJ
{
    public class MainMenu : MonoBehaviour
    {
        [FormerlySerializedAs("_screenMainMenu")]
        [SerializeField] private GameObject _mainMenuScreen;
        [SerializeField] private SettingsScreen _settingsScreen;
        [SerializeField] private GameObject _creditsScreen;
        
        [Header("Buttons")]   
        [SerializeField] private Button _buttonContinue;
        [SerializeField] private Button _buttonNewGame;
        [SerializeField] private Button _buttonSettings;
        [SerializeField] private Button _buttonCredits;
        [SerializeField] private Button _buttonExit;

        void Start()
        {
            _settingsScreen.gameObject.SetActive(false);
            _creditsScreen.SetActive(false);
        }

        public void OnButtonContinuePressed()
        {
            Debug.Log("TODO Continue!");
        }

        public void OnButtonExitPressed()
        {
            Application.Quit();
        }
    }
}
