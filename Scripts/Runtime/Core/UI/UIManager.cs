using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HJ
{
    public class UIManager : Singleton<UIManager>
    {
        [Header("Temporary SFX")]
        [SerializeField] private AudioSource _onButtonHoverStartSFX;
        [SerializeField] private AudioSource _onButtonHoverEndSFX;
        [SerializeField] private AudioSource _onButtonPressedSFX;

        public void OnButtonPointerEnter() {
            _onButtonHoverStartSFX?.Play();
        }
        
        public void OnButtonPointerExit() {
            _onButtonHoverEndSFX?.Play();
        }
        
        public void OnButtonClick() {
            _onButtonPressedSFX?.Play();
        }
    }
}
