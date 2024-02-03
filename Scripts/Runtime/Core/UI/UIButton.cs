using UnityEngine;

namespace HJ.UI
{
    [RequireComponent(typeof(UnityEngine.UIElements.Button))]
    public class UIButton : MonoBehaviour
    {
        public void OnPointerEnter() {
            UIManager.Instance.OnButtonPointerEnter();
        }
        
        public void OnPointerExit() {
            UIManager.Instance.OnButtonPointerExit();
        }
        
        public void OnClick() {
            UIManager.Instance.OnButtonClick();
        }
    }
}
