using UnityEngine;

namespace HJ.Runtime
{
    public class UIButtonTabs : MonoBehaviour
    {
        [SerializeField] private bool _selectFirstTab = true;
        [SerializeField] private GameObject[] _tabs;
        
        private UIButton[] _uiButtons;

        private void Awake()
        {
            _uiButtons = transform.GetComponentsInChildren<UIButton>();
        }

        private void Start()
        {
            if (!_selectFirstTab)
                return;

            SelectTab(0);
            _uiButtons[0].SelectButton();
        }

        public void DeselectOthers(UIButton current)
        {
            foreach (var button in _uiButtons)
            {
                if (button != current)
                    button.DeselectButton();
            }
        }

        public void SelectTab(int index)
        {
            for (int i = 0; i < _tabs.Length; i++)
            {
                var tab = _tabs[i];
                tab.SetActive(i == index);
            }
        }

        public void SelectTabWthButton(int index)
        {
            for (int i = 0; i < _tabs.Length; i++)
            {
                var tab = _tabs[i];
                tab.SetActive(i == index);

                if (i == index) _uiButtons[i].SelectButton();
                else _uiButtons[i].DeselectButton();
            }
        }
    }
}